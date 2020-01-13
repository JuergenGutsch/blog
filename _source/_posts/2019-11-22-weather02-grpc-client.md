---
layout: post
title: "ASP.NET Core 3.0 Weather Application - The gRPC Client"
teaser: "In this post I'm going to write the worker service, which fetches the weather data from the previously created weather station. The worker service will include a gRPC client to connect to the service and it will store the data in a MongoDB database."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Worker Service
- gRPC
---

## Introduction

I'm going to write an application that reads weather data in, stores them and provides statistical information about that weather. In this case I use downloaded data from a weather station in Kent (WA). I'm going to simulate a day in two seconds. 

I will write a small gRPC services which will be our weather station in Kent. I'm also goin to write a worker service that hosts a gRPC Client to connect to the weather station to fetch the data every day. This worker service also stores the date into a database. The third application is a Blazor app that fetches the data from the database and displays the data in a chart and in a table.

In this post I'm going to continue with the client that fetches the data from the server. I will create a worker service, which fetches the weather data from the previously created weather station. The worker service will include a gRPC client to connect to the service and it will store the data in a MongoDB database.

## Setup the app

As already mentioned I would like to use a worker service that fetches the weather data periodically from the weather station. 

With your console change to the directory where the weather stats solution is located. As always we will use the .NET CLI to create new projects or to work with .NET Core projects. The next two commands create a new worker service application and add the project to the the current solution file

``` shell
dotnet new worker -n WeatherStats.Worker -o WeatherStats.Worker
dotnet sln add WeatherStats.Worker
```

This worker service project is basically a console application that executes a background service using the new generic hosting environment:

~~~ csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>();
            });
}
~~~

In the `Program.cs` a `IHostBuilder` is created that initializes some cool features like logging, configuration and dependency injection. But it doesn't initializes the web stack that is needed for ASP.NET Core. In the `ConfigureServices` method a `HostedService` is added to the dependency injection container. This is the actual background service. Let's rename it to `WeatherWorker` and have a short glimpse into the default implementation:

~~~ csharp
public class WeatherWorker : BackgroundService
{
    private readonly ILogger<WeatherWorker> _logger;

    public WeatherWorker(ILogger<WeatherWorker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}
~~~

> I just realized that there is no unique wording for this kind of service. There is `Worker Service`, `Background Service` and `Hosted Service`. In General, it is all the same thing. 
>
> A `HostedService` is a class that gets added to the dependency injection container, to get executed by the generic hosting service once after the application starts. This could be used to initialize a database or something else. This class gets executed asynchronous in the background. If this class runs an endless loop to execute stuff periodically we could call it a service, like a windows service. Because it also runs asynchronously in the background it is a `Background Serivce`. The implementation of a `Background Service` is called a worker in those kind of projects. That's why we also talk about a `Worker Service`. Also the entire application could be called a `Worker Service`, since it runs workers like a service.

Now we need to create the gRPC client to fetch the data from the weather station:

## The gRPC client

Creating the gRPC client needs some configuration, since there is no gRPC client template project available yet in the .NET CLI. Since the server and the client have to use the same proto file to setup a connection, it would make sense to copy the proto file of the server project into the solution folder and to share it between the projects. This is why I created a new Protos folder in the solution folder and moved the `weather.proto` into it.

![]({{site.baseurl}}/img/weatherstats/protofile.png)

This needs us to change the link to the proto file in the project files. The server:

``` xml
<ItemGroup>
  <Protobuf Include="..\Protos\weather.proto" 
    GrpcServices="Server" 
    Link="Protos\weather.proto" />
</ItemGroup>
```

The client:

``` xml
<ItemGroup>
  <Protobuf Include="..\Protos\weather.proto" 
    GrpcServices="Client" 
    Link="Protos\weather.proto" />
</ItemGroup>
```

You see that the code is pretty equal except the value of the `GrpcServices` attribute. This tells the tools to create the client or the server services.

We also need to add some NuGet Packages to the project file:

~~~ xml
<PackageReference Include="Grpc" Version="2.24.0" />
<PackageReference Include="Grpc.Core" Version="2.24.0" />
<PackageReference Include="Google.Protobuf" Version="3.9.2" />
<PackageReference Include="Grpc.Net.Client" Version="2.24.0" />
<PackageReference Include="Grpc.Tools" Version="2.24.0" PrivateAssets="All" />
~~~

These packages are needed to generate the client code out of the `weather.proto` and to access the client in C#

Until yet we didn't add any C# code. So let's open the `Worker.cs` and add some code to the `ExecuteAsync` method. But first remove the lines inside this method.

~~~ csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    _logger.LogInformation("create channel");
    using (var channel = GrpcChannel.ForAddress("https://localhost:5001"))
    {
        _logger.LogInformation("channel created");

        _logger.LogInformation("create client");
        var client = new Weather.WeatherClient(channel);
        _logger.LogInformation("client created");

        // Add your logic here
        // ...
    }
}
~~~

I added a lot of logging in this method, that writes out to the console. This is for debugging purposes and to just see what is happening in the worker app. At first we create a channel to the server. This will connect to the Server with the given address. And than we need to create the actual client using the channel. The client was built with the proto file and contains all the defined methods and is using the defined types.

~~~csharp
var d = new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc);
while (!stoppingToken.IsCancellationRequested)
{
    try
    {
        _logger.LogInformation("load weather data");
        var request = new WeatherRequest
            {
                Date = Timestamp.FromDateTime(d)
            };
        var weather = await client.GetWeatherAsync(
            request, null null, stoppingToken);
        _logger.LogInformation(
            $"Temp: {weather.AvgTemperature}; " +
            $"Precipitaion: {weather.Precipitaion}");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, ex.Message);
    }
    d = d.AddDays(1); // add one day
    await Task.Delay(1000, stoppingToken);
}
~~~

This snippet simulates the daily execution. There is a `DateTIme` defined that represents the first of January in 2019. (This is the first day of our weather time series in the database.) On every iteration of the while loop we add one day to fetch the weather data of the next day.

On line seven of this snippet I use the client to call the generated method `GetWeatherAsync` with a new `WeatherRequest`. The `WeatherRequest` contains the current `DateTime` as a `Google Protobuf Timestamp`. This type already has methods to convert the .NET UTC `DateTimes` into this kind of Timestamps. 

After I retrieved the weather data, I write some of the information out to the console. 

Now I am able to run both applications using two console sessions and it should work. One for the server and one for the client. The worker service application should be able to connect to the weather station and to fetch the data:

![]({{site.baseurl}}/img/weatherstats/run.png)

As you can see in the screenshot it works absolutely fine.

I'm now going to add some code to write the data to the database which is also used by the Web UI.

## The database 

Since the applications will run on docker, I'm going to use an open source data base server to store the data. This time I need to share the database with the UI project. The current app writes the data into the database and the UI project will read and display the data. So I need a separate container that host the database. In this this case I would try to use a MongoDB.

To use the PostgreSQL I need to add the Entity Framework Provider first:

~~~xml
<PackageReference Include="MongoDB.Driver" Version="2.9.3" />
~~~

At first I defined a `WeatherService` that contains the connection to the MongoDB:

~~~csharp
public interface IWeatherService
{
    Task<List<WeatherData>> Get();
    Task<WeatherData> Get(int id);
    Task<WeatherData> Create(WeatherData weather);
    Task Update(int id, WeatherData weatherIn);
    Task Remove(WeatherData weatherIn);
    Task Remove(int id);
}
public class WeatherService : IWeatherService
{
    private readonly IMongoCollection<WeatherData> _weatherData;

    public WeatherService(IWeatherDatabaseSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);

        _weatherData = database.GetCollection<WeatherData>(
            settings.WeatherCollectionName);
    }

    public async Task<List<WeatherData>> Get() =>
        (await _weatherData.FindAsync(book => true)).ToList();

    public async Task<WeatherData> Get(int id) =>
        (await _weatherData.FindAsync<WeatherData>(weather => weather.Id == id)).FirstOrDefault();

    public async Task<WeatherData> Create(WeatherData weather)
    {
        await _weatherData.InsertOneAsync(weather);
        return weather;
    }
    
    public async Task Update(int id, WeatherData weatherIn) =>
        await _weatherData.ReplaceOneAsync(weather => weather.Id == id, weatherIn);

    public async Task Remove(WeatherData weatherIn) =>
        await _weatherData.DeleteOneAsync(weather => weather.Id == weatherIn.Id);

    public async Task Remove(int id) =>
        await _weatherData.DeleteOneAsync(weather => weather.Id == id);
}
~~~

This `WeatherService` and the needed Settings need to be registered in the `Program.cs`:

~~~csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {                    
            services.Configure<WeatherDatabaseSettings>(
                hostContext.Configuration.GetSection(nameof(WeatherDatabaseSettings)));

            services.AddSingleton<IWeatherDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<WeatherDatabaseSettings>>().Value);

            services.AddTransient<IWeatherService, WeatherService>();

            services.AddHostedService<WeatherWorker>();
        });
~~~

This registration works the same way as in the regular ASP.NET Core Startup classes. The DI container is the same, only the location, where the configuration needs to be done is different.

At first I register the `WeatherDatabaseSettings` which reads the settings out of the `appsettings.json`. The second line registers an instance of the settings together with the settings interface. This is not really needed, but shows how you could register a service like this.

The third registration is the actual  `WeatherService`

Since the connection string is read from the `appsettings.json` file, I also need to add the connection string here:

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

Currently it is a MongoDB hosted on Azure, but later on I will use an instance inside a Docker container. It seems to be more useful to have it all all boxed in containers. From my perspective this makes shipping the entire application more easy and flexible.

However, If the `WeatherService` is registered, I'm almost able to use it in the `Worker.cs`. I need to inject the `WeatherService` first:

~~~csharp
public class WeatherWorker : BackgroundService
{
    private readonly ILogger<WeatherWorker> _logger;
    private readonly IWeatherService _weatherService;

    public WeatherWorker(ILogger<WeatherWorker> logger,
        IWeatherService weatherService)
    {
        _logger = logger;
        _weatherService = weatherService;
    }
~~~

Now I can add this lines to save the weather date to the database:

~~~csharp
await _weatherService.Create(new WeatherData
{
    Id = i,
    WeatherStation = "US1WAKG0045",
    AvgTemperature = weather.AvgTemperature,
    AvgWindSpeed = weather.AvgWindSpeed,
    MaxTemperature = weather.MaxTemperature,
    MinTemperature = weather.MinTemperature,
    Precipitaion = weather.Precipitaion,
    Date = weather.Date.ToDateTime()
});
~~~

That's it. Now the weather data fetched from the weather station will be saved into a database using a worker service. 

## Conclusion

This is working quite well. It's actually the first time I use a MongoDB, but it's nice, since it is just working and easy to setup. During development I'm going to use the instance on Azure and later on I will setup a dockerized instance. 

I really like the way gRPC works and how easy it is to setup a gRPC client. But I think it makes sense to have a gRPC client template available with the .NET CLI by default. This way it wouldn't be needed to find the right packages to include in various blog posts and documentations. Because this get's hard and confusing, if some of resources are just a little bit outdated. The way to add a gRPC service as a service reference using VIsual Studio 2019 is nice, but doesn't really help developers who use VSCode or/and are working on different platforms.

As mentioned the worker and the weather station are working pretty well, but there is still a lot to do:

* I need to create the web client
* I will add health checks to monitor the entire application
* I need to dockerize all the stuff 
* I will to push it so somewhere

But this are topics for the next blog posts :-)

