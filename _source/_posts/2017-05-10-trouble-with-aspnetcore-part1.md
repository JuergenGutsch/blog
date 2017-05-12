---
layout: post
title: "ASP.NET Core in trouble"
teaser: "Microsoft removed the support of the full .NET Framework for ASP.NET Core 2.0 and some developers are not really happy about that. ASP.NET Core 2.0 will just run on .NET Core 2.0. In this post I tried to analyze that and it doesn't sound that evil..."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
---

## ASP.NET Core today

Currently ASP.NET Core - Microsoft's new web framework - can be used on top of .NET Core and on top of the .NET Framework. This fact is pretty nice, because you are able to use all the new features of ASP.NET Core with the power of the huge but well known .NET Framework. On the other hand, the new cross-platform .NET Core is even nice, but with a smaller set of features. Today you have the choice between of being x-plat or to use the full .NET Framework. This isn't really bad.

Actually it could be better. Let's see why:

## What is the issue?

Microsoft removed the support of the full .NET Framework for ASP.NET Core 2.0 and some developers are not really happy about that. See [this Github Issue thread](https://github.com/aspnet/Home/issues/2022). ASP.NET Core 2.0 will just run on .NET Core 2.0. This fact results in a hot discussion within that GitHub issue.

It also results in some misleading and confusing headlines and contents on some German IT news publishers:

* [Microsoft will ASP.NET Core nicht mehr auf dem klassischen .NET anbieten](https://www.heise.de/newsticker/meldung/Microsoft-will-ASP-NET-Core-nicht-mehr-auf-dem-klassischen-NET-anbieten-3708715.html)
* [Microsoft verärgert .Net-Entwickler mit Support-Entscheidung](https://www.golem.de/news/asp-net-core-2-0-microsoft-veraergert-net-entwickler-mit-support-entscheidung-1705-127712.html)

While the discussion was running, David Fowler said on Twitter that it's the best to think of [ASP.NET Core 2.0 and .NET Core 2.0 as the same product](https://twitter.com/davidfowl/status/861809298611073024). 

Does this makes sense?

I followed the discussion and thought a lot about it. And yes, it starts to make sense to me. 

## NET Standard

What many people don't recognize or just forget about, is the .NET Standard. The .NET Standard is a API definition that tries to unify the APIs of .NET Core, .NET Framework and Xamarin. But it actually does a little more, it provides the API as a set of Assemblies, which forwards the types to the right Framework.

Does it make sense to you? (Read more about the .NET Standard [in this documentation)](https://github.com/dotnet/standard/blob/master/docs/netstandard-20/README.md)

Currently ASP.NET Core runs on top of .NET Core and .NET Framework, but actually uses a framework that is based on .NET Standard 1.4 and higher. All the referenced libraries, which are used in ASP.NET Core are based on .NET Standard 1.4 or higher. Let's call them ".NET Standard libraries" ;) This libraries contain all the needed features, but doesn't reference a specific platform, but the .NET Standard API.

You are also able to create those kind of libraries with Visual Studio 2017.

By creating such libraries you provide your functionality to multiple platforms like Xamarin, .NET Framework and .NET Core (depending on the .NET Standard Version you choose). Isn't that good?

And in .NET Framework apps you are able to reference .NET Standard based libraries.

## About runtimes

.NET Core is just a runtime to run Apps on Linux, Mac and Windows. Let's see the full .NET Framework as a runtime to run WPF apps, Winforms apps and classic ASP.NET apps on Windows. Let's also see Xamarin as a runtime to run apps on iOS and Android.

Let's also assume, that the .NET Standard 2.0 will provide the almost full API of the .NET Framework to your Application, if it is finished.

Do we really need the full .NET Framework for ASP.NET Core, in this case? No, we don't really need it.

## What if ...

* ... .NET Framework, .NET Core and Xamarin are just runtimes?
* ... .NET Standard 2.0 is as complete as the .NET Framework?
* .. .NET Standard 2.0 libraries will have the same features as the .NET Framework?
* .. ASP.NET 2.0 Core uses the .NET Standard 2.0 libraries?

Do we really need the full .NET Framework as a runtime for ASP.NET Core?

I think, no!

Does it also makes sense to use the full .NET Framework as a runtime for Xamarin Apps?

I also think, no.

## Conclusion

ASP.NET Core and .NET Core shouldn't be really shipped as one product, as David said. Because it is on top of .NET Core and maybe another technology could also be on top of .NET Core in the future. But maybe it makes sense to ship it as one product, to tell the people that ASP.NET Core 2.0 is based on top of .NET Core 2.0 and needs the .NET Core runtime. (The classic ASP.NET is also shipped with the full .NET Framework.)

* .NET Core, Xamarin and the full .NET Framework are just a runtimes.
* The .NET Standard 2.0 will almost have the same API as the .NET Framework.
* The .NET Standard 2.0 libraries will have the same set of features as the .NET Framework
* ASP.NET Core 2.0 uses NET Standard 2.0 libraries as a framework.

With this facts, Microsoft's decision to run ASP.NET Core 2.0 on .NET Core 2.0 only, doesn't sound that evil anymore. 

From my perspective, ASP.NET is **not in trouble** and it's all fine and **it makes absolutely sense**. The troubles are only in the discussion about that on GitHub and on Twitter :)

What do you think? Do you agree?

Let's see what [Microsoft will tell us about it](https://channel9.msdn.com/events/Build/2017/C9L18) at the Microsoft Build conference in the next days. Follow the live stream: [https://channel9.msdn.com/Events/Build/2017](https://channel9.msdn.com/Events/Build/2017)

#### [Update 2017-05-11]

Yesterday in an official blog post of announcing ASP.NET Core 2.0 preview1, (Announcing ASP.NET 2.0.0-Preview1 and Updates for .NET Web Developers) Jeff Fritz wrote, that the preview 1 is limited to .NET Core 2.0 only. The overall goal is to put ASP.NET Core 2.0 on top of .NET Standard 2.0. This means it will be possible to run ASP.NET Core 2.0 apps on .NET Core, Mono and the full .NET Framework.
