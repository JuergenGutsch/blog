---
layout: post
title: "ASP.NET Core in .NET 6 - Support for custom event arguments in Blazor"
teaser: "This is the seventh part of the ASP.NET Core on .NET 6 series. In this post, I want to have a quick  into the support for custom event arguments in Blazor."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- .NET 6
---

This is the seventh part of the [ASP.NET Core on .NET 6 series]({% post_url aspnetcore6-01.md %}). In this post, I want to have a quick into the support for custom event arguments in Blazor.

In Blazor you can create custom events and Microsoft now added the support for custom event arguments for those custom events in Blazor as well. Microsoft added a sample in the [blog post about the preview 2](https://devblogs.microsoft.com/aspnet/asp-net-core-updates-in-net-6-preview-2/) that I like to try in a small Blazor project.

## Exploring custom event arguments in Blazor

 At first, I'm going to create a new Blazor WebAssembly project using the .NET CLI:

``` shell
dotnet new blazorwasm -n BlazorCustomEventArgs -o BlazorCustomEventArgs
cd BlazorCustomEventArgs
code .
```

These commands create the project, change the directory into the project folder, and opens VSCode.

After VSCode opens, I create a new folder called `CustomEvents` and place a new C# file called `CustomPasteEventArgs.cs` in it. This file contains the first snippet:

~~~csharp
using System;
using Microsoft.AspNetCore.Components;

namespace BlazorCustomEventArgs.CustomEvents
{
    [EventHandler("oncustompaste", typeof(CustomPasteEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
    public static class EventHandlers
    {
        // This static class doesn't need to contain any members. It's just a place where we can put
        // [EventHandler] attributes to configure event types on the Razor compiler. This affects the
        // compiler output as well as code completions in the editor.
    }

    public class CustomPasteEventArgs : EventArgs
    {
        // Data for these properties will be supplied by custom JavaScript logic
        public DateTime EventTimestamp { get; set; }
        public string PastedData { get; set; }
    }
}
~~~

Additionally, I added a namespace to be complete.

In the `Index.razor` in the Pages folder we add the next snippet of the blog post:

~~~html
@page "/"
@using BlazorCustomEventArgs.CustomEvents

<p>Try pasting into the following text box:</p>
<input @oncustompaste="HandleCustomPaste" />
<p>@message</p>

@code {
    string message;

    void HandleCustomPaste(CustomPasteEventArgs eventArgs)
    {
        message = $"At {eventArgs.EventTimestamp.ToShortTimeString()}, you pasted: {eventArgs.PastedData}";
    }
}
~~~

I need to add the `using` to match the namespace of the `CustomPasteEventArgs`. This creates an `input` element and outputs a message that will be generated on the `CustomPaste` event handler. 

At the end, we need to add some JavaScript in the `index.html` that is located in the `wwwroot` folder. This file hosts the actual WebAssembly application. Place this script directly after the script tag for the `blazor.webassembly.js`:

~~~html
<script>
    Blazor.registerCustomEventType('custompaste', {
        browserEventName: 'paste',
        createEventArgs: event => {
            // This example only deals with pasting text, but you could use arbitrary JavaScript APIs
            // to deal with users pasting other types of data, such as images
            return {
                eventTimestamp: new Date(),
                pastedData: event.clipboardData.getData('text')
            };
        }
    });
</script>
~~~

This binds the default `paste` event to the `custompaste` event and adds the pasted text data, as well as the current date to the `CustomPasteEventArgs`. In that case the JavaScript object literal should match the `CustomPasteEventArg` to get it working property, except the casing of the properties.

> Blazor doesn't protect you to write some JavaScript ;-)

Let's try it out. I run the application by calling the `dotnet run` command or the `dotnet watch` command in the console:

![dotnet run]({{site.baseurl}}/img/aspnetcore6/run-blazor.png)

If the browser doesn't start automatically copy the displayed HTTPS URL into the browser. It should look like this:

![custom event args 1]({{site.baseurl}}/img/aspnetcore6/customevent01.png)

Now I past some text into the `input` element. Et voilà:

![custom event args 2]({{site.baseurl}}/img/aspnetcore6/customevent02.png)Don't be confused about the date. Since it is created via JavaScript using `new Date()`  it is a UTC date, which means minus two hours within the CET time zone, during daylight saving time. 

## What's next?

In the next part In going to look into the support for [CSS isolation for MVC Views and Razor Pages]({% post_url aspnetcore6-08-css-isolation.md %}) in ASP.NET Core.