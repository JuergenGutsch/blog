---
layout: post
title: "ASP.NET Core in .NET 6 - Part 04 - DynamicComponent in Blazor"
teaser: ""
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- .NET 6
---

This is the fourth part of the [ASP.NET Core on .NET 6 series]({% post_url aspnetcore6-01.md %}). In this post, I want to have a look `DynamicComponent` in Blazor.

What does Microsoft say about it?

> `DynamicComponent` is a new built-in Blazor component that can be used to dynamically render a component specified by type.

That sounds nice. It is a component that dynamically renders any other component. Unfortunately, there is no documentation available yet, except a comment in the blog. So let's create a small one:

## Trying the DynamicComponent

To test it, I created a Blazor Server project using the dotnet CLI

~~~shell
dotnet new blazorserver -n BlazorServerDemo -o BlazorServerDemo
~~~

CD into the project and call `dotnet watch`

Now let's try the `DynamicComponent` on the `index.razor`:

~~~ html 
@page "/"

<h1>Hello, world!</h1>

Welcome to your new app.

<SurveyPrompt Title="How is Blazor working for you?" />
~~~

My idea is to render the `SurveyPrompt` component dynamically with a different title:

~~~html 
@code{
    var someType = typeof(SurveyPrompt);
    var myDictionaryOfParameters = new Dictionary<string, object>
    {
        { "Title", "Foo Bar"}
    };
}

<DynamicComponent Type="@someType" Parameters="@myDictionaryOfParameters" />
~~~

At first, I needed to  define the type of the component I want to render. At second I needed to define the parameters I want to pass to that component. In that case, it is just the title property.

![DynamicComponent]({{site.baseurl}}/img/aspnetcore6/dynamiccomponent.png)

## Why could this be useful?

This is great in case you want to render components dynamically based on data inputs or whatever.

Think about a timeline of news, a newsfeed, or stuff like this on a web page, that can render different kind of content like text, videos, pictures. You can now just loop through the news list and render the `DynamicComponent` and pass the type of the actual component to it, as well as the attribute values the components need.

## What's next?

In the next part In going to look into the support for `ElementReference` in Blazor.