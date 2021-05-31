---
layout: post
title: "ASP.​NET Core in .NET 6 - Part 09 - Infer component generic types from ancestor components"
teaser: "This is the ninth part of the ASP.NET Core on .NET 6 series. In this post, I'd like to have a look into the inferring of generic types from ancestor components."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- .NET 6
- Conponents
- Blazor

---

This is the ninth part of the [ASP.NET Core on .NET 6 series]({% post_url aspnetcore6-01.md %}). In this post, I'd like to have a look into the inferring of generic types from ancestor components.

In Blazor generic ancestor components need to have the generic type defined in the markup code, until yet. With the preview 2 of .NET 6 ancestor components can infer the generic type from the parent component.

In the announcement post, Microsoft shows a quick demo with the Grid component. Let's have a quick look at the snippets:

``` html
<Grid Items="@people">
    <Column TItem="Person" Name="Full name">@context.FirstName @context.LastName</Column>
    <Column TItem="Person" Name="E-mail address">@context.Email</Column>
</Grid>
```

In this snippet, the Column component has the generic type with the `TItem` property. This is not longer needed as they showed with this sample:

~~~html
<Grid Items="@people">
    <Column Name="Full name">@context.FirstName @context.LastName</Column>
    <Column Name="E-mail address">@context.Email</Column>
</Grid>
~~~

Since I don't like grids at all, I would like to try to build a `SimpleList` component that uses a generic  `ListItem` ancestor component to render the items in the list:



## Try to infer generic types 

As usual, I have to create a project first. This time I'm going to use a Blazor Server project

~~~ shell
dotnet new blazorserver -n ComponentGenericTypes -o ComponentGenericTypes
cd ComponentGenericTypes
code .
~~~

This creates a new Blazor Server project called `ComponentGenericTypes`, changes into the project directory, and opens VSCode to start working on the project.

To generate some meaningful dummy data, I'm going to add my favorite NuGet package GenFu:

~~~ shell
dotnet add package GenFu
~~~

In the `Index.razor`, I replaced the existing code with the following:

~~~ Html
@page "/"
@using ComponentGenericTypes.Components
@using ComponentGenericTypes.Data
@using GenFu

<h1>Hello, world!</h1>

<SimpleList Items="@people">
    <ListItem>
        <p>
            Hallo <b>@context.FirstName @context.LastName</b><br />
            @context.Email
        </p>
    </ListItem>
</SimpleList>

@code {
    public IEnumerable<Person> people = A.ListOf<Person>(15);    
}
~~~

This will not work yet, but let's quickly go through it to get the idea. Since this code uses two components that are located in the Components folder, we need to add a using of `ComponentGenericTypes.Components`, as well as a using to `ComponentGenericType.Date` because we like to use the `Person` class. Both the components and the class don't exist yet.

At the bottom of the file, we create a list of 15 persons using `GenFu` and assign it to a variable that is bound to the `SimpleList` component. The `ListItem` component is the direct child component of the `SimpleList` and behaves like a template for the items. It also contains markup code to display the values. 

For the `Person` class I created a new C# file in the `Data` folder and added the following code:

~~~csharp
namespace ComponentGenericTypes.Data
{
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}
~~~

This is a pretty simple class. But the property names are important. If such a class is instantiated by GenFu, it automatically writes first names into the `FirstName` property, last names into the `LastName` property and it also writes valid email addresses into the `Email` property. It also works with Streets, Addresses, ZIP codes, phone numbers, and so on. This is why GenFu is my favorite NuGet package.

Now let's create a `Components` folder and place the `SimpleList` component inside. The code looks like this:

~~~html
@typeparam TItem
@attribute [CascadingTypeParameter(nameof(TItem))]

<CascadingValue IsFixed="true" Value="Items">@ChildContent</CascadingValue>

@code {
    [Parameter] public IEnumerable<TItem> Items { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }
}
~~~

It defines the generic type parameter `TItem` and a property called `Items` that is of type `IEnemuerable` of `TItem`. That makes the component generic to use almost any kind of `IEnumerables`. To use child components, the `SimpleList` also contains a `RenderFragment` property called `ChildContent`. 

The second attribute does the magic. This cascades the generic type parameter of a specific type to the child component. This is why we don't need to specify the generic type in the ancestor component. In the third line, we also cascade the property Items to the child component. 

Now it's time to create the `ListItem` component:

~~~html
@typeparam TItem

@foreach (var item in Items)
{
    <div>@ChildContent(item)</div>
}

@code {
    [CascadingParameter] public IEnumerable<TItem> Items { get; set; }
    [Parameter] public RenderFragment<TItem> ChildContent { get; set; }
}
~~~

This component iterates through the list of items and renders the `ChildContent` which in this case is a generic `RenderFragment`. The generic one creates a `context` variable of type `TItem` that can be used to bind the passed value to child components or HTML markup. As seen in the `Index.razor` the `context` variable will be of type `Person`:

~~~html
<ListItem>
    <p>
        Hallo <b>@context.FirstName @context.LastName</b><br />
        @context.Email
    </p>
</ListItem>
~~~

That's it! The index page now will show a list of 15 persons:

![Generic List Component]({{site.baseurl}}/img/aspnetcore6/genericcomponent.png)

Since I'm not really a Blazor expert the way how I implemented the components might be not completely right, but it's working and shows the idea of the topic of this blog post. 

## What's next?

In the next part In going to look into the support for `preserve prerendered state in Blazor apps` in ASP.NET Core.