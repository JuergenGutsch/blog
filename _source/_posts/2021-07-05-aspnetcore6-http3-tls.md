---
layout: post
title: "ASP.NET Core in .NET 6 - HTTP/3 endpoint TLS configuration"
teaser: "This is the next part of the ASP.NET Core on .NET 6 series. In this post, I'd like to have a look into HTTP/3 endpoint TLS configuration."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- .NET 6
- Blazor

---

This is the next part of the [ASP.NET Core on .NET 6 series]({% post_url aspnetcore6-01.md %}). In this post, I'd like to have a look into HTTP/3 endpoint TLS configuration.

In the preview 3, Microsoft started to add support for HTTP/3 which brings a lot of improvements to the web. HTTP3 brings a faster connection setup as well as improved performance on low-quality networks.

Microsoft now adds support for HTTP/3 and the support for TLS (https). 

## HTTP/3 endpoint TLS configuration

Let's see how you can configure HTTP/3 in a small MVC app using the following commands:

~~~shell
dotnet new mvc -o Http3Tls -n Http3Tls
cd Http3Tls
code .
~~~

This creates an MVC app, changes into the project folder and opens VSCode.

In the `Program.cs` we need to configure HTTP/3 as shown in Microsoft's blog post:

~~~csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                    .ConfigureKestrel((context, options) =>
                    {
                        options.EnableAltSvc = true;
                        options.Listen(IPAddress.Any, 5001, listenOptions =>
                        {
							// Enables HTTP/3
                            listenOptions.Protocols = HttpProtocols.Http3;
                            // Adds a TLS certificate to the endpoint
                            listenOptions.UseHttps(httpsOptions =>
                            {
                                httpsOptions.ServerCertificate = LoadCertificate();
                            });
                        });
                    })
                    .UseStartup<Startup>();
            });
}
~~~

The flag `EnableAltSvc` sets a Alt-Svc header to the browsers to tell them, that there are alternative services to the existing HTTP/1 or HTTP/2. This is needed to tell the browsers, that the alternative services - HTTP/3 in this case - should be treated like the existing ones. This needs a https connection to be secure and trusted.

## What's next?

In the next part In going to look into the support for `.NET Hot Reload support` in ASP.NET Core.
