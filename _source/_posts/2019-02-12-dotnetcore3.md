---
layout: post
title: "title"
teaser: "description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- Unit Test
- XUnit
- MSTest
---

Maybe you already heard or read about the fact that Microsoft brings WinForms and WPF to .NET Core 3.0. Maybe you already saw the presentations on the Connect conference when Scott Hanselman showed how to run a pretty old WPF application on .NET Core. I saw a demo where he run BabySmash on .NET Core. My oldest sun really loved that app when he was a baby :-)

You did not, hear, read or see about it? 

## What?

I was really wondering about this step, even because I wrote an article for a German .NET magazine some months before where I mentioned that Microsoft won't build a UI Stack for .NET Core. There where some other UI stacks built by the community. The most popular is Avalonia.

But this step makes sense anyway. Since the .NET Standards moves the API of .NET Core more to the same level of .NET Framework, it was a question of time when the APIs are almost equal. WPF and WinForms is based on .NET Libraries, it should basically also run on .NET Core.

### Does this mean it runs on Linux?

Nope. Since WinForms and WPF uses Windows only technology in the background, it cannot run on Linux or Mac. The sense of running it on .NET Core is performance and to be independent from any framework. .NET Core is optimized for performance, to run superfast web applications in the cloud. .NET Core is also independent from the installed framework on the machine. Just deploy the runtime together with your application.

You are now able to run fast and self-contained Windows desktop applications. That's awesome, isn't it?

Good I wrote that article some months before ;-)

Anyways...

## The .NET CLI

Every time I install a new version of the .NET Core runtime I try `dotnet new` and I was positively shocked about what I saw:![](../img/netcore3/dotnet-new.png)

You are able to create a Windows Forrms or a WPF application using the .NET CLI. This is cool. And for sure I needed to try it out:

~~~ shaell
dotnet new -n WpfTest -o WpfTest
dotnet new -n WpfTest -o WpfTest
~~~

And yes, it is working:

![](../img/netcore3/wpf.png)



![](../img/netcore3/win.png)

Running dotnet run on both projects:

