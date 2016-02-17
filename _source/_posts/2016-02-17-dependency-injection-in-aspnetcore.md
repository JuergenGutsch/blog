--- 
layout: post
title: "Dependency Injection in ASP.NET Core - a quick overview"
teaser: "Dependency Injection is now a global pattern in ASP.NET Core. All parts of the ASP.NET Stack are using the same DI container. In this post I will try to show, how you configure the DI container and where you can use it."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- Dependency Injection
---

Wich ASP.NET Core Dependency Injection is now a first class citizen in ASP.NET. All parts of the ASP.NET Stack are using the same DI container. In this post I'm going to show you, how to configure the DI container and how to use it.

Let's first create a new and pretty simple service to use in the examples. As always in my examples it is a `CountryService` which provides a list of countries. We also need an interface for this service, let's create it too:

~~~ csharp
public class CountryService : ICountryService 
{ 
    public IEnumerable<Country> All() 
    { 
        return new List<Country> 
        { 
            new Country {Code = "DE", Name = "Germany" }, 
            new Country {Code = "FR", Name = "France" }, 
            new Country {Code = "CH", Name = "Switzerland" }, 
            new Country {Code = "IT", Name = "Italy" }, 
            new Country {Code = "DK", Name = "Danmark" } , 
            new Country {Code = "US", Name = "United States" }
        }; 
    } 
} 

public interface ICountryService 
{ 
    IEnumerable<Country> All(); 
} 

public class Country 
{ 
    public string Code { get; internal set; } 
    public string Name { get; internal set; } 
}
~~~

## Register the services

We now need to add this `ContryService` to the DI container. This needs to be done in the `Startup.cs` in the method `ConfigureServices`:

~~~ csharp
services.AddTransient<ICountryService, CountryService>();
~~~

This mapping between the interface and the concrete type defines, that everytime you request a type of `IContryService`, you'll get a new instance of the `CountryService`. This is what transient means in this case. You are also able to add singleton mappings (using `AddSingleton`) and scoped mappings (using `AddScoped`). Scoped in this case means scoped to a HTTP request, which also means it is a singleton while the current request is running. You can also add an existing instance to the DI container using the method `AddInstance`.

These are the almost complete ways to register to the `IServiceCollection`:

~~~ csharp
services.AddTransient<ICountryService, CountryService>();            
services.AddTransient(typeof (ICountryService), typeof (CountryService));
services.Add(new ServiceDescriptor(typeof(ICountryService), typeof(CountryService), ServiceLifetime.Transient));
services.Add(new ServiceDescriptor(typeof(ICountryService), p => new CountryService(), ServiceLifetime.Transient));

services.AddSingleton<ICountryService, CountryService>();
services.AddSingleton(typeof(ICountryService), typeof(CountryService));
services.Add(new ServiceDescriptor(typeof(ICountryService), typeof(CountryService), ServiceLifetime.Singleton));
services.Add(new ServiceDescriptor(typeof(ICountryService), p => new CountryService(), ServiceLifetime.Singleton));

services.AddScoped<ICountryService, CountryService>();
services.AddScoped(typeof(ICountryService), typeof(CountryService));
services.Add(new ServiceDescriptor(typeof(ICountryService), typeof(CountryService), ServiceLifetime.Scoped));
services.Add(new ServiceDescriptor(typeof(ICountryService), p => new CountryService(), ServiceLifetime.Scoped));

services.AddInstance<ICountryService>(new CountryService());
services.AddInstance(typeof(ICountryService), new CountryService());
services.Add(new ServiceDescriptor(typeof(ICountryService), new CountryService()));
~~~

If you have a lot of services to register, you should create a extension method to the `IServiceCollection` to keep the `Startup.cs` clean. The same way is used by default for MVC and many other tools you want to use in your project:

~~~ csharp
services.AddMvc();
~~~

This extension method add all the services to the `IServiceCollection` which are needed by the MVC MiddleWare.

~~~ csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterServices(
        this IServiceCollection services)
    {
        services.AddTransient<ICountryService, CountryService>();
        // and a lot more Services

        return services;
    }
}
~~~

The method `RegisterServices` looks now much more cleaner:
~~~ csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add framework services.
    services.AddMvc();

    services.RegisterServices();
}
~~~

## Usage

Now we can request an instance of an `CountryService` almost everywhere in our ASP.NET Core application. For example in a MVC controller:

~~~ csharp
public class HomeController : Controller 
{ 
    private readonly ICountryService _countryService; 

    public HomeController(ICountryService countryService) 
    { 
        _countryService = countryService; 
    } 
    // … 
}
~~~

New in ASP.NET Core MVC is, that we can also inject this service into a MVC view. The following line defines the injection in a Razor view:

~~~ razor
@inject DiViews.Services.ICountryService CountryService;
~~~

The first part after the `@inject` directive defines the interface. The second part is the name of the variable which holds our instance.

To inject a service globally into all Views, add this line to the `_ViewImports.cshtml`. In a complete new ASP.NET Core project, there is already a global injection defined for ApplicationInsights:

~~~ razor
@inject Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration TelemetryConfiguration
~~~

We are now able to use the instance in our view:

~~~ razor
@if (countryService.All().Any()) 
{ 
    <ul> 
        @foreach (var country in CountryService.All().OrderBy(x => x.Name)) 
        { 
            <p>@country.Name (@country.Code)</p> 
        } 
    </ul> 
}
~~~

We can also use this service to fill select fields with the list of countries:

~~~ razor
@Html.DropDownList("Coutries", CountryService.All() 
    .OrderBy(x => x.Name) 
    .Select(x => new SelectListItem 
    { 
        Text = x.Name, 
        Value = x.Code 
    }))
~~~

DI is also working in MiddleWares, TagHelpers and ViewComponents. You could use DI in TagHelpers to create reusable CountryList or whatever you want:

~~~ csharp
public class CountryListTagHelper : TagHelper
{
    private readonly ICountryService _countryService;

    public CountryListTagHelper(ICountryService countryService)
    {
        _countryService = countryService;
    }

    public string SelectedValue { get; set; }


    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "select";
        output.Content.Clear();
        foreach (var country in _countryService.All())
        {
            var seleted = "";
            if (SelectedValue != null && SelectedValue.Equals(country.Code, StringComparison.CurrentCultureIgnoreCase))
            {
                seleted = " selected=\"selected\"";
            }
            var listItem = $"<option value=\"{country.Code}\"{seleted}>{country.Name}</option>";
            output.Content.AppendHtml(listItem);
        }
    }
}
~~~

This TagHelper could be used like this:

~~~ razor
<country-list selected-value="@Model.Country"></country-list>
~~~

## Conclusion

You are able to use DI almost everywhere in your application (Except in HtmlHelpers, because this are extension methods.) and you can use every servce which is registered in the IServiceCollection, even the services which are registerd by ASP.NET Core. This also means all the contexts, all the environment and even the logger. This helps a lot to keep a ASP.NET Core application clean, leightweight, maintainable and testable.