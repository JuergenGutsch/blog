---
layout: post
title: "New in ASP.NET Core 3.0 - Generic Hosting Environment"
teaser: "This is a small introduction post about the Generic Hosting Environment in ASP.NET Core 3.0. During the next posts I'm going to write more about it and what you can do with it in combination with some more ASP.NET Core 3.0 features."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
---

In ASP.NET Core 3.0 the hosting environment changes to get more generic. Hosting is not longer bound to Kestrel and not longer bound to ASP.NET Core. This means you are able to create a host, that doesn't start the Kestrel web server and doesn't need to use the ASP.NET Core Framework.

This is a small introduction post about the Generic Hosting Environment in ASP.NET Core 3.0. During the next posts I'm going to write more about it and what you can do with it in combination with some more ASP.NET Core 3.0 features.

In the next posts we will see a lot more details about why this makes sense. For the short term: There are different hosting models. One is the already known web hosting. One other model is running a worker service without a web server and without ASP.NET Core. Also Blazor uses a different hosting model inside the web assembly.

How does it look like in ASP.NET Core 3.0?

First let's recap how it looks in previous versions. This is a ASP.NET Core 2.2 `Startup.cs` that creates an `IWebHostBuilder` to start up Kestrel and to bootstrap ASP.NET Core using the `Startup` class:

~~~csharp
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

The next snippet shows the `Program.cs` of a new ASP.NET Core 3.0 web project:

~~~ csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
~~~

Now a `IHostBuilder` will be created and configured first. If the default host builder is created, a `IWebHostBuilder` is created to use the configured `Startup` class.

The typical .NET Core App features like configuration, logging and dependency injection are configured on the level of the `IHostBuilder`. All the ASP.NET specific features like authentication, Middlewares, ActionFilters, Formatters, etc. are configured on the level of the `IWebHostBuilder`.

## Conclusion

This makes the Hosting environment a lot more generic and flexible. 

I'm going to write about specific scenarios during the next posts about the new ASP.NET Core 3.0 features. But first I will have a look into Startup.cs to see what is new in ASP.NET Core 3.0.

