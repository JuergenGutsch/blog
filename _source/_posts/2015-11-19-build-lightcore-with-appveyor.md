--- 
layout: post
title: "AppVeyor: A simple build server for open source projects"
teaser: "Learn how I used AppVeyor to automate .NET Core builds and Xunit test runs for LightCore"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- LightCore
- Continuous Deployment
- Continuous Integration
- Build
- AppVeyor
---

For LightCore 2.0 I would like to show the build state inside the GitHub repository. I could use my personal Jenkins build server on a Azure VM, because Jenkins also provides the build state, with a plug-in.

But this seems to be the right moment to play around with AppVeyor:

[https://ci.appveyor.com/project/JuergenGutsch/lightcore](https://ci.appveyor.com/project/JuergenGutsch/lightcore)

AppVeyor is a browser based SaaS (Software as a Service) application in the cloud. AppVeyor provides many useful features within a pretty simple, intuitive and clean UI. GitHub is completely integrated and it is really easy to create a build job for a GitHub project:

![](/img/AppVeyor-General.png)

The .NET Version Manager (DNVM) is already installed and you only need to figure out which run-time is used by default. I used the build output to see the results of the "dnvm" commands. Finally I choosed the way to install the needed beta-8 with the batch scripts, every time the build starts:

![](/img/AppVeyor-Environment.png)

~~~ batch
dnvm update-self
dnvm install 1.0.0-beta8 -a x86 -r coreclr -OS win 
dnvm alias lccoreclr 1.0.0-beta8 -a x86 -r coreclr -OS win 
dnvm install 1.0.0-beta8 -a x86 -r clr -OS win
dnvm alias lcclr 1.0.0-beta8 -a x86 -r clr -OS win
~~~

For the builds and the tests I also used the batch command mode with the following lines:

Build:

![](/img/AppVeyor-Build.png)

~~~ batch
cd LightCore 
dnvm use lccoreclr 
dnu restore 
dnu build
~~~

Test:

![](/img/AppVeyor-Tests.png)

~~~ batch
cd ..\LightCore.Tests 
dnvm use lccoreclr 
dnu restore 
dnx test 
~~~

**Show the build state**

Finally I'm able to copy a small piece of MarkDown code, which I can use in the readme.md file in the GitHub repository to show the current build state:

![](/img/AppVeyor-Badge.png)
	
~~~ markdown
[![Build status](https://ci.appveyor.com/api/projects/status/et1fpjlmnsrkw3mv?svg=true)](https://ci.appveyor.com/project/JuergenGutsch/lightcore)
~~~

![Build Status](https://ci.appveyor.com/api/projects/status/et1fpjlmnsrkw3mv?svg=true)

As you can see, it is pretty simple to use and handle AppVeyor. I'm sure I'll also use AppVeyor for my other open source project, the "SimpleObjectStore". But I need to move that library to .NET Core first. ;)

