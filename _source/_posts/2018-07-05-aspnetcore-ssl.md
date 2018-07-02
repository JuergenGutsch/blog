---
layout: post
title: "Configuring HTTPS in ASP.NET Core 2.1"
teaser: "Finally HTTPS gets into ASP.NET Core. It was there before back in 1.1, but was kinda tricky to configure. It was available in 2.0 bit not configured by default. Now it is part of the default configuration and pretty much visible and present to the developers who will create a new ASP.NET Core 2.1 project."
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

Open a console and CD to your favorite location to play around with new projects. It is `C:\git\aspnet\` in my case. 

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

If you call http://localhost:5000, you get redirected immediately to https://localhost:5001. This makes sense if you want to enforce HTTPS.

So from the ASP.NET Core perspective all is done to run the web using HTTPS. Unfortunately the Certificate is missing. For the production mode you need to buy a valid trusted certificate and to install it in the windows certificate store. For the Development mode, you are able to create a development certificate using Visual Studio 2017 or the .NET CLI. VS 2017 is creating a certificate for you automatically. 

Using the .NET CLI tool "dev-certs" you are able to manage your development certificates, like exporting them, cleaning all development certificates, trusting the current one and so on. Just time the following command to get more detailed information:

~~~shell
dotnet dev-certs https --help
~~~

On my machine I trusted the development certificate to not get the ugly error screen in the browser about an untrusted certificate and an unsecure connection every time I want to debug a ASP.NET Core application. This works quite well. 

On Windows you should use the certificate store to register HTTPS certificated. This is the most secured way on Windows machines. But I also like the idea to store the password protected certificate directly in the web folder or somewhere on the web server. This makes it pretty easy to deploy the application to different platforms, because Linux and Mac use different ways to store the certificated. Fortunately there is a way in ASP.NET Core to create a HTTPS connection using a file certificate which is stored on the hard drive. ASP.NET Core is completely customizable. If you want to replace the default certification handling, feel free to do it.

To change the default handling, open the `Program.cs` and take a quick look at the code, especially to the method `CreateWebHostBuilder`:

```csharp
public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
    .UseStartup<Startup>();
```
This method creates the default WebHostBuilder. This has a lot of stuff preconfigured, which is working great in the most scenarios. But it is possible to override all of the default settings here and to replace it with some custom configurations. Wee need to tell the Kestrel webserver which host and port he need to listen on and we are able to configure ListenOptions for specific ports. In this ListenOption we can use Https and pass in the certificate file and a password for that file:

~~~csharp
public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .UseKestrel(options =>
        {
            options.Listen(IPAddress.Loopback, 5000);
            options.Listen(IPAddress.Loopback, 5001, listenOptions =>
            {
                listenOptions.UseHttps("certificate.pfx", "topsecret");
            });
        })
        .UseStartup<Startup>();
~~~

Usually we would use values from a configuration file or environment variables, instead of hardcoding it. 





