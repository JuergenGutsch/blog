---
layout: post
title: "Configuring HTTPS in ASP.NET Core 2.1"
teaser: "description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- SSL
- HTTPS
---

Finally HTTPS gets into ASP.NET Core. It was there before back in 1.1, but was kinda tricky to configure. It was available in 2.0 bit not configured by default. Now it is part of the default configuration and pretty much visible and present to the developers who will create a new ASP.NET Core 2.1 project.

So the title of that blog post is pretty much misleading, because you don't need to configure HTTPS. because it already is. So let's have a look how it is configured and how it can be customized. First create a new ASP.NET Core 2.1 web application.

> Did you already install the latest .NET Core SDK? If not, go to [https://dot.net/](https://dot.net/) to download and install the latest version for your platform.

Open a console and CD to your favorite location to play around with new projects. It is C:\git\aspnet\ in my case. 

~~~ shell
mkdir HttpsSecureWeb && cd HttpSecureWeb
dotnet new mvc -n HttpSecureWeb -o HttpSecureWeb
dotnet run
~~~

This commands will create and run a new application called HttpSecureWeb. And you will see HTTPS the first time in the console output by running an newly created ASP.NET Core 2.1 application:

![](../img/aspnetcore-ssl/dotnet-run-ssl.png)

There are two different URLs where Kestrel is listening on: https://localhost:5001 and http://localhost:5000

If you go to the Configure method in the `Startup.cs` there are some new middlewares used to prepare this web to use https:

In the Production and Staging environment mode there is this middleware:

~~~ csharp
app.UseHsts();
~~~

This enables HSTS, which is a HTTP/2 feature to avoid man-in-the-middle attacks. It tells the browser to cache the certificate for the specific host-headers for a specific time range. If the certificate changes before the time range ends, something is wrong with the page. (more about HSTS)

The next new middleware redirects all requests without HTTPS to use the HTTPS version:

~~~ csharp
app.UseHttpsRedirection();
~~~

If you call http://localhost:5000, you get redirected to https://localhost:5001. This makes sense if you want to enforce HTTPS.

So from the ASP.NET Core perspective all is done to run the web using HTTPS. Unfortunately the Certificate is missing. For the production mode you need to buy a valid trusted certificate and to install it in the windows certificate store. For the Development mode, you are able to create a development certificate using Visual Studio 2017 or the .NET CLI. VS 2017 is creating a certificate for you automatically. 

Using the .NET CLI tool "dev-certs" you are able to manage your development certificates, like exporting them, cleaning all development certificates, trusting the current one and so on. Just time the following command to get more detailed information:

~~~shell
dotnet dev-certs https --help
~~~

On my machine I trusted the development certificate to not get the ugly error screen in the browser about an untrusted certificate and an unsecure connection. This works quite well. 

On Windows you should use the certificate store to register HTTPS certificated. This is the most secured way on Windows machines. But I also like the idea to store the password protected certificate directly in the web folder or somewhere on the web server. This makes it pretty easy to deploy the application to different platforms, because Linux and Mac use different ways to store the certificated. Fortunately there is a way in ASP.NET Core to create a HTTPS connection using a file certificate which is stored on the hard drive.



