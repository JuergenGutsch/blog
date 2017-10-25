---
layout: post
title: ".NET Core 2.0 and ASP.NET 2.0 Core are here and ready to use"
teaser: "Recently I did a overview talk about .NET Core, .NET Standard and ASP.NET Core at the Azure Meetup Freiburg and they asked me some pretty important questions: Should we start using ASP.NET Core and .NET Core? Do we need to migrate our existing web applications to ASP.NET Core?"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- .NET Standard
---

Recently I did a overview talk about [.NET Core, .NET Standard and ASP.NET Core at the Azure Meetup Freiburg](https://www.meetup.com/preview/Azure-NET-Freiburg/events/243454870). I told them about .NET Core 2.0, showed the dotnet CLI and the integration in Visual Studio. Explained the sense of .NET Standard and why developers should take care about it. I also showed them ASP.NET Core, how it works, how to host and explained the main differences to the ASP.NET 4.x versions.

![]({{site.baseurl}}/img/core2-talk/talk01.jpg)

> BTW: This Meetup was really great. Well organized on a pretty nice and modern location. It was really fun to talk there. Thanks to Christian, Patrick and Nadine to organize this event :-)

![]({{site.baseurl}}/img/core2-talk/talk02.jpg)

After that talk they asked me some pretty interesting and important questions: 

**Question 1: "Should we start using ASP.NET Core and .NET Core?"**

My answer is a pretty clear YES. 

* Use .NET Standard for your libraries, if you don't have dependencies to platform specific APIs (eg. Registry, drivers, etc.) even if you don't need to be x-plat. Why? Because it just works and you'll keep a door open to share your library to other platforms later on. Since .NET Standard 2.0 you are not really limited, you are able to do almost all with C# you can do with the full .NET Framework
* Use ASP.NET Core for new web projects, if you don't need to do Web Forms. Because it is fast, lightweight and x-plat. Thanks to .NET standard you are able to reuse your older .NET Framework libraries, if you need to.
* Use ASP.NET Core to use the new modern MVC framework with the tag helpers or the new lightweight razor pages
* Use ASP.NET Core to host your application on various cloud providers. Not only on Azure, but also on Amazon and Google:
  * http://docs.aws.amazon.com/elasticbeanstalk/latest/dg/dotnet-core-tutorial.html
  * https://aws.amazon.com/blogs/developer/running-serverless-asp-net-core-web-apis-with-amazon-lambda/
  * https://codelabs.developers.google.com/codelabs/cloud-app-engine-aspnetcore/#0
  * https://codelabs.developers.google.com/codelabs/cloud-aspnetcore-cloudshell/#0
* Use ASP.NET Core to write lightweight and fast Web API services running either self hosted, in Docker or on Linux, Mac or Windows
* Use ASP.NET Core to create lightweight back-ends for Angular or React based SPA applications.
* Use .NET Core to write tools for different platforms


As an library developer, there is almost no reason to not use the .NET Standard. Since .NET Standard 2.0 the full API of the .NET Framework is available and can be used to write libraries for .NET Core, Xamarin, UWP and the full .NET Framework. It also supports to reference full .NET Framework assemblies.

> The .NET Standard is an API specification that need to be implemented by the platform specific Frameworks. The .NET Framework 4.6.2, .NET Core 2.0 and Xamarin are implementing the .NET Standard 2.0, which means they all uses the same API (namespaces names, class names, method names). Writing libraries against the .NET Standard 2.0 API will run on .NET Framework 2.0, on .NET Core 2.0, as well as on Xamarin and on every other platform specific Framework that supports that API.

**Question 2: Do we need to migrate our existing web applications to ASP.NET Core?**

My answer is: NO. You don't need to and I would propose to not do it if there's no good reason to do it. 

There are a lot blog posts out there about migrating web applications to ASP.NET Core, but you don't need to, if you don't face any problems with your existing one. There are just a few reasons to migrate:

* You want to go x-plat to host on Linux
* You want to host on small devices
* You want to host in Docker containers
* You want to use a faster framework
  * A faster framework is useless, if your code or your dependencies are slow ;-)
* You want to use a modern framework
  * Note: ASP.NET 4.x is not outdated, still supported and still gets new features

Depending on the level of customizing you did in your existing application, the migration could be a lot of effort. Someone needs to pay for the effort, that's why I would propose not to migrate to ASP.NET Core, if you don't have any problems or a real need to do it.

![]({{site.baseurl}}/img/core2-talk/talk03.jpg)

## Conclusion

I would use ASP.NET Core for every new web project and .NET Standard for every library I need to write. Because it is almost mature and really usable since the versions 2.0. You can do almost all the stuff, you can do with the full .NET framework.

BTW: Rick Strahl also just wrote an article about that. Please read it. It's great, as almost all of his posts:
[https://weblog.west-wind.com/posts/2017/Oct/22/NET-Core-20-and-ASPNET-20-Core-are-finally-here](https://weblog.west-wind.com/posts/2017/Oct/22/NET-Core-20-and-ASPNET-20-Core-are-finally-here)

BTW: The slides off that talk are on slide share. If you want me to do that talk in your meetup or in your user group, just ping me on [twitter](https://twitter.com/sarpcms) or drop me an [email](mailto:juergen@gutsch-online.de)

