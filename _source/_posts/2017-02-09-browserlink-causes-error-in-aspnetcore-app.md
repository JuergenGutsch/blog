---
layout: post
title: "BrowserLink causes an error in a new ASP.NET Core web"
teaser: "Using the latest bits of Visual Studio 2017 and the latest SDK of .NET Core, you will possibly get an error while starting your ASP.NET Core Web in Visual Studio 2017 or using dotnet run."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- BrowserLink
---

Using the latest bits of Visual Studio 2017 and the latest SDK of .NET Core, you will possibly get an error while starting your ASP.NET Core Web in Visual Studio 2017 or using dotnet run.

In Visual Studio 2017 the browser opens on pressing F5, but with wired HTML code in the address bar. Debugging starts and stops a few seconds later. Using the console you'l get a more meaningful error message:

![](/img/browserlinkerror/browserlinkerror.png)

This error means, something is missing the system.runtime.dll ("System.Runtime, Version=4.2.0.0") which is not referenced or used somewhere directly. I had a deeper look into the NuGet references and couldn't found it. 

Because I often had problems with BrowserLink in the past, I removed the NuGet reference from the project and all worked fine. I added it again, to be sure that the removal didn't clean up anything. The error happened again.

It seams that the current Version of BrowserLink is referencing a library which is not supported by the app. Remove it, if you get the same or a similar error.

> BrowserLink in general is a pretty cool feature, it refreshes the browser magically if anything changes on the server. With this tool, you are able to edit your CSS files and preview it directly in the browser without doing a manual refresh. It is a VisualStudio Add-in and uses NuGet-Packages to extend your app to support it.

