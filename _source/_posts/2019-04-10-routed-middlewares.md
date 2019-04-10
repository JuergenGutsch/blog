---
layout: post
title: "Routed Middlewares in ASP.​NET Core 3.0"
teaser: "If you have a Middleware that needs to work on a specific path, you should implement it by mapping it to a route in ASP.NET Core 3.0, instead of just checking the path names. This post shows how it should be done in ASP.NET Core 3.0"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Tests
- Tag
---

If you have a Middleware that needs to work on a specific path, you should implement it by mapping it to a route in ASP.NET Core 3.0, instead of just checking the path names. This post doesn't handle regular Middlewares, which need to work all request, or all requests inside a `Map` or `MapWhen` branch.

At the Global MVP Summit 2019 in Redmond  I attended the hackathon where I worked on [my GraphQL Middlewares for ASP.NET Core](https://github.com/JuergenGutsch/graphql-aspnetcore/). I asked [Glen Condron](https://twitter.com/condrong) for a review of the API and the way the Middleware gets configured. He told me that we did it all right. We followed the proposed way to provide and configure an ASP.NET Core Middleware. But he also told me that there is a new way in ASP.NET Core 3.0 to use this kind of Middlewares. 

Glen asked [James Newton King](https://twitter.com/JamesNK) who works on the new routing to show me how this needs to be done in ASP.NET Core 3.0. James pointed me to the ASP.NET Core Health Checks and explained me the new way to go.

> BTW: That's kinda closing the loop: Four summits ago [Damien Bowden](http://twitter.com/damienbod) and I where working on the initial drafts of the ASP.NET Core Health Checks together with Glen Condron. Awesome that this is now in production ;-)

The new ASP.NET Core 3.0 implementation of the GraphQL Middlewares is in the *aspnetcore30* branch of the repository: [https://github.com/JuergenGutsch/graphql-aspnetcore](https://github.com/JuergenGutsch/graphql-aspnetcore)

## How it worked before:

Until now you used `MapWhen()` to map the Middleware to a specific condition defined in a predicate:

~~~ csharp
Func<HttpContext, bool> predicate = context =>
{
    return context.Request.Path.StartsWithSegments(path, out var remaining) &&
                            string.IsNullOrEmpty(remaining);
};

return builder.MapWhen(predicate, b => b.UseMiddleware<GraphQlMiddleware>(schemaProvider, options));
~~~

([ApplicationBuilderExtensions.cs](https://github.com/JuergenGutsch/graphql-aspnetcore/blob/feature/aspnetcore30/GraphQl.AspNetCore/ApplicationBuilderExtensions.cs))

In this case the path is checked. This is pretty common to not only map based on paths. This allows you to also map on all other kind of criteria based on the `HttpContext`.

Also the much simpler `Map()` was a way to go:

~~~ csharp
builder.Map(path, branch => branch.UseMiddleware<GraphQlMiddleware>(schemaProvider, options));
~~~

## How this should be done now

In ASP.NET Core 3.0 these kind of mappings, where you may listen on a specific endpoint, should be done using the `EndpoiontRouteBuilder`. If you create a new ASP.NET Core 3.0 web application. MVC is now added a little different in the `Startup.cs` than before:

~~~ csharp
app.UseRouting(routes =>
{
    routes.MapControllerRoute(
        name: "default",
        template: "{controller=Home}/{action=Index}/{id?}");
    routes.MapRazorPages();
});
~~~

The method `MapControllerRoute()` adds the controller based MVC and Web API. The new ASP.NET Core Health Checks, which also provide an own endpoint will also be added like this. Means we now have `Map()` methods as extension methods on the `IEndpointRouteBuilder` instead of `Use()` methods on the `IApplicationBuilder`. It is still possible to use the `Use` methods.

In case of the GraphQL Middleware it looks like this:

~~~ csharp
var pipeline = routes.CreateApplicationBuilder()
    .UseMiddleware<GraphQlMiddleware>(schemaProvider, options)
    .Build();

return routes.Map(pattern, pipeline)
    .WithDisplayName(_defaultDisplayName);
~~~

([EndpointRouteBuilderExtensions.cs](https://github.com/JuergenGutsch/graphql-aspnetcore/blob/feature/aspnetcore30/GraphQl.AspNetCore/EndpointRouteBuilderExtensions.cs))

Based on the current `IEndpointRouteBuilder` a new `IApplicationBuilder` is created, where we `Use` the GraphQL Middleware as before. We pass the `ISchemaProvider` and the `GraphQlMiddlewareOptions` as arguments to the Middleware. The result is a `RequestDelegate` in the `pipeline` variable.

The configured endpoint `pattern` and the `pipeline` than gets mapped to the `IEndpointRouteBuilder`. The small extension Method `WithDisplayName()` sets the configured display name to the endpoint. 

> I needed to copy this extension method to from the ASP.NET Core repository to my code base, because the current development build of ASP.NET Core didn't contain this method two weeks ago. I need to check the latest version ASAP.

In ASP.NET Core 3.0 the GraphQl and the GraphiQl Middleware can now added like this:

~~~ csharp
app.UseRouting(routes =>
{
    if (env.IsDevelopment())
    {
        routes.MapGraphiQl("/graphiql");
    }
    
    routes.MapGraphQl("/graphql");
    
    routes.MapControllerRoute(
        name: "default",
        template: "{controller=Home}/{action=Index}/{id?}");
    routes.MapRazorPages();
});
~~~

## Conclusion

The new ASP.NET Core 3.0 implementation of the GraphQL Middlewares is on the aspnetcore30 branch of the repository: [https://github.com/JuergenGutsch/graphql-aspnetcore](https://github.com/JuergenGutsch/graphql-aspnetcore)

This approach feels a bit different. In my opinion it messes the `startup.cs` a little bit. Previously we added one middleware after another... line by line to the `IApplicationBuilder` method. With this approach we have some Middlewares still registered on the `IApplicationBuilder` and some others on the `IEndpointRouteBuilder` inside a lambda expression on a new `IApplicationBuilder`. 

The other thing is, that the order isn't really clear anymore. When will the Middlewares inside the `UseRouting()` be executed and in which direction? I will dig deeper into this the next months.