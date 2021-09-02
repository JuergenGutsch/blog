---
layout: post
title: "ASP.​NET Core in .NET 6 - Async streaming"
teaser: "This is the next part of the ASP.NET Core on .NET 6 series. In this post, I'd like to have a look into async streaming."
author: "Jürgen Gutsch"
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

Async streaming is now supported from the controller action down to the response formatter, as well as on the hosting level. This topic is basically about the `IAsyncEnumerable<T>`. This means that those async enumerable will be handled async all the way down to the response stream.  They don't get buffered anymore, which improves the performance and reduces the memory usage a lot. Huge lists of data now get smoothly streamed to the client.

In the past, we handled large data by sending them in small chunks to the output stream, because of the buffering. This way we needed to find the right balance of the chunk size. Smaller chunks increase the CPU load and bigger chunks increase the memory consumption.

This is not longer needed. The `IAsyncEnumerable<T>` does this for you with a lot better performance.

Even EF Core supports the `IAsyncEnumerable<T>` to query the data. Because of that, working with EF Core is improved as well. Data you fetch from the database using EF Core can now be directly streamed to the output. 

This is more or less what Microsoft wrote about async streaming, but I really like to try it by myself. 😃

## Trying to stream large data

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

At first, I add a AppDbContext and a AppDbContextFactory to the project:

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

The factory will be used by the EF tool to work work with the migrations.

At next, I need to register the `DbContext` to the Dependency Injection Container. In the `Program.cs` I add the following snippet right after the registration of Swagger:

~~~csharp
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=app.db");
});
~~~

Next, I'd like to seed a bigger amount of data. To do that I'm using GenFu in a method called SeedDatabase that I placed in the Program.cs to generate 100000 records of `WeatherForecast`:

~~~csharp
// ...more usings
using GenFu;

// ...

app.MapControllers();

SeedDatabase(); // Call the seeding

app.Run();

void SeedDatabase()
{
    using var context = app.Services.CreateScope().ServiceProvider.GetService<AppDbContext>();
    if (context != null && !context.WeatherForecasts.Any())
    {
        var i = 1;
        A.Configure<WeatherForecast>()
            .Fill(c => c.Id, () => { return i++; });

        var weatherForecasts = A.ListOf<WeatherForecast>(100000);
        context.WeatherForecasts.AddRange(weatherForecasts);
        context.SaveChanges();
    }
}
~~~

I need to create a Scope to get the `AppDbContext` out of the `ServiceProvider`. Then we check if the database already contains any data. We also need to configure GenFu to not create random IDs. Otherwise, we would get problems when we safe the data into the database. Then the list of 100000 `WeatherForecast` gets created and stored into the database, in case there are no.

> I would have used the `HasData` method in the `OnModelCreating` method in the `AppDbContext` to seed the data. But seeding large data using this way doesn't really work since EF Migrations creates an insert statement per record in the migration file. This means the size of the migration file exceeds a lot and applying the migration took hours on my machine before I stopped it. The .NET Host needed almost all RAM and the CPU load was at 50%. I tried to seed one million records and 100000 records with no success. And lost three hours this way.
>
> This is why I did the seeding manually before the application starts as proposed in this documentation:
> [https://docs.microsoft.com/en-us/ef/core/modeling/data-seeding](https://docs.microsoft.com/en-us/ef/core/modeling/data-seeding)
>
> I also tried to get one million records loaded with the client, but I got an **Error: Maximum response size reached** message in Postman, so I left it with 100000. Actually, that points me to the question of where the streaming aspect is... Maybe this is a Postman problem 🤔

One more thing to do is to change the `WeatherForecastController` to use the AppDbContext and to return the weather forecasts:

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
        return Ok(_context.WeatherForecasts);
    }
}
~~~

At last, I need to create the EF migration and to update the database using the global tool:

~~~shell
dotnet ef migrations add InitialCreate
dotnet ef database update
~~~

Since I don't seed the data with the migrations, it will be fast.

That's it. I start the application using dotnet run and call the endpoint in Postman:

~~~curl
GET https://localhost:5001/WeatherForecast/
~~~

It is fascinating. The CPU load of the AsyncStreams application is quite low, but the memory consumption is pretty much the same, compared to an action method that buffers the data:

~~~csharp
[HttpGet]
public async Task<IActionResult> Get()
{
    return Ok(await _context.WeatherForecasts.ToListAsync());
}
~~~

I guess, I need to do some more tests to get a better comparison of the memory consumption.

## What's next?

In the next part In going to have a look at the `HTTP logging middleware` in ASP.NET Core 6.0.

