---
layout: post
title: "Customizing ASP.​NET Core Part 02: Configuration"
teaser: "This second part of the blog series about customizing ASP.NET Core is about the application configuration, how to use it and how to customize the configuration to use different ways to configure your app. Maybe you already have an existing XML configuration or want to share a YAML configuration file over different kind of applications. Sometimes it makes also sense to read configuration values out of a database."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- Configuration
---

This second part of the blog series about customizing ASP.NET Core is about the application configuration, how to use it and how to customize the configuration to use different ways to configure your app. 

## The series topics

- [Customizing ASP.NET Core Part 01: Logging]({% post_url customizing-aspnetcore-01-logging.md %})
- **Customizing ASP.NET Core Part 02: Configuration - This article**
- [Customizing ASP.NET Core Part 03: Dependency Injection]({% post_url customizing-aspnetcore-03-dependency-injection.md %})
- [Customizing ASP.NET Core Part 04: HTTPS]({% post_url customizing-aspnetcore-04-https.md %})
- [Customizing ASP.NET Core Part 05: HostedServices]({% post_url customizing-aspnetcore-05-hostedservices.md %})
- [Customizing ASP.NET Core Part 06: Middlewares]({% post_url customizing-aspnetcore-06-middlewares.md %})
- [Customizing ASP.NET Core Part 07: OutputFormatter]({% post_url customizing-aspnetcore-07-outputformatter.md %})
- [Customizing ASP.NET Core Part 08: ModelBinders]({% post_url customizing-aspnetcore-08-modelbinders.md %})
- [Customizing ASP.NET Core Part 09: ActionFilter]({% post_url customizing-aspnetcore-09-actionfilters.md %})
- [Customizing ASP.NET Core Part 10: TagHelpers]({% post_url customizing-aspnetcore-10-taghelpers %})

## Configure the configuration

As well as the logging, since ASP.NET Core 2.0 the configuration is also hidden in the default configuration of the `WebHostBuilder` and not part of the `Startup.cs` anymore. This is done for the same reasons to keep the Startup clean and simple:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)                
            .UseStartup<Startup>();
}
```

Fortunately you are also able to override the default settings to customize the the configuration in a way you need it. 

When you create a new ASP.NET Core project you already have an `appsettings.json` and an `appsettings.Development.json` configured. You can and you should use this configuration files to configure your app. You should because this is the pre-configured way and the most ASP.NET Core developers will look for an `appsettings.json` to configure the application. This is absolutely fine and works pretty well. 

But maybe you already have an existing XML configuration or want to share a YAML configuration file over different kind of applications. This could also make sense. Sometimes it makes also sense to read configuration values out of a database.

The next snippet shows the hidden default configuration to read the `appsettigns.json` files:

~~~ csharp
WebHost.CreateDefaultBuilder(args)	
    .ConfigureAppConfiguration((builderContext, config) =>
    {
        var env = builderContext.HostingEnvironment;

        config.SetBasePath(env.ContentRootPath);
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
        
        config.AddEnvironmentVariables();
    })
    .UseStartup<Startup>();
~~~

This configuration also set the base path of the application and adds the configuration via environment variables. The method `ConfigureAppConfiguration` accepts a lambda method that gets a `ConfigurationBuilderContext` and a `ConfigurationBuilder` passed in

Whenever you customize the the application configuration you should add the configuration via environment variable as the last step. The order of the configuration matters and the latter added configuration providers will override the previously added configurations. Be sure the environment variables always override the configurations via file. This way you ensure the configure on azure web apps via the Application Settings UI on Azure which will be passed to the application as environment variables.

The `IConfigurationBuilder` has a lot of extension methods to add more configurations like XML or INI configuration files, in-memory configurations and so on. You can find a lot more configuration providers provided by the community to read in YAML files, database values and a lot more. In this demo I'm going to show you how to read INI files in.

## Typed configurations

Before trying to read the INI files it makes sense to show how to use typed configuration instead of reading the configuration via the `IConfiguration` key by key. 

To read a type configuration you need to define the type to configure. I usually crate a class called `AppSettings` like this:

~~~ csharp
public class AppSettings
{
    public int Foo { get; set; }
    public string Bar { get; set; }
}
~~~

This classes than can be filled with specific configuration sections inside the method `ConfigureServices` in the `Startup.cs`

~~~ csharp
services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
~~~

This way the typed configuration also gets registered as a service in the dependency injection container and can be used everywhere in the application. You are able to create different configuration types per  configuration section. In the most cases one section should be fine, but maybe it makes sense to divide the settings into different sections.

This configuration than can be used via dependency injection in every part of your application. The next snippets shows how to use the configuration in a MVC controller:

~~~ csharp
public class HomeController : Controller
{
    private readonly AppSettings _options;

    public HomeController(IOptions<AppSettings> options)
    {
        _options = options.Value;
    }
~~~

The `IOptions<AppSettings>` is a wrapper around our `AppSettings` type and the property `Value` contains the actual instance of the `AppSettings` including the values from the configuration file.

To try that out the `appsettings.json` need to have the `AppSettings` section configured, otherwise the values are null or not set.

~~~ json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AppSettings": {
      "Foo": 123,
      "Bar": "Bar"
  }
}
~~~

## Configuration using INI files

To also use INI files to configure the application we need to add the INI configuration inside the method ConfigureAppConfiguration in the Program.cs:

~~~ csharp
config.AddIniFile("appsettings.ini", optional: false, reloadOnChange: true);
config.AddJsonFile($"appsettings.{env.EnvironmentName}.ini", optional: true, reloadOnChange: true);
~~~

This code loads the INI files the same way as the JSON configuration files. The first line is a required configuration and the second one an optional configuration depending on the current runtime environment.

The INI file could look like this:

~~~ ini
[AppSettings]
Bar="FooBar"
~~~

This file also contains a section called AppSettings and a property called Bar. Initially I wrote the order of the configuration matters. If you added the two lines to configure via INI files after the configuration via JSON files, the INI files will override the settings from the JSON files. The property Bar gets overridden with "FooBar" and the property Foo stays the same. Also the values out of the INI file will be available via the previously created AppSettings class.

Every other configuration provider will work the same way. Let's see how a configuration provider would look like.

## Configuration Providers

A configuration provider is an implementation of an IConfigurationProvider that get's created by an configuration source, which is an implementation of an IConfigurationSource. The configuration provider than reads the date in from somewhere and provides it via a Dictionary.

To add a custom or third party configuration provider to ASP.NET Core you need to call the method Add on the configuration builder and put the configuration source in:

```csharp
WebHost.CreateDefaultBuilder(args)	
    .ConfigureAppConfiguration((builderContext, config) =>
    {
        var env = builderContext.HostingEnvironment;

        config.SetBasePath(env.ContentRootPath);
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
        
        // add new configuration source
        config.Add(new MyCustomConfigurationSource{
        	SourceConfig = //configure whatever source 
            Optional = false,
            ReloadOnChange = true
        });
        
        config.AddEnvironmentVariables();
    })
    .UseStartup<Startup>();
```

Usually you would create an extension method to easier add the configuration source:

~~~ csharp
config.AddMyCustomSource("source", optional: false, reloadOnChange: true);
~~~

[A really detailed concrete example about how to create a custom configuration provider](https://andrewlock.net/creating-a-custom-iconfigurationprovider-in-asp-net-core-to-parse-yaml/) is written by the fellow MVP [Andrew Lock](https://andrewlock.net).

## Conclusion

In the most cases it is not needed to add a different configuration provider or to create your own configuration provider, but it's good to know how to change it in case you need it. Also using typed configuration is a nice way to read the settings. In classic ASP.NET we used a manually created façade to to read the application settings in a typed way. Now this is automatically done by just providing a class. This class get's automatically filled and provided via dependency injection.

To learn more about ASP.NET Core Dependency Injection have a look into the next part of the series: [Customizing ASP.NET Core Part 03: Dependency Injection]({% post_url customizing-aspnetcore-03-dependency-injection.md %})

