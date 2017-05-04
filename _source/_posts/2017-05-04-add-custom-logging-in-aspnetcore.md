---
layout: post
title: "How to add custom logging in ASP.NET Core"
teaser: "ASP.NET Core is pretty flexible and extendible. If you don't like the built-in logging, you are able to plug in your own logger or an existing logger like log4net, NLog, Elmah. In this post I'm going to show you how to add a custom logger."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- Logging
---

ASP.NET Core is pretty flexible, customizable and extendible. You are able to change almost everything. Even the logging. If you don't like the built-in logging, you are able to plug in your own logger or an existing logger like log4net, NLog, Elmah. In this post I'm going to show you how to add a custom logger.

The logger I show you, just writes out to the console, but just for one single log level. The feature is to configure different font colors per LogLevel. So this logger is called `ColoredConsoleLogger`.

## General

To add a custom logger, you need to add an `ILoggerProvider` to the `ILoggerFactory`, that is provided in the method Configure in the `Startup.cs`:

~~~ csharp
loggerFactory.AddProvider(new CustomLoggerProvider(new CustomLoggerConfiguration()));
~~~

The `ILoggerProvider` creates one or more `ILogger` which are used by the framework to log the information.

## The Configuration

The idea is, to create different colored console entries per log level and event ID. To configure this we need a configuration type like this:

~~~ csharp
public class ColoredConsoleLoggerConfiguration
{
  public LogLevel LogLevel { get; set; } = LogLevel.Warning;
  public int EventId { get; set; } = 0;
  public ConsoleColor Color { get; set; } = ConsoleColor.Yellow;
}
~~~

This sets the default level to `Warning` and the color to `Yellow`. If the `EventId` is set to 0, we will log all events.

## The Logger

The logger gets a name and the configuration passed in via the constructor. The name is the category name, which usually is the logging source, eg. the type where the logger is created in:

~~~ csharp
public class ColoredConsoleLogger : ILogger
{
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

    if (_config.EventId == 0 || _config.EventId == eventId.Id)
    {
      var color = Console.ForegroundColor;
      Console.ForegroundColor = _config.Color;
      Console.WriteLine($"{logLevel.ToString()} - {eventId.Id} - {_name} - {formatter(state, exception)}");
      Console.ForegroundColor = color;
    }
  }
}
~~~

We are going to create a logger instance per category name with the provider.

## The LoggerProvider

The `LoggerProvider` is the guy who creates the logger instances. Maybe it is not needed to create a logger instance per category, but this makes sense for some Loggers, like NLog or log4net. Doing this you are also able to choose different logging output targets per category if needed:

~~~ csharp
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
~~~

There's no magic here. The method `CreateLogger` creates a single instance of the `ColoredConsoleLogger` per category name and stores it in the dictionary.

## Usage

Now we are able to use the logger in the `Startup.cs`

~~~ csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
  loggerFactory.AddConsole(Configuration.GetSection("Logging"));
  loggerFactory.AddDebug();
  // here is our CustomLogger
  loggerFactory.AddProvider(new ColoredConsoleLoggerProvider(new ColoredConsoleLoggerConfiguration
  {
    LogLevel = LogLevel.Information,
    Color = ConsoleColor.Blue
  }));
  loggerFactory.AddProvider(new ColoredConsoleLoggerProvider(new ColoredConsoleLoggerConfiguration
  {
    LogLevel = LogLevel.Debug,
    Color = ConsoleColor.Gray
  }));
~~~

But this doesn't really look nice from my point of view. I want to use something like this:

```csharp
loggerFactory.AddColoredConsoleLogger(c =>
{
  c.LogLevel = LogLevel.Information;
  c.Color = ConsoleColor.Blue;
});
loggerFactory.AddColoredConsoleLogger(c =>
{
  c.LogLevel = LogLevel.Debug;
 c.Color = ConsoleColor.Gray;
});
```
This means we need to write at least one extension method for the `ILoggerFactory`:

~~~ csharp
public static class ColoredConsoleLoggerExtensions
{
  public static ILoggerFactory AddColoredConsoleLogger(this ILoggerFactory loggerFactory, ColoredConsoleLoggerConfiguration config)
  {
    loggerFactory.AddProvider(new ColoredConsoleLoggerProvider(config));
    return loggerFactory;
  }
  public static ILoggerFactory AddColoredConsoleLogger(this ILoggerFactory loggerFactory)
  {
    var config = new ColoredConsoleLoggerConfiguration();
    return loggerFactory.AddColoredConsoleLogger(config);
  }
  public static ILoggerFactory AddColoredConsoleLogger(this ILoggerFactory loggerFactory, Action<ColoredConsoleLoggerConfiguration> configure)
  {
    var config = new ColoredConsoleLoggerConfiguration();
    configure(config);
    return loggerFactory.AddColoredConsoleLogger(config);
  }
}
~~~

With this extension methods we are able to pass in an already defined configuration object, we can use the default configuration or use the configure Action as shown in the previous example:

~~~ csharp
loggerFactory.AddColoredConsoleLogger();
loggerFactory.AddColoredConsoleLogger(new ColoredConsoleLoggerConfiguration
{
  LogLevel = LogLevel.Debug,
  Color = ConsoleColor.Gray
});
loggerFactory.AddColoredConsoleLogger(c =>
{
  c.LogLevel = LogLevel.Information;
  c.Color = ConsoleColor.Blue;
});
~~~

## Conclusion

Now it's up to you to create a logger that writes the entries to a database, log file or whatever or just add an existing logger to your ASP.NET Core application.

