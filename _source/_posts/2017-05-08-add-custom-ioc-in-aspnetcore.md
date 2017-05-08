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

The method now returns a `IServiceprovider `, which is created in the last line out of the `IServiceCollection` . It is needed to add the contents of the service collection to the container you want to use, because ASP.NET actually adds a lot of dependencies before this method is called. Even if you want to use components and frameworks which are usually adding dependencies to the `IServiceCollection` (like `AddMvc()` method). 

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

The details of the implementation depends on how the container works. E. g. If I'm right, Laurent Bugnion's SimpleIOC already is a IServiceProvider and could be returned directly. Let's see how this works with Autofac:

## Replacing with Autofac





## Conclusion

