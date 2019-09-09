---
layout: post
title: "Neu in ASP.NET Core 3.0 - Blazor Server Side"
teaser: "o have a look into the generic hosting models, we should also have a look into the different application models we have in ASP.NET Core.  In this and the next post I'm going to write about Blazor, which is a new member of the ASP.NET Core family."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Blazor
---

To have a look into the generic hosting models, we should also have a look into the different application models we have in ASP.NET Core.  In this and the next post I'm going to write about Blazor, which is a new member of the ASP.NET Core family. To be more precisely, Blazor are two members of the ASP.NET Core family. On the one hand we have Blazor Server Side which actually is ASP.NET Core running on the server and on the other hand we have Blazor Client Side which looks like ASP.NET Core and is running on the browser inside a WebAssembly. Both frameworks share the same view framework, which is Razor Components. Both Frameworks may share the same view logic and business logic. Both frameworks are single page application (SPA) frameworks, there is no page reload from the server visible while browsing the application. Both frameworks look pretty similar up from the `Program.cs`

Under the hood, both frameworks are hosted completely different. While Blazor Client Side is completely running on the Client, there is no web server needed. Blazor Server Side on the other hand is running upon a web server and is using WebSockets and a generic JavaScript client to simulate the same SPA behavior as Blazor Client Side.

## Hosting and Startup

Within this post I'm trying to compare Blazor Server Side to the already known ASP.NET Core frameworks like MVC and Web API.

First let's create a new Blazor Server Side project using the .NET Core 3 Preview 7 SDK:

``` shell
dotnet new blazorserverside -n BlazorServerSideDemo -o BlazorServerSideDemo
cd BlazorServerSideDemo
code .
```

The second and third line changes the current directory to the project directory and opens it into Visual Studio Code, if it is installed.

The first thing I usually do is to have a short glimpse into the `Program.cs`, but in this case this class looks completely equal to the other projects. There is absolutely no difference:

``` csharp
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
```

At first a default `IHostBuilder` is created and upon this a `IWebHostBuilder` is created to spin up a Kestrel web server and to host a default ASP.NET Core application. Nothing spectacular here.

The `Startup.cs` may be more special.

Actually it looks like a common ASP.NET Core `Startup` class except there are different services registered and a different Middlewares is used:

```csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddSingleton<WeatherForecastService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
        });
    }
}
```

In the `ConfigureServices` method there are the Razor Pages added to the IoC container. Razor Pages is used to provide the page that is hosting the Blazor application. In this case it is the `_Host.cshtml` in the Pages directory. Every single page application (SPA) has at least one almost static page which is hosting the actual application that is running in the browser. React, Vue, Angular and so on have to have the same thing. It is a index.html that is loading all the JavaScripts and hosting the JavaScript application. In case of Blazor there is also a generic JavaScript running on the hosting page. This JavaScript will connect to a SignalR WebSocket that is running on the server side.

Additional to the Razor Pages, the services needed for Blazor Server Side will be added to the IoC container. This services will be needed by the Blazor Hub which actually is the SignalR Hub that provides the WebSocket endpoint.

The Configure also looks similar to the other ASP.NET Core frameworks. The only differences are in the last lines, where the Blazor Hub gets added and where the fallback page gets added. This fallback page actually is the hosting Razor Page mentioned before. Since the SPA supports deep links and created URLs for the different views created on the client, the application need to route to a fallback page in case the user directly navigates to client side route that is not existing on the server. So the server will just provide the hosting page and the client will load the right views depending on the URLs in the browser afterwards.

## Blazor

The key feature of Blazor are the razor based components, which get interpreted on a runtime that understand C# and Razor and rendered on the client. With Blazor Client Side it the Mono runtime running inside the WebAssembly and on the Server Side version it is the .NET Core runtime running on the server. That means the Razor components get interpreted and rendered on the server. After that they get pushed to the client using SignalR and placed on the right place inside the hosting page using the generic JavaScript which is connected to the SignalR.

So we have a server side rendered single page application, without any visible roundtrip to the server.

The Razor components are also placed in the pages folder, but have the file extension `.razor`. Except the `App.razor` which is directly in the project directory. Those are the actual view components, which contain the logic of the application.

If you have a more detailed look into the components, you'll see some similarities to React or Angular, in case you know those frameworks. I mentioned the `App.razor` which is the root component. Angular and React also have this kind of root component. Inside the Shared directory there is a `MainLayout.razor`, which is the layout component. (Also this kind of components are available in React and Angular.) All the other components in the pages directory are using this layout implicitly because it is set as the default layout in the `_Imports.razor`. Those components also define a route that is used to navigate to the component. Reusable components without a specific route are placed inside the Shared directory.

## Conclusion

Even this is just a small introduction and overview about Blazor Server side, but I only want to quickly show the new ASP.NET Core 3.0 frameworks to create web applications. This is the last kind of normal server application I want to show. In the next part, I'm going to show Blazor Client side which uses a completely different hosting model.

Blazor server side by the way is the new replacement for ASP.NET WebForms to create stateful web applications using C#. WebForms won't be migrated to ASP.NET Core. It will be supported in the same way as the full .NET Framework will be supported in the future. Which there will be no new versions and no new features in the future. With this new in mind, it absolutely makes sense to have a more detailed look into Blazor Server Side.