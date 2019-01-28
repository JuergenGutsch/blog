---
layout: post
title: "title"
teaser: "description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- Unit Test
- XUnit
- MSTest
---

In my post about [Configuring HTTPS in ASP.NET Core 2.1](aspnetcore-ssl.md), a reader asked how to configure the HTTPS settings using user secrets.

> "How would I go about using user secrets to pass the password to `listenOptions.UseHttps(...)`? I can't fetch the configuration from within `Program.cs` no matter what I try. I've been Googling solutions for like a half hour so any help would be greatly appreciated."
> [https://github.com/JuergenGutsch/blog/issues/110#issuecomment-441177441](https://github.com/JuergenGutsch/blog/issues/110#issuecomment-441177441)

In this post I'm going to answer this question. 

It is about this Kestrel configuration in the Program.cs. In that post I wrote that you should use user secrets to configure the certificates password:

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

## WebHostBuilderContext

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

In this sample I chose to write the keys using the colon divider because this is the way you need to read nested configurations from the `appsettings.json`:

~~~ json
{
    "AppSettings": {
        "certfilename": "certificate.pfx",
        "certpassword": "topsecret"
    },
    "Logging": {
        "LogLevel": {
            "Default": "Warning"
        }
    },
    "AllowedHosts": "*"
}
~~~

You are also able to read from the user secrets store with this keys:

~~~ shell
dotnet user-secrets init
dotnet user-secrets set "AppSettings:certfilename" "certificate.pfx"
dotnet user-secrets set "AppSettings:certpassword" "topsecret"
~~~

As well as environment variables:

~~~ shell
SET APPSETTINGS_CERTFILENAME=certificate.pfx
SET APPSETTINGS_CERTPASSWORD=topsecret
~~~

