---
layout: post
title: "Another GraphQL library for ASP.​NET Core"
teaser: "I recently read a interesting tweet by Glenn Block about a GraphQL app running on the Linux Subsystem for Windows. He uses another cool GraphQL library for ASP.NET Core"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- Unit Test
- XUnit
- MSTest
---

I recently read a interesting tweet by Glenn Block about a GraphQL app running on the Linux Subsystem for Windows:

[![]({{site.baseurl}}/img/graphql-dotnet/gblocktweet.PNG)](https://twitter.com/gblock/status/958643692247564294)

It is impressive to run a .NET Core app in Linux on Windows, which is not a Virtual Machine on Windows. I never hat the chance to try that. I just played a little bit with the Linux Subsystem for Windows. The second that came to mind was: "wow, did he use my GraphQL Middleware library or something else?"

He uses different libraries, as you can see in his repository on GitHub: [https://github.com/glennblock/orders-graphql](https://github.com/glennblock/orders-graphql) 

* GraphQL.Server.Transports.AspNetCore
* GraphQL.Server.Transports.WebSockets

This libraries are built by the makers of graphql-dotnet. The project is hosted in the graphql-dotnet organization on GitHub: [https://github.com/graphql-dotnet/server](https://github.com/graphql-dotnet/server). They also provide a Middleware that can be used in ASP.NET Core projects. The cool thing about that project is a WebSocket endpoint for GraphQL.

## What about the GraphQL middleware I wrote? 

Because my [GraphQL middleware]({% post_url graphql-middleware-for-aspnetcore.md %}), is also based on graphql-dotnet, I'm not yet sure whether to continue maintaining this middleware or to retire this project. I'm not yet sure what to do, but I'll try the other implementation to find out more.

I'm pretty sure the contributors of the graphql-dotnet project know a lot more about GraphQL and there library, than I do. Both project will work the same way and will return the same result - hopefully. The only difference is the API and the configuration. The only reason to continue working on the project is to learn more about GraphQL or to maybe provide a better API ;-)

If I retire my project, I would try to contribute to the graphql-dotnet projects.

What do you think?  Drop me a comment and tell me.