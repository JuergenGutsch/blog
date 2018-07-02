---
layout: post
title: "Configuring SSL in ASP.NET Core 2.1"
teaser: "description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- SSL
- HTTPS
---

Security is a big thing in ASP.NET Core 2.1 projects. So by default Kestrel is configured to provide an endpoint for https and a second one for https. You will see this in the console output by running an newly created ASP.NET Core 2.1 application:

![](../img/aspnetcore-ssl/dotnet-run-ssl.png)

There is https://localhost:5001 and http://localhost:5000

If you go to the Configure method in the startup.cs there are some new middlewares used to prepare this web to use https:

In the Production and Staging environment mode there is this middleware:

~~~ csharp
app.UseHsts();
~~~

This enables HSTS, which is a HTTP/2 feature to avoid man-in-the-middle attacks. It tells the browser to cache the certificate for the specific host-header for a specific time. If the certificate changes before the time range ends, something is wrong with the page. (more about HSTS)

The next new middleware redirects requests without https to use the https version:

~~~ csharp
app.UseHttpsRedirection();
~~~

If you call http://localhost:5000, you gets redirected to https://localhost:5001. This makes sense if you want to enforce https.

So from the ASP.NET Core perspective all is done to run the web using HTTPS. Unfortunately the Certificate is missing. For the production mode you need to buy a valid trusted certificate and to install it in the windows certificate store. In the Development mode, you are able to create a development certificate using Visual Studio 2017.

