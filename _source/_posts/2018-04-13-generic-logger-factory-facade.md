---
layout: post
title: "A generic logger factory facade for classic ASP.NET"
teaser: "ASP.NET Core already has this feature. There is a ILoggerFactory to create a logger. You are able to inject the ILoggerFactory to your component (Controller, Service, etc.) and to create a named logger out of it. We recently missed that feature in a classic ASP.NET Core project, running on the full .NET Framework. This post is about to create it."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- dependency injection
- Unit Test
- Ninject
- Logging
- log4net
---

ASP.NET Core already has this feature. There is a `ILoggerFactory` to create a logger. You are able to inject the `ILoggerFactory` to your component (Controller, Service, etc.) and to create a named logger out of it. During testing you are able to replace this factory with a mock, to not test the logger as well and to not have an additional dependency to setup.

Recently we had the same requirement in a classic ASP.NET project, where we use Ninject to enable dependency injection and log4net to log all the stuff we do and all exceptions. One important requirement is a named logger per component.

## Creating named loggers

Usually log4net gets created inside the components as a private static instance:

~~~ csharp
private static readonly ILog _logger = LogManager.GetLogger(typeof(HomeController));
~~~

There already is a static factory method to create a named logger. Unfortunately this isn't really testable anymore and we need a different solution.

We could create a bunch of named logger in advance and register them to Ninject, which obviously is not the right solution. We need to have a more generic solution. We figured out two different solutions:

~~~ csharp
// would work well
public MyComponent(ILoggerFactory loggerFactory)
{
    _loggerA = loggerFactory.GetLogger(typeof(MyComponent));
    _loggerB = loggerFactory.GetLogger("MyComponent");
    _loggerC = loggerFactory.GetLogger<MyComponent>();
}
// even more elegant
public MyComponent(
    ILoggerFactory<MyComponent> loggerFactoryA
    ILoggerFactory<MyComponent> loggerFactoryB)
{
    _loggerA = loggerFactoryA.GetLogger();
    _loggerB = loggerFactoryB.GetLogger();
}
~~~

We decided to go with the second approach, which is a a simpler solution. This needs a dependency injection container that supports open generics like Ninject, Autofac and LightCore.

## Implementing the LoggerFactory

Using Ninject the binding of open generics looks like this:

~~~ csharp
Bind(typeof(ILoggerFactory<>)).To(typeof(LoggerFactory<>)).InSingletonScope();
~~~

This binding creates an instance of `LoggerFactory<T>` using the requested generic argument. If I request for an `ILoggerFactory<HomeController>`, Ninject creates an instance of `LoggerFactory<HomeController>`.

We register this as an singleton to reuse the `ILog` instances as we would do using the usual way to create the `ILog` instance in a private static variable.

The implementation of the `LoggerFactory` is pretty easy. We use the generic argument to create the log4net `ILog` instance:

~~~ csharp
public interface ILoggerFactory<T>
{
	ILog GetLogger();
}

public class LoggerFactory<T> : ILoggerFactory<T>
{
    private ILog _logger;
    public ILog GetLogger()
    {
        if (_logger == null)
        {
            var type = typeof(T);
            _logger = LogManager.GetLogger(typeof(T));
        }
        return _logger;
    }
}
~~~

We need to ensure the logger is created before creating a new one. Because Ninject creates a new instance of the `LoggerFactory` per generic argument, the `LoggerFactory` don't need to care about the different loggers. It just stores a single specific logger.

## Conclusion

Now we are able to create one or more named loggers per component.

What we cannot do, using this approach is to create individual named loggers, using a specific string as a name. There is a type needed that gets passed as generic argument. So every time we need an individual named logger we need to create a specific type. In our case this is not a big problem.

If you don't like to create types just to create individual named loggers, feel free to implement a non generic `LoggerFactory` and make a generic `GetLogger` method as well as a `GetLogger` method that accepts strings as logger names.
