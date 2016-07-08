--- 
layout: post
title: "How web development changed for me over the last 20 years"
teaser: "A personal retro: The web changed pretty fast within the last 20 years. More and more logic moves from the server side to the client side. More complex JavaScript needs to be written on the client side. And something freaky things happened the last years: JavaScript was moving to the server and web technology was moving to the desktop. This is nothing new, but who was thinking about that 20 years ago?"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- JavaScript
- NodeJS
- VS Code
- Web Development
---

The web changed pretty fast within the last 20 years. More and more logic moves from the server side to the client side. More complex JavaScript needs to be written on the client side. And something freaky things happened the last years: JavaScript was moving to the server and Web technology was moving to the desktop. That is nothing new, but who was thinking about that 20 years ago?

The web changed, but also my technology stack. It seems my stack changed back to the roots.  20 years ago, I started with HTML and JavaScript, moving forward to classic ASP using VBScript. In 2001 I started playing around with ASP.NET and VB.NET and used it in in production until the end of 2006. In 2007 I started writing ASP.NET using C#. HTML and JavaScript was still involved, but more or less wrapped in third party controls and jQuery was an alias for JavaScript that time. All about JavaScript was just jQuery. ASP.NET WebForms felled pretty huge and not really flexible, but it worked. Later - in 2010 - I also did many stuff with SilverLight, WinForms, WPF. 

ASP.NET MVC came up and the web stuff starts to feel little more naturally again, than ASP.NET WebForms. From an ASP.NET developer perspective, the web changed back to get better, more clean, more flexible, more lightweight and even more naturally. 

But there was something new coming up. Things from outside the ASP.NET world. Strong JavaScript libraries, like KnockOut, Backbone and later on Angular and React. The First Single Page Application frameworks (sorry, I don't wanted to mention the crappy ASP.NET Ajax thing...) came up, and the UI logic moves from the server to the client. (Well, we did a pretty cool SPA back in 2005, but we didn't thought about to create a framework out of it.)

NodeJS change the world again, by using JavaScript on the server. You just need two different languages (HTML and JavaScript) to create cool web applications. I didn't really care about NodeJS, except using it in the back, because some tools are based on it. Maybe that was a mistake, who knows... ;)

Now we got ASP.NET Core, which feels a lot more naturally than the classic ASP.NET MVC. 

> Naturally in this case means, it feels almost the same as writing classic ASP. It means using the stateless web and working with the stateless web, instead of trying to fix it. Working with the Request and Response more directly, than with the classic ASP.NET MVC and even more than in ASP.NET WebForms. It doesn't mean to write the same unstructured, crappy shit than with classic ASP. ;)

Since we got the pretty cool client side JavaScript frameworks and simplified, minimalistic server side frameworks, the server part was reduced to just serve static files and to serve data over RESTish services. 

This is the time where it makes sense to have a deeper look into TypeScript. Until now it didn't makes sense to me. I was writing JavaScript for around 20 years, more and less complex scripts, but I never wrote so much JavaScript within a single project, than as I started using AngularJS last years. Angular2 also was the reason to have a deep look into TypeScript, 'cause now it is completely written in Typescript. And it makes absolutely sense to use it.

A few weeks ago I started the first real NodeJS project. A desktop application which uses NodeJS to provide a high flexible scripting run-time for the users. NodeJS provides the functionality and the UI to the users. All written in TypeScript, instead of plain JavaScript. Why? Because TypeScript has a lot of unexpected benefits:

- You are still able to write JavaScript ;)
- It helps you to write small modules and structured code
- it helps you to write NodeJS compatible modules
- In general you don't need to write all the JavaScript overhead code for every module
- You will just focus on the features you need to write

This is why TypeScript got a great benefit to me. Sure a typed language is also useful in many cases, but - working with JS for 20 years - I also like the flexibility of the implicit typed JavaScript and I'm pretty familiar with it. that means, from my perspective the Good thing about TypeScript is, I am still able to write implicit typed code in TypeScript and to use the flexibility of JavaScript. This is why I wrote "You are still able to write JavaScript"

The web technology changed, my technology stack changed and the tooling changed. All the stuff goes more lightweight, even the tools. The console comes back and the IDEs changed back to the roots: Just being text editors with some benefits like syntax highlighting and IntelliSense. Currently I prefer to use the "Swiss army knife" Visual Studio Code or Adobe Brackets, depending on the type of project. Both are starting pretty fast and include nice features. 

Using that light weight IDEs is pure fun. Everything is fast, because the machines resource could be used by the apps I need to develop, instead by the IDE I need to use to develop the apps. This makes development a lot faster.

Starting the IDE today means, starting cmder (my favorite console on windows). changing to the project folder, starting a console command to watch the typescript files, to compile after save. Starting another console to use the tools like NPM, gulp, typings, dotnet CLI, NodeJS, and so on. Starting my favorite light weight editor to write some code.  :)