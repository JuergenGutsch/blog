---
layout: post
title: "Hardening ASP.NET Core: HTTP Headers for Security"
teaser: "This is yet another security post. The wrong headers can cause security issues, as well as missing HTTP headers. This post will list those headers and describe how to solve the issues."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- AppSec
- Headers
---

This is yet another security post, also for my own reminder. Actually, it happens pretty often that I'm searching the internet for a solution, and the results show me that I already wrote about it. So this post is also for me the next time I search for solving security header issues with ASP.NET Core.

Web security is not just a feature; it is a must. We focus on authentication, authorization, and good code. However, we often forget about HTTP Headers. These small pieces of data, sent with every request and response, can provide a slight improvement in your application's security.

This post shows you how to use HTTP headers to make your ASP.NET Core application stronger. This applies to MVC, Razor Pages, Blazor Server, or Web API. We will do two things: add **security headers** for clients (like browsers) to enforce rules, and remove **"chatty" headers** that show too much server information.

**The Problem:** Default ASP.NET Core setups often do not use strong HTTP security headers. This can leave your app open to common attacks like XSS or Clickjacking. Additionally, default headers can display information about your server and software stack. This helps attackers.

**The Solution:** You must actively add strong security headers and remove verbose ones. This two-part plan enhances your app's security and reduces potential attack points.

## Part 1: Adding Security HTTP Headers to ASP.NET Core

Here, we cover important security headers. You will learn what they do and how to add them to your ASP.NET Core application.

### 1. Content Security Policy (CSP)

CSP helps to stop **Cross-Site Scripting (XSS)** and other content injection attacks. It lets you define which sources (scripts, styles, images) the browser can load.

- **What it is:** An HTTP response header. It instructs browsers to load content only from trusted sources. If a script tries to load from an unknown place, the browser blocks it.

- **Why use it:** It is your first defense against XSS. CSP stops malicious scripts from running and reducing the impact of attacks.

- **How to add in ASP.NET Core:** Use a middleware in the `Program.cs`:

  ```csharp
  app.Use(async (context, next) =>
  {
      // Define your CSP rules. Be strict.
      context.Response.Headers.Add("Content-Security-Policy",
          "default-src 'self'; " + // Only allow resources from your own domain
          "script-src 'self' 'unsafe-inline'; " + // Use 'unsafe-inline' with care!
          "style-src 'self' 'unsafe-inline'; " +
          "img-src 'self' data:; " +
          "font-src 'self'; " +
          "connect-src 'self'; " +
          "frame-ancestors 'none'; " + // Important for Clickjacking
          "form-action 'self';"
          // "report-uri /csp-report-endpoint;" // Optional: for getting reports
      );
      await next();
  });
  ```

  - **Tip 1:** Start CSP in `report-only` mode (`Content-Security-Policy-Report-Only`) with a `report-uri`. This helps you find issues without breaking your app. Switch to `Content-Security-Policy` later. Be very careful with `'unsafe-inline'`. Use hashes or nonces instead if possible.
  - **Tip 2:** You can also generate your CSP using a generator like this: https://report-uri.com/home/generate
    This generator can also take your app and propose a CSP header or validate your existing CSP

### 2. X-Content-Type-Options: NoSniff

This header prevents MIME-sniffing attacks. Browsers might guess content types, which can be a security risk.

- **What it is:** The `X-Content-Type-Options` header with `nosniff`. It tells the browser to use the `Content-Type` header you send and not to guess.

- **Why use it:** Stops browsers from misinterpreting files (e.g., running text as code). This blocks some XSS attacks.

- **How to add in ASP.NET Core:** Use a middleware in the `Program.cs`:

  ```csharp
  app.Use(async (context, next) =>
  {
      context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
      await next();
  });
  ```

### 3. X-Frame-Options: DENY or SAMEORIGIN

**Clickjacking** is an attack where a bad site can put your site in a hidden frame. This tricks users. `X-Frame-Options` stops this.

- **What it is:** This header controls if your app's content can be put into `<iframe>`, `<frame>`, or `<object>` tags.

- **Why use it:** To prevent clickjacking.

  - `DENY`: Stops any other domain from framing your content. This is the most secure option.
  - `SAMEORIGIN`: Allows framing only if the content is from the same domain.

- **How to add in ASP.NET Core:** Use a middleware in the `Program.cs`:

  ```csharp
  app.Use(async (context, next) =>
  {
      context.Response.Headers.Add("X-Frame-Options", "DENY"); // Use DENY for most security
      await next();
  });
  ```

### 4. Strict-Transport-Security (HSTS)

HSTS is very important for security. It forces browsers to always use HTTPS for your site.

- **What it is:** An HTTP response header. It tells browsers to only use HTTPS for your site for a set time (`max-age`).

- **Why use it:** It ensures all communication is encrypted. It stops man-in-the-middle attacks.

- **How to add in ASP.NET Core:** ASP.NET Core has built-in HSTS support that is enabled by default on some project templates.

  ```csharp
  if (!app.Environment.IsDevelopment())
  {
      app.UseHsts(); // Adds Strict-Transport-Security header
  }
  ```

  You can set more options:

  ```csharp
  builder.Services.AddHsts(options =>
  {
      options.Preload = true; // For browser HSTS preload lists
      options.IncludeSubDomains = true;
      options.MaxAge = TimeSpan.FromDays(365); // Recommended: at least 1 year
  });
  ```

  - **Important:** HSTS needs a secure HTTPS connection first. Make sure your app is set up for HTTPS from the start.

### 5. Referrer-Policy

This header controls how much referrer information (the previous URL) is sent with the  requests. This is for user privacy and to prevent information leaks.

- **What it is:** An HTTP header. It controls the `Referer` header value sent by the client.

- **Why use it:** To improve user privacy. It limits sensitive URL information going to other sites and stops some tracking.

- **How to add in ASP.NET Core:**

  ```csharp
  app.Use(async (context, next) =>
  {
      // A good policy:
      // 'strict-origin-when-cross-origin' sends full URL for same-domain requests,
      // but only the origin for cross-domain requests.
      context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
      await next();
  });
  ```

### 6. Permissions-Policy

This header gives you control over browser features (like camera, mic, location) your app and its embedded content can use.

- **What it is:** A security header. It enables or disables browser features for your document and iframes.

- **Why use it:** To improve security and privacy. You declare what features your site uses. This limits bad scripts or third-party content.

- **How to add in ASP.NET Core:**

  ```csharp
  app.Use(async (context, next) =>
  {
      // Example: Only allow 'geolocation' for your own domain
      context.Response.Headers.Add("Permissions-Policy", "geolocation=(self)");
      // Example: Disable camera and microphone completely
      // context.Response.Headers.Add("Permissions-Policy", "camera=(), microphone=()");
      await next();
  });
  ```

## Part 2: Removing Chatty HTTP Headers from ASP.NET Core

Adding security headers makes your app strong. Removing "chatty" headers makes it stealthy. These headers can show details about your server. This helps attackers.

### 1. Server Header

The `Server` header shows your web server software (e.g., Kestrel, IIS).

- **What it is:** Identifies the web server software.

- **Why remove it:** Hiding server details makes it harder for attackers to find known vulnerabilities. This is a small security step, but useful.

- **How to remove in ASP.NET Core:**

  For Kestrel:

  ```csharp
  // In Program.cs (for .NET 6+) or ConfigureServices in Startup.cs
  builder.WebHost.ConfigureKestrel(serverOptions =>
  {
      serverOptions.AddServerHeader = false; // Turn off Server header
  });
  ```

  If you use IIS, you might need to change `web.config` or IIS Manager settings.

  ~~~xml
  <?xml version="1.0" encoding="utf-8"?>
  <configuration>
    <system.webServer>
      <security>
        <requestFiltering removeServerHeader="true" />
      </security>
    </system.webServer>
  </configuration>
  ~~~

### 2. X-Powered-By and X-Aspnet-VersionHeader

Those header often shows the software stack, like "ASP.NET" and the version used

- **What it is:** Shows the application framework and version used.

- **Why remove it:** Stops technology fingerprinting. This makes it harder for tools to find known framework flaws.

- **How to remove in ASP.NET Core:** Newer versions of ASP.NET Core don't add `X-Powered-By` by default. If you see it, you can also remove it using a middleware

  ~~~csharp
  app.Use(async (context, next) =>
  {
      context.Response.Headers.Remove("x-powered-by");
      context.Response.Headers.Remove("x-aspnet-version");
      await next();
  });
  ~~~

## How to Implement and Best Practices

Changing these headers needs careful planning. You want security benefits without breaking your app.

- **Middleware Order is Key:** In ASP.NET Core, the order of middleware in `Program.cs` (or `Startup.cs`) matters. Put your header middleware early. This ensures headers are on all responses.
- **Test Thoroughly:** After header changes, especially CSP, test your entire application. Use different browsers. Check all features. Look at response headers with developer tools or Postman/curl. Make sure nothing is broken.
- **Use Security Scanners:** Tools like OWASP ZAP or Burp Suite can check your header settings. They can find missing or wrong headers.
- **Security by Design:** Make adding security headers a standard part of your ASP.NET Core project setup. Build security in from the start.
- **Centralize Header Code:** For bigger apps, put your header logic in a custom middleware or an extension method. This makes it reusable, easier to maintain, and consistent.

### 3. A generic middleware to manage http headers

To solve this and other header issues, I'm using a `CustomHeadersMiddleware` to remove or add headers:

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

And I'm using the middleware like this to remove or add those headers:

```csharp
app.UseCustomHeaders((opt) =>
{
    // headers to add:     
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; " + 
        "script-src 'self' 'unsafe-inline'; " + 
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "font-src 'self'; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none'; " +
        "form-action 'self';"
        // "report-uri /csp-report-endpoint;" // Optional: for getting reports
    );
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY"); 
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    opt.HeadersToAdd.Add("X-Frame-Options", "DENY");
    // ...
    
	// headers to remove
    opt.HeadersToRemove.Add("X-Powered-By");
    opt.HeadersToRemove.Add("x-aspnet-version");
    // ..
});
```

## Conclusion

Securing your ASP.NET Core application means more than just input validation. It is a full approach, covering every part of web communication. By adding good security headers and removing chatty ones, you make your application much stronger.

This is not a one-time task. Threats change. Your security must change too. Regularly check your header settings for example with [securityheaders.com](https://securityheaders.com/). Stay updated on security best practices. Keep building secure web applications.
