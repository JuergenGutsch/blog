---
layout: post
title: "The ASP.NET Core React Project"
teaser: "description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- React
- Typescript
---

In the last post I had a first look into a plain, clean and lightweight React setup. I'm still impressed how easy the setup is and how fast the loading of a React app really is. Bevore trying to push this setup into a ASP.NET Core application, it would make sense to have a look into the ASP.NET Core React project.

## Create the React project

You can either use the "File New Project ..." dialog in Visual Studio 2017 or the .NET CLI to create a new ASP.NET Core React project:

![]({{site.baseurl}}/img/react-aspnetcore/newproject.PNG)

~~~ shell
dotnet new react -n MyPrettyAwesomeReactApp
~~~

This creates a ready to go React project.

## The first impression

At the first glance I saw the webpack.config.js, which is cool. I love how Webpack works, how it bundles the relevant files recursively and how it saves a lot of time. Also a tsconfig.json is available in the project. This means the React-Code will be written in TypeScript. Webpack compiles the Typescript into JavaScript and bundles it into an output file, called main.js

> Remember: In the last post the JavaScript code was written in ES6 and transpiled using Babel