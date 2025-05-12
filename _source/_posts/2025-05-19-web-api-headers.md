---
layout: post
title: "ASP.NET Core Web API Headers"
teaser: "This is yet another security post mainly for me as a reminder. The wrong headers can cause security issues as well as missing HTTP headers. This post will list those headers and describe how to solve the issues."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- AppSec
- Headers
---

This is yet another security post mainly for me as a reminder. Actually it happens quite often that I'm searching the internet for a solution and the results show me that I already wrote about it. So this post is for me the next time I search for solving security header issues with ASP.NET Core Web API.

The wrong headers can cause security issues as well as missing HTTP headers. This post will list those headers and describe how to solve the issues.

## About HTTP Headers in general

HTTP Headers are transporting meta information that might or might not be relevant or important for the receiving party. Request headers can contain credentials to login, information about the requested language or the requested content types. Response headers for example can contain information about the response like content length, content type, file names. Response headers can also contain information to control the client like cache control, enforcing HTTPS, etc. 

Some of those headers are needed, some of them are important security wise. Also some of them are useless or even dangerous if they are sent to the client or configured wrongly.

Remember: You should make it attackers as hard as possible to know about your system. The less information the attacker gets from the system the harder it is to find vulnerabilities and leaks get into the system.

## Version disclosure vulnerabilities

Unfortunately the IIS tells the client about who he is and also what version he is. Also ASP.NET Core is chatty about it. By default an attacker will know what version of IIS and what version of ASP.NET Core you are using. This can be dangerous, because the attacker can just search for issues on the specific platform and version and use them to get into the system.

You should definitely remove those kind of chatty headers from any response. 

To remove the server header you need to add a web.config file (or adjust an existing one) to your ASP.NET core project. This is needed to configure the IIS that writes the header out on every response:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <security>
      <requestFiltering removeServerHeader="true" />
    </security>
  </system.webServer>
</configuration>
```

This `web.config` can also be deployed to Azure to shut up IIS.

The next one is the `x-powered-by` and the `x-aspnet-version` headers which tells you what version of ASP.NET Core you are using. 

To solve this and other header issues, I'm using a `CustomHeadersMiddleware` to remove chatty headers:

```csharp
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace Pebt.Web.Config;

public class CustomHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly CustomHeadersToAddAndRemove _headers;

    public CustomHeadersMiddleware(RequestDelegate next, CustomHeadersToAddAndRemove headers)
    {
        if (next == null)
        {
            throw new ArgumentNullException(nameof(next));
        }        
        if (headers == null)
        {
            throw new ArgumentNullException(nameof(headers));
        }
        _next = next;
        _headers = headers;
    }

    public async Task Invoke(HttpContext context)
    {
        foreach (var headerValuePair in _headers.HeadersToAdd)
        {
            context.Response.Headers[headerValuePair.Key] = headerValuePair.Value;
        }
        foreach (var header in _headers.HeadersToRemove)
        {
            context.Response.Headers.Remove(header);
        }

        await _next(context);
    }
}

public static class CustomHeadersMiddlewareExtensions
{
    /// <summary>
    /// Enable the Customer Headers middleware and specify the headers to add and remove.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="addHeadersAction">
    /// Action to allow you to specify the headers to add and remove.
    /// Example: (opt) =>  opt.HeadersToAdd.Add("header","value"); opt.HeadersToRemove.Add("header");</param>
    /// <returns></returns>
    public static IApplicationBuilder UseCustomHeaders(
        this IApplicationBuilder builder, 
        Action<CustomHeadersToAddAndRemove> addHeadersAction)
    {
        var headers = new CustomHeadersToAddAndRemove();
        addHeadersAction?.Invoke(headers);
        builder.UseMiddleware<CustomHeadersMiddleware>(headers);
        return builder;
    }
}

public class CustomHeadersToAddAndRemove
{
    public Dictionary<string, string> HeadersToAdd { get; } = new();
    public HashSet<string> HeadersToRemove { get ; } = new();
}

```

And I'm using it like this to remove those headers:

```csharp
app.UseCustomHeaders((opt) =>
{
	// ...
    opt.HeadersToRemove.Add("X-Powered-By");
    opt.HeadersToRemove.Add("x-aspnet-version");
});
```

> **Note:**
>
> **Version disclosure vulnerabilities** can be happen with more than just chatty headers. Also error pages can expose versions and other critical information. Avoid exposing detailed error messages in production environments and show user friendly error pages. 
>
> ASP.NET Core already comes with prepared templates that show detailled error pages in development mode only:
>
> ``` csharp
> if (builder.Environment.IsDevelopment())
> {
>     app.UseDeveloperExceptionPage();
> }
> ```

todo
