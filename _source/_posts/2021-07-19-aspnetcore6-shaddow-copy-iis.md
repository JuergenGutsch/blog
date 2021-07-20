---
layout: post
title: "ASP.NET Core in .NET 6 - Shadow-copying in IIS"
teaser: "This is the next part of the ASP.NET Core on .NET 6 series. In this post, I'd like to explore the Shadow-copying in IIS."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- .NET 6
- Shadow copy
- IIS
---

This is the next part of the [ASP.NET Core on .NET 6 series]({% post_url aspnetcore6-01.md %}). In this post, I'd like to explore the Shadow-copying in IIS.

Since .NET is locking the assemblies that are running by a process, it is impossible to replace them during an update scenario. Specially in scenarios where you self-host an IIS server or where you need to update an running application via FTP. 

To solve this, Microsoft added a new feature to the ASP.NET Core module for IIS to shadow copy the application assemblies to a specific folder.

## Exploring Shadow-copying in IIS

To enable shadow-copying you need to install the latest preview version of the ASP.NET Core module 

> On a self-hosted IIS server, this requires a new version of the hosting bundle. On Azure App Services, you will be required to install a new ASP.NET Core runtime site extension
> (https://devblogs.microsoft.com/aspnet/asp-net-core-updates-in-net-6-preview-3/#shadow-copying-in-iis)

If you have the requirements ready, you should add a `web.config` to your project or edit the `weg.config` that is created during the publish process (dotnet publish). Since most of us are using continuous integration and can't touch the `web.config` after it gets crated automatically, you should add it to the project. Just copy the one that got created using dotnet publish. Continuous integration will not override an existing `web.config`.

To enable it you will need to add some new `handlerSettings` to the `web.config`:

~~~xml
<aspNetCore processPath="%LAUNCHER_PATH%" arguments="%LAUNCHER_ARGS%" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout">
    <handlerSettings>
        <handlerSetting name="experimentalEnableShadowCopy" value="true" />
        <handlerSetting name="shadowCopyDirectory" value="../ShadowCopyDirectory/" />
    </handlerSettings>
</aspNetCore>
~~~

This enables shadow-copying and specifies the shadow copy directory.

After the changes are deployed, you should be able to update the assemblies of a running application.

## What's next?

In the next part In going to look into the support for `BlazorWebView controls for WPF & Windows Forms` in ASP.NET Core.
