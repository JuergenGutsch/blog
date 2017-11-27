---
layout: post
title: "NuGet, Cache and some more problems"
teaser: "Recently I had some problems using NuGet, two of them were huge, which took me a while to solve them. But all of them are easy to fix, if you know how to do it."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- Unit Test
- XUnit
- MSTest
---

Recently I had some problems using NuGet, two of them were huge, which took me a while to solve them. But all of them are easy to fix, if you know how to do it.

## NuGet Cache

The fist and more critical problem was related to the NuGet Cache in .NET Core projects. It seems the underlying problem was a broken package in the cache. I didn't find out the real reason. Anyway, every time I tried to restore or add packages, I got an error message, that told me about an error at the first character in the project.assets.json. Yes, there is still a kind of a project.json even in .NET Core 2.0 projects. This file is in the "obj" folder of a .NET Core project and stores all information about the NuGet packages. 

This error looked like a typical encoding error. This happens often if you try to read a ANSI encoded file, from a UTF-8 encoded file, or vice versa. But the project.assets.json was absolutely fine. It seemed to be a problem with one of the packages. It worked with the predefined .NET Core or ASP.NET Core packages, but it doesn't with any other. I wasn't able anymore to work on any .NET Core projects that targets .NET Core, but it worked with projects that are targeting the full .NET Framework.

I couldn't solve the real problem and I didn't really want to go threw all of the packages to find the broken one. The .NET CLI provides a nice tool to manage the NuGet Cache. It provides a more detailed CLI to NuGet.

~~~ shell
dotnet nuget --help
~~~

This shows you tree different commands to work with NuGet. `delete` and `push` are working on the remote server to delete a package from a server or to push a new package to the server using the NuGet API. The third one is a command to work with local resources:

~~~ shell
dotnet nuget locals --help
~~~

This command shows you the help about the locals command. Try the next one to get a list of local NuGet resources:

~~~ shell
dotnet nuget locals all --list
~~~

![]({{site.baseurl}}/img/nuget/cache.png)

You can now use the clear option to clear all caches:

~~~ shell
dotnet nuget locals all --clear
~~~

Or a specific one by naming it:

~~~ shell
dotnet nuget locals http-cache --clear
~~~

This is much more easier than searching for all the different cache locations and to delete them manually.

This solved my problem. The broken package was gone from all the caches and I was able to load the new, clean and healthy ones from NuGet.

## Versions numbers in packages folders

The second huge problem is not related to .NET Core, but to classic .NET Framework projects using NuGet. If you also use Git-Flow to manage your source code, you'll have at least to different main branches: `Master` and `Develop`. Both branches contain different versions. `Master` contains the current version code and `Develop` contains the next version code. It is also possible that both versions use different versions of dependent NuGet packages. And here is the Problem:

`Master` used e. g. AwesomePackage 1.2.0 and `Develop` uses AwesomePackage 1.3.0-beta-build54321

Both versions of the code are referencing to the AwesomeLib.dll but in different locations:

* Master: /packages/awesomepackage 1.2.0/lib/net4.6/AwesomeLib.dll
* Develop: /packages/awesomepackage 1.3.0-beta-build54321/lib/net4.6/AwesomeLib.dll

If you now release the `Develop` to `Master`, you'll definitely forget to go to all the projects to change the reference paths, don't you? The build of master will fail, because this specific beta folder wont exist on the server, or even more bad: The build will not fail because the folder of the old package still exists on the build server, because you didn't clear the build work space. This will result in runtime errors. This problem will probably happen more likely, if you provide your own packages using your own NuGet Server.

I solved this by using a different NuGet client than NuGet. I use Paket, because it doesn't store the binaries in version specific folder and the reference path will be the same as long as the package name doesn't change. Using Paket I don't need to take care about reference paths and every branch loads the dependencies from the same location.

Paket officially supports the NuGet APIs and is mentioned on NuGet org, in the package details.  

![]({{site.baseurl}}/img/nuget/paket.png)

To learn more about Paket visit the official documentation: [https://fsprojects.github.io/Paket/](https://fsprojects.github.io/Paket/)

## Conclusion

Being an agile developer, doesn't only mean to follow an iterative process. It also means to use the best tools you can buy. But you don't always need to buy the best tools. Many of them are open source and free to use. Just help them by donating some bugs, spread the word, file some issues or contribute in a way to improve the tool. Paket is one of such tools, lightweight, fast, easy to use and it solves many problems. It is also well supported in [CAKE ](https://cakebuild.net/dsl/paket/), which is the build DSL I use to build, test and deploy applications.









