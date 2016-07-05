--- 
layout: post
title: "How to continuously deploy a ASP.​NET Core 1.0 web app to Microsoft Azure"
teaser: "In this post I'm going to show you, how we setup a continuous deployment stuff for a ASP.NET Core 1.0 project. I wont use the direct deployment to an Azure Web App from a git repository because of some reasons."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- Azure
- Continuous Deployment
- FAKE
- Jenkins
---

We started the first real world project with ASP.NET Core RC2 a month ago and we learned a lot of new stuff around ASP.NET Core 
- Continuous Deployment to an Azure Web App
- Token based authentication with Angular2
- Setup Angular2 & TypeScript in a ASP.NET Core project
- Entity Framework Core setup and initial database seeding


In this post, I'm going to show you how we setup a continuous deployment stuff for a ASP.NET Core 1.0 project, without tackling TypeScript and Angular2. Please Remember: The tooling around .NET Core and ASP.NET Core is still in "preview" and will definitely change until RTM. I'll try to keep this post up-to-date. I wont use the direct deployment to an Azure Web App from a git repository because of some reasons, I [mentioned in a previous post] .

I will write some more lines about the other learned stuff in one of the next posts. 

## Let's start with the build
Building is the easiest part of the entire deployment process. To build a ASP.NET Core 1.0, solution you are able to use `MSBuild.exe`. Just pass the solution file to MSBuild and it will build all projects in the solution.

The `*.xproj` files use specific targets, which will wrap and use the dotnet CLI. You are also able to use the dotnet CLI directly. Just call `dotnet build` for each project, or just simpler: call `dotnet build` in the solution folder and the tools will recursively go threw all sub-folders, to look for `project.json` files and build all the projects in the right build order.

Usually I define an output path to build all the projects into a specific folder. This makes it a lot easier for the next step:

## Test the code
Some months ago, I wrote about unit testing DNX libraries ([Xunit]({% post_url unit-testing-dnx-libraries-with-xunit.md %}), [NUnit]({% post_url unit-testing-dnx-libraries-with-nunit.md %})). This didn't really change in .NET Core 1.0. Depending on the Test Framework, a test library could be a console application, which can be called directly. In other cases the test runner is called, which gets the test libraries passed as arguments. We use NUnit to create our unit tests, which doesn't provide a separate runner yet for .NET Core. All of the test libraries are console apps and will build to a `.exe` file. So we are searching the build output folder for our test libraries and call them one by one. We also pass the test output file name to that libraries, to get detailed test results.

This is pretty much all to run the unit tests.

## Throw it to the clouds
Deployment was a little more tricky. But we learned how to do it, from the Visual Studio output. If you do a manual publish with Visual Studio, the output window will tell you how the deployment needs to be done. This are just two steps:

###1. publish to a specific folder using the "dotnet publish" command
We are calling `dotnet publish` with this arguments:

~~~ csharp
Shell.Exec("dotnet", "publish \"" + webPath + "\" --framework net461 --output \"" + 
    publishFolder + "\" --configuration " + buildConf, ".");
~~~
- `webPath` contains the path to the web project which needs to be deployed
- `publishFolder` is the publish target folder
- `buildConf` defines the Debug or Release build (we build with Debug in dev environments)


###2. use msdeploy.exe to publish the complete publish folder to a remote machine. 
The remote machine in our case, is an instance of an Azure Web App, but could also be any other target machine. `msdeploy.exe` is not a new tool, but is still working, even with ASP.NET Core 1.0.

So we just need to call msdeploy.exe like this:
~~~ csharp
Shell.Exec(msdeploy, "-source:contentPath=\"" + publishFolder + "\" -dest:contentPath=" + 
    publishWebName + ",ComputerName=" + computerName + ",UserName=" + username + 
    ",Password=" + publishPassword + ",IncludeAcls='False',AuthType='Basic' -verb:sync -" + 
    "enablerule:AppOffline -enableRule:DoNotDeleteRule -retryAttempts:20",".")
~~~

- `msdeploy` containes the path to the msdeploy.exe which is usually `C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe`.
- `publishFolder` is the publish target folder from the previous command.
- `publishWebName` is the name of the Azure Web App name, which also is the target content path.
- `computername` is the name/URL of the remote machine. In our case `"https://" + publishWebName + ".scm.azurewebsites.net/msdeploy.axd"`
- `username` and `password` are the deployment credentials. the password is hashed, as in the publish profile that you can download from Azure. Just copy paste the hashed password.

## conclusion
I didn't mention all the work that needs to be done to prepare the web app. We also use Angular2 with TypeScript. So we also need to get all the NPM dependencies, we need to move the needed files to the `wwwroot` folder and we need to bundle and to minify all the JavaScript files. This is also done in our build & deployment chain. But in this post, it should be enough to describe just the basic steps for a usual ASP.NET Core 1.0 app.