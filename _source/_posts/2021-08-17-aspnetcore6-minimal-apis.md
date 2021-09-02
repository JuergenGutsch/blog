---
layout: post
title: "ASP.NET Core in .NET 6 - Introducing minimal APIs"
teaser: "This is the next part of the ASP.NET Core on .NET 6 series. In this post, I'd like to have a look into minimal APIs."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- .NET 6
- Web API
- minimal API

---

This is the next part of the [ASP.NET Core on .NET 6 series]({% post_url aspnetcore6-01.md %}). In this post, I'd like to have a look into minimal APIs.

With the preview 4, Microsoft simplified the simplest project template to an absolute minimum. Microsoft created this template to make it easier for new developers to start creating small microservices and HTTP APIs. 

When I saw the minimal APIs the first time some months ago it reminds me on this:

~~~javascript
var express = require("express");
var app = express();

app.listen(3000, () => {
 console.log("Server running on port 3000");
});

app.get("/url", (req, res, next) => {
 res.json(["Tony","Lisa","Michael","Ginger","Food"]);
});
~~~

Yes, that is NodeJS using ExpressJS to bootup an http server that provides a minimal API. Actually, the ASP.NET Core minimal APIs looks as easy as NodeJS and ExpressJS. You don't believe? Just have a look.

## Minimal APIs

To create a minimal API project, you can simply write it on your own or just use the dotnet CLI as usual:

~~~shell
dotnet new web -n MiniApi -o MiniApi
~~~

This command creates a project file, app settings files, and a `Program.cs` that looks like this:

~~~csharp
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapGet("/", () => "Hello World!");

app.Run();
~~~

> Microsoft changed the empty web project in the dotnet CLI to use minimal APIs. You will not find a `Startup.cs` anymore. It is all in the `Program.cs`. This should help new developers to get into ASP.NET Core easier.

If you already know ASP.NET Core, you will know some of the things that are used here. The `WebApplicationBuilder` will be created with the default settings to create the hosting environment, the same way as the `WebHostBuilder`. After Build() was called you can use a `WebApplication` object to map endpoints and to add Middlewares like the `DeveloperExceptionPage`. 

`app.Run()` starts the application to serve the endpoints.

You can start the project like any other ASP.NET Core project by running `dotnet run` or by clicking F5 in your IDE

Actually, it is all working as any other ASP.NET Core project, but most of the stuff is encapsulated and preconfigured in the `WebApplicationBuilder` and can be accessed via properties. If you like to register some additional services, you need to access the Services property of the `WebApplicationBuilder`:

~~~csharp
builder.Services.AddScoped<IMyService, MyService>();
builder.Services.AddTransient<IMyService, MyService>();
builder.Services.AddSingleton<IMyService, MyService>();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
~~~

Here you can also add the known services like authentication, authorization, and even MVC Controllers with views

To configure the `Configuration`, `Logging`, `Host`, etc. you also need to access the relevant properties. 

On the `WebApplication` instance, it works the same way as configuring your application inside the `Configure` method of a `Startup` class. On the `app` variable, you can register all the middlewares and routes you like. In the sample above, it is a simple GET response on the default route. You could also register MVC, authentication, authorization, HSTS, etc. as you can do in a common ASP.NET Core project.

The only difference is that it is all in one file.

Even if it doesn't make sense to configure an MVC application using minimal APIs, but to demonstrate that minimal APIs are just regular ASP.NET Core under the hood:

~~~csharp
using System;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
~~~

I really like this approach for simple APIs in a microservice context. 

What do you think? Just drop me a comment. 

## What's next?

In the next part In going to look into the support for [Async streaming]({% post_url aspnetcore6-async-stream.md %}) in ASP.NET Core.

