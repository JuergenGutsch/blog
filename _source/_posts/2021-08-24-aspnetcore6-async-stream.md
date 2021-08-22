---
layout: post
title: "ASP.NET Core in .NET 6 - Async streaming"
teaser: "This is the next part of the ASP.NET Core on .NET 6 series. In this post, I'd like to have a look into async streaming.
"author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET 6
- ASP.NET Core
- Streams
- Async
---

This is the next part of the [ASP.NET Core on .NET 6 series]({% post_url aspnetcore6-01.md %}). In this post, I'd like to have a look into async streaming.

## IAsyncEnumerable<>

Async streaming is now supported from the controller action down to the response formatter, as well as on the hosting level. This topic is basically about  the `IAsyncEnumerable<T>`. This means that those async enumerables will be handled async all the way down to the down to the response stream.  They don't get buffered anymore, which improves the performance a lot and reduces the memory usage a lot. Huge lists of date now gets smoothly streamed to the client.

In the past, because of the buffering we handled large data, by sending them in small chunks to the output stream. This way we needed to find the right balance of the chunk size. Smaller  chunks increases the CPU consumption and bigger chunks increases the memory consumption.

This is not longer needed. The `IAsyncEnumerable<T>` does this for you with a lot better performance 

Even EF Core supports the `IAsyncEnumerable<T>` to query the data. Because of that, working with EF Core is improved as well. Data you fetch from the database using EF Core can now directly streamed to the output. 





the 



## What's next?

In the next part In going to look into the support for `Async streaming` in ASP.NET Core.

