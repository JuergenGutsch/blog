---
layout: post
title: "ASP.NET Core on .NET 7.0"
teaser: "Description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
---

I really like the transparent development of .NET and ASP.NET Core. It is all openly discussed publicly on GitHub and the first preview version is released just three months after the last version is released.

Same with version 7.0 which will be released beginning of November.

## Roadmap for ASP.NET Core 7.0

Did you know that there is already a roadmap for ASP.NET Core 7.0? It actually is and it is full of improvements:

[ASP.NET Core Roadmap for .NET 7](https://github.com/dotnet/aspnetcore/issues/39504)

Even in version 7.0 Microsoft is planning to improve the runtime performance. Also, the ASP.NET Core web frameworks will be improved. Minimal API, SignalR, and Orleans are the main topics here but also Rate Limiting is a topic. There are also a lot of issues about the web UI technologies Maui, Blazor, MVC and the Razor Compiler are the main topics here. 

The roadmap refers to the specific GitHub issues that contain a lot of exciting discussions. I would propose to have a detailed look at some of those

## ASP.NET Core 7.0 Preview 1

Just yesterday Microsoft released the first preview version of .NET 7.0 and Daniel Roth published a detailed explanation about what was done in ASP.NET Core with this release.

[ASP.NET Core updates in .NET 7 Preview 1](https://devblogs.microsoft.com/dotnet/asp-net-core-updates-in-net-7-preview-1/)  

Again, I will go through the previews and write about interesting upcoming features that will be in the final release like this one:

## IFormFile and IFormFileCollection support in minimal APIs

This is an improvement that is requested since the Minimal API was announced the first time. You can now handle uploaded files in minimal APIs using `IFormFile` and `IFormFileCollection`.

~~~~csharp
app.MapPost("/upload", async(IFormFile file) =>
{
    using var stream = System.IO.File.OpenWrite("upload.txt");
    await file.CopyToAsync(stream); 
});
app.MapPost("/upload-many", async (IFormFileCollection myFiles) => { ... });
~~~~

(This snippet was copied from the blog post mentioned above.)

I'm sure this makes the minimal APIs more useful than before even if there is some limitation that will be addressed in later preview releases of .NET 7.0.

## What's next?

As mentioned, I'll pick interesting features from the roadmap and the announcement posts to have a little  deeper look at those features and to write about it.
