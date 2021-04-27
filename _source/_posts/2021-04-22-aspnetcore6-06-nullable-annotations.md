---
layout: post
title: "ASP.​NET Core in .NET 6 - Part 06 - Nullable Reference Type Annotations"
teaser: "This is the sixth part of the ASP.NET Core on .NET 6 series. In this post, I want to have a quick into the new Nullable Reference Type Annotations in some ASP.NET Core APIs."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- .NET 6
---

This is the sixth part of the [ASP.NET Core on .NET 6 series]({% post_url aspnetcore6-01.md %}). In this post, I want to have a quick into the new Nullable Reference Type Annotations in some ASP.NET Core APIs

Microsoft added Nullable Reference Types in C# 8 and this is why they applied nullability annotations to parts of ASP.NET Core. This provides additional compile-time safety while using reference types and protects against possible null reference exceptions. 

This is not only a new thing with preview 1 but an ongoing change for the next releases. Microsoft will add more and more nullability annotations to the ASP.NET Core APIs in the next versions. You can see the progress in this GitHub Issue: [https://github.com/aspnet/Announcements/issues/444](https://github.com/aspnet/Announcements/issues/444)

## Exploring Nullable Reference Type Annotations 

I'd quickly like to see whether this change is already visible in a newly created MVC project.

~~~ shell
dotnet new mvc -n NullabilityDemo -o NullabilityDemo
cd NullabilityDemo
~~~

This creates a new MVC project and changes the directory into it.

Projects that enable using nullable annotations may see new build-time warnings from ASP.NET Core APIs. To enable nullable reference types, you should add the following property to your project file:

```xml
<PropertyGroup>
    <Nullable>enable</Nullable>
</PropertyGroup>
```

In the following screenshot you'll see, the build result before and after enabling nullable annotations: 

![null warnings on build]({{site.baseurl}}/img/aspnetcore6/nullable-build.png)

Actually, there is no new warning, It just shows a warning for the `RequestId` property in the `ErrorViewModel` because it might be null. After changing it to a nullable string, the warning disappears.

~~~csharp
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
~~~

However. How can I try the changed APIs?

I need to have a look into the already mentioned [GitHub Issue](https://github.com/aspnet/Announcements/issues/444) to choose an API to try. 

I'm going with the `Microsoft.AspNetCore.WebUtilitiesQueryHelpers.ParseQuery` method:

~~~csharp
using Microsoft.AspNetCore.WebUtilities;

// ...

private void ParseQuery(string queryString)
{
    QueryHelpers.ParseQuery(queryString);
}
~~~

If you now set the `queryString` variable to null, you'll get yellow squiggles that tell you that null may be null:

![null hints]({{site.baseurl}}/img/aspnetcore6/nullable-hints.png)

You get the same message if you mark the input variable with a nullable annotation:

~~~csharp
private void ParseQuery(string? queryString)
{
	QueryHelpers.ParseQuery(queryString);
}
~~~

![nullable hints]({{site.baseurl}}/img/aspnetcore6/nullable-hints2.png)

It's working and it is quite cool to prevent null reference exceptions against ASP.NET Core APIs.

## What's next?

In the next part In going to look into the support for [Support for custom event arguments in Blazor]({% post_url aspnetcore6-07-custom-event-arguments.md %}) in ASP.NET Core.
