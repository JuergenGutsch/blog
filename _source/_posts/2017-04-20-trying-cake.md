---
layout: post
title: "A first glimpse into CAKE"
teaser: "Since a couple of years I use FAKE to configure my builds. Also at the YooApps we use FAKE in some projects. One of the projects uses it since more than two years. FAKE is really great and I love to use it, but there is one problem with it: The most C# devs don't really like to use new things. The worst case for the most devs - it seems - is a new tool, that uses an exotic language like F#."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- CAKE
- FAKE
- Build
---

Since a couple of years I use [FAKE](fsharp.github.io/FAKE/) (C# Make) to configure my builds. Also at the [YooApps](http://yooapps.com) we use FAKE in some projects. One of the projects uses it since more than two years. FAKE is really great and I love to use it, but there is one problem with it: The most C# developers don't really like to use new things. The worst case for the most C# developers - it seems - is a new tool, that uses an exotic language like F#. 

This is why I have to maintain the FAKE build scripts, since I introduced FAKE to the team.

It is that new tool and the F# language that must be scary for them, even if they don't really need to learn F# for the most common scenarios. That's why I asked the fellow developers to use [CAKE](http://cakebuild.net/) (C# Make). 

* It is C# make instead of F# make
* It looks pretty similar
* It works the same way
* It is a scripting language
* It works almost everywhere

They really liked the idea to use CAKE. Why? just because of C#? It seems so...

It doesn't really makes sense to me, but anyway, it makes absolutely sense that the developers need to use and to maintain there own build configurations. 

![]({{ site.baseurl }}/img/cake/cake.png)

## How does CAKE work?

CAKE is built using a C# scripting language. It uses the Roslyn compiler to compile the scripts. Instead of using batch files, as FAKE does, it uses a PowerShell script (build.ps1) to bootstrap itself and to run the build script. The bootstrapping step loads CAKE and some dependencies using NuGet. The last step the PowerShell script does, is to call the cake.exe and to execute the build script.

The bootstrapping needs network access, to load all the stuff. It also loads the nuget.exe, if it's not available. If you don't like this, you can also commit the loaded dependencies to the source code repository.

The documentation is great. Just follow the [getting-started guide](http://cakebuild.net/docs/tutorials/getting-started) to get a working example. There's also a nice documentation about [setting up a new project](http://cakebuild.net/docs/tutorials/setting-up-a-new-project) available.

## Configuring the build

If you know FAKE or even MSBuild it will look pretty familiar to you. Let's have a quick look into the first simple example of the getting started guide:

~~~ csharp
var target = Argument("target", "Default");

Task("Default")
  .Does(() =>
        {
          Information("Hello World!");
        });

RunTarget(target);
~~~

The first line retrieves the build target to execute from the command line arguments. Starting from line 3 we see a definition of a build target. This target just prints a "Hello World!" as a information message.

The last line starts the initial target by its name.

A more concrete code sample is the build script from the CAKE example (I removed some lines in this listing to get a shorter example):

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

This script uses another NuGet package to run the NUnit3 tests and references it. A nice feature is to configure the NuGet dependency at the beginning of the script. 

This build script contains five targets. The method IsDependentOn("") wires the targets together in the right execution order. This way is a bit different to FAKE and maybe a little bit confusing. It needs to write the targets in the right execution order. If you don't write the script like this, you need to find the initial target and to follow the way back to the very first target. You will read the execution order from the last to the first target. 

FAKE does this a little easier and wires the targets up in a single statement at the end of the file:

~~~ fsharp
// Dependencies
"Clean"
  ==> "Restore-NuGet-Packages"
  ==> "Build"
  ==> "Run-Unit-Tests"
  ==> "Default"
 
RunTargetOrDefault "Default"
~~~

This could possibly look like this dummy code in CAKE:

~~~ csharp
// Dependencies
WireUp("Clean")
  .Calls("Restore-NuGet-Packages")
  .Calls("Build")
  .Calls("Run-Unit-Tests")
  .Calls("Default");

RunTarget(target);
~~~

## Running the build

To run the build, just call ./build.ps1 in a PowerShell console:

![]({{ site.baseurl }}/img/cake/start-build.png)

If you know FAKE the results looks pretty familiar:

![]({{ site.baseurl }}/img/cake/build-result.png)

## Conclusion

Anyway. I think CAKE gets pretty much faster accepted by the fellow developers at the YooApps than FAKE did. Some things will work a little easier in CAKE than in FAKE and some a little different, but the most stuff will work the same way. So it seems it makes sense to switch to use CAKE at the YooApps. So let's use it. :)

I'm sure, I will write down a comparison of FAKE and CAKE later, if I have used it for a few months.