---
layout: post
title: "ASP.NET Core 3.0 Weather Application - The gRPC Client"
teaser: ""
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Worker Service
- gRPC
---

The next couple of posts will be a series that describes how to build a kind of a microservice app using the latest ASP.NET Core 3.0 features.

I'm going to use gRPC, Worker Services, SignalR, Blazor and maybe the Identity Server to secure all the services. If some time is left, I'll put all the stuff into docker containers.

## Introduction

I'm going to write an application that reads weather data in, stores them and provides statistical information about that weather. In this case I use downloaded data from a weather station in Kent (WA). I'm going to simulate a day in two seconds. 

I will write a small gRPC services which will be our weather station in Kent. I'm also goin to write a worker service that hosts a gRPC Client to connect to the weather station to fetch the data every day. This worker service also stores the date into a database. The third application is a Blazor app that fetches the data from the database and displays the data in a chart and in a table.

In this post I will continue with the client that fetches the data from the server.

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
> A `HostedService` is a class that gets added to the dependency injection container, to get executed by the generic hosting service once after the application starts. This could be used to initialize a database or something else. This class gets executed asynchronous in the background. If this class runs an endless loop to execute stuff periodically we could call it a service, like a windows service. Because it also runs asynchronously in the background it is a `Background Serivce`. The implementation of a `Background Service` is called a worker in those kind of projects. That's why we also talk about a `Worker Service`. Also the entire application could be called a `Worker Service`, since it runs workers like a windows service.

Now we need to create the gRPC client to fetch the data from the weather station.

## The gRPC client

Creating the gRPC client needs some configuration, since there is no gRPC client template project available yet in the .NET CLI. Since the server and the client have to use the same proto file to setup a connection, it would make sense to copy the proto file of the server project into the solution folder and to share it between the projects. This is why I created a new Protos folder in the solution folder and moved the `weather.proto` into it.

![](C:\git\blog\_source\img\weatherstats\protofile.png)

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

After I retrieved the weather data, I write some of the information out to the console. I'm going to add some code here to write the data to the database used by the Web UI.

## The database 

Since the applications will run on docker, I'm going to use an open source data base server to store the data. This time I need to share the database with the UI project. The current app writes the data into the database and the UI project will read and display the data. So I need a separate container that host the database. In this this case I would try to use a PostgreSQL.

To use the PostgreSQL I need to add the Entity Framework Provider first:

~~~xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="3.0.1" />
~~~



At first I defined a really simple `DbContext` that contains just one entity to work with:

~~~csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {        }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherData>()
            .HasKey(x => x.Id);
    }
    
    public DbSet<WeatherData> WeatherData { get; set; }
}
~~~

This `DbContext` needs to be registered in the `Program.cs`:

~~~csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {                    
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    hostContext.Configuration.GetConnectionString("DefaultConnection")));
            services.AddHostedService<WeatherWorker>();
        });
~~~

This registration works the same way as in the regular ASP.NET Core Startup classes. The DI container is the same, only the location, where the configuration needs to be done is different.

Since the connection string is read from the `appsettings.json` file, I also need to add the connection string here:

~~~json
{
    "ConnectionStrings": {
        "DefaultConnection": "Host=my_host;Database=my_db;Username=my_user;Password=my_pw"
    },
    // ...
}
~~~

If the `DbContext` is registered, I'm almost able to use it in the `Worker.cs`. I need to inject the `DbContext` first:

~~~csharp
public class WeatherWorker : BackgroundService
{
    private readonly ILogger<WeatherWorker> _logger;
    private readonly ApplicationDbContext _dbContext;

    public WeatherWorker(ILogger<WeatherWorker> logger,
        ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
~~~

Now I can add the lines to save the weather date to the database:

~~~csharp
_dbContext.WeatherData.Add(new WeatherData
{
    WeatherStation = "US1WAKG0045",
    AvgTemperature = weather.AvgTemperature,
    AvgWindSpeed = weather.AvgWindSpeed,
    MaxTemperature = weather.MaxTemperature,
    MinTemperature = weather.MinTemperature,
    Precipitaion = weather.Precipitaion,
    Date = weather.Date.ToDateTime()
});
await _dbContext.SaveChangesAsync(stoppingToken);
~~~



## Conclusion



