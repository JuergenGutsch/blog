---
layout: post
title: "Trying the REST Client extension for VSCode"
teaser: "I recently stumbled upon a tweet by Lars Richter who mentioned and linked to a REST Client extension in Visual Studio Code. I had a more detailed look and was pretty impressed by this extension. This post is a quick introduction about the REXT Client extension for Visual Studio Code"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- REST
- VSCode
- Test
---

I recently stumbled upon a [tweet](https://twitter.com/n_develop/status/1364848192135782402) by [Lars Richter](https://twitter.com/n_develop) who mentioned and linked to a rest client extension in VSCode. I had a more detailed look and was pretty impressed by this extension. 

I can now get rid of Fiddler and Postman.

## Let's start at the beginning

The REST Client Extension for VSCode was developed by [Huachao Mao](https://twitter.com/HuachaoMao) from China. You will find the extension on the visual studio marketplace or in the extensions explorer in VS Code:

- https://marketplace.visualstudio.com/items?itemName=humao.rest-client

If you follow this link, you will find a really great documentation about the extension, how it works, and how to use it. This also means this post is pretty useless, except you want to read a quick overview ;-)

![rest client extension]({{site.baseurl}}/img/restclient/extension.png)

The source code of the REST Client extension is hosted on GitHub:

- https://github.com/Huachao/vscode-restclient

This extension is actively maintained, has almost one and a half installations and an awesome rating (5.0 out of 5) by more than 250 people 

## What does it solve?

Compared to Fiddler and Postman it is absolutely minimalistic. There is no overloaded and full-blown UI. While Fidler is completely overloaded but full of features, Postman's UI is nicer, easier, and more intuitive, but the REST Client doesn't need a UI at all, except the VSCode shell and a plain text editor.

While Fiddler and Postman cannot easily share the request configurations, the REST Client stores the request configurations in text files using the *.http or *.rest extension that can be committed to the source code repository and shared with the entire team.

## Let's see how it works

To test it out in a demo, let's create a new Web API project, change to the project directory, and open VSCode:

~~~ shell
dotnet new webapi -n RestClient -o RestClient
cd RestClient
code .
~~~

This project already contains a Web API controller. I'm going to use this for the first small test of the REST Client. I will create and use a more complex controller later in the blog post 

To have the *.http files in one place I created an `ApiTest` folder and place a `WeatherForecast.http` in it. I'm not yet sure if it makes sense to put such files into the project, because these files won't go into production. I think, in a real-world project, I would place the files somewhere outside the actual project folder, but inside the source code repository. Let's keep it there for now:

![http file]({{site.baseurl}}/img/restclient/httpfile.png)

I already put the following line into that file:

~~~ http
GET https://localhost:5001/WeatherForecast/ HTTP/1.1
~~~

This is just a simple line of text in a plain text file with the file extension `*.http` but the REST Client extension does some cool magic with it while parsing it: 

On the top border, you can see that the REST Client extension supports the navigation inside the file structure. This is cool. Above the line, it also adds a CodeLens actionable link to the configured request to send the request. 

At first, start the project by pressing F5 or by using `dotnet run` in the shell. 

If the project is running you can click the **Send Request** CodeLens link and see what happens.

![result]({{site.baseurl}}/img/restclient/result01.png)

It opens the response in a new tab group in VSCode and shows you the response headers as well as the response content

## A more complex sample

I created another API controller that handles persons. The `PersonController` uses [GenFu](https://www.nuget.org/packages/GenFu/) to create fake users. The Methods POST, PUT and DELETE don't really do anything but the controller is good to test no.

~~~ csharp
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

using GenFu;

using RestClient.Models;

namespace RestClient.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {

        [HttpGet]
        public ActionResult<IEnumerable<Person>> Get()
        {
            return A.ListOf<Person>(15);
        }

        [HttpGet("{id:int}")]
        public ActionResult<Person> Get(int id)
        {
            var person = A.New<Person>(new Person { Id = id });
            return person;
        }

        [HttpPost]
        public ActionResult Post(Person person)
        {
            return Ok(person);
        }

        [HttpPut("{id:int}")]
        public ActionResult Put(int id, Person person)
        {
            return Ok(person);

        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            return Ok(id);
        }
    }
}
~~~

The `Person` model is simple:

~~~csharp
namespace RestClient.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Street { get; set; }
        public string Zip { get; set; }
        public string City { get; set; }
    }
}
~~~

If you now start the project you will see the new endpoints in the Swagger UI that is already configured in the Web API project. 
Call the following URL to see the Swagger UI: https://localhost:5001/swagger/index.html

![swaggerui]({{site.baseurl}}/img/restclient/swaggerui.png)

The Swagger UI will help you to configure the REST Client files. 

Ok. Let's start. I created a new file called `Person.http` in the `ApiTests` folder. You can add more than one REST Client request configuration into one file.

We don't need the Swagger UI for the two GET endpoints and for the DELETE endpoints, since they are the easy ones and look the same as in the `WeatherForecast.http`:

~~~http
GET https://localhost:5001/Person/ HTTP/1.1

###

GET https://localhost:5001/Person/2 HTTP/1.1

### 

DELETE https://localhost:5001/Person/2 HTTP/1.1
~~~

 The POST request is just a little more complex

If you now open the POST `/Person` section in the Swagger UI and try the request, you'll get all the information you need for the REST Client:

![swagger details]({{site.baseurl}}/img/restclient/swagger-details.png)

In the http file it will look like this:

~~~http
POST https://localhost:5001/Person/ HTTP/1.1
content-type: application/json

{
  "id": 0,
  "firstName": "Juergen",
  "lastName": "Gutsch",
  "email": "juergen@example.com",
  "telephone": "08150815",
  "street": "Mainstr. 2",
  "zip": "12345",
  "city": "Smallville"
}
~~~

You can do the same with the PUT request:

~~~http
PUT https://localhost:5001/Person/2 HTTP/1.1
content-type: application/json

{
  "id": 2,
  "firstName": "Juergen",
  "lastName": "Gutsch",
  "email": "juergen@example.com",
  "telephone": "08150815",
  "street": "Mainstr. 2",
  "zip": "12345",
  "city": "Smallville"
}
~~~

This is how it looks in VSCode, if you click the CodeLens link for the GET request :

![results]({{site.baseurl}}/img/restclient/results02.png)

You are now able to test all the API endpoints this way 

## Conclusion

Actually, it is not only about REST. You can test any kind of HTTP request this way.  You can even send binary data, like images to your endpoint.

This is a really great extension for VSCode and I'm sure I will use Fiddler or Postman only in environments where I don't have a VS Code installed. 