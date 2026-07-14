---
layout: post
title: "Designing a AI Planner for AI Sub Agents in .NET"
teaser: "For a while now, I spend my evenings and spare time on systems where an LLM doesn't only answer a question. That's a different engineering problem than the usual In this post, I'd like to walk through the pattern that worked best for me so far: a planner that delegates to a set of specialists, instead of one model that tries to juggle everything itself."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- AppSec
- Headers
---

For a while now, I spend my evenings and spare time on systems where an LLM doesn't only answer a question — it decides what to do next, acts on that decision, and reacts to whatever comes back. That's a different engineering problem than the usual "send a prompt, render the response" work most of us started with, and it breaks in different places. In this post, I'd like to walk through the pattern that worked best for me so far: a planner that delegates to a set of specialists, instead of one model that tries to juggle everything itself.

### About the problem

Give an LLM more than one or two callable tools, and a flat tool list stops working. The model has to keep every tool's purpose in context on every single turn, choose between tools that often overlap in scope, and keep track of which tool's output feeds into which decision. In practice, this shows up as the model picking the wrong tool, calling the right tool with the wrong arguments, or just ignoring half of what you gave it.

The fix scales the same way it always did in distributed systems: introduce a coordinator.

### About the planner/orchestrator pattern

Instead of exposing every capability directly to one LLM call, split the system into two layers:

- **A planner** — the only agent that reasons about intent and decides which specialist to call next. It doesn't do the work itself.
- **Specialists** — narrow agents that each do one job well, exposed back to the planner as callable tools in their own right.

The planner's context stays small: a handful of well-described specialists, not dozens of overlapping low-level functions. Each specialist's own prompt also stays small, because it only needs to be good at one thing. You get the same benefit you would expect from any well-factored system — easier to test each piece on its own, easier to reason about a failure without it spreading into the rest of the run.

### Let's sketch it out

Assume we need to coordinate a couple of specialists behind one entry point. I keep the setup minimal. This is just dummy code to show the pattern.

The contract every specialist implements is deliberately small:

```csharp
public interface ISpecialist
{
    string Name { get; }
    Task<SpecialistResult> InvokeAsync(string input, CancellationToken cancellationToken);
}

public sealed record SpecialistResult(bool Success, string Summary);
```

And the planner's job is only to pick a specialist, call it, and decide what to do with the result:

```csharp
public sealed class Planner(IEnumerable<ISpecialist> specialists)
{
    public async Task<string> RunAsync(string goal, CancellationToken cancellationToken)
    {
        foreach (var specialist in specialists)
        {
            var result = await specialist.InvokeAsync(goal, cancellationToken);

            if (result.Success)
            {
                return $"{specialist.Name}: {result.Summary}";
            }
        }

        return "No specialist could make progress on this goal.";
    }
}
```

This is of course a simplified stand-in — a real planner asks an LLM which specialist to call and why, instead of looping through all of them. But the shape is what matters here: the planner never touches a specialist's internals, and a specialist doesn't need to know it's being orchestrated at all.

### How this works out with real Agents in C#

A few things matter more here than in a typical request/response API.

**Async, all the way down.** Every specialist call is I/O bound — a network round trip to a model, sometimes several. `async`/`await` isn't optional here; it's what keeps the planner responsive while specialists run, and it's what makes running independent specialists at the same time actually worth it, where the task allows it.

**Cancellation as a first-class citizen.** Propagate one `CancellationToken` from the top of the run through every specialist call. A run the caller wants to stop should really stop — not finish three more specialist calls because cancellation was checked in one place and forgotten in another.

**Be careful about DI lifetimes.** Specialists that hold no state between calls are good candidates for a transient or scoped registration — cheap to create, and no risk that one caller's state leaks into another caller's. Keep singleton lifetime for things that are really meant to be shared, and think hard about thread-safety as soon as a singleton holds anything mutable that concurrent specialist calls might touch.

**Structured, bounded results.** Don't let a specialist hand the planner a raw dump of everything it saw — that's why `SpecialistResult` above is a short record, not a blob. The planner reasons in natural language over whatever you give it, and every token that isn't relevant is a chance for it to get distracted.

**You don't have to build the plumbing yourself.** In the sketch above, `ISpecialist` was only a stand-in to keep the shape visible — a plain interface with one `InvokeAsync` method. [LlmTornado](https://github.com/lofcz/LlmTornado) gives you the same idea more directly, and it makes something the abstract version left out explicit: a specialist is usually not just a deterministic method, it's an LLM-backed agent in its own right, with its own instructions and its own tools. LlmTornado lets you expose that whole agent to the planner as one callable tool via `AsTool()`:

```csharp
using LlmTornado;
using LlmTornado.Agents;

public sealed class WeatherSpecialist(TornadoApi api) : ISpecialist
{
    public TornadoAgent CreateAgent() =>
        new (
            client: api,
            model: ChatModel.OpenAi.Gpt4o,
            name: nameof(WeatherSpecialist),
            instructions: "You answer questions about current weather conditions. Keep answers short.",
            tools: [GetWeatherAsync]);

    [ToolName("get_weather")]
    [Description("Returns the current weather for a given city.")]
    public async Task<string> GetWeatherAsync(string city)
    {
        // call whatever weather API you like here
        return $"It's sunny in {city}.";
    }
}

var api = new TornadoApi(Environment.GetEnvironmentVariable("LLM_API_KEY"));
var specialists = [ new WeatherSpecialist(api) ];

var planner = new TornadoAgent(
    client: api,
    model: ChatModel.OpenAi.Gpt4o,
    name: "Planner",
    instructions: "Decide which specialist can help with the user's question, then delegate to it.",
    tools: [ specialists.Select(x => x.CreateAgent().AsTool() ]);
```

Two things happen here that the abstract sketch didn't show. First, `WeatherSpecialist` is itself an LLM call, not a plain method — it has its own instructions and can reason about the question before it even touches `GetWeatherAsync`. Second, the planner never sees any of that: `AsTool()` wraps the whole specialist agent so it looks, from the planner's side, exactly like any other tool it could call. The layering from the earlier sketch — planner delegates, specialist decides — still holds; LlmTornado just gives you a concrete, LLM-backed way to build the specialist side of it, instead of writing that plumbing yourself.

### Where it gets hard

The clean version of this pattern is easy to describe. Three things make the real version harder — and I'll walk through concrete solutions for each of these in the next post in this series, so take this as the "here's what to expect" preview, not the fix yet:

- **Ambiguous or partial specialist results.** Sometimes a specialist doesn't fail outright — it comes back with something that isn't conclusive. The planner then has to decide: retry, ask for clarification, or move on and note the gap. That decision logic deserves as much design attention as the happy path.
- **Timeouts that actually bound the run.** A single stuck specialist can stall the whole run if timeout handling is an afterthought. Give every specialist an explicit timeout, and make sure a timeout looks, to the planner, like any other result it has to reason about — not an unhandled exception that takes the whole run down with it.
- **Non-determinism.** The same starting input can produce a different specialist order on different runs, because the planner makes judgment calls, it doesn't follow a fixed script. Decide early how much of that you're willing to accept, and log enough detail to tell "the planner made a reasonable, different choice" apart from "something is actually broken."

### Conclusion

Splitting an LLM-driven system into a planner and a set of specialists isn't a new idea — it's the same coordinator pattern we reached for in distributed systems for years, applied to a new kind of caller. What it buys you is a small, focused context for every decision the model has to make, and a codebase where each specialist can be built, tested, and reasoned about on its own.

What it doesn't buy you is graceful failure by default. That has to be designed in on purpose — bounded results, propagated cancellation, explicit timeouts, and an honest answer to how much non-determinism you can live with. That's also exactly where most of the real engineering time goes, and, as promised above, it's what I'll dig into properly in the next post in this series.
