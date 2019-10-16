---
layout: post
title: "ASP.NET Core 3.0 Weather Application - The gRPC Server"
teaser: "As mentioned in the last post, I'm going to write a small application to demonstrate gRPC, Worker Services, SignalR and Blazor to show the new stuff in ASP.NET Core. In this post I'm going to write about the gRPC server that acts as a weather station to provide the weather data to the main application."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Tests
- Tag
---

## Introduction

As mentioned [in the last post]({% post_url aspnetcore30-workerservice.md %}), the next couple of posts will be a series that describes how to build a kind of a microservice application that reads weather data in, stores them and provides statistical information about that weather.  

I'm going to use gRPC, Worker Services, SignalR, Blazor and maybe the Identity Server to secure all the services. If some time is left, I'll put all the stuff into docker containers.

I will write a small gRPC services which will be our weather station in Kent. I'm also goin to write a worker service that hosts a gRPC Client to connect to the weather station to fetch the data every day. This worker service also stores the date into a database. The third application is a Blazor app that fetches the data from the database and displays the data in a chart and in a table.

In this case I use downloaded weather data of Washington state and I'm going to simulate a day in two seconds.

In this post I will start with the weather station.

## Setup the app

In my local git project dump folder I create a new folder called `WeatherStats`, which will be my project solution folder:

``` shell
mkdir weatherstats
cd weatherstats
dotnet new sln -n WeatherStats
dotner new grpc -n WeatherStats.Kent -o WeatherStats.Kent
dotnet sln add WeatherStats.Kent
```

This line create the folder, creates a new solution file (sln) with the name `WeatherStats`. The fourth line creates the gRPC project and the last line adds the project to the solution file.

The solution file helps MSBuild to build all the projects, to see the dependencies and so on. And it helps user who like to use Visual Studio.

If this is done I open VSCode using the code command in the console:

```shell
code .
```

The database is the SQLite database that I created for my talk about the [ASP.NET Core Health Checks]({% post_url dotnetconf2019.md %}). Just copy this database to your own repository into the folder of the weather station `WeatherStats.Kent`.

In the `Startup.cs` we only have the services for gRPC registered:

```csharp
services.AddGrpc();
```

But we also need to add a DbContext:

``` csharp
services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(
        Configuration["ConnectionStrings:DefaultConnection"]);
});
```

The configuration points to a SQLite database in the current project:

``` json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=wa-weather.db"
  },
```

In the `Configure` method the gRPC middleware is mapped to the `WeatherService`:

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseRouting();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapGrpcService<WeatherService>();

        endpoints.MapGet("/", async context =>
        {
            await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
        });
    });
}
```

A special in this project type is the proto folder with the `greet.proto` in it. This is a text file that describes the gRPC endpoint. We are going to rename it to `weather.proto` later on and to change it a little bit. If you change the name outside of Visual Studio 2019, you also need to change it in the project file. I never tried it, but the Visual Studio 2019 tooling should also rename the references.

You will also find a `GreeterService` in the Services folder. This file is the implementation of the service that is defined in the `greeter.proto`.

And last but not least we have the `DbContext` to create, which isn't really complex in out case:

``` csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherData>()
            .HasKey(x => x.Id );
        modelBuilder.Entity<WeatherData>()
            .HasOne(p => p.WeatherStation)
                .WithMany(b => b.WeatherData);
        modelBuilder.Entity<WeatherStation>()
            .HasKey(x => x.Id);
    }

    public DbSet<WeatherData> WeatherData { get; set; }
    public DbSet<WeatherStation> WeatherStation { get; set; }
}
```

## The gRPC endpoint

Let's start changing the gRPC endpoint. Personally I really love starting to code from the UI perspective, this forces me to not do more than the UI really needs. In hour case the gRPC endpoint is the UI. So I use the `weather.proto` file to design the API:

```protobuf
syntax = "proto3";
import "google/protobuf/timestamp.proto";

option csharp_namespace = "WeatherStats.Kent";

package Weather;

// The weather service definition.
service Weather {
  // Sends a greeting
  rpc GetWeather (WeatherRequest) returns (WeatherReply);
}

// The request message containing the date.
message WeatherRequest {
  google.protobuf.Timestamp date = 1;
}

// The response message containing the weather.
message WeatherReply {
  google.protobuf.Timestamp date = 1;
  float avgTemperature = 2;
  float minTemperature = 3;
  float maxTemperature = 4;
  float avgWindSpeed = 5;
  float precipitaion = 6;
}
```

I need to import the support for timestamp to work with dates. The namespace was predefined by the tooling. I changed the package name and the service name to `Weather`. The rpc method now is called `GetWeather` and takes an `WeatherRequest` as an argument and returns a `ReatherReply`.

After that the types (messages) are defined. The `WeatherRequest` only has the date in it, which is the requested date. The `WeatherReply` also contains the date as well as the actual weather data of that specific day.

That's it. When I now build the application, the gRPC tooling builds a lot of C# code in the background for us. This code will be used in the `WeatherService`, that fetches the date from the database:

``` csharp
public class WeatherService : Weather.WeatherBase
{
    private readonly ILogger<WeatherService> _logger;
    private readonly ApplicationDbContext _dbContext;

    public WeatherService(
        ILogger<WeatherService> logger,
        ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public override Task<WeatherReply> GetWeather(
        WeatherRequest request, 
        ServerCallContext context)
    {
        var weatherData = _dbContext.WeatherData
            .SingleOrDefault(x => x.WeatherStationId == WeatherStations.Kent
                && x.Date == request.Date.ToDateTime());

        return Task.FromResult(new WeatherReply
        {
            Date = Timestamp.FromDateTime(weatherData.Date),
            AvgTemperature = weatherData?.AvgTemperature ?? float.MinValue,
            MinTemperature = weatherData?.MinTemperature ?? float.MinValue,
            MaxTemperature = weatherData?.MaxTemperature ?? float.MinValue,
            AvgWindSpeed = weatherData?.AvgWindSpeed ?? float.MinValue,
            Precipitaion = weatherData?.Precipitaion ?? float.MinValue
        });
    }
}
```

This service will fetches a specific `WeatherData` item from the database using a Entity Framework Core `DbContext` that we created previously. gRPC has another date and time implementation. This needs to add the `Google.Protobuf.WellKnownTypes` package via NuGet. This package also provides functions to convert between this two date and time implementations.

The `WeatherService` derives from the `WeatherBase` class, which is auto generated from the `weather.proto` file. Also the types `WeatherRequest` and `WeatherReply` are auto generated as defined in the `weather.proto`. As you can see the `WeatherBase` is in the `WeatherStats.Kent.Weather` namespace, which is a combination of the `csharp_namespace` and the `package` name.

That's it. We are able to test the service after the client is done.

## Conclusion

This is all the code for the weather station. Not really complex, but enough to demonstrate the gRPC server. 

In the next part, I will show how to connect to the gRPC server using a gRPC client and how to store the weather data into a database. The client will run inside a worker service to fetch the date regularly, e. g. once a day.

