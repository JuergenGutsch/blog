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

I decided to use Blazor server side in this part. In the next post I'm going to create a Blazor WebAssembly project, which needs a little more work to do, but uses a lot more techniques to write about. But the server side Blazor seems to be the best for now.

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

You already saw the same service registrations in the worker project. This time I added the registrations in the method `ConfigureServices` of the `Startup.cs` 

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

With the `async` loading in mind I can add a loading message that disappears as soon as the data are available in the variable `weatherData`:

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

The table to show the data is pretty simple and written using common Razor syntax. I'm iterating over the data to render the table rows and their values.

I now can start the application to check weather it's working or not:

~~~shell
cd .\WeatherStats.Web\
dotnet run
~~~

And it works:

![](../img/weatherstats/weathertable.png)



## Playing around: Fetch new data every second

Since I have the connection and the weather data available I can start playing around. At first I want to update the UI every time I get a new data point. This means I need to trigger a push from the server to the client. 

Microsoft says that Blazor is the replacement of Web Forms. That means that Blazor also is stateful. What if we just wrap the data loading part inside a loop to load it every second? This won't work inside the `OnInitializedAsync()` method, because it will await the execution and renders the component when the loop is finished. The loop shouldn't be the problem, but I need to find the right time to execute the loop and to update the view.

The solution is the method `OnAfterRenderAsync()` and the call of the method `StateHasChanged()`. Let's have a look at the code before getting into details:

~~~ csharp
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await ReloadData();
        }
    }

    private async Task ReloadData()
    {
        while(true)
        {
            try
            {
                weatherData = await WeatherService.Get();
                this.StateHasChanged();            
                await Task.Delay(1000);
            }
            catch(Exception e)
            {
                // break in case of an error 
                break;
            }  
        }
    }
~~~

In the `OnAfterRenderAsync` method I check whether it is the first call or not. Because I  want to call the loop to load the data only ones during the first rendering. Inside the `RelaodData()` method I have the loop that executes every second. So every second I load the data from the database and I tell the component that the state has changed and the component needs to re-render.

## Playing around: Plot the data

The next thing I want to try is to plot the data on a graph.

In the past, I worked a lot with time series like this. Actually the time series I worked with in the past were about financial data, but they look the same. We have a time axis (X) and a value axis (Y). And actually it is not important, whether the numbers are weather data or financial data. We have a temporal frequency and a data point with one to many values. In the table above I display three different series, using the same frequency and the same temporal coverage. The weather service actually provides five series, because it also includes the min and max temperature per data point. Those data can be plotted on a graph, e. g. as a line chart. All I need to do now is to find a a graph library for razor that can be fed with the data we fetched from the database.

I found a cool project called Blazorize in GitHub that contains a lot of ready to use Blazor components. It also provides chart controls.

