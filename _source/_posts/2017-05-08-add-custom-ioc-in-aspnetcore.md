---
layout: post
title: "Adding a custom dependency injection container in ASP.NET Core"
teaser: "Description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- Unit Test
- XUnit
- MSTest
---

ASP.NET Core is pretty flexible, customizable and extendible. You are able to change almost everything. Even the built-in dependency injection container can be replaced.

## Why should i do this?

There are not many reasons to replace the built-in dependency injection container, because it works pretty well for the most cases. 

If you prefer a different dependency injection container, because of some reasons, you are able to do it. Maybe you know a faster container, of you like the nice features of Ninject to load dependencies dynamically from an assembly in a specific folder, by file patterns, and so on. I really miss this features in the built in container. It is possible to use [another solution to to load dependencies from other libraries]({% post_url using-dependency-injection-in-multiple-projects.md %}), but this is not as dynamic as the Ninject way.

## Setup the Startup.cs

In ASP.NET Core the IServiceProvider is the component who resolves and creates the dependencies out of a IServiceCollection. The IServiceCollection needs to be manipulated in the method ConfigureServices in the Startup.cs if you want to add dependencies to the IServiceProvider.

The solution is to read the contents of the IServiceCollections to the own container and to provide an own implementation of a IServiceProvider to the application.

This is possible by changing the method ConfigureServices a little bit:

~~~ csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
{
  // Add framework services.
  services.AddMvc();

  return services.BuildServiceProvider();
}
~~~

The method now returns a IServiceCollection, which is created in the last line out of the IServiceCollection. It is needed to add the contents of the service collection to the container you want to use, because ASP.NET actually adds a lot of dependencies before this method is called. Even if you want to use components and frameworks which are usually adding dependencies to the IServiceCollection (like AddMvc() method). In that cases you should use the common way and add the IServiceCollection to the other container afterwards.

The next lines with dummy code, shows you how the implementation could be look like:

~~~ csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
{
  // Add framework services.
  services.AddMvc();

  var container = new CustomContainer();
  container.RegisterFromServiceCollection(services);

  // use and configure the custom container
  container.RegisterSingelton<IProvider, MyProvider>();

  return container.BuildServiceProvider();
}
~~~

The details of the implementation depends on how the container works.

