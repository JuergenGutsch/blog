---
layout: post
title: "Using dependency injection in multiple projects"
teaser: "One of my last post was about Dependency Injection (DI) in .NET Core Console Applications. After that I got a question about how to use the IServiceCollection in multiple projects. In this post I'm going to try to explain, how to use the IServiceCollection in a Solution with more projects."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- Dependency Injection
---

One of my last post was about [Dependency Injection (DI) in .NET Core Console Applications]({% post_url using-dependency-injection-in-dotnet-core-console-apps.md %}). Some days after that post was published, I got a question about how to use the IServiceCollection in multiple projects. In this post I'm going to try to explain, how to use the IServiceCollection in a Solution with more projects.

## Setup

To demonstrate that, I created a Solutions with two .NET Core Console apps and two .NET Standard libraries. One of the console apps uses two of the libraries and the other one is just using on. Each library provides some services which need to be registered to the DI container. Also the console apps provide some services to add.

We now have four projects like this:

* DiDemo.SmallClient
  * .NET Core Console app
  * includes a WriteSimpleDataService
  * references DiDemo.CsvFileConnector
* DiDemo.BigClient
  * .NET Core Console app
  * includes a WriteExtendedDataService
  * includes a NormalizedDataService
  * references DiDemo.SqlDatabaseConnector
  * references DiDemo.CsvFileConnector
* DiDemo.SqlDatabaseConnector
  * .NET Standard library
  * includes a SqlDataService
  * includes a SqlDataProvider used by the service
* DiDemo.CsvFileConnector
  * .NET Standard library
  * includes a CsvDataService

> BTW: Since one of the latest updates the "Class Libraries (.NET Standard)" project diapered from the ".NET Core" node in the "Add New Project" dialogue and the "Class Library (.NET Core)" is back again. The "Class Libraries (.NET Standard)" is now in the ".NET Standard" node under the "Visual C#" node.
>
> In the most cases it doesn't really makes sense to create a .NET Core class library. The difference here is, that the Class Library (.NET Core) has some .NET Core related references. They targeting the netcoreapp1.x instead of the netstandard1.x. This means they have a lot of references, which are not needed in a class library in the most cases, e. g. the Libuv and the .NET Core runtime.

The WriteExtendedDataService uses a INormalizedDataService to get the data and writes it to the console. The NormalizedDataService fetches the data from the CsvDataService and from the SqlDataService and normalize it, to make it usable in the WriteExtendedDataService.

The WriteSimpleDataService uses only the ICsvDataService and writes the data out to the console.

## Setup the DI container

Let's setup the DI container for the SmallClient app. Currently it looks like this:

~~~ csharp
var services = new ServiceCollection();
services.AddTransient<IWriteSimpleDataService, WriteSimpleDataService>();
services.AddTransient<ICsvDataService, CsvDataService>();

var provider = services.BuildServiceProvider();

var writer = provider.GetService<IWriteSimpleDataService>();
writer.write();
~~~

That doesn't  really look wrong, but what happens if the app grows and gets a lot more services to add to the DI container? The CsvDataService is not in the app directly, but it is in the separate library. Usually I don't want to map all the services of the external library. I just want to use the library and I don't want to know anything about the internal stuff. This is why we should set-up the mapping for the DI container also in the external library.

## Let's plug things together

The .NET Standard libraries should reference the Microsoft.Extensions.DependencyInjection.Abstractions to get the IServiceCollection interface. Now we can create a public static class called IServiceCollectionExtensions to create an extension method to work in the IServiceCollection:

~~~ csharp
public static class IServiceCollectionExtension
{
  public static IServiceCollection AddCsvFileConnector(this IServiceCollection services)
  {
    services.AddTransient<ICsvDataService, CsvDataService>();
    return services;
  }
}
~~~

Inside this method we do all the mappings from the interfaces to the concreate classes or all the other registrations to the DI container.  Let's do the same to encapsulate all the services inside the SmallClient app and to keep the program.cs as small as possible:

~~~ csharp
public static class IServiceCollectionExtension
{
  public static IServiceCollection AddInternalServices(this IServiceCollection services)
  {
    services.AddTransient<IWriteSimpleDataService, WriteSimpleDataService>();
    return services;
  }
}
~~~

We can now use this methods in the program.cs of the SmallClient app to plug all that stuff together:

~~~ csharp
var services = new ServiceCollection();
services.AddInternalServices();
services.AddCsvFileConnector();

var provider = services.BuildServiceProvider();

var writer = provider.GetService<IWriteSimpleDataService>();
writer.write();
~~~

It looks much cleaner now. Maybe you remember the AddSomething methods? Exacctly, this is the same way, it is done in ASP.NET Core with e. g. the `services.AddMvc()` method.

We now need to do the same thing for the BigClient app and the SqlDatabaseConnector library. At first let's create the mapping for the SqlDatbaseConnector:

~~~ csharp
public static class IServiceCollectionExtension
{
  public static IServiceCollection AddSqlDatabaseConnector(this IServiceCollection services)
  {
    services.AddTransient<ISqlDataService, SqlDataService>();
    services.AddTransient<ISqlDataProvider, SqlDataProvider>();
    return services;
  }
}
~~~

We also need to create a extension method for the internal services:

~~~ csharp
public static class IServiceCollectionExtension
{
  public static IServiceCollection AddInternalServices(this IServiceCollection services)
  {
    services.AddTransient<IWriteExtendedDataService, WriteExtendedDataService>();
    services.AddTransient<INormalizedDataService, NormalizedDataService>();
    return services;
  }
}
~~~

Now let's plug that stuff together in the BigClient App:

~~~ csharp
var services = new ServiceCollection();
services.AddInternalServices();
services.AddCsvFileConnector();
services.AddSqlDatabaseConnector();

var provider = services.BuildServiceProvider();

var writer = provider.GetService<IWriteExtendedDataService>();
writer.write();
~~~

As you can see, the BigClient app uses the already existing `services.AddCsvFileConnector()` method.

## Does it really work?

It does. Start the BigClient app in Visual Studio to see that it will work as expected:

![]({{ site.baseurl }}/img/di-core/di-multiproj-output.png)

To see the full sources and to try it out by yourself, please visit the GitHub repository: [https://github.com/JuergenGutsch/di-core-multi-demo](https://github.com/JuergenGutsch/di-core-multi-demo)

What do you think? Do you have questions or some ideas to add? Feel free to drop a comment :)