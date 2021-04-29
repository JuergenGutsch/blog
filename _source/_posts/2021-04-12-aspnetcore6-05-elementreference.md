---
layout: post
title: "ASP.​NET Core in .NET 6 - Part 05 - Input ElementReference in Blazor"
teaser: "This is the fifth part of the ASP.NET Core on .NET 6 series. In this post, I want to have a look at the input ElementReference in Blazor that is exposed to relevant components."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- .NET 6
---

This is the fifth part of the [ASP.NET Core on .NET 6 series]({% post_url aspnetcore6-01.md %}). In this post, I want to have a look at the input `ElementReference` in Blazor that is exposed to relevant components.

Microsoft exposes the `ElementReference` of the Blazor input elements to the underlying input. This effects the following components:  `InputCheckbox`, `InputDate`, `InputFile`, `InputNumber`, `InputSelect`, `InputText`, and `InputTextArea`. 

## Exploring the ElementReference

To test it, I created a Blazor Server project using the dotnet CLI:

~~~shell 
dotnet new blazorserver -n ElementReferenceDemo -o ElementReferenceDemo
~~~

CD into the project and call `dotnet watch`

I will reuse the  `index.razor` to try the form `ElementReference`:

~~~ html 
@page "/"

<h1>Hello, world!</h1>

Welcome to your new app.

<SurveyPrompt Title="How is Blazor working for you?" />
~~~

At first, add the following code block at the end of the file:

~~~csharp
@code{
    Person person = new Person{
      FirstName = "John",
      LastName = "Doe"
    };

    InputText firstNameReference;
    InputText lastNameReference;

    public class Person
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
~~~

This creates a `Person` type and initializes it. We will use it later as a model in the `EditForm`.  There are also two variables added that will reference the actual `InputText` elements in the form. We will add some more code later on, but let's add the form first:

~~~html
<EditForm Model=@person>
    <InputText @bind-Value="person.FirstName" @ref="firstNameReference" /><br>
    <InputText @bind-Value="person.LastName" @ref="lastNameReference" /><br>

    <input type="submit" value="Submit" class="btn btn-primary" /><br>
    
    <input type="button" value="Focus FirstName" class="btn btn-secondary" 
        @onclick="HandleFocusFirstName" />
    <input type="button" value="Focus LastName" class="btn btn-secondary" 
        @onclick="HandleFocusLastName" />
</EditForm>
~~~

This form has the `person` object assigned as a model. It contains two `InputText` elements, the default input button as well as two input buttons that will be used to test the `ElementReference`.

The reference Variables are assigned to the `@ref` attribute of the `InputText` elements. We will use these variables later on.

The buttons have `@onclick` methods assigned that we need to add to the code section:

~~~csharp
private async Task HandleFocusFirstName()
{
}

private async Task HandleFocusLastName()
{
}
~~~

As described by Microsoft the input elements now expose the `ElementReference`. This can be used to set the Focus of an element. Add the following lines to focus the `InputText` elements:

 ~~~csharp
private async Task HandleFocusFirstName()
{
    await firstNameReference.Element.Value.FocusAsync();
}

private async Task HandleFocusLastName()
{
    await lastNameReference.Element.Value.FocusAsync();
}
 ~~~

This might be pretty useful. Instead of playing around with JavaScript Interop, you can use C# completely.

On the other hand, it would be great, if Microsoft exposes much more features via the `ElementReference`, instead of just focusing an element. 

## What's next?

In the next part In going to look into the support for [Nullable Reference Type Annotations]({% post_url aspnetcore6-06-nullable-annotations.md %}) in ASP.NET Core.