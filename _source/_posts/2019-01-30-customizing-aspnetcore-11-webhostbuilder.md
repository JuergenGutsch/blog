---
layout: post
title: "Customizing ASP.​NET Core Part 11: WebHostBuilder "
teaser: "In my post about Configuring HTTPS in ASP.NET Core 2.1 a reader asked how to configure the HTTPS settings using user secrets. In this post I'm going to answer this question also by writing about how to configure the WebHostBuilder using app configuration"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- Configurationn
- WebHostBuilder
---

> **Update 2021-01-31**
>
> This series is pretty much outdated. As asked by a reader, I compiled the entire series into a book and updated the contents to the latest version of ASP.NET Core  Read [here]({% post_url my-book.md %}) to learn more about it 

In my post about [Configuring HTTPS in ASP.NET Core 2.1]({% post_url aspnetcore-ssl.md %}), a reader asked how to configure the HTTPS settings using user secrets.

> "How would I go about using user secrets to pass the password to `listenOptions.UseHttps(...)`? I can't fetch the configuration from within `Program.cs` no matter what I try. I've been Googling solutions for like a half hour so any help would be greatly appreciated."
> [https://github.com/JuergenGutsch/blog/issues/110#issuecomment-441177441](https://github.com/JuergenGutsch/blog/issues/110#issuecomment-441177441)

In this post I'm going to answer this question. 

## This series topics

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
- **customizing ASP.NET Core Part 11: WebHostBuilder - This article**
- [customizing ASP.NET Core Part 12: Hosting - This article]({% post_url customizing-aspnetcore-12-hosting.md %})

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

## Why does it work?

Do you remember the days back where you needed to configure app configuration in the `Startup.cs` ASP.NET Core? That was configured in the constructor of the Startup class and looked similar like this, if you added user secrets:

~~~ csharp
 var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

    if (env.IsDevelopment())
    {
        builder.AddUserSecrets();
    }

    builder.AddEnvironmentVariables();
    Configuration = builder.Build();
~~~

This code now is wrapped inside the `CreateDefaultBuilder` Method ([see on GitHub](https://github.com/aspnet/AspNetCore/blob/3c09d644cccdb21801f7a79e1188a1a1212de5d9/src/DefaultBuilder/src/WebHost.cs)) and looks like this:

~~~ csharp
builder.ConfigureAppConfiguration((hostingContext, config) =>
{
    var env = hostingContext.HostingEnvironment;

    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

    if (env.IsDevelopment())
    {
        var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
        if (appAssembly != null)
        {
            config.AddUserSecrets(appAssembly, optional: true);
        }
    }

    config.AddEnvironmentVariables();

    if (args != null)
    {
        config.AddCommandLine(args);
    }
})
~~~

It is almost the same code and it is one of the first things that gets executed when building the `WebHost`. It needs to be one of the first things because the Kestrel is configurable via the app configuration. Maybe you know that you are able to specify ports and URLs and so on using environment variables or the `appsettings.json`:

I found this lines in the [WebHost.cs](https://github.com/aspnet/AspNetCore/blob/3c09d644cccdb21801f7a79e1188a1a1212de5d9/src/DefaultBuilder/src/WebHost.cs): 

~~~ csharp
builder.UseKestrel((builderContext, options) =>
{
    options.Configure(builderContext.Configuration.GetSection("Kestrel"));
})
~~~

That means you are able to add this lines to the `appsettings.json` to configure Kestrel endpoints:

~~~ json
"Kestrel": {
  "EndPoints": {
  "Http": {
  "Url": "http://localhost:5555"
 }}}
~~~

Or to use environment variables like this to configure the endpoint:

~~~ shell
SET KESTREL_ENDPOINTS_HTTP_URL=http://localhost:5555
~~~

Also this configuration isn't executed 

## Conclusion

Inside the `Program.cs` you are able to use app configuration inside the lambdas of the configuration methods, if you have access to the `WebHostBuilderContext`. This way you can use all the configuration you like to configure the `WebHostBuilder`.

I just realized that this post could be placed between [Customizing ASP.NET Core Part 02: Configuration]({% post_url customizing-aspnetcore-02-configuration.md %})  and [Customizing ASP.NET Core Part 04: HTTPS]({% post_url customizing-aspnetcore-04-https.md %}). So I made this the eleventh part of the [Customiting ASP.NET Core Series]({% post_url customizing-aspnetcore-series.md %}).