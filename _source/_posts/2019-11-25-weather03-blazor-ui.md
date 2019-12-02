---
layout: post
title: "ASP.NET Core 3.0 Weather Application - The Blazor UI"
teaser: "Description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Tests
- Tag
---

Post your content here





## The Blazor project

I decided to use Blazor server side in this part. In the next post I'm going to create a Blazor, which needs a little more work to do, but uses a lot more techniques to write about. But the server side Blazor seems to be the best for now.

To create the Blazor project, I use the following commands in the console:

~~~shell
dotnet new blazorserver -n WeatherStats.Web -o WeatherStats.Web
dotnet sln add WeatherStats.Web
~~~

The last line adds the project to the solution file.

Blazor server side is basically a ASP.NET Razor Pages application, that uses SignalR to push servers side rendered Razor components to the client.

The `_Host.cshtml` is the Razor Page which hosts the SignalR client, which retrieves the prerendered components and displays them on the page. The `.razor` files are the components that get rendered here. Those files can also be rendered in the WebAssembly version of Blazor.

## Accessing the database

Since I use a Mongo database to store the fetched weather data, I need to use the same MongoDB connection as the worker project. This is why I'm going to move the data connection code from the worker service project to a shared class library: 

~~~ shell
dotnet new classlib -n WeatherStats.Shared -o WeatherStats.Shared
dotnet sln add WeatherStats.Shared
~~~

I also need to add references to this shared project, both in the web and in the worker project:

~~~shell
dotnet add WeatherStats.Web\ reference WeatherStats.Shared\
dotnet add WeatherStats.Worker\ reference WeatherStats.Shared\
~~~

After I moved the data connection code to the shared project I also need to change the namespaces in the worker project:

~~~csharp
using WeatherStats.Shared.Data;
~~~

I didn't change the actual data connection code, except the namespace, which now matches the name of the new shared project. In the web project I now also need to use this code. At first I need to register the `WeatherDatabaseSettings` and the `WeatherService` to the IoC container:

~~~csharp
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<WeatherDatabaseSettings>(
        Configuration.GetSection(nameof(WeatherDatabaseSettings)));

    services.AddSingleton<IWeatherDatabaseSettings>(sp =>
        sp.GetRequiredService<IOptions<WeatherDatabaseSettings>>().Value);

    services.AddTransient<IWeatherService, WeatherService>();

    services.AddRazorPages();
    services.AddServerSideBlazor();
    services.AddSingleton<WeatherForecastService>();
}
~~~

You already saw the same service registrations in the worker project. This time I added the registrations in the method `ConfigureServices` the `Startup.cs` 

Also the `appsettings` to connect the MongoDB server are the same:

~~~json
{
  "WeatherDatabaseSettings": {
    "WeatherCollectionName": "WeatherData",
    "ConnectionString": "mongodb+srv://weatherstats:weatherstats@instancename.azure.mongodb.net/test?retryWrites=true&w=majority",
    "DatabaseName": "WeacherDataDb"
  },
  // ...
}
~~~



## Display the data

To quickly test the connection to the data base I reuse the `FetchData` Razor component (`FetchData.razor`) and inject the `IWeatherService` directly

~~~ html
@page "/fetchdata"

@using WeatherStats.Shared.Data
@using System.Collections.Generic;
@inject IWeatherService WeatherService

<h1>Weather forecast</h1>

<p>This component demonstrates fetching data from a service.</p>
~~~

This gives me the `WeatherService` instance in a property called `WeatherService`, which I can use in this Razor component. The next snippet shows the razor code that loads the weather data asynchronously from the service:

~~~ csharp
@code {
    List<WeatherData> weatherData;

    protected override async Task OnInitializedAsync()
    {
        weatherData = await WeatherService.Get();
    }
}
~~~

With the async loading in mind I can add a loading message that disapears as soon as the data are available:

~~~html
@if (weatherData == null)
{
    <p><em>Loading...</em></p>
}
else
{
    <table class="table">
        <thead>
            <tr>
                <th>Date</th>
                <th>Temperature</th>
                <th>Wind Speed</th>
                <th>Precipitaion</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var weather in weatherData)
            {
                <tr>
                    <td>@weather.Date.ToShortDateString()</td>
                    <td>@weather.AvgTemperature</td>
                    <td>@weather.AvgWindSpeed</td>
                    <td>@weather.Precipitaion</td>
                </tr>
            }
        </tbody>
    </table>
}
~~~

I now can start the application to check weather it's working or not:

~~~shell
cd .\WeatherStats.Web\
dotnet run
~~~

And it works:

![](../img/weatherstats/weathertable.png)





