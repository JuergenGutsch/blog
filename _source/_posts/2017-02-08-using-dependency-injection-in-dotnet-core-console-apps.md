---
layout: post
title: "Using Dependency Injection in .NET Core Console Apps"
teaser: "The Dependency Injection Container used in ASP.NET Core is not limited to ASP.NET Core. You are able to use it in any kind of .NET Project. This post shows how to use it in an .NET Core Console application."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- Formatter
---

The Dependency Injection (DI) Container used in ASP.NET Core is not limited to ASP.NET Core. You are able to use it in any kind of .NET Project. This post shows how to use it in an .NET Core Console application.

Create a Console Application using the dotnet CLI or Visual Studio 2017. The DI Container is not available by default, bit the `IServiceProvider` is. If you want to use an Custom or third party DI Container, you should provide an implementation if an `IServiceProvider`, as an encapsulation of a DI Container.

In this post I want to use the DI Container used in the ASP.NET Core projects. This needs an additional NuGet package "Microsoft.Extensions.DependencyInjection" (currently it is version 1.1.0)

> Since this library is a .[NET Standard Library](https://blogs.msdn.microsoft.com/dotnet/2016/09/26/introducing-net-standard/), it should also work in a .NET 4.6 application. You just need to add a reference to "Microsoft.Extensions.DependencyInjection"

After adding that package we can start to use it. I created two simple classes which are dependent to each other, to show the how it works in a simple way:

~~~ csharp
public class Service1 : IDisposable
{
  private readonly Service2 _child;
  public Service1(Service2 child)
  {
    Console.WriteLine("Constructor Service1");
    _child = child;
  }

  public void Dispose()
  {
    Console.WriteLine("Dispose Service1");
    _child.Dispose();
  }
}

public class Service2 : IDisposable
{
  public Service2()
  {
    Console.WriteLine("Constructor Service2");
  }

  public void Dispose()
  {
    Console.WriteLine("Dispose Service2");
  }
}
~~~

Usually you would also use interfaces and create the relationship between this two classes, instead of the concrete implementation. Anyway, we just want to test if it works.

In the `static void Main` of the console app, we create a new `ServiceCollection` and register the classes in a transient scope:

~~~ csharp
var services = new ServiceCollection();
services.AddTransient<Service2>();
services.AddTransient<Service1>();
~~~

This `ServiceCollection` comes from the added NuGet package. Your favorite DI container possibly uses another way to register the services. You could now share the `ServiceCollection` to additional components, who wants to share some more services, in the same way ASP.NET Core does it with the AddSomething (e. g. AddMvc()) extension methods.

Now we need to create the `ServiceContainer` out of that collection:

~~~ csharp
var provider = services.BuildServiceProvider();
~~~

We can also share the `ServiceProvider` in our application to retrieve the services, but the proper way is to use it only on a single entry point:

~~~ csharp
using (var service1 = provider.GetService<Service1>())
{
  // so something with the class
}
~~~

Now, let's start the console app and look at the console output:

![]({{ site.baseurl }}/img/di-core/di-output.png)

As you can see, this DI container is working in any .NET Core app.