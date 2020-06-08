---
layout: post
title: "Getting the .editorconfig working with MSBuild"
teaser: "Description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Tests
- Tag
---

In January I wrote a post about setting up VS2019 and VSCode to use the .editorconfig

It works like it should work: In the editors. And it works in VS2019 at build-time. But it doesn't work at build time using MSBuild. This means it won't work with the .NET CLI, it won't work with VSCode and it won't work on any build server.

Actually this is a huge downside about the .editorconfig. Why shall we use the .editoconfig to enforce the code style, if a build in VSCode doesn't fail, but it fails in VS2019 does? Why shall we use the .editorconfig, if the build on a build server doesn't fail. Not all of the developers are using VS2019, sometimes VSCode is the better choice. And we don't want to install VS2019 on a build server and don't want to call vs.exe to build the sources.

The reason why it is like this is as simple as bad: The Roslyn analyzers to check the codes using the .editorconfig are not yet done. 

Actually, Microsoft is working on that and is porting the VS2019 checks to Roslyn analyzers that can be downloaded and used via NuGet. Currently, the half of the work is done and some of the analyzers can be used in the project.

With this post I'd like to try it out. We need this for our projects in the YOO, the company I work for and I'm really curious about how this is going to work in a real project

## Let's try it out

To try it out, I'm going to use the Weather Stats App I created in previous posts. Feel free to clone it from GitHub and follow the steps I do within this post.

