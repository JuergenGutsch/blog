---
layout: post
title: "Customizing ASP.​NET Core Part 12: Hosting "
teaser: ""
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- Configurationn
- WebHostBuilder
---





## Initial series topics

- [Customizing ASP.NET Core Part 01: Logging]({% post_url customizing-aspnetcore-01-logging.md %})
- [Customizing ASP.NET Core Part 02: Configuration]({% post_url customizing-aspnetcore-02-configuration.md %})
- [Customizing ASP.NET Core Part 03: Dependency Injection]({% post_url customizing-aspnetcore-03-dependency-injection.md %})
- [Customizing ASP.NET Core Part 04: HTTPS]({% post_url customizing-aspnetcore-04-https.md %})
- [Customizing ASP.NET Core Part 05: HostedServices]({% post_url customizing-aspnetcore-05-hostedservices.md %})
- [Customizing ASP.NET Core Part 06: Middlewares]({% post_url customizing-aspnetcore-06-middlewares.md %})
- [Customizing ASP.NET Core Part 07: OutputFormatter]({% post_url customizing-aspnetcore-07-outputformatter.md %})
- [Customizing ASP.NET Core Part 08: ModelBinders]({% post_url customizing-aspnetcore-08-modelbinders.md %})
- [Customizing ASP.NET Core Part 09: ActionFilter]({% post_url customizing-aspnetcore-09-actionfilters.md %})
- [Customizing ASP.NET Core Part 10: TagHelpers]({% post_url customizing-aspnetcore-10-taghelpers.md %})
- [Customizing ASP.NET Core Part 11: WebHostBuilder]({% post_url customizing-aspnetcore-11-webhostbuilder.md %})
- **customizing ASP.NET Core Part 12: Hosting - This article**

## WebHostBuilderContext

It is about this Kestrel configuration in the `Program.cs`. In that post I wrote that you should use user secrets to configure the certificates password:

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
                options.Listen(IPAddress.Loopback, 5000);
                options.Listen(IPAddress.Loopback, 5001, listenOptions =>
                {
                    listenOptions.UseHttps("certificate.pfx", "topsecret");
                });
            })
        	.UseStartup<Startup>();
}
~~~

The reader wrote that he couldn't fetch the configuration inside this code. And he is true, if we are only looking at this snippet. You need to know that the method UseKestrel() is overloaded:

~~~csharp
.UseKestrel((host, options) =>
{
    // ...
})
~~~

This first argument is a `WebHostBuilderContext`. Using this you are able to access the configuration.

So lets rewrite the lambda a little bit to use this context:

~~~ csharp
.UseKestrel((host, options) =>
{
    var filename = host.Configuration.GetValue("AppSettings:certfilename", "");
    var password = host.Configuration.GetValue("AppSettings:certpassword", "");
    
    options.Listen(IPAddress.Loopback, 5000);
    options.Listen(IPAddress.Loopback, 5001, listenOptions =>
    {
        listenOptions.UseHttps(filename, password);
    });
})
~~~





## Conclusion

