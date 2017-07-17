---
layout: post
title: "New Visual Studio Web Application: The ASP.NET Core Razor Pages"
teaser: "Description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- Razor Pages
---

I think, everyone who followed the last couple of ASP.NET Community Standup session heard about Razor Pages. Did you try the Razor Pages. I didn't. I focused completely on ASP.NET Core MVC and Web API. With this post I will have a first look into it. To try it out, you need to have the latest preview on Visual Studio 2017 installed on your machine, because the Razor Pages came with ASP.NET Core 2.0 preview.

## Creating a Razor Pages project

Using Visual Studio 2017, I used "File... New Project" to create a new project. I navigate to ".NET Core", chose the "ASP.NET Core Web Application (.NET Core)" project  and I chose a name and a location for that project.

![]()

In the next dialogue, I needed to switch to ASP.NET Core 2.0 to see all the new available project types. (I will write about the other one in the next posts.) I selected the "Web Application (Razor Pages)" and pressed "OK".

![]()

## Program.cs and Startup.cs

I you are already familiar with ASP.NET core projects, you'll find nothing new in the Program.cs and in the Startup.cs. Both files look pretty much the same as in ASP.NET Core projects.

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

The Startup.cs has a services.AddMvc() and an app.UseMvc() with a configured route:

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

That means the Razor Pages are part of the MVC framework, as Damien Edwards always said in the Community Standup. 

## The solution 

But the solution looks a little different. Instead of a MVC, Views and Controller folders, there is only a pages folder with the razor files in it. Even there are known files: the _layout.cshtml, _ViewImports.cshtml, _ViewStart.cshtml.

![]()

Within the _ViewImports.cshtml awe also have the import of the default TagHelpers

~~~ razor
@namespace RazorPages.Pages
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
~~~

This makes sense, since the Razor Pages are part of the MVC Framework. 

We also have the standard pages of every new ASP.NET project. As every new web project in Visual Studio, also this is ready to run. Pressing F5 starts the web application and opens the browser

![]()

## Frontend

For the frontend dependencies "bower" is used. It will put all the stuff into wwwroot/bin. So even this is working the same way as in MVC. Custom CSS and custom JavaScript is in the css and the js folder in wwwroot. This should all be familiar for ASP.NET Corer developers.

Also the way the resources are used in the _Layout.cshtml.

## Welcome back "Code Behind"

In the solution explorer, you can see that there are files nested under the Index, About, Contact and Error pages. This files are looking almost like Code Behind files:

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

They are not called Page, but Model and they have something like an handler in it to do something on a specific action. It seems to be a GET in this case. On GET the message "Your contact page." will be set. This message gets displayed on the specific page:

~~~ html
@page
@model ContactModel
@{
    ViewData["Title"] = "Contact";
}
<h2>@ViewData["Title"].</h2>
<h3>@Model.Message</h3>
~~~

It is not really a code behind page as in the classic web form pages. As you can see in the razor page, it is the Model passed to the view, the same way as in ASP.Net MVC. The only difference is, that there's no Action to write code for and something will call the OnGet Method in the PageModel.

## Conclusion

Tis is just a pretty fast first look, but the Razor Pages seem to be pretty cool for small and low budget projects, e. g. for promotion micro sites with less of dynamic stuff. There's less code to write and less things to ramp up, no controllers and actions to think about. I'm pretty sure I'll use it in such projects.