---
layout: post
title: "Customizing ASP.​NET Core Part 05: HostedServices"
teaser: "This this part of this series doesn't really show a customization. This part is more about a feature you can use to create background services to run tasks asynchronously inside your application. Actually I use this feature to regularly fetch data from a remote service in a small ASP.NET Core application. "
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- HostedServices
---

This fifth part of this series doesn't really show a customization. This part is more about a feature you can use to create background services to run tasks asynchronously inside your application. Actually I use this feature to regularly fetch data from a remote service in a small ASP.NET Core application. 

## The series topics

- [Customizing ASP.NET Core Part 01: Logging]({% post_url customizing-aspnetcore-01-logging.md %})
- [Customizing ASP.NET Core Part 02: Configuration]({% post_url customizing-aspnetcore-02-configuration.md %})
- [Customizing ASP.NET Core Part 03: Dependency Injection]({% post_url customizing-aspnetcore-03-dependency-injection.md %})
- [Customizing ASP.NET Core Part 04: HTTPS]({% post_url customizing-aspnetcore-04-https.md %})
- **Customizing ASP.NET Core Part 05: HostedServices - This article**
- [Customizing ASP.NET Core Part 06: Middlewares]({% post_url customizing-aspnetcore-06-middlewares.md %})
- [Customizing ASP.NET Core Part 07: OutputFormatter]({% post_url customizing-aspnetcore-07-outputformatter.md %})
- [Customizing ASP.NET Core Part 08: ModelBinders]({% post_url customizing-aspnetcore-08-modelbinders.md %})
- [Customizing ASP.NET Core Part 09: ActionFilter]({% post_url customizing-aspnetcore-09-actionfilters.md %})
- [Customizing ASP.NET Core Part 10: TagHelpers]({% post_url customizing-aspnetcore-10-taghelpers %})

## About HostedServcices 

`HostedServices` are a new thing in ASP.NET Core 2.0 and can be used to run tasks in the asynchronously in the background of your application. This can be used to fetch data periodically, do some calculations in the background or some cleanups. This can also be used to send preconfigured emails or whatever you need to do in the background.

`HostedServices` are basically simple classes, which implements the `IHostedService` interface.

~~~ csharp
public class SampleHostedService : IHostedService
{
	public Task StartAsync(CancellationToken cancellationToken)
	{
	}
	
	public Task StopAsync(CancellationToken cancellationToken)
	{
	}
}
~~~

A `HostedService` needs to implement a `StartAsync()` and a `StopAsync()` method. The `StartAsync()` is the place where you implement the logic to execute. This method gets executed once immediately after the application starts. The method `StopAsync()` on the other hand gets executed just before the application stops. This also means, to start a kind of a scheduled service you need to implement it by your own. You will need to implement a loop which executes the code regularly.

To get a `HostedService` executed you need to register it in the ASP.NET Core dependency injection container as a singleton instance:

~~~ csharp
services.AddSingleton<IHostedService, SampleHostedService>();
~~~

To see how a hosted service work, I created the next snippet. It writes a log message on start, on stop and every two seconds to the console:

~~~ csharp
public class SampleHostedService : IHostedService
{
	private readonly ILogger<SampleHostedService> logger;
	
	// inject a logger
	public SampleHostedService(ILogger<SampleHostedService> logger)
	{
		this.logger = logger;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Hosted service starting");

		return Task.Factory.StartNew(async () =>
		{
			// loop until a cancalation is requested
			while (!cancellationToken.IsCancellationRequested)
			{
				logger.LogInformation("Hosted service executing - {0}", DateTime.Now);
				try
				{
					// wait for 3 seconds
					await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
				}
				catch (OperationCanceledException) { }
			}
		}, cancellationToken);
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Hosted service stopping");
		return Task.CompletedTask;
	}
}
~~~

To test this, I simply created a new ASP.NET Core application, placed this snippet inside, register the `HostedService` and started the application by calling the next command in the console:

~~~ shell
dotnet run
~~~

This results in the following console output:

![]({{site.baseurl}}/img/customize-aspnetcore/hosted-service.png)

As you can see the log output is written to the console every two seconds.

## Conclusion

You can now start to do some more complex thing with the `HostedServices`. Be careful with the hosted service, because it runs all in the same application. Don't use to much CPU or memory, this could slow down your application.

For bigger applications I would suggest to move such tasks in a separate application that is specialized to execute background tasks. A separate Docker container, a BackroundWorker on Azure, Azure Functions or something like this. However it should be separated from the main application in that case

In the next part I'm going to write about `Middlewares` and how you can use them to implement special logic to the request pipeline, or how you are able to serve specific logic on different paths. [Customizing ASP.NET Core Part 06: Middlewares]({% post_url customizing-aspnetcore-06-middlewares.md %})
