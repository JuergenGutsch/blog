---
layout: post
title: "ASP.​NET Core in .NET 6 - Support for IAsyncDisposable in MVC"
teaser: "This is the third part of the ASP.NET Core on .NET 6 series. In this post, I want to have a look at the IAsyncDisposable interface."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- .NET 6
---

This is the third part of the [ASP.NET Core on .NET 6 series]({% post_url aspnetcore6-01.md %}). In this post, I want to have a look at the Support for `IAsyncDisposable` in MVC.

The `IAsyncDisposable` is a thing since .NET Core 3.0. If I'm right, we got that together with the async streams to release those kind of streams asynchronously. Now MVC is supporting this interface as well and you can use it anywhere in your code on controllers, classes, etc. to release async resources.

## When should I use IAsyncDisposable?

When you work with asynchronous enumerators like in async steams and when you work with instances of unmanaged resources which needs resource-intensive I/O operation to release.

When implementing this interface you can use the DisposeAsync method to release those kind of resources. 

## Let's try it

Let's assume we have a controller that creates and uses a `Utf8JsonWriter` which as well is a `IAsyncDisposable` resource

~~~csharp
public class HomeController : Controller, IAsyncDisposable
{
    private Utf8JsonWriter _jsonWriter;

    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
        _jsonWriter = new Utf8JsonWriter(new MemoryStream());
    }
~~~

The interface needs us to implement the `DisposeAsync` method. This should be done like this:

~~~csharp
public async ValueTask DisposeAsync()
{
    // Perform async cleanup.
    await DisposeAsyncCore();
    // Dispose of unmanaged resources.
    Dispose(false);
    // Suppress GC to call the finalizer.
    GC.SuppressFinalize(this);
}
~~~

This is a higher level method that calls a DisposeAsyncCore that actually does the async cleanup. It also calls the regular Dispose method to release other unmanaged resources and it tells the garbage collector not to call the finalizer. I guess this could release the instance before the async cleanup finishes.

This needs us to add another method called DisposeAsyncCore():

~~~csharp
protected async virtual ValueTask DisposeAsyncCore()
{
    if (_jsonWriter is not null)
    {
        await _jsonWriter.DisposeAsync();
    }

    _jsonWriter = null;
}
~~~

This will actually dispose the async resource .

## Further reading

Microsoft has some really detailed docs about it:

* [IAsyncDisposable Interface](https://docs.microsoft.com/en-us/dotnet/api/system.iasyncdisposable?view=net-5.0)
* [Implement a DisposeAsync method](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync)

## What's next?

In the next part In going to look into the support for [DynamicComponent]({% post_url aspnetcore6-04-dynamiccomponent.md %}) in Blazor.