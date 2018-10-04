---
layout: post
title: "Customizing ASP.NET Core Part 07: OutputFormatter"
teaser: "In this seventh post I want to write about, how to send your Data in different formats and types to the client. By default the ASP.NET Core Web API sends the data as JSON, but there are some more ways to send the dat."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- OutputFormatter
---

In this seventh post I want to write about, how to send your Data in different formats and types to the client. By default the ASP.NET Core Web API sends the data as JSON, but there are some more ways to send the dat.

## The series topics

- [Customizing ASP.NET Core Part 01: Logging]({% post_url customizing-aspnetcore-01-logging.md %})
- [Customizing ASP.NET Core Part 02: Configuration]({% post_url customizing-aspnetcore-02-configuration.md %})
- [Customizing ASP.NET Core Part 03: Dependency Injection]({% post_url customizing-aspnetcore-03-dependency-injection.md %})
- [Customizing ASP.NET Core Part 04: HTTPS]({% post_url customizing-aspnetcore-04-https.md %})
- [Customizing ASP.NET Core Part 05: HostedServices]({% post_url customizing-aspnetcore-05-hostedservices.md %})
- [Customizing ASP.NET Core Part 06: MiddleWares]({% post_url customizing-aspnetcore-06-middlewares.md %})
- **Customizing ASP.NET Core Part 07: OutputFormatter - This article**
- Customizing ASP.NET Core Part 08: ModelBinder
- Customizing ASP.NET Core Part 09: ActionFilter
- Customizing ASP.NET Core Part 10: TagHelpers

## About OutputFormatters

OutputFormatters are classes that turn your data into a different format to sent them trough HTTP to the clients. Web API uses a default OutputFormatter to turn objects into JSON, which is the default format to send data in a structured way. Other build in formatters are a XML formatter and a plan text formatter.

With the - so called - content negotiation the client is able to decide which format he wants to retrieve .The client need to specify the content type of the format in the Accept-Header. The content negotiation is implemented in the ObjectResult.

By default the Web API always returns JSON, even if you accept `text/xml` in the header. This is why the build in XML formatter is not registered by default. There are two ways to add a `XmlSerializerOutputFormatter` to ASP.NET Core:

~~~ csharp
services.AddMvc()
    .AddXmlSerializerFormatters();
~~~

or 

~~~ csharp
services.AddMvc(options =>
{
    options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
});
~~~

> There is also a XmlDataContractSerializerOutputFormatter available

Also any Accept header gets turned into `application/json`. If you want to allow the clients to accept different headers, you need to switch that translation off:

~~~ json
services.AddMvc(options =>
{
    options.RespectBrowserAcceptHeader = true; // false by default
});
~~~

To try the formatters let's setup a small test project.

## Prepare a test project

Using the console we will create a small ASP.NET Core Web API project. Execute the following commands line by line:

~~~ shell
dotnet new webapi -n WebApiTest -o WebApiTest
cd WebApiTest
dotnet add package GenFu
dotnet add package CsvHelper
~~~

This creates a new Web API projects and adds two NuGet packages to it. GenFu is a awesome library to easily create test data. The second one helps us to easily write CSV data.

Now open the project in Visual Studio or in Visual Studio Code and open the `ValuesController.cs` and change the `Get()` method like this:

~~~ csharp
[HttpGet]
public ActionResult<IEnumerable<Person>> Get()
{
	var persons = A.ListOf<Person>(25);
	return persons;
}
~~~

This crates a list of 25 Persons using GenFu. The properties get automatically filled with almost realistic data. You'll see the magic of GenFu and the results later on.

In the Models folder create a new file `Person.cs` with the the `Person` class inside:

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

Open the `Startup.cs` as well and add the Xml formatters and allow other accept headers as described earlier:

~~~ csharp
services.AddMvc(options =>
{
    options.RespectBrowserAcceptHeader = true; // false by default
    options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
});
~~~

That's it for now. Now you are able to retrieve the data from the Web API. Start the project by using the `dotnet run` command.

