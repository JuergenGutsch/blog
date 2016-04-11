--- 
layout: post
title: "View Components in ASP.​NET 5"
teaser: "One of the nicest new features in ASP.NET 5 are the ViewComponents. These are a kind of mini MVC inside the MVC application.."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET 5
- MVC 6
- View Components
---

One of the nicest new features in ASP.NET 5 is the ViewComponent. This is a kind of a 'mini MVC' inside the MVC application which can be used like partial Views. ViewComponents are like partial Views with an own controller, almost comparable with a UserControl in ASP.NET WebForms. 

Use cases are multiple reused components of a web application. That means all things that must not be managed by the current controller action. Let's use a Blog as an example, which has some more elements like menus, tag lists, link lists, archive overviews, etc. The data of these elements can be passed via the current actions to the view, but this needs to be done in every single action and produces a lot of duplicate code.

It would be nice if the controller actions only have to do one single task: fetching and passing blog posts to the view. All the other things should be done on other locations, to not mess up the controller actions.

That's where the ViewComponents entering the stage.
	
## Let me show you how ViewCompnents look like

First we need to create a simple class which derives from ViewComponent and which needs to have "ViewCompoennt" as a sufix. E. g. "Top20TagsViewComponent":	
	
~~~ csharp
public class Top20TagsViewComponent : ViewComponent 
{ 
    private readonly ITagService _tagService; 

    public Top20TagsViewComponent(ITagService tagService) 
    { 
        _tagService = tagService; 
    } 

    public IViewComponentResult Invoke() 
    { 
         var tags = _tagService.LoadTop20Tags(); 
         var models = tags.Select( 
            new TagViewModel 
            { 
                Id = tag.Id, 
                Name = tag.Name 
            }); 
        return View(models); 
    } 
}
~~~

The method Invoke almost looks like a Action in a usual Controller, which creates and returns a View. The used TagService is injected with the default IoC. Because is available everywhere in ASP.NET 5, you can access everything what accessible with a usual Controller.

The View is pretty common:

~~~ aspnet
@model IEnumerable<DotNetFn.ViewComponents.TagViewModel>

@if (Model.Any()) 
{ 
	<ul> 
        @foreach (var tag in Tags) 
        { 
            <li> 
                [@tag.Id] @tag.Name 
            </li> 
        } 
    </ul> 
}
~~~

Only the location where the View needs to be saved is a bit special. You need to save the default View with the name `Default.cshtml` in a folder, which is named like the ViewComponent without the suffix inside `/Views/Shared/Components/` Our ViewComponent is stored in `/Shared/Components/Top20Tags/Default.cshtml`

The default name is Default.cshtml, but you can use any other name, if you pass that name to the View: 

~~~ csharp
return View("TheNicerName", models);
~~~

With this you are able to switch the Templates inside the ViewComponent, if it is needed.

The described Component will be used almost as a partial View:

~~~ aspnet
@Component.Invoke("TopTags");
~~~

## Passing arguments

A very interesting thing is to pass arguments to a ViewComponent. Maybe you want to change the number of Tags to display, depending on where we want to use this Component.

We only need to extend the Invoke method with one ore more arguments:

~~~ csharp
public IViewComponentResult Invoke(int count)     
{ 
    var tags = _tagService.LoadTopTags().Take(count);     
    var models = tags.Select(tag => 
        new TagViewModel 
        { 
            Id = tag.Id, 
            Name = tag.Name 
        }); 
     return View(models); 
} 
~~~

Now we able to call the ViewComponent with that additional argument:

~~~ aspnet
@Component.Invoke("TopTags", 10);
~~~

## Asynchronous ViewComponents

To support asynchronous Views, we can also use a asynchronous Invoke method instead:

~~~ csharp
public async Task<IViewComponentResult> InvokeAsync(int count)     
{ 
    var tags = await _tagService.LoadTopTags();     
    var models = tags.Select(=> 
        new TagViewModel 
        { 
            Id = tag.Id, 
            Name = tag.Name 
        }).Take(count); 
     return View(models); 
} 
~~~

We only need to use await in the View to use this InvokeAsync:

~~~ aspnet
@await Component.InvokeAsync("TopTags", 10);
~~~