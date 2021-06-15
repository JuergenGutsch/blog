---
layout: post
title: "ASP.​NET Core in .NET 6 - Preserve prerendered state in Blazor apps"
teaser: "This is the next part of the ASP.NET Core on .NET 6 series. In this post, I'd like to have a look into preserve prerendered state in Blazor apps."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- .NET 6
- Blazor

---

This is the next part of the [ASP.NET Core on .NET 6 series]({% post_url aspnetcore6-01.md %}). In this post, I'd like to have a look into preserve prerendered state in Blazor apps.

In Blazor apps can be prerendered on the server to optimize the load time. The app gets rendered immediately in the browser and is available for the user. Unfortunately, the state that is used on while prerendering on the server is lost on the client and needs to be recreated if the page is fully loaded and the UI may flicker, if the state is recreated and the prerendered HTML will be replaced by the HTML that is rendered again on the client.

To solve that, Microsoft adds support to persist the state into the prerendered page using the `<preserve-component-state />` tag helper. This helps to set a stage that is identically on the server and on the client.

> Actually, I have no idea why this isn't implemented as a default behavior in case the app get's prerendered. It should be done easily and wouldn't break anything, I guess. 

## Try to preserve prerendered states

I tried it with a new Blazor app and it worked quite well on the `FetchData` page. In this case, it doesn't make sense to write down a demo here, because the demo in the blog post is really good and easy to understand:

[Preserve prerendered state in Blazor apps](https://devblogs.microsoft.com/aspnet/asp-net-core-updates-in-net-6-preview-2/#preserve-prerendered-state-in-blazor-apps)

## What's next?

In the next part In going to look into the support for `HTTP/3 endpoint TLS configuration` in ASP.NET Core.