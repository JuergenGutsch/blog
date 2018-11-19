---
layout: post
title: "Customizing ASP.​NET Core Part 04: HTTPS"
teaser: "HTTPS is on by default now and a first class feature. On Windows the certificate which is needed to enable HTTPS is loaded from the windows certificate store. If you create a project on Linux and Mac the certificate is loaded from a certificate file."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- HTTPS
---

HTTPS is on by default now and a first class feature. On Windows the certificate which is needed to enable HTTPS is loaded from the windows certificate store. If you create a project on Linux and Mac the certificate is loaded from a certificate file. 

> Even if you want to create a project to run it behind and IIS or an NGinX webserver HTTPS is enabled. Usually you would manage the certificate on the IIS or NGinX webserver in that case. But this shouldn't be a problem and you shouldn't disable HTTPS in the ASP.NET Core settings.
>
> To manage the certificate within the ASP.NET Core application directly makes sense if you run services behind the firewall, services which are not accessible from the internet. Services like background services for a micro service based applications, or services in a self hosted ASP.NET Core application.

There are some scenarios where it makes sense to also load the certificate from a file on Windows. This could be in an application that you will run on docker for Windows, and also on docker for Linux.

Personally I like the flexible way to load the certificate from a file.

## The series topics

- [Customizing ASP.NET Core Part 01: Logging]({% post_url customizing-aspnetcore-01-logging.md %})
- [Customizing ASP.NET Core Part 02: Configuration]({% post_url customizing-aspnetcore-02-configuration.md %})
- [Customizing ASP.NET Core Part 03: Dependency Injection]({% post_url customizing-aspnetcore-03-dependency-injection.md %})
- **Customizing ASP.NET Core Part 04: HTTPS - This article**
- [Customizing ASP.NET Core Part 05: HostedServices]({% post_url customizing-aspnetcore-05-hostedservices.md %})
- [Customizing ASP.NET Core Part 06: Middlewares]({% post_url customizing-aspnetcore-06-middlewares.md %})
- [Customizing ASP.NET Core Part 07: OutputFormatter]({% post_url customizing-aspnetcore-07-outputformatter.md %})
- [Customizing ASP.NET Core Part 08: ModelBinders]({% post_url customizing-aspnetcore-08-modelbinders.md %})
- [Customizing ASP.NET Core Part 09: ActionFilter]({% post_url customizing-aspnetcore-09-actionfilters.md %})
- [Customizing ASP.NET Core Part 10: TagHelpers]({% post_url customizing-aspnetcore-10-taghelpers %})

## Setup Kestrel

As well as in the first to parts of this blog series, we need override the default `WebHostBuilder` a little bit. With ASP.NET Core it is possible to replace the default Kestrel based hosting with an hosting based on an `HttpListener`. This means the Kestrel webserver is configured somehow to the host builder. You are able to add and configure Kestrel manually by **using** it. That means by calling the `UseKestrel()` method on the `IWebHostBuilder`:

~~~ csharp
public class Program
{
	public static void Main(string[] args)
	{
		CreateWebHostBuilder(args).Build().Run();
	}

	public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
		WebHost.CreateDefaultBuilder(args)
			.UseKestrel(options => 
			{	
			})
			.UseStartup<Startup>();
}
~~~

This method accepts an action to configure the Kestrel webserver. What we actually need to do is to configure the addresses and ports the webserver is listen on. For the HTTPS port we also need to configure how the certificate should be loaded.

~~~ csharp
.UseKestrel(options => 
{
	options.Listen(IPAddress.Loopback, 5000);
	options.Listen(IPAddress.Loopback, 5001, listenOptions =>
	{
		listenOptions.UseHttps("certificate.pfx", "topsecret");
	});
})
~~~

In this snippet we add to addresses and ports to listen on. The second one is defined as secure endpoint configured to use HTTPS. The method `UseHttps()` is overloaded multiple times, to load certificates from the windows certificate store as well as from files. In this case we use a file called `certificate.pfx` located in the project folder.

> Reminder to myself: Replacing the host actually would be an idea for an eleventh part of this series.

To create such a certificate file to just play around with this configuration open the certificate store and export the development certificate created by visual studio.

## For your safety

Use the following line **ONLY** to play around with this configuration:

~~~ csharp
listenOptions.UseHttps("certificate.pfx", "topsecret");
~~~

The problem is the hard coded password. **Never ever** store a password in a code file that gets pushed to any source code repository. Ensure you load the password from the configuration API of ASP.NET Core. Use the user secrets on your local development machine and use environment variables on a server. On Azure use the Application Settings to store the passwords. Passwords will be hidden on the Azure Portal UI, if they are marked as passwords.

## Conclusion

This is just a small customization. Anyway, this helps if you want to share the code between different platforms, if you want to run your application on Docker and don't want to care about certificate stores, etc.

Usually, if you run your application behind an web server like IIS or NGinX, you don't need to care about certificates in your ASP.NET Core application. But you need to if you host your application inside another application, on Docker or without an IIS or NGinX.

ASP.NET Core has a new feature to run tasks in the background inside the application. To learn more about that, read the next post about [Customizing ASP.NET Core Part 05: HostedServices]({% post_url customizing-aspnetcore-05-hostedservices.md %}).
