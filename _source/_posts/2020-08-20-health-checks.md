---
layout: post
title: "ASP.NET Core Health Checks"
teaser: "In this post I'd like to what are the ASP.NET Core Health Checks for and what you can do with it."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- health checks
- Tag
---

Since a while I planned to write about the ASP.NET Health Checks which are actually pretty cool. The development of the ASP.NET Core Health Checks started in fall 2016. At that time it was a architectural draft. In November 2016 during the Global MVP Summit in Redmond we got ask to hack some health checks based on the architectural draft. It was Damien Bowden and me who met Glen Condron and Andrew Nurse during the Hackathon on the last summit day to get into the ASP.NET Health Checks and to write the very first checks and to try the framework. 

> Actually, I prepared a talk about the ASP.NET Health Checks. And I would be happy to do the presentation at your user group or your conference.

## What are the health checks for?

Imagine that you are creating an ASP.NET application that is pretty much dependent on some sub systems, like a database, a file system, an API, or something like that. This is a pretty common scenario. Almost every application is dependent on a database. If the connection to the database got lost for different reasons, the application will definitely break. This is how applications are developed since years. The database is the simplest scenario to imagine what the ASP.NET health checks are good for, but not the real reason why they are developed. So let's continue with the database scenario. 

* What if you where able the check whether the database is available or not before you actually connect to it. 
* What if you where able to tell your application to show a user friendly message about the database that is not available. 
* What if you could simply switch to a fallback database in case the actual one is not available? 
* What if you could tell a load balancer to switch to a different fallback environment, in case your application is unhealthy because of the missing database?

You can exactly do this with the ASP.NET Health Checks:

Check the health and availability of your sub-systems, provide an endpoint that tells other systems about the health of the current application, and consume health check endpoints of other systems.

Health checks are mainly made for microservice environments. where loosely coupled applications need to know the health state of the systems they are depending on. But they are also useful in more monolithic applications that are also dependent on some kind of subsystems and infrastructure.

# How to enable health checks?

I'd like to show the health check configuration in a new, plain and simple ASP.NET MVC project that I will create using the .NET CLI in my favorite console:

~~~shell
dotnet new mvc -n HealthCheck.MainApp -o HealthCheck.MainApp
~~~

The health checks are already in the framework and you don't need to add an separate NuGet package to use it. It is in the `Microsoft.Extensions.Diagnostics.HealthChecks` package that should be already available after the installation of the latest version of .NET Core.

To enable the health checks you need to add the relating services to the DI container:

~~~csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks();
    services.AddControllersWithViews();
}
~~~

This is also the place where we add the checks later on. But this should be good for now.

To also provide an endpoint to tell other applications about the state of the current system you need to map a route to the health checks inside the  Configure method of the Startup class:

~~~csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health");
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});
~~~

This will give you a URL where you can check the health state of your application. Let's quickly run the application and call this endpoint with a browser:

![]({{site.baseurl}}/img/healthchecks/run.png)

Celling the endpoint:

![]({{site.baseurl}}/img/healthchecks/healthy.png)

Our application is absolutely healthy. For sure, because there is no health check yet, that checks for something.

## Writing health checks

Like in many other APIs (e. g. the Middlewares) there are many ways to add health checks . The simplest way and the best way to understand how it is working is to use lambda methods:

~~~csharp
services.AddHealthChecks()
    .AddCheck("Foo", () =>
        HealthCheckResult.Healthy("Foo is OK!"), tags: new[] { "foo_tag" })
    .AddCheck("Bar", () =>
        HealthCheckResult.Degraded("Bar is somewhat OK!"), tags: new[] { "bar_tag" })
    .AddCheck("FooBar", () =>
        HealthCheckResult.Unhealthy("FooBar is not OK!"), tags: new[] { "foobar_tag" });
~~~

Those lines add three different health checks. They are named and the actual check is a Lambda expression that returns a specific `HealthCheckResult`. The result can be Healthy, Degraded or Unhealthy.

* **Healthy**: All is fine obviously.
* **Degraded**: The system is not really healthy, but it's not critical. Maybe a performance problem or something like that.
* **Unhealthy**: Something critical isn't working.

Usually a health check result has at least one tag to group them by topic or whatever. The message should be meaningful to easily identify the actual problem.

Those lines are not really useful, but they show how the health check are working. If we run the app again and call the endpoint, we would see a Unhealthy state, because it always shows the lowest state, which is Unhealthy. Feel free to play around with the different `HealthCheckResult`

Now let's demonstrate an more useful health check. This one pings a needed resource in the internet and checks the availability:

~~~csharp
services.AddHealthChecks()
    .AddCheck("ping", () =>
    {
        try
        {
            using (var ping = new Ping())
            {
                var reply = ping.Send("asp.net-hacker.rocks");
                if (reply.Status != IPStatus.Success)
                {
                    return HealthCheckResult.Unhealthy("Ping is unhealthy");
                }

                if (reply.RoundtripTime > 100)
                {
                    return HealthCheckResult.Degraded("Ping is degraded");
                }

                return HealthCheckResult.Healthy("Ping is healthy");
            }
        }
        catch
        {
            return HealthCheckResult.Unhealthy("Ping is unhealthy");
        }
    });
~~~

This actually won't work, because my blog runs on Azure and Microsoft doesn't allow to ping the app services. Anyway, this demo shows you how to handle the specific results and how to return the right `HealthCheckResults` depending on the state of the the actual check.  

But it doesn't really make sense to write those tests as lambda expressions and to mess with the `Startup` class. Good there is a way to also add class based health checks. 

Also just a simple and useless one, but it demonstrates the basic concepts:

~~~csharp
public class ExampleHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var healthCheckResultHealthy = true;

        if (healthCheckResultHealthy)
        {
            return Task.FromResult(
                HealthCheckResult.Healthy("A healthy result."));
        }

        return Task.FromResult(
            HealthCheckResult.Unhealthy("An unhealthy result."));
    }
}
~~~

This class implements `CheckHealthAsync` method from the `IHealthCheck` interface. The `HealthCheckContext` contains the already registered health checks in the property `Registration`. This might be useful to check the state of other specific health checks. 

To add this class as a health check in the application we need to use the generic `AddCheck` method:

~~~csharp
services.AddHealthChecks()
    .AddCheck<ExampleHealthCheck>("class based", null, new[] { "class" });
~~~

We also need to specify a name and at least one tag. With the second argument I'm able to set a default failing state. But null is fine, in case I handle all exceptions inside the health check, I guess.



## Expose the health state

As mentioned, I'm able to provide an endpoint to expose the health state of my application to systems that depends on the current app. But by default it responses just with a simple string that only shows the simple state. It would be nice to see some more details to tell the consumer what actually is happening.

Fortunately this is also possible by passing `HealthCheckOptions` into the `MapHealthChecks` method:

~~~csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health", new HealthCheckOptions()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});
~~~

With the Predicate you are able to filter specific health checks to execute and to get the state of those. In this case I want to execute them all. The `ResponseWriter` is needed to write the health information of the specific checks to the response. In that case I used a `ResponseWriter` from a community project that provides some cool UI features and a ton of ready-to-use health checks.

* GitHub: [https://github.com/xabaril/AspNetCore.Diagnostics.HealthChecks](https://github.com/xabaril/AspNetCore.Diagnostics.HealthChecks)
* NuGet: [HealthChecks.UI.Client](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI/)

~~~shell
dotnet add package AspNetCore.HealthChecks.UI
~~~

The `UIResponseWriter` of that project writes a JSON output  to the HTTP response that includes many details about the used health checks:

~~~json
{
  "status": "Unhealthy",
  "totalDuration": "00:00:00.7348450",
  "entries": {
    "Foo": {
      "data": {},
      "description": "Foo is OK!",
      "duration": "00:00:00.0010118",
      "status": "Healthy"
    },
    "Bar": {
      "data": {},
      "description": "Bar is somewhat OK!",
      "duration": "00:00:00.0009935",
      "status": "Degraded"
    },
    "FooBar": {
      "data": {},
      "description": "FooBar is not OK!",
      "duration": "00:00:00.0010034",
      "status": "Unhealthy"
    },
    "ping": {
      "data": {},
      "description": "Ping is degraded",
      "duration": "00:00:00.7165044",
      "status": "Degraded"
    },
    "class based": {
      "data": {},
      "description": "A healthy result.",
      "duration": "00:00:00.0008822",
      "status": "Healthy"
    }
  }
}
~~~

In case the overall state is Unhealthy the endpoint sends the result with a 503 HTTP response status, otherwise it is a 200. This is really useful if you just want to handle the HTTP response status.

The community project provides a lot more features. Also a nice UI to visualize the health state to humans. I'm going to show you this in a later section.

## Handle the states inside the application 

In the most cases you don't want to just expose the state to depending consumer of your app. It might also be the case that you need to handle the different states in your application, by showing a message in case the application is not working properly, disabling parts of the application that are not working, switching to a fallback source, or whatever is needed to run the application in an degraded state.

To do things like this, you can use the `HealthCheckService` that is already registered to the IoC Container with the `AddHealthChecks()` method. You can inject the `HealthCheckService` using the `IHealthCheckService` interface wherever you want.

Let's see how this is working!

In the `HomeController` I created a constructor that injects the `IHealthCheckService` the same way as other services need to be injected. I also created a new Action called Health that uses the `HealthCheckService` and calls the method `CheckHealthAsync`() to execute the checks and to retrieve a `HealthReport`. The `HealthReport` is than passed to the view:

~~~csharp
public class HomeController : Controller
{
    private readonly IHealthCheckService _healthCheckService;

    public HomeController(
        IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    public async Task<IActionResult> Health()
    {
        var healthReport = await _healthCheckService.CheckHealthAsync();
        
        return View(healthReport);
    }
~~~

Optionally you are able to pass a predicate to the method `CheckHealthAsync()`. With the Predicate you are able to filter specific health checks to execute and to get the state of those. In this case I want to execute them all. 

I also created a view called `Health.cshtml`. This view retrieves the `HealthReport` and displays the results:

~~~html
@using Microsoft.Extensions.Diagnostics.HealthChecks;
@model HealthReport

@{
    ViewData["Title"] = "Health";
}
<h1>@ViewData["Title"]</h1>

<p>Use this page to detail your site's health.</p>

<p>
    <span>@Model.Status</span> - <span>Duration: @Model.TotalDuration.TotalMilliseconds</span>
</p>
<ul>
    @foreach (var entry in Model.Entries)
    {
    <li>
        @entry.Value.Status - @entry.Value.Description<br>
        Tags: @String.Join(", ", entry.Value.Tags)<br>
        Duration: @entry.Value.Duration.TotalMilliseconds
    </li>
    }
</ul>
~~~

To try it out, I just need to run the application using `dotnet run` in the console and calling https://localhost:5001/home/health in the browser:

![]({{site.baseurl}}/img/healthchecks/healthview.png)

You could also try to analyze the `HealthReport` in the Controller, in your services to do something specific in case the the application isn't healthy anymore. 

## A pretty health state UI

The already mentioned GitHub project [AspNetCore.Diagnostics.HealthChecks](https://github.com/xabaril/AspNetCore.Diagnostics.HealthChecks) also provides a pretty UI to display the results in a nice and human readable way.

This just needs a little more configuration in the `Startup.cs`

Inside the method `ConfigureServices()` I needed to add the health checks UI services

~~~csharp
services.AddHealthChecksUI();
~~~

And inside the method `Configure()` I need to map the health checks UI Middleware right after the call of `MapHealthChecks`:

~~~csharp
endpoints.MapHealthChecksUI();
~~~

This adds a new route to our application to call the UI: `/healthchecks-ui`

We also need to register our health API to the UI. This will be done using small setting to the appsetings.json:

~~~ json
{
  ... ,
  "HealthChecksUI": {
    "HealthChecks": [
      {
        "Name": "HTTP-Api",
        "Uri": "https://localhost:5001/health"
      }
    ],
    "EvaluationTimeOnSeconds": 10,
    "MinimumSecondsBetweenFailureNotifications": 60
  }
}

~~~

This way you are able to register as many health endpoints to the UI as you like. Think about a separate application that only shows the health states of all your microservices. This would be the way to go.

Let's call the UI using this route `/healthchecks-ui`

![]({{site.baseurl}}/img/healthchecks/healthchecksui.png)

(Wow... Actually, the ping seemed to work, when I did this screenshot. )

This is awesome. This is a really great user interface to display the health of all your services. 

About the Webhooks and customization of the UI, you should read the great docs in the repository.

## Conclusion

The health checks are definitely a thing you should look into. No matter what kind of web application you are writing, it can help you to create more stable and more responsive applications. Applications that know about their health can handle degraded of unhealthy states in a way that won't break the whole application. This is very useful, at least from my perspective ;-) 

To play around with the demo application used for this post visit the repository on GitHub: 
https://github.com/JuergenGutsch/healthchecks-demo
