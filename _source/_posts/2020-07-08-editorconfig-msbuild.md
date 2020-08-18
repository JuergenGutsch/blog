---
layout: post
title: "Getting the .editorconfig working with MSBuild"
teaser: "In January I wrote a post about setting up VS2019 and VSCode to use the .editorconfig. In this post I'm going to write about how to get the .editorconfig settings checked during build time."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- VSCode
- VS2019
- editorconfig
- Roslyn
---

> UPDATE: While trying the .editorconfig and writing this post, I did a fundamental mistake. I added a ruleset file to the projects and this is the reason why it worked. It wasn't really the .editorconfig in this case. I'm really sorry about that.
> Please find [this post to learn how it is really working]({% post_url editorconfig-netframework.md %}).

In January I wrote a post about setting up VS2019 and VSCode to use the `.editorconfig`. In this post I'm going to write about how to get the `.editorconfig` settings checked during build time.

It works like it should work: In the editors. And it works in VS2019 at build-time. But it doesn't work at build time using MSBuild. This means it won't work with the .NET CLI, it won't work with VSCode and it won't work on any build server that uses MSBuild.

Actually this is a huge downside about the `.editorconfig`. Why shall we use the `.editoconfig` to enforce the coding style, if a build in VSCode doesn't fail, but it fails in VS2019 does? Why shall we use the `.editorconfig`, if the build on a build server doesn't fail. Not all of the developers are using VS2019, sometimes VSCode is the better choice. And we don't want to install VS2019 on a build server and don't want to call vs.exe to build the sources.

The reason why it is like this is as simple as bad: The Roslyn analyzers to check the codes using the `.editorconfig` are not yet done. 

Actually, Microsoft is working on that and is porting the VS2019 coding style analyzers to Roslyn analyzers that can be downloaded and used via NuGet. Currently, the half of the work is done and some of the analyzers can be used in the project. See here: [#33558](https://github.com/dotnet/roslyn/issues/33558)

With this post I'd like to try it out. We need this for our projects in the YOO, the company I work for and I'm really curious about how this is going to work in a real project

## Code Analyzers

To try it out, I'm going to use the Weather Stats App I created in previous posts. Feel free [to clone it from GitHub](https://github.com/JuergenGutsch/weatherstats-demo/) and follow the steps I do within this post.

At first you need to add a NuGet package:

[Microsoft.CodeAnalysis.CSharp.CodeStyle](https://dotnet.myget.org/feed/roslyn/package/nuget/Microsoft.CodeAnalysis.CSharp.CodeStyle)

This is currently a development version and hosted on MyGet. This needs you to follow the installation instructions on MyGet. Currently it is the following .NET CLI command:

~~~ shell
dotnet add package Microsoft.CodeAnalysis.CSharp.CodeStyle --version 3.8.0-1.20330.5 --source https://dotnet.myget.org/F/roslyn/api/v3/index.json
~~~

The version number might change in the future. Currently I use the version 3.8.0-1.20330.5 which is out since June 30th.

You need to execute this command for every project in your solution.

After executing this command you'll have the following new lines in the project files:

~~~xml
<PackageReference Include="Microsoft.CodeAnalysis.CSharp.CodeStyle" Version="3.8.0-1.20330.5">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
</PackageReference>
~~~

If not just copy this line into the project file and run `dotnet restore` to actually load the package.

This should be enough to get it running. 

### Adding coding style errors

To try it out I need to add some coding style errors. I simply added some these:

![]({{site.baseurl}}/img/editorconfig/errors.png)

### Roslyn conflicts

Maybe you will get a lot of warnings about that an instance of the analyzers cannot be created because of a missing Microsoft.CodeAnalysis 3.6.0 Assembly like this:

` Could not load file or assembly 'Microsoft.CodeAnalysis, Version=3.6.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'.`

This is might strange because the code analysis assemblies should already be available in case Roslyn is used. Actually this error happens, if you do a `dotnet build` while VSCode is running the Roslyn analyzers. Strange but reproduceable. Maybe Roslyn analyzers can only run once at the same time.

To get it running without those warnings, you can simply close VSCode or wait for a few seconds.

## Get it running

Actually it didn't work in my machine the first times. The reason was that I forgot to update the `global.json`. I still used a 3.0 runtime to run the analyzers. This doesn't work.

After updating the `global.json` to a 5.0 runtime (preview 6 in my case) it failed as expected:

![]({{site.baseurl}}/img/editorconfig/failedbuild.png)

Since the migration of the IDE analyzers to Roslyn analyzers is half done, not all of the errors will fail the build. This is why the the IDE0003 rule doesn't appear here. I used the `this` keyword twice in the code above, that should also fail the build.

## Conclusion

Actually I was wondering why Microsoft didn't start earlier to convert the VS2019 analyzers into Roslyn code analyzers. This is really valuable for teams where developers use VSCode, VS2019, VS for Mac or any other tool to write .NET Core applications. It is not only about showing coding style errors in an editor, it should also fail the build in case coding style errors are checked in. 

Anyway, it is working Good. And hopefully Microsoft will complete the set of analyzers as soon as possible. 