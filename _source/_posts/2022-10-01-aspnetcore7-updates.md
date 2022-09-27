---
layout: post
title: "ASP.NET Core 7 updates"
teaser: "Description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
---

Release candidate 1 of ASP.NET Core 7 is out for around two weeks and the release date isn't that far. The beginning of November usually is the time when Microsoft is releasing the new version of .NET. Please find the announcement post here: [ASP.NET Core updates in .NET 7 Release Candidate 1](https://devblogs.microsoft.com/dotnet/asp-net-core-updates-in-dotnet-7-rc-1/). I will not repeat this post but pick some personal highlights to write about.

## ASP.NET Core Roadmap for .NET 7

First of all, a [look at the ASP.NET Core roadmap for .NET 7](https://github.com/dotnet/aspnetcore/issues/39504) shows us, that there are only a few issues open and planned for the upcoming release. That means the release is complete and almost only bugfixes will be pushed to that release. Many other open issues are already stroked through and probably assigned to a later release. Guess we'll have a published roadmap for ASP.NET Core on .NET 8 soon. At the latest at the beginning of November.

What are the updates of this RC 1?

## A lot of Blazor

Even this release is full of Blazor improvements. Those who are working a lot with Blazor will be happy about improved JavaScript interop, debugging improvements, handling location-changing events, and dynamic authentication requests. 

Actually, Blazor isn't that relevant for me, since I see a downside in each, Blazor Server and Blazor WASM that are blocking me from using it for web projects that need to be accessible from the internet. I'm sure there are scenarios where Blazor makes a lot of sense, unfortunately not in my current projects, neither in projects of the company I work for.

However, there are some quite interesting improvements within this release that might be great for almost every ASP.NET Core developer:

## Faster HTTP/2 uploads and HTTP3 performance improvements

The team increases the default upload connection window size of HTTP/2 which results in a much faster upload time. Stream handling is always tricky and needs a lot of fine tuning to find the right balance. Improving the upload speed by more than five times is awesome and really helpful to upload bigger files. Even in HTTP/3 the performance was increased by reducing HTTP/3 allocations. Feature parity with HTTP/1, HTTP/2 and HTTP/3 is as useful as Server Name Indication (SNI) when configuring connection certificates. 

## Rate limiting middleware improvements

The rate limiting middleware got some small improvements to make it easier and more flexible to configure. You can now add attributes to  controller actions to enable or disable rate limiting on specific endpoints. To do the same on Minimal API endpoints and endpoint groups you can use methods to enable or disable rate limiting. This way you can enable rate limiting for a endpoint group, but disable it for a specific one inside this group.

On botch attributes, endpoints and endpoint groups methods you can specify the rate limiting policy. Unlike to the attributes who support named policies only the Minimal API methods can also take and instance of a policy.

## Experimental stuff added to this release

WebTransport is a new draft specification for HTTP/3 that works similar to WebSockets, but supports multiple streams per connection. The support for WebTransport is now added as an experimental feature to the RC1

One of the new features in .NET 7 is gRPC JSON transcoding to torn gRPC APIs into RESTful APIs. Any RESTful API should have a OpenAPI documentation, so does gRPC JSON transcoding as well. This release now contains experimental support to add Swashbuckle Swagger to gRPC to render a OpenAPI documentation

## Conclusion

ASP.NET Core on .NET 7 seems to be complete now and I'm really looking forward to the .NET Conf 2022 beginning of November which will be the launch event for .NET 7.

And exactly this reminds me to start thinking about the next edition of my book "Customizing ASP.NET Core" that needs to be updated to .NET 8 and enhanced by probably three more chapter next year. 





## 
