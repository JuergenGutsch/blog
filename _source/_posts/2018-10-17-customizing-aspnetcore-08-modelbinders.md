---
layout: post
title: "Customizing ASP.​NET Core Part 08: ModelBinders"
teaser: "In the last post about OutputFormatters I wrote about sending data out to the clients in different formats. In this post we are going to do it the other way. This post is about data you get into your Web API from outside. What if you get data in a special format or what if you get data you need to validate in a special way. ModelBinders will help you handling this."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- ModelBinders
---

In the last post about `OutputFormatters` I wrote about sending data out to the clients in different formats. In this post we are going to do it the other way. This post is about data you get into your Web API from outside. What if you get data in a special format or what if you get data you need to validate in a special way. `ModelBinders` will help you handling this.

## The series topics

- [Customizing ASP.NET Core Part 01: Logging]({% post_url customizing-aspnetcore-01-logging.md %})
- [Customizing ASP.NET Core Part 02: Configuration]({% post_url customizing-aspnetcore-02-configuration.md %})
- [Customizing ASP.NET Core Part 03: Dependency Injection]({% post_url customizing-aspnetcore-03-dependency-injection.md %})
- [Customizing ASP.NET Core Part 04: HTTPS]({% post_url customizing-aspnetcore-04-https.md %})
- [Customizing ASP.NET Core Part 05: HostedServices]({% post_url customizing-aspnetcore-05-hostedservices.md %})
- [Customizing ASP.NET Core Part 06: Middlewares]({% post_url customizing-aspnetcore-06-middlewares.md %})
- [Customizing ASP.NET Core Part 07: OutputFormatter]({% post_url customizing-aspnetcore-07-outputformatter.md %})
- **Customizing ASP.NET Core Part 08: ModelBinders - This article**
- [Customizing ASP.NET Core Part 09: ActionFilter]({% post_url customizing-aspnetcore-09-actionfilters.md %})
- [Customizing ASP.NET Core Part 10: TagHelpers]({% post_url customizing-aspnetcore-10-taghelpers %})

## About ModelBinders

ModelBinders are responsible to bind the incoming data to specific action method parameters. It binds the data sent with the request to the parameters. The default binders are able to bind data that are sent via the QueryString or sent within the request body. Within the body the data can be sent in URL format or JSON.

The model binding tries to find the values in the request by the parameter names. The form values, the route data and the query string values are stored as a key-value pair collection and the binding tries to find the parameter name in the keys of the collection.

## Preparation of the test project

In this post I'd like to send CSV data to a Web API method. I will reuse the CSV data we created in the last post:

~~~ csv
Id,FirstName,LastName,Age,EmailAddress,Address,City,Phone
48,Samantha,White,18,Angel.Morgan@shaw.ca,"8202 77th Street ",Mascouche,(682) 381-4092
1,Eric,Wright,2,Briana.Ross@gmx.com,"8104 Scott Avenue ",Canutillo,(253) 366-5637
55,Amber,Watson,46,Sarah.Foster@gmx.com,"9206 Lewis Avenue ",Coleman,(632) 375-4415
99,Alexander,King,59,Ross.Timms@live.com,"3089 Paerdegat 7th Street ",Monte Alto,(366) 319-4154
69,Autumn,Hayes,25,Mark.Diaz@shaw.ca,"3263 Avenue O  ",Montreal West (Montréal-Ouest),(283) 438-7801
94,Destiny,James,47,Kylie.Walker@telus.net,"1057 14th Street ",Montreal,(570) 574-3208
59,Christina,Bennett,87,Madeline.Adams@att.com,"5672 19th Lane ",Corrigan,(467) 304-0309
71,Isaac,Hayes,33,Trevor.Robinson@hotmail.com,"9707 Langham Street ",Huntington,(635) 317-0231
23,Jason,Morgan,77,Jennifer.Powell@rogers.ca,"4413 Debevoise Avenue ",Pinole,(265) 467-1984
43,Jenna,Brandzin,92,Natalie.Reed@gmail.com,"4691 Sea Breeze Avenue ",Cushing-Douglass,(502) 427-9135
79,Madison,Verstraete,69,Abigail.Wright@hotmail.com,"2066 104th Street ",Moose Lake,(448) 423-7550
80,Lorrie,Long,89,Melissa.Bennett@microsoft.com,"3048 Allen Avenue ",Munday,(576) 707-6183
79,Alejandro,Daeninck,51,Matthew.Phillips@att.com,"9997 41st Street ",North Bay,(455) 297-2648
14,Makayla,Clark,44,Joshua.Jackson@rogers.ca,"4518 Folsom Place ",Cortland,(772) 692-0732
12,Isaac,Sanchez,37,Paige.MacKenzie@live.com,"2094 Mc Kenny Street ",Brockville,(563) 735-0233
68,Jesus,Brandzin,34,Molly.Clark@telus.net,"3532 Durland Place ",Comfort,(627) 319-9704
59,Logan,Howard,59,Jorge.Brandzin@rogers.ca,"3458 Wythe Avenue ",Enderby,(226) 520-9653
48,Nathaniel,Richardson,58,Amanda.Pitt@gmail.com,"6926 Sunnyside Court ",Los Altos Hills,(513) 338-4602
34,Tiffany,Miller,18,Claire.Alexander@att.com,"1985 Devon Avenue ",Sansom Park,(357) 274-3606
~~~

So let's start by creating a new project using the .NET CLI:

~~~ shell
dotnet new webapi -n ModelBinderSample -o ModelBinderSample
~~~

This creates a new Web API project. 

In this new project I created a new controller with a small action inside:

~~~ csharp
namespace ModelBinderSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonsController : ControllerBase
    {
        public ActionResult<object> Post(IEnumerable<Person> persons)
        {
            return new
            {
                ItemsRead = persons.Count(),
                Persons = persons
            };
        }
    }
}
~~~

This looks basically like any other action. It accepts a list of persons and returns an anonymous object that contains the number of persons as well as the list of persons. This action is pretty useless, but helps us to debug the ModelBinder using Postman.

We also need the Person class:

~~~ csharp
public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public string EmailAddress { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Phone { get; set; }
}
~~~

This actually will work fine, if we would send JSON based data to that action.

As a last preparation step, we need to add the CsvHelper NuGet package to easier parse the CSV data. I also love to use the .NET CLI here:

~~~ shell
dotnet package add CsvHelper
~~~

## Creating a CsvModelBinder

To create the `ModelBinder` add a new class called `CsvModelBinder`, which implements the `IModelBinder`. The next snippet shows a generic binder that should work with any list of models:

~~~ csharp
public class CsvModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        // Specify a default argument name if none is set by ModelBinderAttribute
        var modelName = bindingContext.ModelName;
        if (String.IsNullOrEmpty(modelName))
        {
            modelName = "model";
        }

        // Try to fetch the value of the argument by name
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;
        // Check if the argument value is null or empty
        if (String.IsNullOrEmpty(value))
        {
            return Task.CompletedTask;
        }

        var stringReader = new StringReader(value);
        var reader = new CsvReader(stringReader);

        var modelElementType = bindingContext.ModelMetadata.ElementType;
        var model = reader.GetRecords(modelElementType).ToList();

        bindingContext.Result = ModelBindingResult.Success(model);

        return Task.CompletedTask;
    }
}
~~~

In the method `BindModelAsync` we get the `ModelBindingContext` with all the information in it we need to get the data and to de-serialize it.

First the `context` get's checked against null values. After that we set a default argument name to model, if none is specified. If this is done we are able to fetch the value by the name we previously set. 

If there's no value, we shouldn't throw an exception in this case. The reason is that maybe the next configured `ModelBinder` is responsible. If we throw an exception the execution of the current request is broken and the next configured `ModelBinder` doesn't have the chance to get executed.

With a `StringReader` we read the value into the `CsvReader` and de-serialize it to the list of models. We get the type for the de-serialization out of the `ModelMetadata` property. This contains all the relevant information about the current model.

## Using the ModelBinder

The Binder isn't used automatically, because it isn't registered in the dependency injection container and not configured to use within the MVC framework.

The easiest way use this model binder is to use the ModelBinderAttribute on the argument of the action where the model should be bound:

~~~ csharp
[HttpPost]
public ActionResult<object> Post(
    [ModelBinder(binderType: typeof(CsvModelBinder))] 
    IEnumerable<Person> persons)
{
    return new
    {
        ItemsRead = persons.Count(),
        Persons = persons
    };
}
~~~

Here the type of our `CsvModelBinder` is set as `binderType` to that attribute.

[Steve Gordon](https://twitter.com/stevejgordon) wrote about a second option in his blog post: [Custom ModelBinding in ASP.NET MVC Core](https://www.stevejgordon.co.uk/html-encode-string-aspnet-core-model-binding/). He uses a `ModelBinderProvider` to add the `ModelBinder` to the list of existing ones. 

I personally prefer the explicit declaration, because the most custom `ModelBinders` will be pretty specific to an action or to an specific type and `theres` no hidden magic in the background.

## Testing the ModelBinder

To test it, we need to create a new Request in Postman. I set the request type to POST and put the URL https://localhost:5001/api/persons in the address bar. No I need to add the CSV data in the body of the request. Because it is a URL formatted body, I needed to put the data as `persons` variable into the body:

~~~ text
persons=Id,FirstName,LastName,Age,EmailAddress,Address,City,Phone
48,Samantha,White,18,Angel.Morgan@shaw.ca,"8202 77th Street ",Mascouche,(682) 381-4092
1,Eric,Wright,2,Briana.Ross@gmx.com,"8104 Scott Avenue ",Canutillo,(253) 366-5637
55,Amber,Watson,46,Sarah.Foster@gmx.com,"9206 Lewis Avenue ",Coleman,(632) 375-4415

~~~

After pressing send, I got the result as shown below:

![]({{site.baseurl}}/img/customize-aspnetcore/modelbinder.PNG)

Now the clients are able to send CSV based data to the server.

## Conclusion

This is a good way to transform the input in a way the action really needs. You could also use the ModelBinders to do some custom validation against the database or whatever you need to do before the model get's passed to the action.

To learn more about ModelBinders, you need to have a look into the pretty detailed documentation:

* [Model Binding in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding)
* [Custom Model Binding in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/custom-model-binding)

While playing around with the `ModelBinderProvider` Steve describes in his blog, I stumbled upon `InputFormatters`. Would this actually be the right way to transform CSV input into objects? I definitely need to learn some more details about the `InputFormatters`and will use this as 12th topic of this series.

Please follow the [introduction post of this series]({% post_url customizing-aspnetcore-series.md %}) to find additional customizing topics I will write about.

In the next part I will show you what you can do with ActionFilters: [Customizing ASP.NET Core Part 09: ActionFilter]({% post_url customizing-aspnetcore-09-actionfilters.md %})

