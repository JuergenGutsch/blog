--- 
layout: post
title: "Moving .NET libraries to .NET Core"
teaser: "Moving .NET libraries to .NET Core is a bit tricky and you need to know .NET Core and portable libraries. In this post I'll tell you how to move your .NET libary to .NET Core using my own open source project 'Simple Object Store' as an example."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- Libraries
- DNX
---

At the MVP Summit 2015 [Albert Weinert](http://blog.der-albert.com/) told us that ASP.NET has a huge problem: Almost all of our favorite tools are not usable with .NET Core. Many unit test frameworks, IoC containers,  almost all mocking frameworks will not work with .NET Core and needs to be moved to DNX libraries to get portable. Additionally almost all mocking frameworks are based on the Castle.Core library, which also needs to be moved to be portable.

Currently I'm working at [LightCore](http://lightcore.ch) to make it compatible to .NET Core, to make the world a little better ;) Hopefully. This needs some steps to do. More details are in a separate blog post about [LightCore 2.0]({% post_url lightcore20.md %}). Because the unit tests of LightCore don't use mocking tools this was easier than expected.

With this post I want to tell you, what you need to do to move your library to .NET Core. I will use the 'Simple Object Store' to make a step by step tutorial. At the end my open source library will be compatible with .NET Core :)

> ### But why should I do this? Is this future proof? Does the effort make sense?
> At first I need to know that DNX projects (that's the name of the .NET Core libraries) are a kind of portable libraries. The only difference is that portable class libraries building a single assembly and DNX projects creating a NuGet package. All the other stuff is equal. This means the the libraries are compatible to many different platforms and frameworks. If I build a DNX library, I can use this library in .NET Core, .NET Framework, UWP and Mono. This should answer the questions above. 

## The current state of the Simple Object Store

Currently the solution includes many framework specific projects with linked source files to build against different Framework versions. This needs to be replaced with one single DNX project. I don't want to support frameworks lower than .NET 4.0 (Please tell me if you need a build for a lower version than 4.0.) 

That means I have four projects for the SimpleObjectStore and the same number of projects for the AzureStorageProviders. And I have two test projects, one for the main library and one for the providers library.

The goal is to have four different libraries instead of 10.

## Step 1: Convert the main library

In the Solution I remove the main library and create a new DNX library with the same name. I need to rename old the project folder before. I Add all the existing code files into the new DNX project After that I need to add the frameworks I want to support and to update the dependencies in the project.json to get the project building.

## Step 2: Convert the providers library

To get the AzureStorageProviders library running on .NET Core I have to do exactly the same for as for the main library. Additionally I need to add a reference to the main library. To get the right reference I have to add a dependency to the main project without a version number. (If I would add a version number, the build look for an existing NuGet package on nuget.org)

## Step 3: Converting the unit test projects

Currently I'm using NUnit to test the SimpleObjectStore. I need to decide whether to change to Xunit or to use the new NUnit 3.0.0 portable build.

I'll give the new NUnit a try. In the tutorial about using the portable build, they show the way to use a DNX console application to create a test project. I disagree with that. I would like to have a separate DNX console application as a NUnit runner. This should work in the same way as the Xunit runner. I just created it in a separate project.

Because I have the separate runner I can use the same way as in Step 1 to create DNX libraries for the test projects. Additionally I add a reference to the NUnit runner and add a command called test, which runs the Runner and passes the current test library.

~~~ json
"commands": {
	"test": "nunit.runner.dnx"
}
~~~

(I use the NUnit namespace because I want to contribute this runner to the NUnit project. I use it here as a kind of dog-fooding to test the runner.)

If this is done, we need to get this projects compiled. I did this, by try and error, building, fixing, building, fixing, and so on... The old NUnit API is almost equal to the new NUnit 3.0.0 API and there is less to do than expected.

## Step 4: Add a CI server

To get this compiled and published I also use AppVeyor as a favorite CI server i the same way as written in the [last post about Building LightCore 2.0](/2015/11/17/build-lightcore-with-appveyor.html)

## Final words

Hopefully this post will help you to make your libraries running on .NET Core and any other  platform and framework. As you can see this isn't really a big deal. You need to know some small things about DNX libraries to create packages which are targeting as many platforms as possible. From my point of view, with the new possibilities given by .NET Core it is really important to get ready to go the same way as Microsoft. Prepare your .NET libraries to get also used on Linux and Mac. That's pretty awesome. Have you really thought about that a few years ago? ;)

A pretty detailed tutorial about how to move libraries to DNX was written by Marc Gravell: [http://blog.marcgravell.com/2015/11/the-road-to-dnx-part-1.html](http://blog.marcgravell.com/2015/11/the-road-to-dnx-part-1.html)