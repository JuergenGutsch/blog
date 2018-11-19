---
layout: post
title: "Customizing ASP.​NET Core Part 09: ActionFilter"
teaser: "We keep on customizing on the controller level in this ninth post of this series. I'll have a look into ActionFilters and hot to create your own ActionFilter to keep your Actions small and readable."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- ActionFilters
- Logging
---

This post is a little late this time. My initial plan was to throw out two posts of this series per week, but this doesn't work out, since there are sometimes some more family and work tasks to do than expected. 

Anyway, we keep on customizing on the controller level in this ninth post of this blog series. I'll have a look into ActionFilters and hot to create your own ActionFilter to keep your Actions small and readable.

## The series topics

- [Customizing ASP.NET Core Part 01: Logging]({% post_url customizing-aspnetcore-01-logging.md %})
- [Customizing ASP.NET Core Part 02: Configuration]({% post_url customizing-aspnetcore-02-configuration.md %})
- [Customizing ASP.NET Core Part 03: Dependency Injection]({% post_url customizing-aspnetcore-03-dependency-injection.md %})
- [Customizing ASP.NET Core Part 04: HTTPS]({% post_url customizing-aspnetcore-04-https.md %})
- [Customizing ASP.NET Core Part 05: HostedServices]({% post_url customizing-aspnetcore-05-hostedservices.md %})
- [Customizing ASP.NET Core Part 06: Middlewares]({% post_url customizing-aspnetcore-06-middlewares.md %})
- [Customizing ASP.NET Core Part 07: OutputFormatter]({% post_url customizing-aspnetcore-07-outputformatter.md %})
- [Customizing ASP.NET Core Part 08: ModelBinders]({% post_url customizing-aspnetcore-08-modelbinders.md %})
- **Customizing ASP.NET Core Part 09: ActionFilters - This article**
- [Customizing ASP.NET Core Part 10: TagHelpers]({% post_url customizing-aspnetcore-10-taghelpers %})

## About ActionFilters

Action filters are a little bit like MiddleWares, but are executed immediately on a specific action or on all actions of a specific controller. If you apply an ActionFilter as a global one, it executes on all actions in your application. ActionFilters are created to execute code right before the actions is executed or after the action is executed. They are introduced to execute aspects that are not part of the actual action logic. Authorization is such an aspect. I'm sure you already know the `AuthorizeAttribute` to allow users or groups to access specific Actions or Controllers. The `AuthorizeAttribute` actually is an ActionFilter. It checks whether the logged-on user is authorized or not. If not it redirects to the log-on page.

The next sample shows the skeletons of a normal ActionFilters and an async ActionFilter:

~~~ csharp
public class SampleActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // do something before the action executes
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // do something after the action executes
    }
}

public class SampleAsyncActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        // do something before the action executes
        var resultContext = await next();
        // do something after the action executes; resultContext.Result will be set
    }
}
~~~

As you can see here there are always two section to place code to execute before and after the action is executed. This ActionFilters cannot be uses as attributes. If you want to use the ActionFilters as attributes in your Controllers, you need to drive from Attribute or from `ActionFilterAttribute`:

~~~ csharp
public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }
}
~~~

This code shows a simple ActionFilter which always returns a `BadRequestObjectResult`, if the `ModelState` is not valid. This may be useful an a Web API as a default check on POST, PUT and PATCH requests. This could be extended with a lot more validation logic. We'll see how to use it later on.

Another possible use case for an ActionFilter is logging. You don't need to log in the Controllers and Actions directly. You can do this in an action filter to not mess up the actions with not relevant code:

~~~ csharp
public class LoggingActionFilter : IActionFilter
{
    ILogger _logger;
    public LoggingActionFilter(ILoggerFactory loggerFactory)
    {

        _logger = loggerFactory.CreateLogger<LoggingActionFilter>();
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // do something before the action executes
        _logger.LogInformation($"Action '{context.ActionDescriptor.DisplayName}' executing");
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // do something after the action executes
        _logger.LogInformation($"Action '{context.ActionDescriptor.DisplayName}' executed");
    }
}
~~~

This logs an information message out to the console. You are able to get more information about the current Action out of the `ActionExecutingContext` or the `ActionExecutedContext` e.g. the arguments, the argument values and so on. This makes the ActionFilters pretty useful.

## Using the ActionFilters

ActionFilters that actually are Attributes can be registered as an attribute of an Action or a Controller:

```csharp
[HttpPost]
[ValidateModel] // ActionFilter as attribute
public ActionResult<Person> Post([FromBody] Person model)
{
    // save the person
    
	return model; //just to test the action
}
```

Here we use the `ValidateModelAttribute` that checks the `ModelState` and returns a `BadRequestObjectResult` in case the `ModelState` is invalid and I don't need to check the `ModelState` in the actual Action.

To register ActionFilters globally you need to extend the MVC registration in the `CofnigureServices` method of the `Startup.cs`:

~~~ csharp
services.AddMvc()
    .AddMvcOptions(options =>
    {
        options.Filters.Add(new SampleActionFilter());
        options.Filters.Add(new SampleAsyncActionFilter());
    });
~~~

ActionFilters registered like this are getting executed on every action. This way you are able to use ActionFilters that don't derive from Attribute.

The Logging `LoggingActionFilter` we created previously is a little more special. It is depending on an instance of an `ILoggerFactory`, which need to be passed into the constructor. This won't work well as an attribute, because Attributes don't support constructor injection via dependency injection. The `ILoggerFactory` is registered in the ASP.NET Core dependency injection container and needs to be injected into the `LoggingActionFilter`.

Because of this there are some more ways to register ActionFilters. Globally we are able to register it as a type, that gets instantiated by the dependency injection container and the dependencies can be solved by the container.

~~~ csharp
services.AddMvc()
    .AddMvcOptions(options =>
    {
        options.Filters.Add<LoggingActionFilter>();
    })
~~~

This works well. We now have the `ILoggerFactory` in the filter

To support automatic resolution in Attributes, you need to use the `ServiceFilterAttribute` on the Controller or Action level:

~~~ csharp
[ServiceFilter(typeof(LoggingActionFilter))]
public class HomeController : Controller
{
~~~

in addition to the global filter registration, the ActionFilter needs to be registered in the `ServiceCollection` before we can use it with the `ServiceFilterAttribute`:

~~~ csharp
services.AddSingleton<LoggingActionFilter>();
~~~

To be complete there is another way to use ActionFilters that needs arguments passed into the constructor.  You can use the `TypeFilterAttribute` to automatically instantiate the filter. But using this attribute the Filter isn't instantiate by the dependency injection container and the arguments need to get specified as argument of the `TypeFilterAttribute`. See the next snippet from the [docs](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters):

~~~ csharp
[TypeFilter(typeof(AddHeaderAttribute),
    Arguments = new object[] { "Author", "Juergen Gutsch (@sharpcms)" })]
public IActionResult Hi(string name)
{
    return Content($"Hi {name}");
}
~~~

The Type of the filter end the arguments are specified with the `TypeFilterAttribute` 

## Conclusion

Personally I like the way to keep the actions clean using ActionFilters. If I find repeating tasks inside my Actions, that are not really relevant to the actual responsibility of the Action, I try to move it out to an ActionFilter, or maybe a ModelBinder or a MiddleWare, depending on how globally it should work. The more it is relevant to an Action the more likely I use an ActionFilter. 

There are some more kind of filters, which all work similar. To learn more about the different kind of filters, you definitely need to [read the docs](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters).

In the tenth part of the series we move to the actual view logic and extend the Razor Views with custom TagHelpers: [Customizing ASP.NET Core Part 10: TagHelpers]({% post_url customizing-aspnetcore-10-taghelpers %})