---
layout: post
title: "Customizing ASP.NET Core Part 03: Dependency Injection"
teaser: "In the third part we'll take a look into the ASP.NET Core dependency injection and how to customize it to use a different dependency injection container if needed."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- Unit Test
- XUnit
- MSTest
---

In the third part we'll take a look into the ASP.NET Core dependency injection and how to customize it to use a different dependency injection container if needed.

## The series topics

- [Customizing ASP.NET Core Part 01: Logging]({% post_url customizing-aspnetcore-01-logging.md %})
- [Customizing ASP.NET Core Part 02: Configuration]({% post_url customizing-aspnetcore-02-configuration.md %})
- Customizing ASP.NET Core Part 03: Dependency Injection
- Customizing ASP.NET Core Part 04: HTTPS
- Customizing ASP.NET Core Part 05: HostedServices
- Customizing ASP.NET Core Part 06: MiddleWares
- Customizing ASP.NET Core Part 07: OutputFormatter
- Customizing ASP.NET Core Part 08: ModelBinder
- Customizing ASP.NET Core Part 09: ActionFilter
- Customizing ASP.NET Core Part 10: TagHelpers

## Why using a different dependency injection container?

In the most projects you don't really need to use a different dependency injection Container. The DI implementation in ASP.NET Core supports the main basic features and works well and pretty fast. Anyway, some other DI container support some interesting features you maybe want to use in your application.

* Maybe you like to create an application that support modules as lightweight dependencies.
  * E.g. modules you want to put into a specific directory and they get automatically registered in your application
  * This could be done with NInject.
* Maybe you want to configure the services in a configuration file outside the application, in an XML or JSON file instead in C# only
  * This is a common feature in various DI containers, but not yet supported in ASP.NET Core.
* Maybe you don't want to have an immutable DI container, because you want to add services at runtime.
  * This is also a common feature in some DI containers.

## Preparing the Startup.cs

Create a new ASP.NET Core project and open the Startup.cs, you will find the method to configure the services which looks like this:

~~~ csharp
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{
	services.Configure<CookiePolicyOptions>(options =>
	{
		// This lambda determines whether user consent for non-essential cookies is needed for a given request.
		options.CheckConsentNeeded = context => true;
		options.MinimumSameSitePolicy = SameSiteMode.None;
	});
    
    services.AddTransient<IService, MyService>();

	services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
}
~~~

This method gets the IServiceCollection, which already filled with a bunch of services. This services got added by the hosting services and parts of ASP.NET Core that got executed before the method ConfigureSercices is called.

Inside the method some more services gets added. First a configuration class that contains cookie policy options is added to the ServiceCollection. In this sample I also add a custom service called MyService that implements the IService interface. After that the method AddMvc() adds another bunch of services needed by the MVC framework. Until yet we have around 140 services registered to the IServiceCollection. But the service collections isn't the actual dependency injection container. 

The actual DI container is the so called service provider, which will be created out of the service collection. The IServiceCollection has an extension method registered to create a IServiceProvider out of the service collection.

~~~ csharp
IServiceProvider provider = services.BuildServiceProvider()
~~~

The ServiceProvider than is an immutable container that cannot be changed at runtime. With the default method ConfigureServices the IServiceProvider gets created in the background after this method was called, but it is possible to change the method a little bit:

~~~ csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    services.Configure<CookiePolicyOptions>(options =>
    {
        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
        options.CheckConsentNeeded = context => true;
        options.MinimumSameSitePolicy = SameSiteMode.None;
    });
    
    services.AddTransient<IService, MyService>(); // custom service
    
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
    
    return services.BuildServiceProvider()
}
~~~

I changed the return type to IServiceProvider and return the ServiceProvider created with the method BuildServiceProvider(). This change is still working in ASP.NET Core. 

To change to a different or custom DI container you need to replace the default implementation of the IServiceProvider with a different one. Additionally you need to find a way to move the already registered services to the new container.

The next sample uses Autofac as a third party container.

~~~ csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    services.Configure<CookiePolicyOptions>(options =>
    {
        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
        options.CheckConsentNeeded = context => true;
        options.MinimumSameSitePolicy = SameSiteMode.None;
    });

    //services.AddTransient<IService, MyService>();

    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

    // create a Autofac container builder
    var builder = new ContainerBuilder();

    // read service collection to Autofac
    builder.Populate(services);

    // use and configure Autofac
    builder.RegisterType<MyService>().As<IService>();

    // build the Autofac container
    ApplicationContainer = builder.Build();

    // creating the IServiceProvider out of the Autofac container
    return new AutofacServiceProvider(ApplicationContainer);
}

// IContainer instance in the Startup class 
public IContainer ApplicationContainer { get; private set; }
~~~

Autofac as well works with a kind of a service collection inside the ContainerBuilder and it creates the actual container out of the ContainerBuilder. To get the registered services out of the IServiceCollection into the ContainerBuilder, Autofac uses the Populate() method. This copies all the existing services to the Autofac container.

Our custom service MyService now gets registered using the Autofac way.

After that the container gets build and stored in a property of type IContainer. In the last line of the method we create a AutofacServiceProvider and pass in the IContainer. This is IServiceProvider we need to return to use Autofac within our application.

## Conclusion



