---
layout: post
title: "ASP.NET Core on .NET 7.0 - Output caching"
teaser: "Finally, Microsoft added output caching to the ASP.NET Core 7.0 preview 6. Output caching is a middleware that caches the entire output of an endpoint instead of executing the endpoint every time it gets requested."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Tests
- Tag
---

Finally, Microsoft added output caching to the ASP.NET Core 7.0 preview 6. 

Output caching is a middleware that caches the entire output of an endpoint instead of executing the endpoint every time it gets requested. This will make your endpoints a lot faster.

This kind of caching is useful for APIs that provide data that don't change a lot or that gets accessed pretty frequently. It is also useful for more or less static pages, e.g. CMS output, etc. Different caching options will help you to fine-tune your output cache or to vary the cache based on header or query parameter.

For more dynamic pages or APIs that serve data that change a lot, it would make sense to cache more specifically on the data level instead of the entire output.

## Trying output caching

To try output caching I created a new empty web app using the .NET CLI:

```shell
dotnet new web -n OutputCaching -o OutputCaching
cd OutputCaching
code .
```

This will create the new project and opens it in VSCode.

In the `Program.cs` you now need to add output caching to the `ServiceCollection` as well as using the middleware on the `app`:

~~~ csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOutputCache();

var app = builder.Build();

app.UseOutputCache();

app.MapGet("/", () => "Hello World!");

app.Run();
~~~

This enables output caching in your application.

Let's use output caching with the classic example that displays the current date and time.

~~~ csharp
app.MapGet("/time", () => DateTime.Now.ToString());
~~~

This creates a new endpoint that displays the current date and time. Every time you refresh the result in the browser, you got a new time displayed. No magic here. Now we are going to add some caching magic to another endpoint:

~~~ csharp
app.MapGet("/time_cached", () => DateTime.Now.ToString())
	.CacheOutput();
~~~

If you access this endpoint and refresh it in the browser, the time will not change. The initial output got cached and you'll receive the cached output every time you refresh the browser.

This is good for more or less static outputs that don't change a lot. What if you have a frequently used API that just needs a short cache to reduce the calculation effort or to just reduce the database access. You can reduce the caching time to, let's say, 10 seconds:

~~~ csharp
 builder.Services.AddOutputCache(options =>
 {
     options.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(10);
 });
 ~~~

This reduces the default cache expiration timespan to 10 seconds.

If you now start refreshing the endpoint we created previously, you'll get a new time every 10 seconds. This means the cache get's released every 10 seconds. Using the options you can also define the size of the cached body or the overall cache size.

If you provide a more dynamic API that receives parameters using query strings. You can vary the cache by the query string:

~~~ csharp
app.MapGet("/time_refreshable", () => DateTime.Now.ToString())
    .CacheOutput(p => p.VaryByQuery("time"));
~~~

This adds another endpoint that varies the cache by the query string argument called "time". This means the query string `?time=now`, caches a different result than the query string `?time=later` or `?time=before`. 

The `VaryByQuery` function allows you to add more than one query string:

~~~ csharp
app.MapGet("/time_refreshable", () => DateTime.Now.ToString())
    .CacheOutput(p => p.VaryByQuery("time", "culture", "format"));
~~~

In case you like to vary the cache by HTTP headers you can do this the same way using the `VaryByHeader` function:

~~~ csharp
app.MapGet("/time_cached", () => DateTime.Now.ToString())
    .CacheOutput(p => p.VaryByHeader("content-type"));
~~~

## Further reading

If you like to explore more complex examples of output caching, it would make sense to have a look into the samples project:

https://github.com/dotnet/aspnetcore/blob/main/src/Middleware/OutputCaching/samples/OutputCachingSample/Startup.cs



