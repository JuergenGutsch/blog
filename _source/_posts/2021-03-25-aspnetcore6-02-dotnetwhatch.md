---
layout: post
title: "ASP.NET Core in .NET 6 - Part 02 - Update on dotnet watch"
teaser: "This is the second part of the ASP.NET Core on .NET 6 series. In this post, I want to have a look into the updates on dotnet watch."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Tests
- Tag
---

This is the second part of the [ASP.NET Core on .NET 6 series]({% post_url aspnetcore6-01.md %}). In this post, I want to have a look into the updates on `dotnet watch`. The [announcement post from February 17th](https://devblogs.microsoft.com/aspnet/asp-net-core-updates-in-net-6-preview-1/) mentioned that `dotnet watch` now does `dotnet watch run` by default.

Actually, this doesn't work in preview 1 because this feature didn't make it to this release by accident: https://github.com/dotnet/aspnetcore/issues/30470

BTW: This feature isn't mentioned anymore. The team changed the post and didn't add it to preview 2 though.

The idea is to just use `dotnet watch` without specifying the `run` command that should be executed after a file is changed. `run` is now the default command:

![dotnetwatch.png]({{site.baseurl}}/img/aspnetcore6/dotnetwatch.png)

This is just a small thing but might save some time.

## What's next?

In the next part In going to look into the support for IAsyncDisposable in MVC.