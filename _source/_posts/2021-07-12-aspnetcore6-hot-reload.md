---
layout: post
title: "ASP.​NET Core in .NET 6 - HTTP/3 endpoint TLS configuration"
teaser: "This is the next part of the ASP.NET Core on .NET 6 series. In this post, I'd like to have a look at the .NET 6 support for Hot Reload."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- .NET 6
---

This is the next part of the [ASP.NET Core on .NET 6 series]({% post_url aspnetcore6-01.md %}). In this post, I'd like to have a look at the .NET 6 support for Hot Reload.

In the preview 3, Microsoft started to add support for Hot Reload, which automatically gets started when you write `dotnet watch`. The preview 4 also includes support for Hot Reload in Visual Studio. Currently, I'm using the preview 5 to play around with Hot Reload.

## Playing around with Hot Reload

To play around and to see how it works, I also create a new MVC project using the following commands:

~~~shell
dotnet new mvc -o HotReload -n HotReload
cd HotReload
code .
~~~

These commands create an MVC app, change into the project folder, and open VSCode.

`dotnet run` will not start the application with Hot Reload enabled, but `dotnet watch` does. 

Run the command `dotnet watch` and see what happens, if you change some C#, HTML, or CSS files. It immediately updates the browser and shows you the results. You can see what's happening in the console as well.

![image-20210705173955666](C:\Users\webma\AppData\Roaming\Typora\typora-user-images\image-20210705173955666.png)

As mentioned initially, Hot Reload is enabled by default, if you use `dotnet watch`. If you don't want to use Hot Reload, you need to add the option `--no-hot-reload` to the command:

~~~shell
dotnet watch --no-hot-reload
~~~

Hot Reload should also work with WPF and Windows Forms Projects, as well as with .NET MAUI projects. I had a quick try with WPF and it didn't really work with XAML files. Sometimes it also did an infinite build loop. Every build 

More about Hot Reload in this blog post: [https://devblogs.microsoft.com/dotnet/introducing-net-hot-reload/](https://devblogs.microsoft.com/dotnet/introducing-net-hot-reload/)

## What's next?

In the next part In going to look into the support for `BlazorWebView controls for WPF & Windows Forms` in ASP.NET Core.
