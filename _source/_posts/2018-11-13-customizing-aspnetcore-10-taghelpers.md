---
layout: post
title: "Customizing ASP.NET Core Part 10: TagHelpers"
teaser: "description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- TagHelpers
---

This was initially planned as the last topic of this series, because this also was the last part of the talk about customizing ASP.NET Core I did in the past. See the [initial post]({% post_url customizing-aspnetcore-series.md %}) about this series. Now I have three additional customizing topics to talk about. If you like to propose another topic feel free to drop a comment in the initial post.

In this tenth part of this series I'm going to write about TagHelpers. The built in TagHelpers are pretty useful and making the razor more pretty and more readable. Creating custom TagHelpers will make your life much easier.

## This series topics


- [Customizing ASP.NET Core Part 01: Logging]({% post_url customizing-aspnetcore-01-logging.md %})
- [Customizing ASP.NET Core Part 02: Configuration]({% post_url customizing-aspnetcore-02-configuration.md %})
- [Customizing ASP.NET Core Part 03: Dependency Injection]({% post_url customizing-aspnetcore-03-dependency-injection.md %})
- [Customizing ASP.NET Core Part 04: HTTPS]({% post_url customizing-aspnetcore-04-https.md %})
- [Customizing ASP.NET Core Part 05: HostedServices]({% post_url customizing-aspnetcore-05-hostedservices.md %})
- [Customizing ASP.NET Core Part 06: Middlewares]({% post_url customizing-aspnetcore-06-middlewares.md %})
- [Customizing ASP.NET Core Part 07: OutputFormatter]({% post_url customizing-aspnetcore-07-outputformatter.md %})
- [Customizing ASP.NET Core Part 08: ModelBinders]({% post_url customizing-aspnetcore-08-modelbinders.md %})
- [Customizing ASP.NET Core Part 09: ActionFilter]({% post_url customizing-aspnetcore-09-actionfilters.md %})
- **Customizing ASP.NET Core Part 10: TagHelpers - This article**

## About TagHelpers

With TagHelpers you are able to extend existing HTML tags or to create new tags that get rendered on the server side. The extensions or the new tags are not visible in the browsers. TagHelpers a only kind of shortcuts to write easier and less HTML or Razor code on the server side. TagHelpers wil be interpreted on the server and will produce "real" HTML code for the browsers.

TagHelpers are not a new thing in ASP.NET Core, it was there since the first version of ASP.NET Core. The most existing and built-in TagHelpers are a replacement for the old fashioned HTML Helpers, which are still existing and working in ASP.NET Core to keep the Razor views compatible to ASP.NET Core.

A very basic example of extending HTML tags is the built in AnchorTagHelper:

~~~ csharp

~~~








