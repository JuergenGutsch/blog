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

~~~ html
<!-- old fashioned HtmlHelper -->
<li>@Html.Link("Home", "Index", "Home")</li>
<!-- new TagHelper -->
<li><a asp-controller="Home" asp-action="Index">Home</a></li>
~~~

The HtmlHelper are knida strange between the HTML tags, for HTML developers. It is hard to read code. and kinds disturbing and interrupting while reading the code. It is maybe not for ASP.NET Core developers who are used to read that kind of code. But compared to the TagHelpers it really is ugly. The TagHelpers feel more natural and more like HTML even if they are not and even if they are getting rendered on the server. 

Many of the HtmlHelper can be replaced with a TagHelper. 

There are also some new tags built with TagHelpers. Tags that are not existing in HTML, but look like HTML. One example is the EnvironmentTagHelper:

~~~ html
<environment include="Development">
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.css" />
    <link rel="stylesheet" href="~/css/site.css" />
</environment>
<environment exclude="Development">
    <link rel="stylesheet" href="https://ajax.aspnetcdn.com/ajax/bootstrap/3.3.7/css/bootstrap.min.css"
            asp-fallback-href="~/lib/bootstrap/dist/css/bootstrap.min.css"
            asp-fallback-test-class="sr-only" asp-fallback-test-property="position" asp-fallback-test-value="absolute" />
    <link rel="stylesheet" href="~/css/site.min.css" asp-append-version="true" />
</environment>
~~~

This TagHelper renders or doesn't render the contents depending of the current runtime environment. In this case the target environment is the development mode. The first environment tag renders the contents if the current runtime environment is set to Development and the second one renders the contents if it not set to Development. This makes it a useful helper to render debugable scripts or styles in development mode and minified and optimized code in any other runtime environment.

## Creating custom TagHelpers






