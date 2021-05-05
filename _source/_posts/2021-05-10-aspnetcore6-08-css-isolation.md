---
layout: post
title: "ASP.NET Core in .NET 6 - Part 08 - CSS isolation for MVC Views and Razor Pages"
teaser: "This is the eighth part of the ASP.NET Core on .NET 6 series. In this post, I'd like to have a quick  into the support for SS isolation for MVC Views and Razor Pages."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- .NET 6
---

This is the eighth part of the [ASP.NET Core on .NET 6 series]({% post_url aspnetcore6-01.md %}). In this post, I'd like to have a quick  into the support for SS isolation for MVC Views and Razor Pages.

Blazor components already support CSS isolation. MVC Views and Razor Pages now do the same. Since the official blogpost shows it on Razor Pages, I'd like to try it in a MVC application.

## Trying CSS isolation for MVC Views

 At first, I'm going to create a new MVC application project using the .NET CLI:

``` shell
dotnet new mvc -n CssIsolation -o CssIsolation
cd CssIsolation
code .
```

These commands create the project, change the directory into the project folder, and opens VSCode.

After VSCode opens, 















## What's next?

In the next part In going to look into the support for `Infer component generic types from ancestor components` in ASP.NET Core.

