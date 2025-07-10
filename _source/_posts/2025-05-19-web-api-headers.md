---
layout: post
title: "Security Headers for ASP.NET Core"
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

This is yet another security post mainly for me as a reminder. Actually it happens quite often that I'm searching the internet for a solution and the results show me that I already wrote about it. So this post is for me the next time I search for solving security header issues with ASP.NET Core.

The wrong headers can cause security issues as well as missing HTTP headers. This post will list those headers and describe how to solve the issues.

## About HTTP Headers in general

HTTP Headers are transporting meta information that might or might not be relevant or important for the receiving party. Request headers can contain credentials to login, information about the requested language or the requested content types. Response headers for example can contain information about the response like content length, content type, file names. Response headers can also contain information to control the client like cache control, enforcing HTTPS, etc. 

Some of those headers are needed, some of them are important security wise. Also some of them are useless or even dangerous if they are sent to the client or configured wrongly.

Remember: You should make it attackers as hard as possible to know about your system. The less information the attacker gets from the system the harder it is to find vulnerabilities and leaks get into the system.

## Remove chatty HTTP-headers

Chatty headers can be dangerous, because an attacker can just search for issues on the specific platform and version and use them to get into the system:

![image-20250512212916984](C:\Users\JürgenGutsch\AppData\Roaming\Typora\typora-user-images\image-20250512212916984.png)

This screen shows the headers of a page running ASP.NET 4.0.30319 on IIS 8.0. We can now search the web for known vulnerabilities within these versions. 

> Not only IIS is a chatty webserver. Also nginx, Apache and Kestrel like to tell the world about.

You should definitely remove those kind of chatty headers from any response:

* Server 
* X-Aspnet-Version
* X-Powered-By

### IIS Server Header

To remove the `Server`-header you need to add a `web.config` file (or adjust an existing one) to your ASP.NET core project. This is needed to configure the IIS that writes the header out on every response:

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

### Kestrel Server Header

Also the Kestrel web server is writing a Server header out to the response headers. The `web.config` is not working here. To remove the Kestrel server header you can use the `KestrelServerOptions`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});
```

### X-PoweredBy and X-Aspnet-Version

Actually the latest versions of ASP.NET Core don't expose those headers. In case your application does here is the way you can remove them.

The next ones are the `X-Powered-By` and the `X-Aspnet-Version` headers which tells you what version of ASP.NET Core you are using. 

To solve this and other header issues, I'm using a `CustomHeadersMiddleware` to remove chatty headers:

```csharp
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace Application.Configuration;

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

And I'm using the middleware like this to remove those headers:

```csharp
app.UseCustomHeaders((opt) =>
{
	// ...
    opt.HeadersToRemove.Add("X-Powered-By");
    opt.HeadersToRemove.Add("x-aspnet-version");
});
```

> **Note:**
>**Version disclosure vulnerabilities** can be happen with more than just chatty headers. Also error pages can expose versions and other critical information. Avoid exposing detailed error messages in production environments and show user friendly error pages. 
> 
>ASP.NET Core already comes with prepared templates that show detailed error pages in development mode only:
> 
>``` csharp
> if (builder.Environment.IsDevelopment())
> {
>  app.UseDeveloperExceptionPage();
>    }
> ```

## Adding Security Headers



### Strict-Transport-Security

This header enforces HTTPS and enables HSTS to avoid man-in-the-middle attacks. It specifies ho long the certificate should be cached on the client side to validate the certificate on future requests. This way the certificate can't be changed or replaced by a man-in-the-middle.

```
app.UseCustomHeaders((opt) =>
{
	...
    opt.HeadersToAdd.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    ...
});
```

### X-Frame-Options

This header blocks the page to be shown inside a frame. This also avoids clickjacking 

```
app.UseCustomHeaders((opt) =>
{
	...
    opt.HeadersToAdd.Add("X-Frame-Options", "DENY");
    ...
});
```

### X-XSS-Protection

This is depricated header to block cross-side-scripting, It is a non-standard header and not longer supported by modern browser, but may help with browsers that don't support the CSP headers. 

``` 
app.UseCustomHeaders((opt) =>
{
	...
    opt.HeadersToAdd.Add("X-XSS-Protection", "1; mode=block");
    ...
});
```

### X-Content-Type-Options



```
app.UseCustomHeaders((opt) =>
{
	...
    opt.HeadersToAdd.Add("X-Content-Type-Options", "nosniff");
    ...
});
```

### Content-Security-Policy



```
app.UseCustomHeaders((opt) =>
{
	...
    opt.HeadersToAdd.Add("Content-Security-Policy", "default-src 'self';script-src 'unsafe-inline' 'self';style-src 'unsafe-inline' 'self';object-src 'none';base-uri 'self';connect-src 'self';font-src 'self';frame-src 'self';img-src 'self' data:;manifest-src 'self';media-src 'self';worker-src 'none';");
    ...
});
```

