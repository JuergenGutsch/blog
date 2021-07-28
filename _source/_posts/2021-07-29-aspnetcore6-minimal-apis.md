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

When I saw the minimal APIs the first time it reminds me on this:

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

It looks as easy as NodeJS and ExpressJS. You don't believe? Just have a look.

## Minimal APIs

To create a minimal API project, you can simply write it by your own or just use the dotnet CLI as usual:

~~~shell
dotnet new web -n MiniApi -o MiniApi
~~~

This command creates a project file, app settings files and a Program.cs that looks like this:

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

If you already know ASP.NET Core, you will know some of the things that are used here. The WebApplicationBuilder will be created with the default settings to create the hosting environment, the same way as the WebHostBuilder. After Build() was called you can use a WebApplication object to map endpoints and to add Middlewares like the DeveloperExceptionPage. 

app.Run() starts the application to serve the endpoints.











## What's next?

In the next part In going to look into the support for `Async streaming` in ASP.NET Core.

