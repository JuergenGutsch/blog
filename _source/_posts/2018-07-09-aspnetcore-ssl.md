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

![]({{ site.baseurl }}img/aspnetcore-ssl/dotnet-run-ssl.png)

There are two different URLs where Kestrel is listening on: https://localhost:5001 and http://localhost:5000

If you go to the Configure method in the `Startup.cs` there are some new middlewares used to prepare this web to use HTTPS:

In the Production and Staging environment mode there is this middleware:

~~~ csharp
app.UseHsts();
~~~

This enables HSTS (HTTP Strinct Transport Protocol), which is a HTTP/2 feature to avoid man-in-the-middle attacks. It tells the browser to cache the certificate for the specific host-headers and for a specific time range. If the certificate changes before the time range ends, something is wrong with the page. ([More about HSTS](https://en.wikipedia.org/wiki/HTTP_Strict_Transport_Security))

The next new middleware redirects all requests without HTTPS to use the HTTPS version:

~~~ csharp
app.UseHttpsRedirection();
~~~

If you call http://localhost:5000, you get redirected immediately to https://localhost:5001. This makes sense if you want to enforce HTTPS.

So from the ASP.NET Core perspective all is done to run the web using HTTPS. Unfortunately the Certificate is missing. For the production mode you need to buy a valid trusted certificate and to install it in the windows certificate store. For the Development mode, you are able to create a development certificate using Visual Studio 2017 or the .NET CLI. VS 2017 is creating a certificate for you automatically. 

Using the .NET CLI tool "dev-certs" you are able to manage your development certificates, like exporting them, cleaning all development certificates, trusting the current one and so on. Just type the following command to get more detailed information:

~~~shell
dotnet dev-certs https --help
~~~

On my machine I trusted the development certificate to not get the ugly error screen in the browser about an untrusted certificate and an unsecure connection every time I want to debug a ASP.NET Core application. This works quite well:

~~~ shell
dotnet dev-cert https --trust
~~~

This command trusts the development certificate, by adding it to the certificate store or to the keychain on Mac. 

On Windows you should use the certificate store to register HTTPS certificated. This is the most secured way on Windows machines. But I also like the idea to store the password protected certificate directly in the web folder or somewhere on the web server. This makes it pretty easy to deploy the application to different platforms, because Linux and Mac use different ways to store the certificated. Fortunately there is a way in ASP.NET Core to create a HTTPS connection using a file certificate which is stored on the hard drive. ASP.NET Core is completely customizable. If you want to replace the default certification handling, feel free to do it.

To change the default handling, open the `Program.cs` and take a quick look at the code, especially to the method `CreateWebHostBuilder`:

```csharp
public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
    .UseStartup<Startup>();
```
This method creates the default WebHostBuilder. This has a lot of stuff preconfigured, which is working great in the most scenarios. But it is possible to override all of the default settings here and to replace it with some custom configurations. We need to tell the Kestrel webserver which host and port he need to listen on and we are able to configure the ListenOptions for specific ports. In this ListenOptions we can use HTTPS and pass in the certificate file and a password for that file:

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

Usually we would use the hardcoded values from a configuration file or environment variables, instead of hardcoding it. 

Be sure the certificate is password protected using a long password or even better a pass-phrase. Be sure to not store the password or the pass-phrase into a configuration file. In development mode you should use the [user secrets]({% post_url user-secrets-in-aspnetcore.md %}) to store such secret date and in production mode the [Azure Key Vault](https://docs.microsoft.com/de-de/azure/key-vault/) could be an option.

## Conclusion

I hope this helps to get you a rough overview over the the usage of HTTPS in ASP.NET Core. This is not really a deep dive, but tries to explain what are the new middlewares good for and how to configure HTTPS for different platforms.

BTW: I just saw in the [blog post about HTTPS improvements](https://blogs.msdn.microsoft.com/webdev/2018/02/27/asp-net-core-2-1-https-improvements), about HSTS in ASP.NET Core, there is a way to store the HTTPS configuration in the launchSettings.json. This is an easy way to pass in environment variables on startup to the application. The samples also shows to add the certificate password to this settings file. Please never ever do this! Because a file is easily shared to a source code repository or any other way, so the password inside is shared as well. Please use different mechanisms to set passwords in an application, like the already mentioned  [user secrets]({% post_url user-secrets-in-aspnetcore.md %}) or the  [Azure Key Vault](https://docs.microsoft.com/de-de/azure/key-vault/).

