---
layout: post
title: "Component based ASP.NET development with Blazor Web"
teaser: "Description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Tests
- Tag
---

Blazor Web is the new app model that completes the Blazor stack. With .NET 7 and earlier we got Blazor WebAssembly that runs in the Browser, Blazor Server that renders on the server but uses a SignalR connection to send the rendered HTML to the client. We also got Blazor Hybrid to run Blazor in a Webview on Desktop and Mobile Apps using .NET MAUI. Blazor Web is also server rendered but without the SignalR connection. It doesn't behave like a Single Page Application and needs a full server roundtrip every time the UI needs to be refreshed like MVC and Razor Pages.

For those of you how know me, also know that I would prefer this classical web-like behavior. I actually like the possibility to run C# code in the browser. I also like SignalR a lot but it feels a bit strange and a little bit wrong to render UIs using this technologies. Actually, it makes sense in some cases, but feels kinda wrong in the internet. However, this is my personal opinion and shouldn't influence your point of view.

At the YOO, we use Angular or React to create frontend UIs and ASP.NET APIs on the backend side. In some cases and for smaller projects we also use server rendered technologies like ASP.NET MVC. Blazor Web could be the new player as an addition or a replacement of MVC. The component based development in Blazor is great and I like it a lot.

Comparing the Blazors

Blazor WebAssembly has a complete different hosting model since it is not rendered on a server. It also uses a progam.cs that looks similar to what we know in other ASP.NET Stacks but it's not.

Blazor Server and Blazor Web look actually quite similar. The main difference is the SignalR connection that is used in Blazor Server to move the server rendered HTML to the client. 

That are really the main differences. The Blazor components are similar in the most cases and can be shared between the different Blazor stacks. Except where you use server features within the components. For example Blazor WebAssebmly would connect to an HTTP Endpoint to fetch data, Blazor Server and Blazor Web can connect directly to the database, Also Authentication will look different. With such scenarios in mind, you would always wrap that topics into services that get injected into the components to build shared or reusable components
