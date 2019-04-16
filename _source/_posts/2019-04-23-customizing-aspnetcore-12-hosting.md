---
layout: post
title: "Customizing ASP.​NET Core Part 12: Hosting "
teaser: ""
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- Configuration
- WebHostBuilder
---

In this 12th part of this series, I'm going to write about how to customize hosting in ASP.NET Core. We will look into the hosting options, different kind of hosting and a quick look into hosting on the IIS. And while writing this post this again seems to get a long one.

> This will change in ASP.NET Core 3.0. I anyway decided to do this post about ASP.NET Core 2.2 because it still needs some time until ASP.NET Core 3.0 is released.

## This series topics

- [Customizing ASP.NET Core Part 01: Logging]({% post_url customizing-aspnetcore-01-logging.md %})
- [Customizing ASP.NET Core Part 02: Configuration]({% post_url customizing-aspnetcore-02-configuration.md %})
- [Customizing ASP.NET Core Part 03: Dependency Injection]({% post_url customizing-aspnetcore-03-dependency-injection.md %})
- [Customizing ASP.NET Core Part 04: HTTPS]({% post_url customizing-aspnetcore-04-https.md %})
- [Customizing ASP.NET Core Part 05: HostedServices]({% post_url customizing-aspnetcore-05-hostedservices.md %})
- [Customizing ASP.NET Core Part 06: Middlewares]({% post_url customizing-aspnetcore-06-middlewares.md %})
- [Customizing ASP.NET Core Part 07: OutputFormatter]({% post_url customizing-aspnetcore-07-outputformatter.md %})
- [Customizing ASP.NET Core Part 08: ModelBinders]({% post_url customizing-aspnetcore-08-modelbinders.md %})
- [Customizing ASP.NET Core Part 09: ActionFilter]({% post_url customizing-aspnetcore-09-actionfilters.md %})
- [Customizing ASP.NET Core Part 10: TagHelpers]({% post_url customizing-aspnetcore-10-taghelpers.md %})
- [Customizing ASP.NET Core Part 11: WebHostBuilder]({% post_url customizing-aspnetcore-11-webhostbuilder.md %})
- **customizing ASP.NET Core Part 12: Hosting - This article**

## Quick setup

For this series we just need to setup a small empty web application.

~~~ shell
dotnet new web -n ExploreHosting -o ExploreHosting
~~~

That's it. Open it with Visual Studio Code:

~~~ shell
cd ExploreHosting
code .
~~~

And voila, we get a simple project open in VS Code:

![](../img/customize-aspnetcore/simpleproject.PNG)

## WebHostBuilder

Like in the last post, we will focus on the `Program.cs`. The `WebHostBuilder` is our friend. This is where we configure and create the web host. The next snippet is the default configuration of every new ASP.NET Core web we create using `File => New => Project` in Visual Studio or `dotnet new` with the .NET CLI:

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

As we already know from the previous posts the default build has all the needed stuff pre-configured. All you need to run an application successfully on Azure or on an on-premise IIS is configured for you.

But you are able to override almost all of this default configurations. Also the hosting configuration.

### Kestrel

After the `WebHostBuilder` is created we can use various functions to configure the builder. Here we already see one of them, which specifies the `Startup` class that should be used. In the last post we saw the `UseKestrel` method to configure the Kestrel options:

~~~csharp
.UseKestrel((host, options) =>
{
    // ...
})
~~~

> Reminder: Kestrel is one possibility to host your application. Kestrel is a web server built in .NET and based on .NET socket implementations. Previously it was built on top of libuv, which is the same web server that is used by NodeJS. Microsoft removes the dependency to libuv and created an own web server implementation based on .NET sockets.
>
> Docs: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel

This first argument is a `WebHostBuilderContext` to access already configured hosting settings or the configuration itself. The second argument is an object to configure Kestrel. This snippet shows what we did in the last post to configure the socket endpoints where the host needs to listen to:

~~~ csharp
.UseKestrel((host, options) =>
{
    var filename = host.Configuration.GetValue("AppSettings:certfilename", "");
    var password = host.Configuration.GetValue("AppSettings:certpassword", "");
    
    options.Listen(IPAddress.Loopback, 5000);
    options.Listen(IPAddress.Loopback, 5001, listenOptions =>
    {
        listenOptions.UseHttps(filename, password);
    });
})
~~~

This will override the default configuration where you are able to pass in URLs, eg. using the `applicationUrl` property of the `launchSettings.json` or an environment variable.

### HTTP.sys

Did you know that there is another hosting option? A different web server implementation? It is [HTTP.sys](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/httpsys). This is a pretty mature library deep within Windows that can be used to host your application.

~~~ csharp
.UseHttpSys(options =>
{
    // ...
})
~~~

The HTTP.sys is different to Kestrel. It cannot be used in IIS because it is not compatible with the [ASP.NET Core Module](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/aspnet-core-module?view=aspnetcore-2.2) for IIS. 

The main reason to use HTTP.sys instead of Kestrel is Windows Authentication which cannot be used in Kestrel only. Another reason is, if you need to expose it to the internet without the IIS. 

Also the IIS is running on top of HTTP.sys for years. Which means `UseHttpSys()` and IIS are using the same web server implementation. To learn more about HTTP.sys please read the [docs](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/httpsys).

### Hosting on IIS 

ASP.NET Core shouldn't be directly exposed to the internet, even if it's supported for even Kestrel or the HTTP.sys. It would be the best to have something like a reverse proxy in between or at least a service that watches the hosting process. For ASP.NET Core the IIS isn't only a reverse proxy. It also takes care of the hosting process in case it brakes because of an error or whatever. It'll restart the process in that case. Also NgineX may be used as an reverse proxy on Linux that also takes care of the hosting process.

To host an ASP.NET Core web on an IIS or on Azure you need to publish it first. Publishing doesn't only compiles the project. It also prepares the project to host it on IIS or on an webserver on Linux like NgineX. 

> dotnet publish

![](../img/customize-aspnetcore/dotnet-publish.png)

This produces an output that can be mapped in the IIS



## HostBuilder

