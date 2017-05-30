---
layout: post
title: "A first glimpse into .NET Core 2.0 Preview 1 and ASP.​NET Core 2.0.0 Preview 1"
teaser: "At the Build 2017 conference Microsoft announced the preview 1 versions of .NET Core 2.0, of the .NET Standard 2.0 and ASP.NET Core 2.0. I recently had a quick look into it and want to show you a little bit about it with this post."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core 2.0
- ASP.NET Core 2.0
---

At the Build 2017 conference Microsoft announced the preview 1 versions of .NET Core 2.0, of the .NET Standard 2.0 and ASP.NET Core 2.0. I recently had a quick look into it and want to show you a little bit about it with this post.

## .NET Core 2.0 Preview 1

Rich Lander (Program Manager at Microsoft) wrote about the release of the preview 1, .NET Standard 2.0, tools support in this post: [Announcing .NET Core 2.0 Preview 1](https://blogs.msdn.microsoft.com/dotnet/2017/05/10/announcing-net-core-2-0-preview-1/). It is important to read the first part about the requirements carefully. Especially the requirement of Visual Studio 2017 15.3 Preview. At the first quick look I was wondering about the requirement of installing a preview version of Visual Studio 2017, because I have already installed the final version since a few months. But the details is in the numbers. The final version of Visual Studio 2017 is the 15.2. The new tooling for .NET Core 2.0 preview is in the 15.3 which is in preview currently. 

So if you want to use .NET Core 2. preview 1 with Visual Studio 2017 you need to install the preview of 15.3

The good thing is, the preview can be installed side by side with the current final of Visual Studio 2017. It doesn't double the usage of disk space, because both versions are able share some SDKs, e.g. the Windows SDK. But you need to install the add-ins you want to use for this version separately.

After the Visual Studio you need to install the new .NET Core SDK which also installs NET Core 2.0 Preview 1 and the .NET CLI.

## The .NET CLI

After the new version of .NET Core is installed type `dotnet --version` in a command prompt. It will show you the version of the currently used .NET SDK:

![]({{ site.baseurl }}/img/netcore2/01-dotnetversion.png)

Wait. I installed a preview 1 version and this is now the default on the entire machine? Yes.

The CLI uses the latest installed SDK on the machine by default. But anyway you are able to run different .NET Core SDKs side by side. To see what versions are installed on our machine type `dotnet --info` in a command prompt and copy the first part of the base path and past it to a new explorer window:

![]({{ site.baseurl }}/img/netcore2/02-dotnetinfo.png)

![]({{ site.baseurl }}/img/netcore2/03-dotnetsdks.png)

You are able to use all of them if you want to.

This is possible by adding a "global.json" to your solution folder. This is a pretty small file which defines the SDK version you want to use:

~~~ json
{
  "projects": [ "src", "test" ],
  "sdk": {
    "version": "1.0.4"
  }
}
~~~

Inside the folder "C:\git\dotnetcore\", I added two different folders: the "v104" should use the current final version 1.0.4 and the "v200" should use the preview 1 of 2.0.0. to get it working I just need to put the "global.json" into the "v104" folder:

![]({{ site.baseurl }}/img/netcore2/04-global-json.PNG)

## The SDK

Now I want to have a look into the new SDK. The first thing I do after installing a new version is to type `dotnet --help` in a command prompt. The first level help doesn't contain any surprises, just the version number differs. The most interesting difference is visible by typing `dotnet new --help`. We get a new template to add an ASP.NET Core Web App based on Razor pages. We also get the possibility to just add single files, like a razor page, "NuGet.config" or a "Web.Config". This is pretty nice.

![]({{ site.baseurl }}/img/netcore2/05-dotnetnew.PNG)

I also played around with the SDK by creating a new console app. I typed `dotnet new console -n consoleapp`:

![]({{ site.baseurl }}/img/netcore2/06-dotnetnewconsole.PNG)

As you can see in the screenshot dotnet new will directly download the NuGet packages from the package source. It runs dotnet restore for you. It is not a super cool feature but good to know if you get some NuGet restore errors while creating a new app.

When I opened the "consoleapp.csproj", I saw the expected TargetFramework "netcoreapp2.0"

~~~ xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp2.0</TargetFramework>
  </PropertyGroup>

</Project>
~~~

This is the only difference between the 2.0.0 preview 1 and the 1.0.4

In ASP.NET Core are a lot more changes done. Let's have a quick look here too:

## ASP.NET Core 2.0 Preview 1

Also for the ASP.NET 2.0 Preview 1, Jeffrey T. Fritz (Program Manager for ASP.NET) wrote a pretty detailed announcement post in the webdev blog: [Announcing ASP.NET Core 2.0.0-Preview1 and Updates for .NET Web Developers](https://blogs.msdn.microsoft.com/webdev/2017/05/10/aspnet-2-preview-1/).

To create a new ASP.NET Web App, I need to type `dotnet new mvc -n webapp` in a command prompt window. This command immediately creates the web app and starts to download the needed packages:

![]({{ site.baseurl }}/img/netcore2/07-dotnetnewmvc.PNG)

Let's see what changed, starting with the "Program.cs":

~~~ csharp
public class Program
{
  public static void Main(string[] args)
  {
    BuildWebHost(args).Run();
  }

  public static IWebHost BuildWebHost(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
      .UseStartup<Startup>()
      .Build();
}
~~~

The first thing I mentioned is the encapsulation of the code that creates and configures the `WebHostBuilder`. In the previous versions it was all in the static void main. But there's no instantiation of the `WebHostBuilder` anymore. This is hidden in the `.CreateDefaultBuilder()` method. This look a little cleaner now, but also hides the configuration from the developer. It is anyway possible to use the old way to configure the `WebHostBuilder`, but this wrapper does a little more than the old configuration. This Method also wraps the configuration of the `ConfigurationBuilder` and the `LoggerFactory`. The default configurations were moved from the "Startup.cs" to the `.CreateDefaultBuilder()`. Let's have a look into the "Startup.cs":

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
    services.AddMvc();
  }

  // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
  public void Configure(IApplicationBuilder app, IHostingEnvironment env)
  {
    if (env.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
    }
    else
    {
      app.UseExceptionHandler("/Home/Error");
    }

    app.UseStaticFiles();

    app.UseMvc(routes =>
               {
                 routes.MapRoute(
                   name: "default",
                   template: "{controller=Home}/{action=Index}/{id?}");
               });
  }
}
~~~

Even this file is much cleaner now. 

But if you now want to customize the Configuration, the Logging and the other stuff, you need to replace the `.CreateDefaultBuilder()` with the previous style of bootstrapping the application or you need to extend the `WebHostBuilder` returned by this method. You could have a look into the [sources of the WebHost class in the ASP.NET repository on GitHub](https://github.com/aspnet/MetaPackages/blob/4b18cf52ae3c22c7124fd9cb35ae0253b390b28e/src/Microsoft.AspNetCore/WebHost.cs) (around line 150) to see how this is done inside the `.CreateDefaultBuilder().` The code of that method looks pretty familiar for someone who already used the previous version.

BTW: BrowserLink was removed from the templates of this preview version. Which is good from my perspective, because it causes an error while starting up the applications. 

## Result

This is just a first short glimpse into the .NET Core 2.0 Preview 1. I need some more time to play around with it and learn a little more about the upcoming changes. For sure I need to rewrite my post about the custom logging a little bit :)

> BTW: Last week, I created [a 45 min video about it in German](https://www.youtube.com/watch?v=6WZ3UIAVUxU). This is not a video with a good quality. It is quite bad. I just wanted to test a new microphone and Camtasia Studio and I chose ".NET Core 2.0 Preview 1" as the topic to present. Even if it has a awful quality, maybe it is anyway useful to some of my German speaking readers. :)

I'll come with some more .NET 2.0 topics within the next months.
