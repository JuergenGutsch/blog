---
layout: post
title: "Book Review: ASP.​NET Core 2 and Angular 5"
teaser: "Last fall, I did my first technical review of a book written by Valerio De Sanctis, called 'ASP.NET Core 2 and Angular 5'. This book is about to use Visual Studio 2017 to create a Single Page Application using ASP.NET Core and Angular."
author: "Jürgen Gutsch"
comments: true
image: /img/bookreview/aspnetcoreangular.jpg
tags: 
- ASP.NET Core
- Angular
- XUnit
- MSTest
---

Last fall, I did my first technical review of a book written by Valerio De Sanctis, called [ASP.NET Core 2 and Angular 5](http://amzn.to/2r90Zoo). This book is about to use Visual Studio 2017 to create a Single Page Application using ASP.NET Core and Angular. 

## About this book

The full title is "[ASP.NET Core 2 and Angular 5: Full-Stack Web Development with .NET Core and Angular](http://amzn.to/2r90Zoo)" and was published by [PacktPub](https://www.packtpub.com/application-development/aspnet-core-2-and-angular-5) and also available on [Amazon](http://amzn.to/2r90Zoo). It is available as a printed version and via various e-book formats.

This book doesn't cover both technologies in deep, but gives you a good introduction on how both technologies are working together. It leads you step by step from the initial setup to the finished application. Don't expect a book for expert developers. But this book is great for ASP.NET Developers who want to start with ASP.NET Core and Angular. This book is a step by step tutorial to create all parts of an Application that manages tests, its questions, answers and results. It describes the database as well as the Web APIs, the Angular parts and the HTML, the authentication and finally the deployment to a web server.

Valerio uses the Angular based SPA project , which is available in Visual Studio 2017 and the .NET Core 2.0 SDK. This project template is not the best solution for bigger projects, but but it fits good for small size projects as described in this book.

## About the technical review

It was my first technical review of an entire book. It was kinda fun to do this. I'm pretty sure it was a pretty hard job for Valerio, because the technologies changed while he was working on the chapters. ASP.NET Core 2.0 was released after he finished four or five chapter and he needed to rewrite those chapters. He changed the whole Angular integration into the ASP.NET Project, because of the new Angular SPA project template. Also Angular 5 came out during writing. Fortunately there wasn't so much relevant changes between, version 4 and version 5. In know this issues, about writing good contents, while technology changes. I did a article series for a developer magazine about ASP.NET Core and Angular 2 and both ASP.NET Core and Angular changes many times. And changes again right after I finished the articles. I rewrote that stuff a lot and worked almost six months on only three articles. Even my Angular posts in this blog are pretty much outdated and don't work anymore with the latest versions. 

Kudos to Valerio, he really did a great job.

I got one chapter by another to review. My job wasn't just to read the chapters, but also to find logical errors, mistakes that will possibly confuse the readers and also to find not working code parts. I followed the chapters as written by Valerio to build this sample application. I followed all instructions and samples to find errors. I reported a lot of errors, I think. And I'm sure that all of them where removed. After I finished the review of the last chapter, I also finished the coding and got a running application deployed on a webserver.

![]({{site.baseurl}}/img/bookreview/aspnetcoreangular.jpg)

## Readers reviews on Amazon and PacktPub

I just had a look into the readers reviews on Amazon and PacktPub. There are not so much reviews done currently, but unfortunately there are (currently) 4 out of 9 reviews talking about errors in the code samples. Mostly about errors in the client side Angular code. This is a lot IMHO. This turns me sadly. And I really apologize that. I was pretty sure I found almost all mistakes, maybe at least those errors that prevents a running application. Because I got it running at the end. Additionally I wasn't the only technical review. There was Ramchandra Vellanki who also did a great job, for sure.

What happened that some readers found errors? Two reasons I thought about first:

1. The readers didn't follow the instructions really carefully. Especially experienced developers really know how it works or how it should work in their perspective. They don't read exactly, because they seem to know where the way goes. I did so as well during the first three or four chapters and I needed to start again from the beginning. 
2. Dependencies were changing since the book was published. Especially if the package versions inside the package.json were not fixed to a specific version. `npm install` then loads the latest version, which may contain breaking changes. The package.json in the book has fixed version, but the sources on GitHub doesn't.

I'm pretty sure there are some errors left in the codes, but at the end the application should run.

Also there are conceptual differences. While writing about Angular and ASP.NET Core and while working with it, I learned a lot and from my current point of view, I would not host an Angular app inside an ASP.NET Core application anymore. (Maybe I'll think about doing that in a really small application.) Anyway, there is that ASP.NET Core Angular SPA project and it is really easy to setup a SPA using this. So, why not using this project template to describe the concepts and interaction of Angular and ASP.NET Core? This keeps the book simple and short for beginners. 

## Conclusion

I would definitely do a technical review again, if needed. As I said, it is fun and an honor to help an author to write a book like this. 

Too bad, that some readers struggle about errors anyway and couldn't get the code running. But writing a book is hard work. And we developers all know, that no application is really bug free, so even a book about quickly changing technologies cannot be free of errors.

