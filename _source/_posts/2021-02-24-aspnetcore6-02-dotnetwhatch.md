---
layout: post
title: "ASP.NET Core in .NET 6 - Part 02 - Update on dotnet watch"
teaser: "Description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Tests
- Tag
---

Post your content here

This is the second part of the [ASP.NET Core on .NET 6 series]({% post_url aspnetcore6-01.md %}). In this post I want to have a look onto the updates on `dotnet watch`. The [announcement post  from February 17th](https://devblogs.microsoft.com/aspnet/asp-net-core-updates-in-net-6-preview-1/) mentioned that `dotnet watch` now does `dotnet watch run` by default

Actually this doesn't work in the current preview.

The idea is to just use `dotnet watch` without specifying the `run` command that should executed after a file changed. `run` should be the default command. But it is not. 

I had a look into the sources of the preview

https://github.com/dotnet/sdk/blob/release/6.0.1xx-preview1/src/BuiltInTools/dotnet-watch/Program.cs



