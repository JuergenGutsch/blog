---
layout: post
title: "ASP.​NET Core on .NET 7.0 - File upload and streams using Minimal API"
teaser: "It seems the Minimal API that got introduced in ASP.NET Core 6.0 will now be finished in 7.0. One feature that was heavily missed in 6.0 was the File Upload, as well as the possibility to read the request body as a stream. Let's have a look how this would look alike. "
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Minimal API
---

It seems the Minimal API that got introduced in ASP.NET Core 6.0 will now be finished in 7.0. One feature that was heavily missed in 6.0 was the File Upload, as well as the possibility to read the request body as a stream. Let's have a look how this would look alike. 

## The Minimal API

Creating endpoints using the Minimal API is great for beginners, or to create small endpoints like for microservice applications, or of your endpoints need to be super fast, without the overhead of binding routes to controllers and actions. However, endpoints created with the Minimal API might be quite useful. 

By adding the mentioned features they are even more useful. And many more Minimal PI improvements will come in ASP.NET Core 7.0.

 To try this I created a new empty web app using the .NET CLI

```shell
dotnet new web -n MinimalApi -o MinimalApi
cd MinimalApi
code .
```

This will create the new project and opens it in VSCode.

Inside VSCode open the Program.cs that should look like this

```Csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
```

Here we see a simple endpoint that sends a "Hello World!" on a GET request.

## Uploading files using `IFormFile` and `IFormFileCollection`

To upload files we should map an endpoint that listens to POST

Inside the `Program.cs`, lets create two endpoints, one that receives a `IFormFile` and another one that receives a `IFormFileCollection`

```csharp
app.MapPost("/upload", async(IFormFile file) =>
{
    string tempfile = CreateTempfilePath();
    using var stream = File.OpenWrite(tempfile);
    await file.CopyToAsync(stream);

    // dom more fancy stuff with the IFormFile
});

app.MapPost("/uploadmany", async (IFormFileCollection myFiles) => 
{
    foreach (var file in files)
    {
        string tempfile = CreateTempfilePath();
        using var stream = File.OpenWrite(tempfile);
        await file.CopyToAsync(stream);

        // dom more fancy stuff with the IFormFile
    }
});
```

The `IFormfile` is the regular interface `Microsoft.AspNetCore.Http.IFormFile` that contains all the useful information about the uploaded file, like `FileName`, `ContentType`, `FileSize`, etc.

The `CreateTempfilePath` that is used here is a small method I wrote to generate a temp file and a path to it. It also creates the folder in case it doesn't exist:

```csharp
static string CreateTempfilePath()
{
    var filename = $"{Guid.NewGuid()}.tmp";
    var directoryPath = Path.Combine("temp", "uploads");
    if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

    return Path.Combine(directoryPath, filename);
}
```

The creation of a temporary filename like this is needed because the actual filename and extension should be exposed to the filesystem for security reason.

Once the file is saved, you can do whatever you need to do with it. 

> **Important note:**
> Currently the file upload doesn't work in case there is a cookie header in the POST request or in case authentication is enabled. This will be fixed in one of the next preview versions. For now you should delete the cookies before sending the request

![iformfile]({{site.baseurl}}/img/aspnetcore7/iformfile.png)

# Read the request body as stream

This is cool, you can now read the body of a request as a stream and do what ever you like to do. To try it out I created another endpoint into the `Program.cs`:

```csharp
app.MapPost("v2/stream", async (Stream body) =>
{
    string tempfile = CreateTempfilePath();
    using var stream = File.OpenWrite(tempfile);
    await body.CopyToAsync(stream);
});
```

I'm going to use this endpoint to to store a binary in the file system. BTW: This stream is readonly and not buffered, that means it can only be read once:

![request body as stream]({{site.baseurl}}/img/aspnetcore7/stream.png)

It works the same way by using a `PipeReader` instead of a `Stream`:

```csharp
app.MapPost("v3/stream", async (PipeReader body) =>
{
    string tempfile = CreateTempfilePath();
    using var stream = File.OpenWrite(tempfile);
    await body.CopyToAsync(stream);
});
```

## Conclusion

This features makes the Minimal API much more useful. What do you think? Please drop a comment about your opinion.

This aren't the only new features that will come in ASP.NET Core 7.0, many more will come. I'm really looking forward to the route grouping that is announced in the roadmap. 
