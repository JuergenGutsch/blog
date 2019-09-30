---
layout: post
title: "Neu in ASP.NET Core 3.0 - Blazor Server Side"
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

In the last post we had a quick look into Blazor Server Side, which doesn't really differ on the hosting level. This is a regular ASP.NET Core application that will run on a web server. Blazor Client Site on the other hand differs for sure, because it doesn't need a web server, it completely runs in the browser.

Microsoft compiled the Mono runtime into a WebAssembly. With this, it is possible to execute .NET Assemblies natively inside the WebAssembly in the browser. This doesn't need a web serve. There is no HTTP traffic between the browser and a server part anymore.

## Let's have a look at the HostBuilder

This time the `Program.cs` look different compared to the default ASP.NET Core projects:

``` csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IWebAssemblyHostBuilder CreateHostBuilder(string[] args) =>
        BlazorWebAssemblyHost.CreateDefaultBuilder()
            .UseBlazorStartup<Startup>();
}
```

This time we create `IWebAssemblyHostBuilder` instead of a `IHostBuilder`. Actually it is a completely different Interface and doesn't derive from the `IHostBuilder` at the time I wrote this. But it looks pretty similar. In this case also a default configuration of the `IWebAssemblyHostBuilder` is created and similar to the ASP.NET Core projects, a `Startup` class is used to configure the application.

The `Startup` class is pretty empty but has the same structure as all the other ones. You are able to configure services to the IoC container and to configure the application:

``` csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
    }

    public void Configure(IComponentsApplicationBuilder app)
    {
        app.AddComponent<App>("app");
    }
}
```

Usually you won't configure a lot here, except the services. The only thing you can really do here is to execute code on startup, to maybe initialize a kind of a database or whatever you need to do on startup.

The important line of code here is the line where the root component is added to the application. Actually it is the `App.cshtml` in the root of the project. In Blazor server side this is the host page that calls the root component and here it is configured in the `Startup`. 

All the other UI stuff is pretty much equal in both versions of Blazor. 

## What you can do with Blazor Client Side

In general you can do the same things in both versions of Blazor. You can also share the same UI logic. Both versions are made to create single page application with C# and Razor and without to learn a JavaScript framework like React or Angular. It will be pretty easy for you to build single page applications, if you know C# and Razor.

The Client side version will live in the WebAssembly only and will work without a connection to a web server, if no remote service is needed. Usually every single page application needs a remote service to fetch data or to store date.

Blazor Client Side will have a lot faster UI, because it is all rendered natively on the client. All the C# and Razor code is running in the WebAssembly and Blazor Server Side still needs to send UI from the server to the client.

## Conclusion

In this part you learned a different kind of Hosting  in ASP.NET Core and this will lead us back to the generic hosting approach of ASP.NET Core 3.0.

In the next post I will write about a different hosting model to run service worker and background services without the full web server stack.