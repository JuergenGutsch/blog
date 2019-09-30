---
layout: post
title: "New in ASP.NET Core 3.0 - Taking a quick look into the Startup.cs"
teaser: "I the last post, I took a quick look into the Program.cs of ASP.NET Core 3.0 and I quickly explored the Generic Hosting Model. But also the Startup class has something new in it. We will see some small but important changes."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
---

I the [last post](https://asp.net-hacker.rocks/2019/08/05/aspnetcore30-generic-hosting-environment.html), I took a quick look into the `Program.cs` of ASP.NET Core 3.0 and I quickly explored the Generic Hosting Model. But also the Startup class has something new in it. We will see some small but important changes.

> Just one thing I forgot to mention in the last post: It should just work ASP.NET Core 2.1 code of the `Program.cs` and the `Startup.cs` in ASP.NET Core 3.0, if there is no or less customizing. The `IWebHostBuilder` is still there and can be uses the 2.1 way and also the default 2.1 `Startup.cs` should run in ASP.NET Core 3.0. It may be that you only need to do some small changes there.

The next snippet is the `Startup` class of an newly created empty web project:

~~~ csharp
public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        });
    }
}
~~~

The empty web project is a ASP.NET Core project without any ASP.NET Core UI feature. This is why the `ConfigureServices` method is empty. There is no additional service added to the dependency injection container.

The new stuff is into in the `Configure` method. The first lines look familiar. Depending on the hosting environment the development exception page will be shown.

`app.UseRouting()` is new. This is a middleware that enables the new endpoint routing. The new thing is, that routing is decoupled from the specific ASP.NET Feature. In the previous Version every feature (MVC, Razor Pages, SIgnalR, etc.) had its own endpoint implementation. Now the endpoint and routing configuration can be done independently. The Middlewares that need to handle a specific endpoint will now be mapped to a specific endpoint or route. So the Middlewares don't need to handle the routes anymore.

If you wrote a Middleware in the past which needs to work on a specific endpoint, you added the logic to check the endpoint inside the middleware or you used the `MapWhen()` extension method on the `IApplicationBuilder` to add the Middleware to a specific endpoint.

Now you create a new pipeline (using `IApplicationBuilder)` per endpoint and Map the Middleware to the specific new pipeline.

The `MapGet()` method above does this implicitly. It created a new endpoint "/" and maps the delegate Middleware to the new pipeline that was created internally.

That was a simple snippet. Now let's have a look into the `Startup.cs` of a new full blown web application using individual authentication. Created by using this .NET CLI command:

~~~ shell
dotnet new mvc --auth Individual
~~~

Overall this also looks pretty familiar if you already know the previous versions:

~~~ csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(
                Configuration.GetConnectionString("DefaultConnection")));
        services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddControllersWithViews();
        services.AddRazorPages();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseDatabaseErrorPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            endpoints.MapRazorPages();
        });
    }
}
~~~

This is a MVC application, but did you see the lines where MVC is added? I'm sure you did. It is not longer called MVC, even if it is the MVC pattern used, because it was a little bit confusing with Web API.

To add MVC you now need to add `AddControllersWithViews()`. If you want to add Web API only you just need to add `AddControllers()`. I think this is a small but useful change. This way you can be more specific by adding ASP.NET Core features. In this case also Razor pages where added to the project. It is absolutely no problem to mix ASP.NET Core features. 

> `AddMvc()` still exists and is still working in ASP.NET Core

The Configure method doesn't really change, except the new endpoint routing part. There are two endpoints configured. One for controller routes (Which is Web API and MVC) and one for RazorPages.

## Conclusion

This is also just a quick look into the `Startup.cs` with just some small but useful changes. 

In the next post I'm going to do a little more detailed look into the new endpoint routing. While working on the [GraphQL endpoint for ASP.NET Core](https://github.com/JuergenGutsch/graphql-aspnetcore), I learned a lot about the endpoint routing. This feature makes a lot of sense to me, even if it means to rethink some things, when you build and provide a Middleware.

