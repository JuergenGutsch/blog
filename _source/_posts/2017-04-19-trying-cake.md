---
layout: post
title: "A first glimpse into CAKE"
teaser: "Since a few years I use FAKE to configure my builds. Also at the YooApps we have two projects running using FAKE. One of the projects uses it since more than two years. FAKE is really great and I love to use it, but there is one problem with it: The most C# devs don't really like to use new things. The worst case for the most devs - it seems - is a new tool, that uses an exotic language like F#."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- CAKE
- FAKE
- Build
---

Since a few years I use FAKE to configure my builds. Also at the YooApps we have two projects running using FAKE. One of the projects uses it since more than two years. FAKE is really great and I love to use it, but there is one problem with it: The most C# devs don't really like to use new things. The worst case for the most devs - it seems - is a new tool, that uses an exotic language like F#. 

> This is why I have to maintain the build scripts, since I introduced FAKE to the team.

For the most common scenarios, nobody really needs to learn F# to use FAKE. It is the tool and the F# that must be scary for them. That's why I asked the fellow devs to use CAKE. 

* It is C# make instead of F# make
* It looks pretty similar
* It works the same way
* It is a scripting language
* It works almost everywhere

They really liked the idea to use CAKE. Why? just because of C#? It seems so...

It doesn't really makes sense to me, but anyway. The devs need to use and to maintain there own build configurations. 

## How does CAKE work?

CAKE is build using a C# scripting language. It uses the Roslyn compiler to compile the scripts. Instead of using batch files, as fake does, it uses a PowerShell script to bootstrap itself and to run the build script. The bootstrapping step loads CAKE and some dependencies using NuGet. The last step is to call the cake.exe and to execute the build script.

The bootstrapping needs network access, to load all the stuff and NuGet needs to be available. If you don't like this, you can also commit the loaded dependencies to the source code repository.

The documentation is great. Just follow the [getting-started guide](http://cakebuild.net/docs/tutorials/getting-started) to get a working example. There's also a nice documentation about [setting up a new project](http://cakebuild.net/docs/tutorials/setting-up-a-new-project) available.

## Configuring the build

If you know FAKE or even MSBuild it will looks pretty familiar to you. Let's have a look into the first simple example of the getting started guide:

~~~ csharp
var target = Argument("target", "Default");

Task("Default")
  .Does(() =>
        {
          Information("Hello World!");
        });

RunTarget(target);
~~~

The first line retrieves the build target to execute from the command lines. Starting from line 3 we see a definition of a build target. This target just prints a "Hello World!" as a information message.

The last line starts the "Default" target by its name.

A more concrete code sample is the build scripts from the CAKE example (I removed some lines to get a shorter example):

~~~ csharp
#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

// Define the build directory.
var buildDir = Directory("./src/Example/bin") + Directory(configuration);

Task("Clean")
  .Does(() =>
        {
          CleanDirectory(buildDir);
        });

Task("Restore-NuGet-Packages")
  .IsDependentOn("Clean")
  .Does(() =>
        {
          NuGetRestore("./src/Example.sln");
        });

Task("Build")
  .IsDependentOn("Restore-NuGet-Packages")
  .Does(() =>
        {
          MSBuild("./src/Example.sln", settings =>
                  settings.SetConfiguration(configuration));
        });

Task("Run-Unit-Tests")
  .IsDependentOn("Build")
  .Does(() =>
        {
          NUnit3("./src/**/bin/" + configuration + "/*.Tests.dll", new NUnit3Settings {
            NoResults = true
          });
        });

Task("Default")
  .IsDependentOn("Run-Unit-Tests");

RunTarget(target);
~~~

This script uses another NuGet package to run the NUnit3 tests and references it. This build contains four targets. The method IsDependentOn("") wires the targets together in the right execution order.

This way is a bit different to FAKE and maybe a little bit confusing. It needs to write the targets in the right execution order. If you don't write the script like this, you need to find the initial target and to follow the way back to the very first target. You will read the execution order from the last to the first target.

FAKE does this a little easier. and wires the targets up in a single statement at the end of the file. This should more look like this in CAKE

~~~ csharp
Task("Default")
  .IsDependentOn("Run-Unit-Tests")
  .IsDependentOn("Build")
  .IsDependentOn("Restore-NuGet-Packages")
  .IsDependentOn("Clean");

RunTarget(target);
~~~

Like this it is still the wrong direction but more condensed to a few lines of code.

## Conclusion

Anyway. I think CAKE gets pretty much faster accepted by the fellow devs than FAKE. Some things will work a little easier in CAKE than in FAKE. So it seems it makes sense to switch to use CAKE at the YooApps. 

