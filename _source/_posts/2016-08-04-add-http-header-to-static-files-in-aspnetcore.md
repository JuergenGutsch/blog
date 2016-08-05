--- 
layout: post
title: "Add HTTP headers to static files in ASP.​NET Core"
teaser: "Usually, static files like JavaScript, CSS, images and so on, are cached on the client after the first request. But sometimes, you need to disable the cache or to add a special cache handling."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.​NET Core
- HTTP Headers
- Cache
---

Usually, static files like JavaScript, CSS, images and so on, are cached on the client after the first request. But sometimes, you need to disable the cache or to add a special cache handling.

To provide static files in a ASP.NET Core application, you use the `StaticFileMiddleware`:

~~~ csharp
app.UseStaticFiles();
~~~

This extension method has two overloads. One of them needs a `StaticFileOptions` instance, which is our friend in this case. This options has a property called `OnPrepareResponse` of type `Action<StaticFileResponseContext>`. Inside this Action, you have access to the `HttpContext` and many more. Let's see how it looks like to set the cache life time to 12 hours:

~~~ csharp
app.UseStaticFiles(new StaticFileOptions()
{
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers["Cache-Control"] = 
                "private, max-age=43200";

        context.Context.Response.Headers["Expires"] = 
                DateTime.UtcNow.AddHours(12).ToString("R");
    }
});
~~~

With the `StaticFileResponseContext`, you also have access to the file of the currently handled file. With this info, it is possible to manipulate the HTTP headers just for a specific file or file type.

This approach ensures, that the client doesn't use pretty much outdated files, but use cached versions while working with it. We use this in a ASP.NET Core single page application, which uses many JavaScript, and HTML template files. In combination with continuous deployment, we need to ensure the Application uses the latest files.