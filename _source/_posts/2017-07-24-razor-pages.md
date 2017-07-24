---
layout: post
title: "New Visual Studio Web Application: The ASP.NET Core Razor Pages"
teaser: "I think, everyone who followed the last couple of ASP.NET Community Standup session heard about Razor Pages. Did you try the Razor Pages? I was also a little bit skeptical about it and compared it to the ASP.NET Web Site project. That was definitely wrong. With this post I'm going to have a first look into it."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- Razor Pages
---

I think, everyone who followed the last couple of ASP.NET Community Standup session heard about Razor Pages. Did you try the Razor Pages? I didn't. I focused completely on ASP.NET Core MVC and Web API. With this post I'm going to have a first look into it. I'm going to try it out. 

I was also a little bit skeptical about it and compared it to the ASP.NET Web Site project. That was definitely wrong.

You need to have the latest preview on Visual Studio 2017 installed on your machine, because the Razor Pages came with ASP.NET Core 2.0 preview. It is based on ASP.NET Core and part of the MVC framework.

## Creating a Razor Pages project

Using Visual Studio 2017, I used "File... New Project" to create a new project. I navigate to ".NET Core", chose the "ASP.NET Core Web Application (.NET Core)" project  and I chose a name and a location for that project.

![]({{ site.baseurl }}/img/razor-pages/new-project.PNG)

In the next dialogue, I needed to switch to ASP.NET Core 2.0 to see all the new available project types. (I will write about the other one in the next posts.) I selected the "Web Application (Razor Pages)" and pressed "OK".

![]({{ site.baseurl }}/img/razor-pages/new-razor-pages.PNG)

## Program.cs and Startup.cs

I you are already familiar with ASP.NET core projects, you'll find nothing new in the `Program.cs` and in the `Startup.cs`. Both files look pretty much the same.

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

The `Startup.cs` has a `services.AddMvc()` and an `app.UseMvc()` with a configured route:

~~~ csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
  if (env.IsDevelopment())
  {
    app.UseDeveloperExceptionPage();
    app.UseBrowserLink();
  }
  else
  {
    app.UseExceptionHandler("/Error");
  }

  app.UseStaticFiles();

  app.UseMvc(routes =>
  {
    routes.MapRoute(
      name: "default",
      template: "{controller=Home}/{action=Index}/{id?}");
  });
}
~~~

That means the Razor Pages are actually part of the MVC framework, as Damien Edwards always said in the Community Standups. 

## The solution 

But the solution looks a little different. Instead of a Views and a Controller folders, there is only a Pages folder with the razor files in it. Even there are known files: the `_layout.cshtml`, `_ViewImports.cshtml`, `_ViewStart.cshtml`.

![]({{ site.baseurl }}/img/razor-pages/razor-solution.PNG)

Within the `_ViewImports.cshtml` we also have the import of the default TagHelpers

~~~ razor
@namespace RazorPages.Pages
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
~~~

This makes sense, since the Razor Pages are part of the MVC Framework. 

We also have the standard pages of every new ASP.NET project: Home, Contact and About. (I'm going to have a look at this files later on.)

As every new web project in Visual Studio, also this project type is ready to run. Pressing F5 starts the web application and opens the URL in the browser: 

![]({{ site.baseurl }}/img/razor-pages/ready-to-run.PNG)

## Frontend

For the frontend dependencies "bower" is used. It will put all the stuff into `wwwroot/bin`. So even this is working the same way as in MVC. Custom CSS and custom JavaScript ar in the css and the js folder under `wwwroot`. This should all be familiar for ASP.NET Corer developers.

Also the way the resources are used in the _Layout.cshtml are the same.

## Welcome back "Code Behind"

This was my first thought for just a second, when I saw the that there are nested files under the Index, About, Contact and Error pages. At the first glimpse this files are looking almost like Code Behind files of Web Form based ASP.NET Pages, but are completely different:

~~~ csharp
public class ContactModel : PageModel
{
    public string Message { get; set; }

    public void OnGet()
    {
        Message = "Your contact page.";
    }
}
~~~

They are not called Page, but Model and they have something like an handler in it, to do something on a specific action. Actually it is not a handler, it is an an method which gets automatically invoked, if this method exists. This is a lot better than the pretty old Web Forms concept. The base class PageModel just provides access to some Properties like the Contexts, Request, Response, User, RouteData, ModelStates, ViewData and so on. It also provides methods to redirect to other pages, to respond with specific HTTP status codes, to sign-in and sign-out. This is pretty much it. 

The method `OnGet` allows us to access the page via a GET request. `OnPost` does the same for POST. Gues what `OnPut` does ;) 

Do you remember Web Forms? There's no need to ask if the current request is a GET or POST request. There's a single decoupled method per HTTP method. This is really nice.

On our Contact page, inside the method `OnGet` the message "Your contact page." will be set. This message gets displayed on the specific Contact page:

~~~ html
@page
@model ContactModel
@{
    ViewData["Title"] = "Contact";
}
<h2>@ViewData["Title"].</h2>
<h3>@Model.Message</h3>
~~~

As you can see in the razor page, the `PageModel` is actually the model of that view which gets passed to the view, the same way as in ASP.Net MVC. The only difference is, that there's no Action to write code for and something will invoke the `OnGet` Method in the `PageModel`.

## Conclusion

This is just a pretty fast first look, but the Razor Pages seem to be pretty cool for small and low budget projects, e. g. for promotional micro sites with less of dynamic stuff. There's less code to write and less things to ramp up, no controllers and actions to think about. This makes it pretty easy to quickly start a project or to prototype some features. 

But there's no limitation to do just small projects. It is a real ASP.NET Core application, which get's compiled and can easily use additional libraries. Even Dependency Injection is available in ASP.NET Core RazorPages. That means it is possible to let the application grow.

Even though it is possible to use the MVC concept in parallel by adding the Controllers and the Views folders to that application. You can also share the `_layout.cshtml` between both, the Razor Pages and the VMC Views, just by telling the `_ViewStart.cshtml` where the `_layout.cshtml` is. 

Don't believe me? Try this out: [https://github.com/juergengutsch/razor-pages-demo/](https://github.com/juergengutsch/razor-pages-demo/)

I'm pretty sure I'll use it in some of my real projects in the future.