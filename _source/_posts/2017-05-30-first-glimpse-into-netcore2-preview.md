---
layout: post
title: "A first glimpse into .NET Core 2.0 Preview 1"
teaser: "Description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core 2
---

At the Build 2017 conference Microsoft announced the preview 1 version of .NET Core 2. I recently had a quick look into it and want to show you a little bit about it.

## The announcement

Microsoft wrote about the release of the preview 1 in this post: [https://blogs.msdn.microsoft.com/dotnet/2017/05/10/announcing-net-core-2-0-preview-1/](https://blogs.msdn.microsoft.com/dotnet/2017/05/10/announcing-net-core-2-0-preview-1/) It is important to read the first part about the requirements carefully. Especially the requirement of Visual Studio 2017 15.3 Preview. At the first quick look I was wondering about the requirement of installing a preview version of Visual Studio 2017, because I have already installed the final version since a few months. But the details is in the numbers. The final version of Visual Studio 2017 is the 15.2. The new tooling for .NET Core 2.0 preview is in the 15.3 which is in preview currently. 

So if you wanna use .NET Core 2. preview 1 with Visual Studio 2017 you need to install the preview of 15.3

The good thing is, the preview can be installed side by side with the current final of Visual Studio 2017. It doesn't double the usage of disk space, because both versions are able share some SDKs, e.g. the Windows SDK. But you need to install the add-ins you want to use for this version separately.

After the Visual Studio you need to install the new .NET Core SDK which also installs NET Core 2.0 Preview 1 and the .NET CLI.

## The .NET CLI

After the new version of .NET Core is installed type dotnet --version in a command prompt. It will show you the version of the currently used .NET SDK:

![{{ site.baseurl }}/netcore2/01-dotnetversion.PNG]()

Wait. I installed a preview 1 version and this is now the default on the entire machine? Yes.

The CLI uses the latest installed SDK on the machine by default. But anyway you are able to run different .NET Core SDKs side by side. To see what versions are instaled on our machine type dotnet --info in a command prompt and copy the first part of the base path and past it to a new explorer window:

![{{ site.baseurl }}/netcore2/02-dotnetinfo.PNG]()

![{{ site.baseurl }}/netcore2/03-dotnetsdks.PNG]()

You are able to use all of them if you want to.

This is possible by adding a global.json to your solution folder. This is a pretty small file which defines the SDK version you want to use:

~~~ json
{
  "projects": [ "src", "test" ],
  "sdk": {
    "version": "1.0.4"
  }
}
~~~

Inside the folder C:\git\dotnetcore\, I added two different folders: the v104 should use the current final version 1.0.4 and the v200 should use the preview 1 of 2.0.0. to get it working I just need to put the global.json into the v104 folder:

![{{ site.baseurl }}/netcore2/04-global-json.PNG]()

## The SDK

Now I want to have a look into the new SDK. The first thing I do after installing a new version is to type dotnet --help in a command prompt. The first level help doesn't contain any surprises, just the version number differs. The most interesting difference is visible by typing dotnet new --help. We get a new template to add an ASP.NET Core Web App based on Razor pages. We also get the possibility to just add single files, like a razor page, Nuget.config or a Web.Config. This is pretty nice.

![{{ site.baseurl }}/netcore2/05-dotnetnew.PNG]()

I also played around with the SDK by creating a new console app. I typed dotnet new console -consoleapp

![{{ site.baseurl }}/netcore2/06-dotnetnewconsole.PNG]()

As you can see in the screenshot dotnet new will directly download the NuGet packages from the package source. It runs dotnet restore for you. It is not a super cool feature but good to know if you get some NuGet restore errors while creating a new app.

When I opened the consoleapp.csproj, I saw the expected TargetFramework "netcoreapp2.0"

~~~ xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.0</TargetFramework>
  </PropertyGroup>

</Project>
~~~

This is the only difference between the 2.0.0 preview 1 and the 1.0.4

In ASP.NET Core are a lot more changes done. Let's have a quick look here too:

## ASP.NET Core 2.0



