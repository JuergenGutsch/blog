---
layout: post
title: "Play with Playwright"
teaser: "In this blog post, I don't want to introduce Playwright. I would like to play around with it and to use it a little differently. Instead of testing a pre-hosted web application, I'd like to test a web application that is self hosted in the test project using the WebApplicationFactory."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- UI Test
- Plaiwright
---



## What is Playwright?

[Playwright](https://playwright.dev/) is a Web UI testing framework that supports different languages and is maintained by Microsoft. Playwright can be used with JavaScript/TypeScript, Python, Java and for sure C#. It comes with windowless browser support with various browsers. It has to be used with unit testing frameworks and because of this, you can just run it within your CI/CD pipeline. The syntax is pretty intuitive and I actually love it. Besides that the documentation is really good and helps a lot to easily start working with it. 

In this blog post, I don't want to introduce Playwright. Actually, the website and the documentation is a much better resource to learn about the it. I would like to play around with it and to use it differently. Instead of testing a pre-hosted web application, I'd like to test a web application that is self hosted in the test project using the `WebApplicationFactory`. This way you have really isolated UI tests that don't relate to on another infrastructure and won't fail because of network problems.

Does it work? 

Let's try it:

## Setting up the solution

The following lines create an ASP.NET Core MVC project and an NUnit test project. After that, a solution file will be created and the projects will be added to the solution. The last command adds the NUnit implementation of Playwright to the test project:

~~~shell
dotnet new mvc -n PlayWithPlaywright
dotnet new nunit -n PlayWithPlaywright.Tests
dotnet new sln -n PLayWithPlaywright
dotnet sln add .\PlayWithPlaywright\
dotnet sln add .\PlayWithPlaywright.Tests\

dotnet add .\PlayWithPlaywright.Tests\ package Microsoft.Playwright.NUnit
~~~

Run those commands and build the solution:

~~~shell
dotnet build
~~~

The build is needed to copy a PowerShell script to the output directory of the test project. This PowerShell script is the command line interface to control playwright.

At next we need to install the required browsers to execute the tests via that PowerShell:

~~~shell
.\PlayWithPlaywright.Tests\bin\Debug\net7.0\playwright.ps1 install
~~~

## Generating test code

Using the `codegen` command helps you to autogenerate test code that can be copied to the test project:

~~~shell
.\PlayWithPlaywright.Tests\bin\Debug\net7.0\playwright.ps1 codegen https://asp.net-hacker.rocks/
~~~

This command opens the Playwright Inspector where you can record your test case. While clicking through your application the test code will be generated on the right hand side:

![plaiwright codegen]({{site.baseurl}}/img/playwright/playwright01.png)

Instead of testing an external website like I did, you can also call `codegen` with a locally running application. 

Just copy the generated code into the NUnit test project and fix the namespace and class name to match the namespace of your project.

Using the generated code as an example you will be able to write more the tests manually.

If this is done, just run `dotnet test` to execute the generated test and just to verify that Playwright is working.

## Start playing

Usually Playwright is testing applications that are running somewhere on a server. This as one simple problem: If the test cannot connect to the running application because of network issues the test will fail. Usually a test should only have one single reason to fail: It should fail because the expected behavior didn't occure.

The solution would be to test a web application that is hosted on the same infrastructure and within the same process as the actual test.

Microsoft already provided the possibility to [write integration tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0) against a web application using the [WebApplicationFactory](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.testing.webapplicationfactory-1?view=aspnetcore-7.0). My Idea was to use this `WebApplicationFactory` to host an application that can be tested with Playwright.

Since the WebApplicationFactory also provides a HttpClient, I would expect to have an URL to connect to. That HttpClient would have a BaseAddress that I can use to pass to Playwright.

Would this really work?

## WebApplicationFactory and Playwright

Actually, we can't combine them by default because the `WebApplicationFactory` doesn't really host a web application over HTTP. That means it doesn't use Kestrel to expose an endpoint over HTTP. The `WebApplicationFactory` creates a test server that hosts the application in memory and just simulates an actual HTTP server. 

We need to find a way to start a HTTP server, like Kestrel, to host the application. Actually we could start `WebApplicationBuilder` but the Idea was to reuse the configuration of the Program.cs of the application we want to test. Like it is done with the `WebApplicationFactory`.

[Daniel Donbavand](https://twitter.com/Donbavand/) actually found a solution [how to override the WebApplicationFactory to actually host the application](https://danieldonbavand.com/2022/06/13/using-playwright-with-the-webapplicationfactory-to-test-a-blazor-application/) over HTTP and to get an endpoint that can be used with Playwright. I used Daniels solution but made it a little more Generic. 

Let's see how this works together with Playwright.

First, add a project reference to the web project within the Playwright test project and add a package reference to [Microsoft.AspNetCore.Mvc.Testing](https://nuget.org).

~~~shell
dotnet add .\PlayWithPlaywright.Tests\ reference .\PlayWithPlaywright\

dotnet add .\PlayWithPlaywright.Tests\ package Microsoft.AspNetCore.Mvc.Testing
~~~

The first one is needed to use the `Program.cs` with the `WebApplicationFactory`. The second one adds the `WebApplicationFactory` and the test server to the test project.

To use the `Program` class that is defined in a `Program.cs` that uses the minimal API you can simply add an empty partial Program class to the `Program.cs`.

I just put the following line at the end of the `Program.cs`:

```csharp
public partial class Program { }
```

To make the Playwright tests as generic as possible I created an abstract `SelfHostedPageTest` class that inherits the `PageTest` class that comes with Playwright and use the `CustomWebApplicationFactory` there and just expose the server address to the test class that inherits the `SelfHostedPageTest`:

```csharp
public abstract class SelfHostedPageTest<TEntryPoint> : PageTest where TEntryPoint : class
{
    private readonly CustomWebApplicationFactory<TEntryPoint> _webApplicationFactory;

    public SelfHostedPageTest(Action<IServiceCollection> configureServices)
    {
        _webApplicationFactory = new CustomWebApplicationFactory<TEntryPoint>(configureServices);
    }

    protected string GetServerAddress() => _webApplicationFactory.ServerAddress;
}
```

 The actual Playwright test just inherits the `SelfHostedPageTest` as follows instead of the `PageTest`:

```csharp
public class PlayWithPlaywrightHomeTests : SelfHostedPageTest<Program>
{
    public PlayWithPlaywrightHomeTests() :
        base(services =>
        {
			// configure needed services, like mocked db access, fake mail service, etc.
        }) { }
        
	// ...
}
```

As you can see, I pass in the Program type as generic argument to the `SelfHostedPageTest`. The `CustomWebApplicationFactory` that is used inside is almost the same implementation as done by Daniel. I just added the generic argument for the Program class and added the possibility to pass the service configuration via the constructor:

~~~csharp
internal class CustomWebApplicationFactory<TEntryPoint> :
   WebApplicationFactory<TEntryPoint> where TEntryPoint : class
{
    private readonly Action<IServiceCollection> _configureServices;
    private readonly string _environment;

    public CustomWebApplicationFactory(
        Action<IServiceCollection> configureServices,
        string environment = "Development")
    {
        _configureServices = configureServices; 
        _environment = environment;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_environment);
        base.ConfigureWebHost(builder);

        // Add mock/test services to the builder here
        if(_configureServices is not null)
        {
        	builder.ConfigureServices(_configureServices);
        }
    }
    
    // ...
    
}
~~~

Now we can use `GetServerAddress()` to get the server address and to pass it to the `Page.GotoAsync()` method:

```csharp
[Test]
public async Task TestWithWebApplicationFactory()
{
    var serverAddress = GetServerAddress();

    await Page.GotoAsync(serverAddress);
    await Expect(Page).ToHaveTitleAsync(new Regex("Home Page - PlayWithPlaywright"));

    Assert.Pass();
}
```

That's it.

To try it out. just call dotnet test on the Command Line or PowerShell or run the relevant test in a test explorer.

## Conclusion

The result with my test project looks like the following while running all the tests when I was offline:

![test result]({{site.baseurl}}/img/playwright/playwright02.png)

One failing test is the recorded test session of my blog on [https://asp.net-hacker.rocks/](https://asp.net-hacker.rocks/) and the other one is the demo test I found on [https://playwright.dev](https://playwright.dev). The passed test is the one that uses the `CustomWebApplicationFactory`

This is exactly the result I expected. 

You'll find the the example on my GitHub repository.

