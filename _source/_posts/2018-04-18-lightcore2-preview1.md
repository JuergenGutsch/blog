---
layout: post
title: "title"
teaser: "description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- LightCore
- .NET Standard
- Dependency Injection
---

Finally it's done! Peter Bucher and I, we got LightCore 2.0 working on ASP.NET Core 2.x

## A little bit of history

> LightCore is a light-weight dependency injection container inspired by Autofaq and started as a learning project back in 2008. Performance is one of the main features in LightCore, as well as the easy to use API. 

During the MVP Global Summit 2015, I decided to create a .NET Core version of LightCore. A little later the .NET Standard was the big topic and the idea was to create a .NET Standard version of LightCore that runs on almost all platforms. Unfortunately the API set of .NET Standard smaller 2.0 wasn't enough to get it done. So we need to wait for .NET Standard 2.0 which came out last summer. I started again to move LightCore to the .NET Standard. This time it works out pretty well. 

Only the ASP.NET Core integration took a bit time. Especially an issue in LightCore, which returns null, if a generic argument of a list couldn't be returned. LightCore should return an empty list instead and it does now.

## Use LightCore 2.0

To use LightCore in ASP.NET Core you need to change the `Program.cs` a little bit and after that the `Startup.cs` should be changed to.

In the `Program.cs` the Method `BuildWebHost` adds LightCore:

~~~ csharp
public static IWebHost BuildWebHost(string[] args) => WebHost.CreateDefaultBuilder(args)
    .ConfigureServices(services => services.AddLightCore()) // <== Add LightCore
    .UseStartup<Startup>()
    .Build();
~~~

This additional line adds the `LightCoreServiceProviderFactory` to ASP.NET Core. This enables you to add another Method to the `Startup.cs`:

~~~ csharp
// This method gets called by the runtime. Use this method to add services to the service collection.
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();

    //services.AddTransient<IServiceOne, MyServiceOne>();
}

// This method gets called by LightCore. Use this method to add services to LightCore
public void ConfigureContainer(ContainerBuilder builder)
{
    //builder.Register<IServiceTwo, MyServiceTwo>();
}
~~~

The method `ConfigureContainer` is a new one and gets called by LightCore. This method may be used to configure the LightCore container. 

* Use the `ConfigureServices` to add Services in the common ASP.NET Core way, e.g. to add MVC, to add configuration, to add Identity, to add Entity Framework contexts and so on.
* Use the `ConfigureContainer` to configure LightCore, e. g. to load services dynamically by configuration files, etc.

All the registrations done on the IServiceCollection gets moved into ContianerBuilder and registered in LightCore. At the end LightCore is used to create and manage the instances in this ASP.NET Core application.

## NuGet

The LightCore2 preview1 build goes to MyGet. To try LightCore 2.0 you need to add the MyGet feed to the NuGet sources. 

https://myget.com/juergengutsch/LightCore/

Just add the package `LightCore.Integration.AspNetCore`, the `LightCore` package will be added for your project automatically.

## Package Summary

We now have this packages available:

* LightCore
  * .NET Standard 2.0
  * Contains the dependency injection container and the core of LightCore
* LightCore.Configuration
  * .NET Standard 2.0
  * Enables the configuration via configuration files
  * XAML configuration format was removed, because it doesn't work on .NET Core
  * JSON configuration format under .NET Core and the full .NET Framework
* LightCore.Integration.AspNet
  * .NET Framework 4.6
  * Support for ASP.NET
* LightCore.Integration.AspNet.Mvc
  * .NET Framework 4.6
  * Support for ASP.NET MVC
* LightCore.Integration.AspNet.WebApi
  * .NET Framework 4.6
  * Support for ASP.NET Web API
* LightCore.Integration.AspNetCore
  * .NET Standard 2.0
  * Support for ASP.NET Core
* LightCore.CommonServiceLocator
  * .NET Framework 4.6
  * Support for the CommonServiceLocator

We retired the Silverlight integration and I'm not sure whether prism is still a thing. If yes I will add my prism integration to the LightCore repository too. Currently it is a personal project and not supported by LightCore. Prism used the Unity DI container by default and I created an implementation for LightCore.

## Current State

To get a release done we have some small open points to finalize.

* We need to update and to cleanup the sample applications.
* We need to finalize the new documentation. The old one is archived.

We are going to do this in the next few weeks.
