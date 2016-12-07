--- 
layout: post
title: "Configure your ASP.​NET Core 1.0 Application"
teaser: "The Web.Config is gone and the AppSettings are gone with ASP.NET Core 1.0. How do we configure our ASP.NET Core Application now? With the Web.Config, also the config transform feature is gone. How do we configure a ASP.NET Core Application for specific deployment environments?"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- Configuration
---

The `Web.Config` is gone and the `AppSettings` are gone with ASP.NET Core 1.0. How do we configure our ASP.NET Core Application now? With the `Web.Config`, also the config transform feature is gone. How do we configure a ASP.NET Core Application for specific deployment environments?

## Configuring

Unfortunately a newly started ASP.NET Core Application doesn't include a complete configuration as a sample. This makes the jump-start a little difficult. The new Configuration is quite better than the old one and it would make sense to add some settings by default. Anyway, lets start by creating a new Project.

Open the `Startup.cs` and take a look at the controller. There's already something like a configuration setup. This is exactly what the newly created application needs to run. 

~~~ csharp 
public Startup(IHostingEnvironment env)
{
    // Set up configuration sources.
    var builder = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables();

    if (env.IsDevelopment())
    {
        // This will push telemetry data through Application Insights 
        // pipeline faster, allowing you to view results immediately.
        builder.AddApplicationInsightsSettings(developerMode: true);
    }
    Configuration = builder.Build();
}
~~~

But in the most cases you need much more configuration. This code creates a `ConfigurationBuilder` and adds a `appsettigns.json` and environment variables to the `ConfigurationBuilder`. In development mode, it also adds ApplicationInsights settings.

If you take a look into the appsettings.json, you'll only find a ApplicationInsights key and some logging specific settings (In case you chose a individual authentication you'll also see a connection string):

~~~ json
{
  "ApplicationInsights": {
    "InstrumentationKey": ""
  },
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Verbose",
      "System": "Information",
      "Microsoft": "Information"
    }
  }
}
~~~

Where do we need to store our custom application settings?

We can use this `appsettings.json` or any other JSON file to store our settings. Let's use the existing one to add a new section called `AppSettings`:

~~~ json
{
    ...
    
    "AppSettings" : {
        "ApplicationTitle" : "My Application Title",
        "TopItemsOnStart" : 10,
        "ShowEditLink" : true
    }
}
~~~

This looks nice, but how do we read this settings?

In the `Startup.cs` the Configuration is already built and we could use it like this:

~~~ csharp
var configurationSection = Configuration.GetSection("AppSettings");
var title = configurationSection.Get<string>("ApplicationTitle");
var topItmes = configurationSection.Get<int>("TopItemsOnStart");
var showLink = configurationSection.Get<bool>("ShowEditLink");
~~~

We can also provide a default value in case that item doesn't exist or in case it is null

~~~ csharp
var topItmes = configurationSection.Get<int>("TopItemsOnStart", 15);
~~~

To use it everywhere we need to register the `IConfigurationRoot` to the dependency injection container:

~~~ csharpw
services.AddInstance<IConfigurationRoot>(Configuration);
~~~

But this seems not to be a really useful way to provide the application settings to our application. And it looks almost similar as in the previous ASP.NET Versions. But the new configuration is pretty much better. In previous versions we created a settings facade to encapsulate the settings, to not access the configuration directly and to get typed settings.

No we just need to create a simple POCO to provide access to the settings globally inside the application:

~~~ csharp
public class AppSettings
{
    public string ApplicationTitle { get; set; }
    public int TopItemsOnStart { get; set; }
    public bool ShowEditLink { get; set; }
}
~~~

The properties of this class should match the keys in the configuration section. Is this done we are able to map the section to that `AppSettings` class:

~~~ csharp
services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
~~~

This fills our `AppSettings` class with the values from the configuration section. This code also adds the settings to the IoC container and we are now able to use it everywhere in the application by requesting for the `IOptions<AppSettings>`:

~~~ csharo
public class HomeController : Controller
{
    private readonly AppSettings _settings;

    public HomeController(IOptions<AppSettings> settings)
    {
        _settings = settings.Value;
    }

    public IActionResult Index()
    {
        ViewData["Message"] = _settings.ApplicationTitle;
        return View();
    }
~~~

Even directly in the view:

~~~ aspnet
@inject IOptions<AppSettings> AppSettings
@{
    ViewData["Title"] = AppSettings.Value.ApplicationTitle;
}
<h2>@ViewData["Title"].</h2>
<ul>
    @for (var i = 0; i < AppSettings.Value.TopItemsOnStart; i++)
    {
        <li>
            <span>Item no. @i</span><br/>
            @if (AppSettings.Value.ShowEditLink) {
                <a asp-action="Edit" asp-controller="Home"
                   asp-route-id="@i">Edit</a>
            }
        </li>
    }
</ul>
~~~

With this approach, you are able to create as many configuration sections as you need and you are able to provide as many settings objects as you need to your application.

What do you think about it? Please let me know and drop a comment.

## Environment specific configuration

Now we need to have differnt configurations per deployment environment. Let's assume we have a production, a staging and a development environment where we run our application. All this environments need another configuration, another connections string, mail settings, Azure access keys, whatever...

Let's go back to the `Startup.cs` to have a look into the constructor. We can use the `IHostingEnvironment` to load different `appsettings.json` files per environment. But we can do this in a pretty elegant way:

~~~ csharp
.AddJsonFile("appsettings.json")
.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
~~~

We can just load another JSON file with an environment specific name and with `optional` set to true. Let's say the appsettings.json contain the production and the default settings and the `appsettings.Staging.json` contains the staging sepcific settings. It we are running in Staging mode, the second settings file will be loaded and the existing settings will be overridden by the new one. We just need to sepcify the settings we want to override.

Setting the flag `optional` to true means, the settings file doesn't need to exist. Whith this approatch you can commit some default setings to the source code repository and the top secret access keys and connections string, could be stored in an `appsettings.Development.json`, an `appsettings.staging.json` and an `appsettings.Production.json`
on the buildserver or on the webserver directly. 

## Conclusion

As you can see, configuration in ASP.NET Core is pretty easy. You just need to know how to do it. Because it is not directly visible in a new project, it is a bit difficult to find the way to start.