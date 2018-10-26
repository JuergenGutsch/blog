---
layout: post
title: "Customizing ASP.NET Core Part 09: ActionFilter"
teaser: "We keep on customizing on the controller level in this ninth post of this series. I'll have a look into ActionFilters and hot to create your own ActionFilter to keep your Actions small and readable."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- Unit Test
- XUnit
- MSTest
---

A little late this time. My initial plan was to throw out two posts of this series per week, but this doesn't work out, since there are sometimes some more family and work tasks to do than expected. 

Anyway, we keep on customizing on the controller level in this ninth post of this series. I'll have a look into ActionFilters and hot to create your own ActionFilter to keep your Actions small and readable.

## Initial series topics

- [Customizing ASP.NET Core Part 01: Logging]({% post_url customizing-aspnetcore-01-logging.md %})
- [Customizing ASP.NET Core Part 02: Configuration]({% post_url customizing-aspnetcore-02-configuration.md %})
- [Customizing ASP.NET Core Part 03: Dependency Injection]({% post_url customizing-aspnetcore-03-dependency-injection.md %})
- [Customizing ASP.NET Core Part 04: HTTPS]({% post_url customizing-aspnetcore-04-https.md %})
- [Customizing ASP.NET Core Part 05: HostedServices]({% post_url customizing-aspnetcore-05-hostedservices.md %})
- [Customizing ASP.NET Core Part 06: Middlewares]({% post_url customizing-aspnetcore-06-middlewares.md %})
- [Customizing ASP.NET Core Part 07: OutputFormatter]({% post_url customizing-aspnetcore-07-outputformatter.md %})
- [Customizing ASP.NET Core Part 08: ModelBinders]({% post_url customizing-aspnetcore-08-modelbinders.md %})
- **Customizing ASP.NET Core Part 09: ActionFilters - This article**
- Customizing ASP.NET Core Part 10: TagHelpers









