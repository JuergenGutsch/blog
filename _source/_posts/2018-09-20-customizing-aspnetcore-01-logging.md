---
layout: post
title: "Customizing ASP.​NET Core Part 01: Logging"
teaser: "In this first part of the new blog series about customizing ASP.NET Core, I will show you how to customize the logging. The default logging only writes to the console or to the debug window. This is quite good for the most cases, but maybe you need to log to a sink like a file or a database. Maybe you want to extend the logger with additional information. In that cases you need to know how to change the default logging."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- Logging
---

In this first part of the new blog series about customizing ASP.NET Core, I will show you how to customize the logging. The default logging only writes to the console or to the debug window. This is quite good for the most cases, but maybe you need to log to a sink like a file or a database. Maybe you want to extend the logger with additional information. In that cases you need to know how to change the default logging.

## The series topics

- **Customizing ASP.NET Core Part 01: Logging - This article**
- [Customizing ASP.NET Core Part 02: Configuration]({% post_url customizing-aspnetcore-02-configuration.md %})
- [Customizing ASP.NET Core Part 03: Dependency Injection]({% post_url customizing-aspnetcore-03-dependency-injection.md %})
- [Customizing ASP.NET Core Part 04: HTTPS]({% post_url customizing-aspnetcore-04-https.md %})
- [Customizing ASP.NET Core Part 05: HostedServices]({% post_url customizing-aspnetcore-05-hostedservices.md %})
- [Customizing ASP.NET Core Part 06: Middlewares]({% post_url customizing-aspnetcore-06-middlewares.md %})
- [Customizing ASP.NET Core Part 07: OutputFormatter]({% post_url customizing-aspnetcore-07-outputformatter.md %})
- [Customizing ASP.NET Core Part 08: ModelBinders]({% post_url customizing-aspnetcore-08-modelbinders.md %})
- [Customizing ASP.NET Core Part 09: ActionFilter]({% post_url customizing-aspnetcore-09-actionfilters.md %})
- [Customizing ASP.NET Core Part 10: TagHelpers]({% post_url customizing-aspnetcore-10-taghelpers %})

## Configure logging

In previous versions of ASP.NET Core (pre 2.0) the logging was configured in the `Startup.cs`. Since 2.0 the `Startup.cs` was simplified and a lot of configurations where moved to a default `WebHostBuilder`, which is called in the `Program.cs`. Also the logging was moved to the default `WebHostBuilder`:

~~~ csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)                
            .UseStartup<Startup>();
}
~~~

In ASP.NET Core you are able to override and customize almost everything. So you can with the logging. The `IWebHostBuilder` has a lot of extension methods to override the default behavior. To override the default settings for the logging we need to choose the `ConfigureLogging` method. The next snippet shows exactly the same logging as it was configured inside the `CreateDefaultBuilder()` method:

~~~ csharp
WebHost.CreateDefaultBuilder(args)	
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
        logging.AddConsole();
        logging.AddDebug();
    })                
    .UseStartup<Startup>();
~~~

This method needs a lambda that gets a `WebHostBuilderContext` that contains the hosting context and a `LoggingBuilder` to configure the logging.

## Create a custom logger

To demonstrate a custom logger, I created a small useless logger that is able to colorize log entries with an specific log level in the console. This so called `ColoredConsoleLogger` will be added and created using a LoggerProvider we also need to write by our own. To specify the color and the log level to colorize, we need to add a configuration class. In the next snippet all three parts (Logger, LoggerProvider and Configuration) are shown:

~~~ csharp
public class ColoredConsoleLoggerConfiguration
{
    public LogLevel LogLevel { get; set; } = LogLevel.Warning;
    public int EventId { get; set; } = 0;
    public ConsoleColor Color { get; set; } = ConsoleColor.Yellow;
}

public class ColoredConsoleLoggerProvider : ILoggerProvider
{
    private readonly ColoredConsoleLoggerConfiguration _config;
    private readonly ConcurrentDictionary<string, ColoredConsoleLogger> _loggers = new ConcurrentDictionary<string, ColoredConsoleLogger>();

    public ColoredConsoleLoggerProvider(ColoredConsoleLoggerConfiguration config)
    {
        _config = config;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new ColoredConsoleLogger(name, _config));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}

public class ColoredConsoleLogger : ILogger
{
	private static object _lock = new Object();
    private readonly string _name;
    private readonly ColoredConsoleLoggerConfiguration _config;

    public ColoredConsoleLogger(string name, ColoredConsoleLoggerConfiguration config)
    {
        _name = name;
        _config = config;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel == _config.LogLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        lock (_lock)
        {
            if (_config.EventId == 0 || _config.EventId == eventId.Id)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = _config.Color;
                Console.WriteLine($"{logLevel.ToString()} - {eventId.Id} - {_name} - {formatter(state, exception)}");
                Console.ForegroundColor = color;
            }
        }
    }
}
~~~

> We need to lock the actual console output, because we will get some race conditions where wrong log entries get colored with the wrong color, because the console itself is not really thread save.

If this is done we can start to plug in the new logger to the configuration:

~~~ csharp
logging.ClearProviders();

var config = new ColoredConsoleLoggerConfiguration
{
    LogLevel = LogLevel.Information,
    Color = ConsoleColor.Red
};
logging.AddProvider(new ColoredConsoleLoggerProvider(config));
~~~

If needed you are able to clear all the previously added logger providers. Than we call `AddProvider` to add a new instance of our `ColoredConsoleLoggerProvider` with the specific settings. We could also add some more instances of the provider with different settings.

This shows ho to handle different log levels in a a different way. You can use this to send an emails on hard errors, to log debug messages to a different log sink than regular informational messages and so on. 

![]({{site.baseurl}}/img/customize-aspnetcore/custom-logger.PNG)

In many cases it doesn't make sense to write a custom logger because there are already many good third party loggers, like elmah, log4net and NLog. In the next section I'm going to show you how to use NLog in ASP.NET Core

## Plug-in an existing Third-Party logger provider

NLog was one of the very first loggers, which was available as a .NET Standard library and usable in ASP.NET Core. NLog also already provides a Logger Provider to easily plug it into ASP.NET Core.

The next snippet shows a typical `NLog.Config` that defines two different sinks to log all messages in one log file and custom messages only into another file:

~~~ xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true"
      internalLogLevel="Warn"
      internalLogFile="C:\git\dotnetconf\001-logging\internal-nlog.txt">

  <!-- Load the ASP.NET Core plugin -->
  <extensions>
    <add assembly="NLog.Web.AspNetCore"/>
  </extensions>

  <!-- the targets to write to -->
  <targets>
     <!-- write logs to file -->
     <target xsi:type="File" name="allfile" fileName="C:\git\dotnetconf\001-logging\nlog-all-${shortdate}.log"
                 layout="${longdate}|${event-properties:item=EventId.Id}|${logger}|${uppercase:${level}}|${message} ${exception}" />

   <!-- another file log, only own logs. Uses some ASP.NET core renderers -->
     <target xsi:type="File" name="ownFile-web" fileName="C:\git\dotnetconf\001-logging\nlog-own-${shortdate}.log"
             layout="${longdate}|${event-properties:item=EventId.Id}|${logger}|${uppercase:${level}}|  ${message} ${exception}|url: ${aspnet-request-url}|action: ${aspnet-mvc-action}" />

     <!-- write to the void aka just remove -->
    <target xsi:type="Null" name="blackhole" />
  </targets>

  <!-- rules to map from logger name to target -->
  <rules>
    <!--All logs, including from Microsoft-->
    <logger name="*" minlevel="Trace" writeTo="allfile" />

    <!--Skip Microsoft logs and so log only own logs-->
    <logger name="Microsoft.*" minlevel="Trace" writeTo="blackhole" final="true" />
    <logger name="*" minlevel="Trace" writeTo="ownFile-web" />
  </rules>
</nlog>
~~~

We than need to add the NLog ASP.NET Core package from NuGet:

~~~ shell
dotnet add package NLog.Web.AspNetCore
~~~

(Be sure you are in the project directory before you execute that command)

Now you only need to add NLog in the `ConfigureLogging` method in the `Program.cs`

~~~ csharp
hostingContext.HostingEnvironment.ConfigureNLog("NLog.Config");
logging.AddProvider(new NLogLoggerProvider());
~~~

The first line configures NLog to use the previously created `NLog.Config` and the second line adds the `NLogLoggerProvider` to the list of logging providers. Here you can add as many logger providers you need.

## Conclusion

The good thing of hiding the basic configuration is only to clean up the newly scaffolded projects and to keep the actual start as simple as possible. The developer is able to focus on the actual features. But the more the application grows the more important is logging. The default logging configuration is easy and it works like charm, but in production you need a persisted log to see errors from the past. So you need to add a custom logging or a more flexible logger like NLog or log4net.

To learn more about ASP.NET Core configuration have a look into the next part of the series: [Customizing ASP.NET Core Part 02: Configuration]({% post_url customizing-aspnetcore-02-configuration.md %}).''
