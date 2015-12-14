--- 
layout: post
title: "ASP.NET 5 on a Mac"
teaser: "Just because I got my Mac running again, I tried to run ASP.NET 5 rc1 on that machine. Yes we already know it should work on a Mac, but I wanted to see this by myself :)"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET 5
- .NET Core
- Mac
- Yeoman
---

I just got my Mac-Mini running running again and I wanted to play around with ASP.NET 5. This Mac-Mini is a build server with a Jenkins installed to build Cordova based iOS Apps. After many weeks I didn't access this machine, I had to install a lot of updates and I had to get all the build stuff again. And by the way I also installed and updated some tools to run ASP.NET 5.

To install ASP.NET 5 I used the installer from [get.asp.net](http://get.asp.net). This installs the .NET Version Manager (dnvm) and the latest .NET Execution Environment (DNX) for .NET Core and Mono. (If you want to use the command line to install all that stuff, visit [docs.asp.net/en/latest/getting-started/installing-on-mac.html](http://docs.asp.net/en/latest/getting-started/installing-on-mac.html).) I also needed to update node.js and NPM and to install Yeoman as described in the last blog post. 

The next steps were pretty easy: Using a terminal cd to Documents, create a new folder dev, cd to dev and complete the Yeoman wizard to create a project called "demo01":

~~~ batch
cd Documents
mkdir dev
cd dev
yo aspnet
~~~

After the project was successfully created, I started to application.

~~~ batch
cd demo01
dnu restore
dnx web
~~~

![]({{ site.baseurl }}/img/mac/terminal.png)

That works completely without any errors. Do you know that bad feeling, if something works unexpectedly fine on the first try? That's how I felt after I called the web on localhost:5000 in Safari:

![]({{ site.baseurl }}/img/mac/safari.png)

That's really, really cool :) I really love to have the possibility to run my ASP.NET 5 applications also on Linux Mac.

The real reason I tried .NET Core on Mac is, that I need some extra automation on my build server. For example: Creating a full text index for my Cordova apps is currently done with a Jenkins on a Windows Server machine where the builds for Windows Phone and Android apps are running and currently I copy that full text index to an Azure blob store to use it for the iOS builds. I don't need to transfer the full text indexes to the Mac anymore to build the iOS Apps. The code to create the full text index is written in C# and could be easily moved to a DNX console application to run on every build machine.