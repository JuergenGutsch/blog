---
layout: post
title: "ASP.NET Core in .NET 6 - Async streaming"
teaser: "This is the next part of the ASP.NET Core on .NET 6 series. In this post, I'd like to have a look into async streaming.
"author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET 6
- ASP.NET Core
- Streams
- Async
---

This is the next part of the [ASP.NET Core on .NET 6 series]({% post_url aspnetcore6-01.md %}). In this post, I'd like to have a look into async streaming.

Async streaming basically means the usage of `IAsyncEnumerable<>` 

## IAsyncEnumerable<>

Async streaming is now supported from the controller action down to the response formatter, as well as on the hosting level. This topic is basically about  the `IAsyncEnumerable<T>`. This means that those async enumerables will be handled async all the way down to the response stream.  They don't get buffered anymore, which improves the performance and reduces the memory usage a lot. Huge lists of date now gets smoothly streamed to the client.

In the past, we handled large data by sending them in small chunks to the output stream, because of the buffering. This way we needed to find the right balance of the chunk size. Smaller  chunks increases the CPU load and bigger chunks increases the memory consumption.

This is not longer needed. The `IAsyncEnumerable<T>` does this for you with a lot better performance 

Even EF Core supports the `IAsyncEnumerable<T>` to query the data. Because of that, working with EF Core is improved as well. Data you fetch from the database using EF Core can now directly streamed to the output. 

I'd like to try streaming a lot of data from the database to the client. So I create a new web API project using the .NET CLI:

~~~shell
dotnet new webapi -n AsyncStreams -o AsyncStreams
cd AsyncStreams\

code .
~~~

> Microsoft changed the most .NET CLI project templates to use the minimal API approach.  

This creates a web API project and opens it in Visual Studio Code. We need to add some EF Core packages to work with SQLite and to create EF migrations:

```shell
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 6.0.0-preview.7.21378.4
dotnet add package Microsoft.EntityFrameworkCore.Design --version 6.0.0-preview.7.21378.4
```

To generate that load of data, I also need to add my favorite package GenFu:

~~~shell
dotnet add package GenFu
~~~

This package is pretty useful to create test and mock data.

If you never installed ef global tool you should do it using the following command. The version should be the same as for the Microsoft.EntityFrameworkCore.Design package. I'm currently using the preview 7:

~~~shell
dotnet tool install --global dotnet-ef --version 6.0.0-preview.7.21378.4
~~~

Now let's write some code.

At first I add a DbContext  and a DdContextFactory to the project:

~~~csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AsyncStreams
{
    public class AppDbContext : DbContext
    {
        public DbSet<WeatherForecast> WeatherForecasts => Set<WeatherForecast>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }
    }
    
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>();
            options.UseSqlite("Data Source=app.db");

            return new AppDbContext(options.Options);
        }
    }
}
~~~

The factory will be used by the ef tool to work work with the migrations.

At next I'd like to seed the huge amount of data. To do that I'm using GenFu in the  `OnModelCreating` method to generate one million records of `WeatherForecast`:

~~~csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    var i = 1;
    A.Configure<WeatherForecast>()
        .Fill(c => c.Id, () => { return i++; });

    var weatherForecasts = A.ListOf<WeatherForecast>(1000000);
    modelBuilder.Entity<WeatherForecast>().HasData(weatherForecasts);
}
~~~

At first we need to configure GenFu not create random IDs. Otherwise we would get problems when we safe the date into the database.

Than the list of one million `WeatherForecast` gets created and stored into the database, in case there are no.

At next, I need to register the the DbContext to the Dependency Injection Container. In the `Program.cs` I add the following snippet right after the registration of Swagger:

~~~csharp
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=app.db");
});
~~~

One more thing to do is to change the `WeatherForecastController` to use the DbContext and to return the weather forecasts:

~~~csharp
using Microsoft.AspNetCore.Mvc;

namespace AsyncStreams.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly AppDbContext _context;

    public WeatherForecastController(
        ILogger<WeatherForecastController> logger,
        AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var weathers = _context.WeatherForecasts;
        return Ok(weathers);
    }
}
~~~

At last, I need to create the EF migration and to update the database using the global tool:

~~~shell
dotnet ef migrations add InitialCreate
dotnet ef database update
~~~

Actually it takes some minutes to create the database.







the 



## What's next?

In the next part In going to look into the support for `Async streaming` in ASP.NET Core.

