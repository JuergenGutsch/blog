---
layout: post
title: "Adding a custom dependency injection container in ASP.NET Core"
teaser: "ASP.NET Core is pretty flexible, customizable and extendable. You are able to change almost everything. Even the built-in dependency injection container can be replaced. If you prefer a different dependency injection container, because of some reasons, you are able to do it. This blog post will show you how to replace the existing DI container with another one. I'm going to use Autofac as a replacement."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- Unit Test
- XUnit
- MSTest
---

ASP.NET Core is pretty flexible, customizable and extendable. You are able to change almost everything. Even the built-in dependency injection container can be replaced. This blog post will show you how to replace the existing DI container with another one. I'm going to use Autofac as a replacement.

## Why should I do this?

There are not many reasons to replace the built-in dependency injection container, because it works pretty well for the most cases. 

If you prefer a different dependency injection container, because of some reasons, you are able to do it. Maybe you know a faster container, if you like the nice features of Ninject to load dependencies dynamically from an assembly in a specific folder, by file patterns, and so on. I really miss this features in the built in container. It is possible to use [another solution to to load dependencies from other libraries]({% post_url using-dependency-injection-in-multiple-projects.md %}), but this is not as dynamic as the Ninject way.

## Setup the Startup.cs

In ASP.NET Core the `IServiceProvider` is the component that resolves and creates the dependencies out of a `IServiceCollection`. The `IServiceCollection` needs to be manipulated in the method ConfigureServices within the `Startup.cs` if you want to add dependencies to the `IServiceProvider`.

The solution is to read the contents of the `IServiceCollections` to the own container and to provide an own implementation of a `IServiceProvider` to the application. Reading the IServiceCollection to the different container isn't that trivial, because you need to translate the different mappings types, which are probably not all available in all containers. E. g. the scoped registration (per request singleton) is a special one, that is only needed in web applications and not implemented in all containers. 

Providing a custom `IServiceprovider` is possible by changing the method `ConfigureServices` a little bit:

~~~ csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
{
  // Add framework services.
  services.AddMvc();

  return services.BuildServiceProvider();
}
~~~

The method now returns a `IServiceprovider `, which is created in the last line out of the `IServiceCollection`. It is needed to add the contents of the service collection to the container you want to use, because ASP.NET actually adds around 40 dependencies before this method is called:  

~~~ text
1: Singleton - Microsoft.AspNetCore.Hosting.IHostingEnvironment => Microsoft.AspNetCore.Hosting.Internal.HostingEnvironment
2: Singleton - Microsoft.Extensions.Logging.ILogger`1 => Microsoft.Extensions.Logging.Logger`1
3: Transient - Microsoft.AspNetCore.Hosting.Builder.IApplicationBuilderFactory => Microsoft.AspNetCore.Hosting.Builder.ApplicationBuilderFactory
4: Transient - Microsoft.AspNetCore.Http.IHttpContextFactory => Microsoft.AspNetCore.Http.HttpContextFactory
5: Singleton - Microsoft.Extensions.Options.IOptions`1 => Microsoft.Extensions.Options.OptionsManager`1
6: Singleton - Microsoft.Extensions.Options.IOptionsMonitor`1 => Microsoft.Extensions.Options.OptionsMonitor`1
7: Scoped - Microsoft.Extensions.Options.IOptionsSnapshot`1 => Microsoft.Extensions.Options.OptionsSnapshot`1
8: Transient - Microsoft.AspNetCore.Hosting.IStartupFilter => Microsoft.AspNetCore.Hosting.Internal.AutoRequestServicesStartupFilter
9: Transient - Microsoft.Extensions.DependencyInjection.IServiceProviderFactory`1[[Microsoft.Extensions.DependencyInjection.IServiceCollection, Microsoft.Extensions.DependencyInjection.Abstractions, Version=1.1.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60]] => Microsoft.Extensions.DependencyInjection.DefaultServiceProviderFactory
10: Singleton - Microsoft.Extensions.ObjectPool.ObjectPoolProvider => Microsoft.Extensions.ObjectPool.DefaultObjectPoolProvider
11: Transient - Microsoft.Extensions.Options.IConfigureOptions`1[[Microsoft.AspNetCore.Server.Kestrel.KestrelServerOptions, Microsoft.AspNetCore.Server.Kestrel, Version=1.1.1.0, Culture=neutral, PublicKeyToken=adb9793829ddae60]] => Microsoft.AspNetCore.Server.Kestrel.Internal.KestrelServerOptionsSetup
12: Singleton - Microsoft.AspNetCore.Hosting.Server.IServer => Microsoft.AspNetCore.Server.Kestrel.KestrelServer
13: Singleton - Microsoft.AspNetCore.Hosting.IStartup => Microsoft.AspNetCore.Hosting.ConventionBasedStartup
14: Singleton - Microsoft.AspNetCore.Http.IHttpContextAccessor => Microsoft.AspNetCore.Http.HttpContextAccessor
15: Singleton - Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer => Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers.AzureWebAppRoleEnvironmentTelemetryInitializer
16: Singleton - Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer => Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers.DomainNameRoleInstanceTelemetryInitializer
17: Singleton - Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer => Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers.ComponentVersionTelemetryInitializer
18: Singleton - Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer => Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers.ClientIpHeaderTelemetryInitializer
19: Singleton - Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer => Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers.OperationIdTelemetryInitializer
20: Singleton - Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer => Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers.OperationNameTelemetryInitializer
21: Singleton - Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer => Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers.SyntheticTelemetryInitializer
22: Singleton - Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer => Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers.WebSessionTelemetryInitializer
23: Singleton - Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer => Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers.WebUserTelemetryInitializer
24: Singleton - Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer => Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers.AspNetCoreEnvironmentTelemetryInitializer
25: Singleton - Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration => Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration
26: Singleton - Microsoft.ApplicationInsights.TelemetryClient => Microsoft.ApplicationInsights.TelemetryClient
27: Singleton - Microsoft.ApplicationInsights.AspNetCore.ApplicationInsightsInitializer => Microsoft.ApplicationInsights.AspNetCore.ApplicationInsightsInitializer
28: Singleton - Microsoft.ApplicationInsights.AspNetCore.DiagnosticListeners.IApplicationInsightDiagnosticListener => Microsoft.ApplicationInsights.AspNetCore.DiagnosticListeners.HostingDiagnosticListener
29: Singleton - Microsoft.ApplicationInsights.AspNetCore.DiagnosticListeners.IApplicationInsightDiagnosticListener => Microsoft.ApplicationInsights.AspNetCore.DiagnosticListeners.MvcDiagnosticsListener
30: Singleton - Microsoft.AspNetCore.Hosting.IStartupFilter => Microsoft.ApplicationInsights.AspNetCore.ApplicationInsightsStartupFilter
31: Singleton - Microsoft.ApplicationInsights.AspNetCore.JavaScriptSnippet => Microsoft.ApplicationInsights.AspNetCore.JavaScriptSnippet
32: Singleton - Microsoft.ApplicationInsights.AspNetCore.Logging.DebugLoggerControl => Microsoft.ApplicationInsights.AspNetCore.Logging.DebugLoggerControl
33: Singleton - Microsoft.Extensions.Options.IOptions`1[[Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration, Microsoft.ApplicationInsights, Version=2.2.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]] => Microsoft.Extensions.DependencyInjection.TelemetryConfigurationOptions
34: Singleton - Microsoft.Extensions.Options.IConfigureOptions`1[[Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration, Microsoft.ApplicationInsights, Version=2.2.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]] => Microsoft.Extensions.DependencyInjection.TelemetryConfigurationOptionsSetup
35: Singleton - Microsoft.Extensions.Options.IConfigureOptions`1[[Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions, Microsoft.ApplicationInsights.AspNetCore, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]] => Microsoft.AspNetCore.Hosting.DefaultApplicationInsightsServiceConfigureOptions
36: Singleton - Microsoft.Extensions.Logging.ILoggerFactory => Microsoft.Extensions.Logging.LoggerFactory
37: Singleton - System.Diagnostics.DiagnosticListener => System.Diagnostics.DiagnosticListener
38: Singleton - System.Diagnostics.DiagnosticSource => System.Diagnostics.DiagnosticListener
39: Singleton - Microsoft.AspNetCore.Hosting.IApplicationLifetime => Microsoft.AspNetCore.Hosting.Internal.ApplicationLifetime
~~~

140 more services gets added by the `AddMvc()` method. And even more, if you want to use more components and frameworks, like Identity and Entity Framework Core.

Because of that, you should use the common way to add framework services to the `IServiceCollection` and read the added services to the other container afterwards.

The next lines with dummy code, shows you how the implementation could be look like:

~~~ csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
{
  // Add framework services.  
  services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

  services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

  services.AddMvc();
  services.AddOtherStuff();

  // create custom container
  var container = new CustomContainer();
  
  // read service collection to the custom container
  container.RegisterFromServiceCollection(services);

  // use and configure the custom container
  container.RegisterSingelton<IProvider, MyProvider>();

  // creating the IServiceProvider out of the custom container
  return container.BuildServiceProvider();
}
~~~

The details of the implementation depends on how the container works. E. g. If I'm right, Laurent Bugnion's SimpleIOC already is a `IServiceProvider` and could be returned directly. Let's see how this works with Autofac:

## Replacing with Autofac

Autofac provides an extension library to support this container in ASP.NET Core projects. I added both the container and the extension library packages from NuGet:

```text
Autofac, 4.5.0
Autofac.Extensions.DependencyInjection, 4.1.0
```
I also added the related `usings` to the `Startup.cs`:

~~~ csharp
using Autofac;
using Autofac.Extensions.DependencyInjection;
~~~

Now I'm able to create the Autofac container in the `ConfigureServices` method:

~~~ csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
{
  // Add framework services.  
  services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

  services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

  services.AddMvc();
  services.AddOtherStuff();
  
  // create a Autofac container builder
  var builder = new ContainerBuilder();

  // read service collection to Autofac
  builder.Populate(services);

  // use and configure Autofac
  builder.RegisterType<MyProvider>().As<IProvider>();

  // build the Autofac container
  ApplicationContainer = builder.Build();
  
  // creating the IServiceProvider out of the Autofac container
  return new AutofacServiceProvider(ApplicationContainer);
}

// IContainer instance in the Startup class 
public IContainer ApplicationContainer { get; private set; }
~~~

With this implementation, Autofac is used as the dependency injection container in this ASP.NET application. 

If you also want to resolve the controllers from the container, you should add this to the container too Otherwise the framework will resolve the Controllers and some special DI cases are not possible. A small call adds the Controllers to the `IServiceColection`:

~~~ csharp
services.AddMvc().AddControllersAsServices();
~~~

That's it. 

More about Autofac: [http://docs.autofac.org/en/latest/integration/aspnetcore.html](http://docs.autofac.org/en/latest/integration/aspnetcore.html)

## Conclusion

Fortunately Autofac supports the .NET Standard 1.6 and there is this nice extension library to get it working in ASP.NET too. Some other containers don't and it needs some more effort to get it running.