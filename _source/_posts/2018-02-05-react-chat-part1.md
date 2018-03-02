---
layout: post
title: "Creating a chat application using React and ASP.​NET Core - Part 1"
teaser: "In this blog series I'm going to create a small chat application using React and ASP.NET Core, to learn more about React and to learn how React behaves in an ASP.NET Core project during development and deployment. This topic describes the basic requirements and the project setup."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- React
- TypeScript
---

In this blog series, I'm going to create a small chat application using React and ASP.NET Core, to learn more about React and to learn how React behaves in an ASP.NET Core project during development and deployment. This Series is divided into 5 parts, which should cover all relevant topics:

1. **React Chat Part 1: Requirements & Setup**
2. [React Chat Part 2: Creating the UI & React Components]({% post_url react-chat-part2.md %})
3. [React Chat Part 3: Adding Websockets using SignalR]({% post_url react-chat-part3.md %})
4. [React Chat Part 4: Authentication & Storage]({% post_url react-chat-part4.md %})
5. [React Chat Part 5: Deployment to Azure]({% post_url react-chat-part5.md %})

I also set-up a GitHub repository where you can follow the project: [https://github.com/JuergenGutsch/react-chat-demo](https://github.com/JuergenGutsch/react-chat-demo). Feel free to share your ideas about that topic in the comments below or in issues on GitHub. Because I'm still learning React, please tell me about significant and conceptual errors, by dropping a comment or by creating an Issue on GitHub. Thanks.

## Requirements

I want to create a small chat application that uses React, SignalR and ASP.NET Core 2.0. The frontend should be created using React. The backend serves a Websocket end-point using SignalR and some basic Web API end-points to fetch some initial data, some lookup data and to do the authentication (I'll use IdentityServer4 to do the authentication). The project setup should be based on the Visual Studio React Project I [introduced in one of the last posts]({% post_url react-aspnetcore-project.md %}).

The UI should be clean and easy to use. It should be possible to use the chat without a mouse. So the focus is also on usability and a basic accessibility. We will have a large chat area to display the messages, with an input field for the messages below. The return key should be the primary method to send the message. There's one additional button to select emojis, using the mouse. But basic emojis should also be available using text symbols.

On the left side, I'll create a list of online users. Every new logged on user should be mentioned in the chat area. The user list should be auto updated after a user logs on. We will use SignalR here too.

* User list using SignalR
  * small area on the left hand side
  * Initially fetching the logged on users using Web API 
* Chat area using SignalR
  * large area on the right hand side
  * Initially fetching the last 50 messages using Web API
* Message field below the chat area
  * Enter kay should send the message
  * Emojis using text symbols
* Storing the chat history in a database (using Azure Table Storage)
* Authentication using IdentityServer4

## Project setup

The initial project setup is easy and [already described in one of the last post]({% post_url react-aspnetcore-project.md %}). I'll just do a quick introduction here. 

You can either use visual studio 2017 to create a new project

![]({{site.baseurl}}/img/react-aspnetcore/newproject.PNG)

or the .NET CLI

~~~ shell
dotnet new react -n react-chat-app
~~~

It takes some time to fetch the dependent packages. Especially the NPM packages are a lot. The node_modules folder contains around 10k files and will require 85 MB  on disk.

I also added the "@aspnet/signalr-client": "1.0.0-alpha2-final" to the package.json

> Don'be be confused, with the documentation. In the GitHub repository they wrote the NPM name signalr-client should not longer used and the new name is just signalr. But when I wrote this lines, the package with the new name is not yet available on NPM. So I'm still using the signalr-client package.

After adding that package, an optional dependency wasn't found and the NPM dependency node in Visual Studio will display a yellow exclamation mark. This is annoying and id seems to be an critical error, but it will work anyway:

![]({{site.baseurl}}/img/react-chat-app/npm-signalr.PNG)

The NuGet packages are fine. To use SignalR I used the the Microsoft.AspNetCore.SignalR package with the version 1.0.0-alpha2-final.

The project compiles without errors. And after pressing F5, the app starts as expected.

> Since a while I configured Edge as the start-up browser to run ASP.NET Core projects, because Chrome got very slow. Once the IISExpress or Kestrel is running you can easily use Chrome or any other browser to call and debug the web. Which makes sense, since the React developer tolls are not yet available for Edge and IE.

This is all to setup and to configure. All the preconfigured TypeScript and Webpack stuff is fine and runs as expected. If there's no critical issue, you don't really need to know about it. It just works. I would anyway recommend to learn about the TypeScript configuration and Webpack to be safe.

## Closing words

Now the requirements are clear and the project is set-up. In this series I will not set up an automated build using CAKE. I'll also not write about unit tests. The focus is React, SignalR and ASP.NET Core only.

In the next chapter I'm going build the UI React components and to implement the basic client logic to get the UI working.