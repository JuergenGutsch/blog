---
layout: post
title: "Where Autonomous AI Agents Break: A Field Guide"
teaser: "In my last post, I ended it with a short list of the things that make the clean version of that pattern harder in practice and I said I'd come back to those with solutions instead of just naming them. This post is about where these systems might break, and what I do about each one."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET
- CSharp
- AI
- LLM
- Agents
- Orchestration
---

Back in [my last post]({% post_url aspnetcore6-01.md %}), I walked through a planner/specialist pattern for LLM tool-calling in .NET, and I ended it with a short list of the things that make the clean version of that pattern harder in practice: ambiguous results, timeouts, non-determinism. I said I'd come back to those with real solutions instead of just naming them. This post is that follow-through - a field guide to where these systems actually break, and what I do about each one.

### About the problem

An agent loop looks simple on a whiteboard: observe, decide, act, observe again. In code, every one of those four words hides a place where something can go wrong, and none of them fail the way a normal function call fails. A function either returns or throws. A step in an agent loop can return something that's technically successful and still wrong, or something that looks like a failure and isn't. Once you've seen enough runs, four failure modes show up again and again.

### Failure mode 1 - non-determinism

The same starting input can lead the planner to a different specialist, or a different order of specialists, on two separate runs. That's not a defect to eliminate - it's what you get when a model reasons about the next step instead of following a fixed script. Fighting it directly (forcing temperature to zero, pinning outputs) mostly just hides it, it doesn't remove it.

What actually helps is making the decision visible instead of invisible:

```csharp
public sealed record PlannerDecision(string SpecialistName, string Reason);

public sealed class Planner(IEnumerable<ISpecialist> specialists, ILogger<Planner> logger)
{
    public async Task<string> RunAsync(string goal, CancellationToken cancellationToken)
    {
        var decision = await ChooseSpecialistAsync(goal, cancellationToken);
        logger.LogInformation("Chose {Specialist} because: {Reason}", decision.SpecialistName, decision.Reason);

        var specialist = specialists.Single(s => s.Name == decision.SpecialistName);
        var result = await specialist.InvokeAsync(goal, cancellationToken);

        return $"{specialist.Name}: {result.Summary}";
    }
}
```

Logging the reason next to the choice turns "it picked something different this time" from a mystery into a question you can actually answer: was this a reasonable judgment call on slightly different phrasing, or is something actually broken? You can't tell those two apart without the reason on record.

### Failure mode 2 - tool-call and specialist errors

A specialist doesn't only succeed or fail outright. Often it comes back with something in between - a partial answer, a result it isn't confident about, an argument it filled in with a guess. Modeling that result as a plain `bool Success` throws away exactly the information the planner needs to decide what to do next:

```csharp
public enum SpecialistOutcome { Success, Inconclusive, Failure }

public sealed record SpecialistResult(SpecialistOutcome Outcome, string Summary);
```

With three outcomes instead of two, the planner's decision logic actually has something to work with:

```csharp
switch (result.Outcome)
{
    case SpecialistOutcome.Success:
        return result.Summary;
    case SpecialistOutcome.Inconclusive:
        return await RetryOrEscalateAsync(specialist, goal, cancellationToken);
    case SpecialistOutcome.Failure:
        return await TryNextSpecialistAsync(goal, cancellationToken);
}
```

The point isn't the specific enum - it's that "not done yet" and "actually failed" are different situations, and a planner that can't tell them apart will either retry a lost cause forever, or give up on something that was one more attempt away from working.

### Failure mode 3 - runaway loops

"Keep trying until it works" is not a stopping condition, it's the absence of one. Without an explicit budget, a loop that reruns on `Inconclusive` results can keep going long after it stopped being useful. The fix is boring on purpose - give the loop a hard limit and make hitting that limit a normal, expected outcome, not an afterthought:

```csharp
public async Task<string> RunWithBudgetAsync(string goal, int maxAttempts, CancellationToken cancellationToken)
{
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        var result = await TryOnceAsync(goal, cancellationToken);

        if (result.Outcome != SpecialistOutcome.Inconclusive)
        {
            return result.Summary;
        }
    }

    return $"Gave up after {maxAttempts} attempts - needs a human to look at this.";
}
```

That last line matters as much as the loop around it. A run that hits its budget should say so plainly, not disappear into a generic error. Someone downstream has to know the difference between "this finished" and "this stopped trying."

### Failure mode 4 - timeouts that don't bound the run

A single stuck specialist call - a slow model response, a hung network call - can stall a whole run if nothing bounds it. `CancellationTokenSource.CreateLinkedTokenSource` combined with a timeout gives every specialist call an upper limit, without changing how the rest of the loop treats the result:

```csharp
public async Task<SpecialistResult> InvokeWithTimeoutAsync(
    ISpecialist specialist, string goal, TimeSpan timeout, CancellationToken cancellationToken)
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    cts.CancelAfter(timeout);

    try
    {
        return await specialist.InvokeAsync(goal, cts.Token);
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
        return new SpecialistResult(SpecialistOutcome.Inconclusive, "Timed out before finishing.");
    }
}
```

The important detail is the `catch` - a timeout turns into an `Inconclusive` result the planner already knows how to handle, not an unhandled exception that takes the whole run down with it. The planner shouldn't need a special code path for "this step took too long." It's just one more version of "not done yet."

### Conclusion

None of these four fixes are exotic - a logged reason next to a decision, a third outcome value instead of a bare bool, a loop with a budget, a timeout that resolves into a normal result. What they have in common is that they all replace an implicit assumption ("the model will get it right," "it'll finish eventually," "a fast call doesn't need a timeout") with something explicit the rest of the system can actually reason about.

That's really the whole difference between a demo agent and one you can trust with something that matters: not a smarter model, but a loop that has been designed to fail in ways you can see and recover from, instead of ways that surprise you in production.
