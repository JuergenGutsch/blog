---
layout: post
title: "Customizing ASP.NET Core Part 04: HTTPS"
teaser: "HTTPS is on by default now and a first class feature. On Windows the certificate which is needed to enable HTTPS is loaded from the windows certificate store. If you create a project on Linux and Mac the certificate is loaded from a certificate file."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- Unit Test
- XUnit
- MSTest
---

HTTPS is on by default now and a first class feature. On Windows the certificate which is needed to enable HTTPS is loaded from the windows certificate store. If you create a project on Linux and Mac the certificate is loaded from a certificate file. 

> Even if you want to create a project to run it behind and IIS or an NGinX webserver HTTPS is enabled. Usually you would manage the certificate on the IIS or NGinX webserver in that case. But this shouldn't be a problem and you shouldn't disable HTTPS in the ASP.NET Core settings.
>
> To manage the certificate within the ASP.NET Core application directly makes sense if you run services behind the firewall, services which are not accessible from the internet. Services like background services for a micro service based applications, or services in a self hosted ASP.NET Core application.

There are some scenarios where it makes sense to also load the certificate from a file on Windows. This could be in an application that you will run on docker for Windows, and also on docker for Linux.

Personally I like the flexible way to load the certificate from a file.

## The series topics

- [Customizing ASP.NET Core Part 01: Logging]({% post_url customizing-aspnetcore-01-logging.md %})
- [Customizing ASP.NET Core Part 02: Configuration]({% post_url customizing-aspnetcore-02-configuration.md %})
- [Customizing ASP.NET Core Part 03: Dependency Injection]({% post_url customizing-aspnetcore-02-dependency-injection.md %})
- Customizing ASP.NET Core Part 04: HTTPS
- Customizing ASP.NET Core Part 05: HostedServices
- Customizing ASP.NET Core Part 06: MiddleWares
- Customizing ASP.NET Core Part 07: OutputFormatter
- Customizing ASP.NET Core Part 08: ModelBinder
- Customizing ASP.NET Core Part 09: ActionFilter
- Customizing ASP.NET Core Part 10: TagHelpers

## Setup Kestrel