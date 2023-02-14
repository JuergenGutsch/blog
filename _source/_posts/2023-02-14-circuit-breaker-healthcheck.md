---
layout: post
title: "Creating a circuit breaker health check using Polly CircuitBreaker"
teaser: "In this blog post I try to implement an ASP.NET Core Health Check that uses Polly CircuitBreaker that breaks if the number of errors reaches a limit within a specific period of time."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Health Check
- Circuit Breaker
- Polly
---

Finally! After months of not writing a blog post, here it is:

A GitHub Issue on the ASP.NET Core Docs points me to [Polly CircuitBreaker](https://github.com/App-vNext/Polly/wiki/Circuit-Breaker). Which is really great. Before that, I didn't even know that circuit breakers is a term in the software industry. Actually, I implemented mechanisms that work like that but never called them circuit breakers. Maybe that's the curse of never having visited a university :-D

http://www.thepollyproject.org/

## What is a circuit breaker?

Let's assume you have a connection to an external resource that breaks from time to time, which doesn't break your application but degraded the health of your application. In case you check that broken connection your application will be in a degraded state from time to time. What if those connection issues increase? When will it be a broken state? One broken connection out of one thousand might be okay. One out of ten might look quite unhealthy, right? If so, it makes sense to count the number of issues within a period of time and throw an exception if the number of exceptions exceeds the allowed number of exceptions. Exactly this is a circuit breaker.

Please excuse the amateurish explanation, I'm sure [Martin Fowler can do it much better](https://martinfowler.com/bliki/CircuitBreaker.html).

With Polly's circuit breaker, you can define how many specific exceptions are allowed to happen within a specific time period before throwing an exception.

The following snippet shows the usage of Polly's circuit breaker:

~~~csharp
var policy = Policy.Handle<HttpRequestException>()
    .CircuitBreakerAsync(
        exceptionsAllowedBeforeBreaking: 2,
        durationOfBreak: TimeSpan.FromMinutes(1)
    ));
await policy.ExecuteAsync(async () =>
{
    var client = new HttpClient();
    var response = await client.GetAsync("http://localhost:5259/api/dummy");
    if (!response.IsSuccessStatusCode)
    {
        throw new HttpRequestException();
    }
});
~~~

This creates an `AsyncCircuitBreakerPoliy` that allows two exceptions within a minute before throwing an exception.

Actually, I wanted to see how this would look like in an ASP.NET Core Health Check. The health check I'm going to show here isn't perfect but shows the concept of it:

## Creating a circuit breaker health check 

Adding a circuit breaker to a health check or in general into a web application requires you to persist the state of that circuit breaker over multiple scopes or requests. This means we need to store instances of the `AsyncCircuitBreakerPolicy` as singletons in the service collection. [See here](https://github.com/App-vNext/Polly/wiki/Circuit-Breaker#scoping-circuitbreaker-instances).

### Preparing the test application

To test the implementation I created a minimal endpoint that fails randomly within a new web application:

~~~csharp
app.MapGet("/api/dummy", () =>
{
    var rnd = Random.Shared.NextInt64(0, 1000);
    if ((rnd % 5) == 0)
    {
        throw new Exception("new exception");
    }
    return rnd;
});
~~~

This endpoint returns a random number and fails in case the random number can be divided by five. This exception is meaningless, but the endpoint is good to test the health check we will implement.

We also need to create a health check endpoint that we will call to see the current health state. This endpoint also executes the health check every time we call it. When calling it, the health check will call the dummy API and gets a randomly generated error.

~~~csharp
app.UseHealthChecks("/health");
~~~

### Implementing the health check

Next, we are going to write a health check that gets an `AsyncCircuitBreakerPolicy` via the service provider and executes a web request against the dummy breaking endpoint:

~~~csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CircuitBreakerChecks;

public class ApiCircuitBreakerHealthCheck<TPolicy> : IHealthCheck where TPolicy : ApiCircuitBreakerContainer
{
    private readonly ApiCircuitBreakerHealthCheckConfig _config;
    private readonly IServiceProvider _services;

    public ApiCircuitBreakerHealthCheck(
        ApiCircuitBreakerHealthCheckConfig config, 
        IServiceProvider services)
    {
        _config = config;
        _services = services;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var policy = _services.GetService<TPolicy>()?.Policy;

        try
        {
            if (policy is not null)
            {
                await policy.ExecuteAsync(async () =>
                {
                    var client = new HttpClient();
                    var response = await client.GetAsync(_config.Url);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException();
                    }
                });
            }
        }
        catch (Exception)
        {
            return HealthCheckResult.Unhealthy("Unhealthy");
        }

        return HealthCheckResult.Healthy("Healthy");
    }
}
~~~

This health check is generic and receives a container for the `AsyncCircuitBreakerPolicy` as a generic type argument. We'll see later why.

In the `CheckHealthMethod` we take the specific container from the service provider to get the actual Polly `AsyncCircuitBreakerPolicy` and we use it as shown in the first snippet. That part was quite common.

The container is a really simple object that just stores the Policy:

~~~csharp
using Polly.CircuitBreaker;

namespace CircuitBreakerChecks;

public class ApiCircuitBreakerContainer
{
    private readonly AsyncCircuitBreakerPolicy _policy;
    public ApiCircuitBreakerContainer(AsyncCircuitBreakerPolicy policy)
    {
        _policy = policy;
    }
    public AsyncCircuitBreakerPolicy Policy => _policy;
}
~~~

This container gets registered as a singleton to persist the policy for a longer period of time.

The health check also uses a configuration class that passes the configuration arguments to the health check. Currently, it is just the URL of the API to test and the name of the health check registration:

~~~
namespace CircuitBreakerChecks;

public class ApiCircuitBreakerHealthCheckConfig
{
    public string Url { get; set; }
}
~~~

This configuration class gets registered as transient.

Now let's puzzle that all together to get it running. We could do that all in the `Program.cs`, but will mess up the file, though.

Instead of messing up the `Program.cs`, I would like to have a configuration like this:

~~~csharp
builder.Services.AddApiCircuitBreakerHealthCheck(
    "http://localhost:5259/api/dummy", // URL to check
    "AddApiCircuitBreakerHealthCheck", // Name of the health check registration
    Policy.Handle<HttpRequestException>() // Polly CircuitBreaker Async Policy
        .CircuitBreakerAsync(
            exceptionsAllowedBeforeBreaking: 2,
            durationOfBreak: TimeSpan.FromMinutes(1)
        ));
~~~

In your project, you might need to change the URL to match your local port. 

The call in this snippet will register and configure the `ApiCircuitBreakerHealthCheck`. This means we will create an extension method on the `IServiceCollection` to stick that all together:

~~~csharp
using Polly.CircuitBreaker;

namespace CircuitBreakerChecks;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddApiCircuitBreakerHealthCheck(
        this IServiceCollection services,
        string url,
        string name,
        AsyncCircuitBreakerPolicy policy)
    {
        services.AddTransient(_ => new ApiCircuitBreakerHealthCheckConfig { Url = url });
        services.AddSingleton(new ApiCircuitBreakerContainer(policy));
        services.AddHealthChecks()
        	.AddCheck<ApiCircuitBreakerHealthCheck<ApiCircuitBreakerContainer>>(name);
        return services;
    }
}
~~~

That's it.

## Trying it out 

To try it out, run the application and call the health check endpoint in the browser.

The first two calls should display a healthy state for sure. Then it stays healthy until it gets at least two errors within a period of one minute. After that, it stays unhealthy until it gets less than two errors within that period.

Play around with it. Debug the minimal endpoint or debug the health check. It is kind of fun.

I published the demo code to GitHub: https://github.com/JuergenGutsch/aspnetcore-circuitbreaker-healthcheck

## One issue left

With this implementation, we can just use one registration of this endpoint. Creating a generic health check that way didn't really make sense. The reason is the singleton instance of the Policy CircuitBreaker. This instance would be shared over multiple health check registrations. To solve this we need to find a way to register a unique singleton of a policy per health check registration. But this is a different story.

