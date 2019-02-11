---
layout: post
title: "WPF and WinForms will run on .NET Core 3"
teaser: "Maybe you already heard or read about the fact that Microsoft brings WinForms and WPF to .NET Core 3.0. Since the .NET Standards moves the API of .NET Core more to the same level of .NET Framework, it was a question of time when the APIs are almost equal."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- WPF
- WinForms
- UI
---

Maybe you already heard or read about the fact that Microsoft brings WinForms and WPF to .NET Core 3.0. Maybe you already saw the presentations on the Connect conference, or any other conference or recording when Scott Hanselman shows how to run a pretty old WPF application on .NET Core. I saw a demo where he run [BabySmash](https://www.hanselman.com/babysmash/) on .NET Core. 

> BTW: My oldest sun really loved that BybySmash when he was a baby :-)

You did not hear, read or see about it? 

## WPF and WinForms on .NET Core?

I was really wondering about this step, even because I wrote an article for a German .NET magazine some months before where I mentioned that Microsoft won't build a UI Stack for .NET Core. There where some other UI stacks built by the community. The most popular is [Avalonia](http://avaloniaui.net/).

But this step makes sense anyway. Since the .NET Standards moves the API of .NET Core more to the same level of .NET Framework, it was a question of time when the APIs are almost equal. WPF and WinForms is based on .NET Libraries, it should basically also run on .NET Core.

### Does this mean it runs on Linux and Mac?

Nope! Since WinForms and WPF uses Windows only technology in the background, it cannot run on Linux or Mac. It is really depending on Windows. The sense of running it on .NET Core is performance and to be independent from any framework. .NET Core is optimized for performance, to run superfast web applications in the cloud. .NET Core is also independent from the installed framework on the machine. Just deploy the runtime together with your application.

You are now **able to run fast and self-contained Windows desktop applications**. That's awesome, isn't it?

Good I wrote that article some months before ;-)

Anyways...

## The .NET CLI

Every time I install a new version of the .NET Core runtime I try `dotnet new` and I was positively shocked about what I saw this time:![](../img/netcore3/dotnet-new.png)

You are now able to create a Windows Forrms or a WPF application using the .NET CLI. This is cool. And for sure I needed to try it out:

~~~ shaell
dotnet new -n WpfTest -o WpfTest
dotnet new -n WpfTest -o WpfTest
~~~

And yes, it is working as you can see here in Visual Studio Code:

![](../img/netcore3/wpf.png)

And this is the WinForms project in VS Code

![](../img/netcore3/win.png)

Running `dotnet run` on the WPF project:

![](../img/netcore3/wpf-gui.png)

And again on the WinForms GUI:

![](../img/netcore3/win-gui.png)

## IDE

Visual Studio Code isn't the right editor for this kind of projects. If you know XAML pretty well, it will work, but WinForms definitely won't work well. You need to write the designer code manually and you don't have any designer support yet. Maybe there will be some designer in the future, but I'm not sure. 

The best choice to work with WinForms and WPF on .NET Core is Visual Studio 2017 or newer.

## Last words

I don't think I will now start to write desktop apps on .NET Core 3, because I'm a web guy. But it is a really nice option to build apps like this on .NET Core. 

> BTW: Even EF 6 will work in .NET Core 3, that means you also son't need to rewrite the database access part of your desktop application.

As I wrote, you can now use this super fast framework and the option to create self contained apps. I would suggest to try it out, to play around with it. Do you have an older desktop application based on WPF or WinForms? I would be curious about whether you can run it on .NET Core 3. Tell me how easy it was to get it running on .NET Core 3.