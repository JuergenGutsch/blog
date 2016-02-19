--- 
layout: post
title: "10 ways to extend your Razor views in ASP.NET core - the complete overview"
teaser: "Currently there are many ways to extend or to organize your Razor views in ASP.NET Core MVC. This post is about the different ways and how to use it"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- MVC
- Razor
---

Currently there are many ways to extend or to organize your Razor views in ASP.NET Core MVC. Let us start with the very basics and let us go to the more complex ways. If your are familiar with previous ASP.NET MVC Frameworks you'll definitely know most of this. But almost all of that "old" stuff is still possible in ASP.NET Core MVC. Some of them listed below shouldn't be used anymore and some of that stuff is completely new in ASP.NET Core MVC. With this post I'm going to try to write down all options to organize and extend MVC Views.

## #1: Typed Views

A very basic View without any dynamic stuff  is not very common. Even in Visual Studio it is not really visible, because you usually get a running per-configured web, if you start a new project. It is simply a HTML page with *.cshtml as the file extension. You can use Razor syntax, HtmlHelpers and UrlHelpers here to make your HTML code more dynamic. You can use the ViewBag Object or the ViewData collection to pass Data from your Controller action to your View. But this data are not typed and you don't really know whether the data exist in this list or what type the data are. 

To use typed date in your view, you need to define a model to use in your view.

~~~ aspnet
@model ExtendViews.ViewModels.AboutModel

<!-- usage: --->
@Model.FullName
~~~

This is pretty common for ASP.NET MVC developers, even the next topic is a known and pretty basic way:

## #2: Layouts

Almost equal to the MasterPages in ASP.NET WebForms, there is a central way to define the basic layout of your Razor view. This is done with a _Layout.cshtml, which is located in the Views\Shared\ folder. This file usually contains the HTML header, body tags and all the things which are shared between all of your Views.

You can also nest layout views to have a basic layout and different per area on your web site. To use a Layout you need to call it by its name without the file extension:

~~~ aspnet
@{
    Layout = "_Layout";
} 
~~~

This call needs to be in the first lines of your views. But you don't need to define the Layout in every view, if you already have defined a default Layout. This is already done, if you start a new ASP.NET Core project in Visual Studio. There is a _ViewStart.cshtml in the Views folder where the default Layout is set-up.

Inside the _Layout.cshtml there is a mothod call` RenderBody()`, which calls the rendering ov the current view at this location:
 
~~~ aspnet
@RenderBody()
~~~

Place this method call at that location where where your view should be rendered.

## #3: Sections

Sometimes you need to create HTML code in your view, which should be rendered on another location than the main parts of the view. This can be done with Sections. Sections are named areas in your view and usually used to put JavaScripts from your views in to a separate location, e.g. at the end of the page.

To define a section for some JavaScripts just call the Section you want to render somewhere in the _Layout.cshtml:

~~~ aspnet
@RenderSection("scripts", required: false)
~~~

With the flag required you are able to define whether the sections is needed or optional. Now you can use the section in your view:

~~~ aspnet
@section scripts
{
    <script>
        $(function() {
            // some more js code here;
        });
    </script>
}
~~~

If you use nested layouts, you probably  need to nest this areas. This means you need to call the `RenderSection()` inside a Section:

~~~ aspnet
@section scripts
{
	@RenderSection("scripts", required: false)
}
~~~

## #4: PartialViews

To reuse parts of your views you can extract this parts and put it into a new Razor view. This view doesn't have an own Action in the controller. This thing is called a PartialView. A PartialView should be placed in the same folder as the View which uses the PartialView or even in the Views\Shared\ folder.

A PartialView can also be a typed view (but don't have to) to get data from the parent View:

~~~ aspnet
@model IEnumerable<UserModel>
@if (Model.Any())
{
    <ul>
        @foreach (var user in Model)
        {
            <li>@user.FullName</li>
        }
    </ul>
}
~~~

This PartialView needs a list of users from the parent view

~~~ aspnet
@{ await Html.RenderPartialAsync("Users", Model.Users);}
~~~

If your PartialView doesn't have a model defined, you don't need to pass the second parameter.

## #5: ViewComponents

This is new in ASP.NET Core

Sometimes you need to have something like PartialView, but with some more logic behind. In the past there was a way to use ChildActions to render the results of controller actions into a view. In ASP.NET Core MVC there is a new way (which I already showed in this post about [ViewCmponents]({% post_url viewcomponents-aspnet5.md %})) with ViewComponents. This are a kind of mini MVC inside MVC, which means they have an own Controller, with an own single action and a view. This ViewComponents are completely independent from your current view, but also can get values passed in from your view.

To render a ViewComponent you need to call it like this:

~~~ aspnet
@Component.Invoke("Top10Articles");
~~~

Please have a look at my previews post about ViewComponent to learn how to create your own.

## #6: HtmlHelpers

You can extend the Razor syntax by creating your own extension methods on the HtmlHelper class:

~~~ aspnet
public static class HtmlHelperExtensions
{
    public static HtmlString MyOwnHtmlHelper(this HtmlHelper helper, string message)
    {
        return new HtmlString($"<span>{message}<span>");
    }
}
~~~

This is pretty useful to create reusable parts of your view, which includes some more logic than a PartialView. But even better than HtmlHelper extensions are the new TagHelpers. HtmlHelpers are still a valid option to extend your Views.

## #7: TagHelper

This is pretty new in ASP.NET Core.

This little helpers are extensions of your view, which are looking like real HTML tags. In ASP.NET Core MVC you should use this TagHelpers instead of the HtmlHelpers because they are more cleaner and easier to use. Another huge benefit is Dependency Injection, which can't be used with the HtmlHelpers, because the static context of extension methods. TagHelpers are common classes where we can easily inject services via the constructor.

A pretty simple example on how a TagHelper could look like:

~~~ aspnet
[TargetElement("hi")] 
public class HelloTagHelper : TagHelper 
{ 
    public override void Process(TagHelperContext context, TagHelperOutput output) 
    { 
        output.TagName = "p"; 
        output.Attributes.Add("id", context.UniqueId); 

        output.PreContent.SetContent("Hello "); 
        output.PostContent.SetContent(string.Format(", time is now: {0}",  
                DateTime.Now.ToString("HH:mm"))); 
    } 
}
~~~

This guy defines a HTML Tag called "hi" and renders a p-tag and the contents and the current Time.

Usage:

~~~ html
<hi>John Smith</hi>
~~~

Result:

~~~ html
<p>Hello John Smith, time is now: 18:55</p>
~~~

ASP.NET Core MVC provides many built in TagHelpers to replace the most used HtmlHelpers. E. g. the ActionLink can now replaced with an Anchor TagHelper:

~~~ aspnet
@Html.ActionLink(“About me”, “About”, “Home”)
~~~

The new TagHelper to create a link to an action looks like this:

~~~ html
<a asp-controller=”Home” asp-action=”About”>About me</a>
~~~

The result in both cases is a clean a-Tag with the URL to the about page:

~~~ html
<a href=”/Home/About”>About me</a>
~~~

As you can see the TagHelpers feel more than HTML and they are easier to use and more readable inside the Views.

## #8: Dependency Injection

This is new in ASP.NET Core too.

The biggest improvement to extend your view is dependency injection. Yes, you are able to use DI in your View. Does this really make sense? Doesn't it mess up my view and doesn't it completely break with the MVC pattern? (Questions like this are currently asked on StackOverflow and reddit)

I think, no. Sure, you need be careful and you should only use it, if it is really needed. This could be a valid scenario: If you create a form to edit a user profile, where the user can add its job position, the country where he lives, his city, and so on. I would prefer not to pass the job positions, the country and the cities from the action to the view. I would prefer only to pass the user profile itself and I only want to handle the user profile in the action. This is why it is pretty useful in this case to inject the services which gives me this look-up data. The action and the ViewModel keeps clean and easy to maintain.

Just register your specific service in the method `ConfigureServices` in the `Startup.cs` and use one line of code to inject it into your view:

~~~ aspnet
@inject DiViews.Services.ICountryService CountryService;
~~~

No you are able to use the `ContryService` in your View to fill a SelectBox with list of countries.

I wrote more about Dependency Injection in ASP.NET Core [this posts]({% post_url dependency-injection-in-aspnetcore.md %})).

## #9: Functions

I never used functions in real ASP.NET MVC projects. I only used it with a the razor engine in an Umbraco web. Anyway, this is another possibility to extend your views a little bit. Maybe you have some a more complex view logic, in this case you can write C# methods in an functions area inside your view:

~~~ aspnet
@functions
{
    public string ReverseString(string input)
    {
        return String.Join("", input.Reverse());
    }
}
~~~

## #10: Global view configuration

Last but not least, there is a separate razor file you can use to configure some things globally. Use the _ViewImports.cshtml to configure usings, dependency injections and many more which should be used in all Views.

## Conclusion

There are many ways to extend our views. some of them are known from previous MVC versions and some of them are new. Some of them shouldn't be used anymore because there are new or better ways. But you are free to decide which feature you want to use to get your problems solved.

Did I forget something? Please drop me a comment, to tell me what I need to add. If you miss something what you used in previous MVC versions, this is possibly not longer working in ASP.NET Core MVC (e.g. ChildActions). Anyway, feel free to ask me :)