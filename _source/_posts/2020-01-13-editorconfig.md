---
layout: post
title: "Using the .editorconfig in VS2019 and VSCode"
teaser: "In the backend developer team of the YOO we are currently discussing coding style guidelines and ways to enforce them. Since we are developer with different mindsets and backgrounds, we need to find a way to enforce the rules in a way that works in different editors too."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Tests
- Tag
---

In the backend developer team of the YOO we are currently discussing coding style guidelines and ways to enforce them. Since we are developer with different mindsets and backgrounds, we need to find a way to enforce the rules in a way that works in different editors too.

> BTW: C# developers often came from other languages and technologies before they started to work with this awesome language. Universities mostly teach Java, ore the developer were front end developers in the past, or started with PHP. Often .NET developers start with VB.NET and switch to C# later. Me as well: I also started as a front end developer with HTML4, CSS2 and JavaScript, used VB Script and VB6 on the server side in 2001. Later I used VB.NET on the server and switched to C# in 2007. 

In our company we use ASP.NET Core more and more. This also means we are more and more free to use the best editor we want to use. And we are more and more free to use platform we want to work with. Some of use already prefer VSCode to work on ASP.NET Core projects. Maybe we'll have a fellow colleague in the future who prefers VSCode on Linux or VS on a Mac. This also makes the development environments divers. 

Back when we used Visual Studio only, Style Cop was the tool to enforce coding style guidelines. Since a couple of years there is a new tool that works in almost all editors out in the world.

The `.editorconfig` is a text file that overrides the settings of the editor of your choice. 

Almost every code editor has settings to style the code in a way you like, or the way your team likes. If this editor supports the `.editorconfig` you are able to override this settings with a simple text file that usually is checked in with your source code and available for all developers who work on those sources.

[Visual Studio 2019 supports](https://docs.microsoft.com/en-us/visualstudio/ide/create-portable-custom-editor-options?view=vs-2019) the `.editorconfig` by default, [VS for Mac also supports it](https://docs.microsoft.com/en-us/visualstudio/mac/editorconfig?view=vsmac-2019) and VSCode supports it with a few special settings.

## The only downside of the `.editorconfig` I can see

Since the `.editorconfig` is a settings file that overrides the settings of the code editor, it might be that only those settings will work that are supported by the editor. So it might work that not all of the settings will work on all code editors. 

But there is a workaround at least on the NodeJS side and on the .NET side. Both technologies support the `.editorconfig` on the code analysis side instead of the editor side, which means NodeJS or the .NET Compiler will check the code and enforce the rules instead of the editor. The editor only displays the error and helps the author to fix those errors. 

As far as I got it: On the .NET side it VS2019 on the one hand and Omnisharp on the other hand. Omnisharp is a project that support .NET development on many code editors, so on VSCode as well. Even if VSCode is called a Visual Studio, it doesn't support .NET and C# natively. It is the Omnisharp add-in that enables .NET and brings the Roslyn compiler to the Editor.

> "CROSS PLATFORM .NET DEVELOPMENT!
> OmniSharp is a family of Open Source projects, each with one goal: To enable a great .NET experience in **YOUR** editor of choice"
> http://www.omnisharp.net/

So the `.editorconfig` is supported by Omnisharp in VSCode. This means it might be that the support of the `.editorconfig` differs between VS2019 and VSCode. 

## Enable the .editorconfig in VSCode

As I wrote the `.editorconfig` is enabled by default in VS2019. There is nothing to do about it. If VS2019 finds an `.editorconfig` it will use it immediately and it will check your code on every code change. If VS2019 finds an `.editorconfig` in your solution, it will tell you about it and propose to add it to a solution folder to make it easier accessible for you in the editor. 

In VSCode you need to install an [add-in called EditorConfig](https://github.com/editorconfig/editorconfig-vscode). This doesn't enable the `.editorconfig` even if it is telling you about it. Maybe it actually does, but it doesn't work with C# because Omnisharp does something. But this add-in supports you to create or edit your `.editorconfig`.

To actually enable the support of the `.editorconfig` in VSCode you need to change two Omnisharp settings in VSCode:

Open the settings in VSCode and search for Omnisharp. Than you need to "Enable Editor Config Support" and to "Enable Roslyn Analyzers"

![]({{site.baseurl}}/img/editorconfig/omnisharp.png)

After you changed those settings, you need to restart VSCode to restart the Omnisharp server in the background.

That's it!

## Conclusion

Now the `.editorconfig` works in VSCode almost the same way as in VS2019. And it works great. I tried it by opening the same project in VSCode and in VS2019 and changed some settings in the `.editorconfig`. The changed settings immediately changed the editors. Both editors helped me to change the code to match the code styles. 

We at the YOO still need to discuss about some coding styles, but for now we use the recommended styles and change the things we discuss as soon we have a decision.

Do you ever discussed about coding styles in a team? If yes, you know that we are discussing about how to enforce var over the explicit type and whether to use simple `usings` or not, or whether to always use curly braces with if statements or not... This might be annoying, but it is really important to get a common sense and it is important that everybody agree on it.