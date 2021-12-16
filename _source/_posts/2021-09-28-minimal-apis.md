---
layout: post
title: "Thoughts about Minimal APIs"
teaser: "Description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Minimal API
- .NET 6
---

When a was [writing about the minimal API]() I was using the Preview 6 of the .NET 6 SDK where just the empty Web project and the Web API  was changed to Minimal API.

With the Release Candidate 1, all of the ASP.NET Core template projects are changed to use the Minimal API, even the MVC project. The minimal API was introduced to speed up small Microservice APIs and to have a simpler start for ASP.NET Core beginners. But is a MVC project a kind of project that needs a simple startup code?

I completely agree to have a easier start with small web projects like small Web API projects, Razor Pages, empty web projects and so on. But do we really need that for a project that will have growing complexity, like MVC, bigger web API projects as well as the SPA projects?

I don't thinks so. 

With the Release Candidate 1, even the Web API project is using controller classes. Which is kind of weird, because the performance benefit of the Minimal API is only given with the routed delegates.

I agree with the aspect, that having a larger web API project it would make sense to have the endpoints somehow grouped into classes instead of putting all the endpoints into  program.cs file.

I played around a little bit and put the exact same endpoint that is in the default Web API project into an empty web project. To be complete: This is the controller class of the Web API project:

~~~csharp
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}

~~~

Let's do it step by step. 

At first don't forget to add the NuGet package for Swagger

~~~~shell
dotnet add package Swashbuckle.AspNetCore
~~~~



I also want to use Swagger and SwaggerUI to describe the endpoints and to play around with the endpoints. This needs me to add Swagger to the Service Collection:

~~~csharp
using System;
using MiniWebApi;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MiniWebApi", Version = "v1" });
});
~~~

right after the app was built, I add Swagger in the development mode:

~~~csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MiniWebApi v1"));
}

app.UseHttpsRedirection();
~~~

I also added HTTPS redirection.

As next I add the actual endpoint:

~~~csharp
app.MapGet("/weatherforecast", (ILogger<WeatherForecast> logger)  => {
    var Summaries = new string[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    var result =  Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();

    return Results.Ok(result);
});
~~~

One last step is optional, but makes sense: I added "swagger" to the launchUrl in the launchSetting.json:

~~~json
"profiles": {
    "MiniWebApi": {
        ...
        "launchUrl": "swagger",
        ...
    },
    "IIS Express": {
        ...
        "launchUrl": "swagger",
        ...
    }
}
~~~

This will open the SwaggerUI in case you run the application using Visual Studio or using `dotnet watch`

It would be a mess to put more than three endpoints like this in the Program.cs, but since this is just C# code yo are free to refactor that code and to put the delegate into a separate class without having the controller route mapping overhead:

The result would be a class like this:

~~~csharp
namespace MiniWebApi;

public class WeatherForecastActions
{
    public static IResult WeatherForecastAction(ILogger<WeatherForecast> logger)
    {
        var Summaries = new string[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

        return Results.Ok(result);

    }
}
~~~

In the Program.cs this class needs to be used like this instead of the delegate:

~~~csharp
app.MapGet("/weatherforecast", WeatherForecastActions.WeatherForecastAction);
~~~

Actually this is working.  If you now would put around ten endpoints in the Program.cs it would still look clean. If the amount of endpoints increase, you can still create classes that bundle the endpoint registrations :

~~~csharp
namespace MiniWebApi;

public class WeatherForecastActions
{
    public static WebApplication MapWeatherForecastEndpoints(WebApplication app)
    {
        app.MapGet("/weatherforecast", WeatherForecastActions.WeatherForecastAction);
        app.MapGet("/weatherforecast", WeatherForecastActions.WeatherHistoryAction);

        return app;
    }
    // ...
~~~

In the program.cs:

~~~csharp
app.MapWeatherForecastEndpoints();
~~~









