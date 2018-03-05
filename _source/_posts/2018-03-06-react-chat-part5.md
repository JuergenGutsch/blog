---
layout: post
title: "Creating a chat application using React and ASP.NET Core - Part 5"
teaser: "In this blog series I'm going to create a small chat application using React and ASP.NET Core, to learn more about React and to learn how React behaves in an ASP.NET Core project during development and deployment. This last post describes how to deploy the application to an Azure Website."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- React
- IdentityServer
- Azure
- Deploy
- CAKE
---

In this blog series, I'm going to create a small chat application using React and ASP.NET Core, to learn more about React and to learn how React behaves in an ASP.NET Core project during development and deployment. This Series is divided into 5 parts, which should cover all relevant topics:

1. [React Chat Part 1: Requirements & Setup]({% post_url react-chat-part1.md %})
2. [React Chat Part 2: Creating the UI & React Components]({% post_url react-chat-part2.md %})
3. [React Chat Part 3: Adding Websockets using SignalR]({% post_url react-chat-part3.md %})
4. [React Chat Part 4: Authentication & Storage]({% post_url react-chat-part3.md %})
5. **React Chat Part 5: Deployment to Azure**

I also set-up a GitHub repository where you can follow the project: [https://github.com/JuergenGutsch/react-chat-demo](https://github.com/JuergenGutsch/react-chat-demo). Feel free to share your ideas about that topic in the comments below or in issues on GitHub. Because I'm still learning React, please tell me about significant and conceptual errors, by dropping a comment or by creating an Issue on GitHub. Thanks.

## Intro

In this post I will write about the deployment of the app to Azure App Services. I will use CAKE to build pack and deploy the apps, both the identity server and the actual app. I will run the build an AppVeyor, which is a free build server for open source projects and works great for projects hosted on GitHub.

Will not go deep into the AppVeyor configuration, the important topics are cake and azure and the app itself.

BTW: SignalR is going into the next version. It is not longer alpha. The current version is 1.0.0-preview1-final. I updated the version in the package.json and in the ReactChatDemo.csproj. Also the NPM package name changed from "@aspnet/signalr-client" to "@aspnet/signalr". I needed to update the import statement in the WebsocketService.ts file as well. After updating SignalR I got some small breaking changes, which are easily fixed. (please see the GitHub repo, to learn about the changes.)

## Setup CAKE

CAKE is a build DSL, that is built on top of Roslyn to use C#. CAKE is open source and has a huge community, who creates a ton of add-ins for it. It also has a lot of built-in features.

Setting up CAKE is easily done. Just open the PowerShell and cd to the solution folder. Now you need to load a PowerShell script that bootraps the CAKE build and loads more dependencies if needed.

~~~ powershell
Invoke-WebRequest https://cakebuild.net/download/bootstrapper/windows -OutFile build.ps1
~~~

Later you need to run the build.ps1 to start your build script. Now the Setup is complete and I can start the create the script. I create a new file called build.cake. To edit the file it makes sense to use Visual Studio Code, because @code also has IntelliSense. In Visual Studio 2017 you only have syntax highlighting. Currently I don't know an add-in for VS to enable IntelliSense.

My starting point for every build script is, the simple example from the [quick start demo](https://cakebuild.net/docs/tutorials/setting-up-a-new-project):

~~~ c#
var target = Argument("target", "Default");

Task("Default")
  .Does(() =>
{
  Information("Hello World!");
});

RunTarget(target);
~~~







## Use CAKE in AppVeyor









## Closing words

