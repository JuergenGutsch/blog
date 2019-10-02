---
layout: post
title: "New in ASP.NET Core 3.0: Worker Services"
teaser: "Description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Tests
- Tag
---

I mentioned in on of the first posts of this series, that we are now able to create ASP.NET Core applications without a web server and without all the HTTP stuff that is needed to provide content via HTTP or HTTPS. At the first glance it sounds wired. Why should I create a ASP.NET application that doesn't provide any kind of an endpoint over HTTP? Is this really ASP.NET. Well, it is not ASP.NET in the sense of creating web applications. But it is part of the ASP.NET Core and uses all the cool features that we are got used to in ASP.NTE Core:

* Logging
* Configuration
* Dependency Injection
* etc.

In this kind of applications we are able to span up a worker service that is completely independent from the HTTP stack. 

> Worker services can run in any kind of .NET Core applications, but they don't need the `IWebHostBuilder` to run

## The worker service project

In Visual Studio or by using the .NET CLI you are able to create a new worker service project. This looks pretty much like a common .NET Core project, but all the web specific stuff is missing. The only 3 code files here are the `Program.cs` and a `Worker.cs`.

The `Program.cs` looks a little different compared to the other ASP.NET Core projects:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        	.ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>();
            });
}
```

There is just a `IHostBuilder` created, bot no `IWebHostBuilder`. There is also no `Startup.cs` created, which actually isn't needed in general. The `Startup.cs` should only be used to keep the `Program.cs` clean and simple. Actually the DI container is configure in the `Program.cs` in the method `ConfigureServices`.

In a regular ASP.NET Core application the line to register the Worker in the DI container, will actually also work in the `Startup.cs`. 

The worker is just a simple class that derives from `BackgroundService`:

``` csharp
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}
```

The `BackgroundService` base class is still a well known `IHostedService` that exists for a while. It just has some base implementation in it to simplify the API. You would also be able to create a `WorkerService` by implementing the `IHostedService` directly.

This demo worker just does a endless loop and writes the current date and time every second to the logger.



## What you can do with Worker Services

With this kind of services you are able to create services that do some stuff for you in the background or you can simply create service applications that can run as a windows service or as a service inside a docker container.

Worker Services are running one time on startup or just create a infinite loop to do stuff periodically. They run asynchronously in a separate thread and don't block the main application. With this in mind you are able to execute tasks that aren't really related to the applications domain logic

* Fetching data periodically 
* Sending mails periodically 
* Calculating data in the background
* Startup initialization

In a microservice environment it would make sense to run one or more worker services in console applications inside docker containers. This way it is easy to maintain and deploy them separately from the main application and they can be scaled separately.

