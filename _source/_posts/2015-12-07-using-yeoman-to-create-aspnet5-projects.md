--- 
layout: post
title: "Using Yeoman to create ASP.NET 5 projects"
teaser: "To quick start a new ASP.NET 5 project, we know the wizards in Visual Studio 2015. But how can you quickly setup a new project on Linux or Mac? This blog post is about Yeoman, a cross platform command line tool to scaffold a new project"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET 5
- .NET Core
- Yeoman
---

To start a ASP.NET 5 application with MVC 6 from scratch, is a bit annoying because it is much to write and always the same work to do. This is why the new project wizards in Visual Studio are still a good thing. But how do we setup a new project on Linux or Mac where we don't have a Visual Studio? VS Code doesn't have something like such wizards.

In the real world - the parallel universe to ASP.NET development, where another kind of web developers are living - there is a pretty cool tool to scaffold new web projects. And since Microsoft is working on ASP.NET 5, they are opening a door to that parallel universe, by using tools from the other side. NPM, Bower, Gulp, Grunt... all of this guys are well known tools in that parallel universe, but pretty new to the ASP.NET web developers. 

## "Welcome to the marvellous ASP.NET 5 generator!"

One of this tools is Yeoman. It works almost like the Visual Studio wizards. It is a small but strong tool scaffold all kind of projects. And Microsoft adds support for ASP.NET 5 projects.

You need to have node.js and NPM installed on your machine to install and use Yeoman:

~~~ batch
npm install -g yo
npm install -g generator-aspnet
~~~

if the installation is done you are able to use the Yeoman aspnet generator even offline:

~~~ batch
yo aspnet
~~~

Use the argument --grunt to use Grunt instead of Gulp. Yeoman is welcome you to the ASP.NET generator wizard:

![]({{ site.baseurl }}/img/yeoman/starting.png)

To select one of the shown templates, you can easily use the arrow keys. For the demos I prefer to use the "Web Application Basic" template out the following:

- Empty Application
- Console Application
- Web Application
- Web Application Basic [without Membership and Authorization]
- Web API Application
- Nancy ASP.NET Application
- Class Library
- Unit test project

This will create you a basic project, as known as from the new project wizard of Visual Studio 2015. The "Empty Application" will create you a project with a startup.cs, project.json, a dockerfile and a wwwroot folder which only contains readme.md and a web.config.

All of the yeoman templates are including a dockerfile to create a Docker image out of it and to to run the application on Docker:

~~~ dockerfile
FROM microsoft/aspnet:1.0.0-rc1

COPY . /app
WORKDIR /app
RUN ["dnu", "restore"]

EXPOSE 5000/tcp
ENTRYPOINT ["dnx", "-p", "project.json", "web"]
~~~

I already wrote about setting up Docker on Windows on my German speaking blog. I'll translate it as soon as possible to add this posts here in the new blog.

After you select a template, Yeoman will ask you for a application name:

![]({{ site.baseurl }}/img/yeoman/naming.png)

This name will used as the application folder name and the default namespace. It will also be used in the title tag in the layout page. Yeoman will show you what files are created and what to do next to start the application:

![]({{ site.baseurl }}/img/yeoman/ending.png)

That's pretty much it. :)

Now you are done setting up your new application and you can start developing using Visual Studio Code or any other tool you want to use.

## Additional generators

The Yeoman ASP.NET generator additionally includes many sub generators to create some spacial files, directly in the working directory. For example you can easily create AngularJS controllers, directives, json configs, views, controllers, plan class files. And many other what you maybe needs. To see a list of all sub generators add the argument --help

~~~ batch
yo aspnet --help
~~~

As you can see, this Yeoman ASP.NET generator and the sub generators are pretty useful to quick setup your ASP.NET project on Linux, Mac and as well on Windows machines.