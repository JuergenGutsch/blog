---
layout: post
title: "Unit Testing an ASP.​NET Core Application"
teaser: "ASP.NET Core 2.0 is out and it is great. Testing worked well in the previous versions, but in 2.0 it is much more easier. In this post I'm going to show you how to unit test your controllers and how to write integration tests for your controllers."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- Unit Test
- XUnit
---

ASP.NET Core 2.0 is out and it is great. Testing worked well in the previous versions, but in 2.0 it is much more easier. 

Xunit, Moq and FluentAssertions are working great with the new Version of .NET Core  and ASP.NET Core. Using this tools Unit Testing is really fun. Even more fun with testing is provided in ASP.NET Core. Testing Controllers wasn't easier in the previous versions.

If you remember the old Web API and MVC version, based on System.Web, you'll probably also remember how to write unit test for the Controllers.

In this post I'm going to show you how to unit test your controllers and how to write integration tests for your controllers.

## Preparing the project to test:

To show you how this works, I created a new "ASP.NET Core Web Application" : 

![]({{ site.baseurl }}/img/testing-aspnetcore/newproject01.png)

Now I needed to select the Web API project. Be sure to select ".NET Core" and "ASP.NET Core 2.0":

![]({{ site.baseurl }}/img/testing-aspnetcore/newproject02.png)

To keep this post simple, I didn't select an authentication type.

In this project is nothing special, except the new `PersonsController`, which is using a `PersonService`:

~~~ csharp
[Route("api/[controller]")]
public class PersonsController : Controller
{
    private IPersonService _personService;

    public PersonsController(IPersonService personService)
    {
        _personService = personService;
    }
    // GET api/values
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var models = _personService.GetAll();

        return Ok(models);
    }

    // GET api/values/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var model = _personService.Get(id);

        return Ok(model);
    }

    // POST api/values
    [HttpPost]
    public async Task<IActionResult> Post([FromBody]Person model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var person = _personService.Add(model);

        return CreatedAtAction("Get", new { id = person.Id }, person);
    }

    // PUT api/values/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody]Person model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _personService.Update(id, model);

        return NoContent();
    }

    // DELETE api/values/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _personService.Delete(id);
        return NoContent();
    }
}
~~~

The `Person` class is created in a new folder "Models" and is a simple POCO:

~~~ csharp
public class Person
{
  public int Id { get; set; }
  [Required]
  public string FirstName { get; set; }
  [Required]
  public string LastName { get; set; }
  public string Title { get; set; }
  public int Age { get; set; }
  public string Address { get; set; }
  public string City { get; set; }
  [Required]
  [Phone]
  public string Phone { get; set; }
  [Required]
  [EmailAddress]
  public string Email { get; set; }
}
~~~

 The `PersonService` uses GenFu to auto generate a list of Persons:

~~~ csharp
public class PersonService : IPersonService
{
    private List<Person> Persons { get; set; }

    public PersonService()
    {
        var i = 0;
        Persons = A.ListOf<Person>(50);
        Persons.ForEach(person =>
        {
            i++;
            person.Id = i;
        });
    }

    public IEnumerable<Person> GetAll()
    {
        return Persons;
    }

    public Person Get(int id)
    {
        return Persons.First(_ => _.Id == id);
    }

    public Person Add(Person person)
    {
        var newid = Persons.OrderBy(_ => _.Id).Last().Id + 1;
        person.Id = newid;

        Persons.Add(person);

        return person;
    }

    public void Update(int id, Person person)
    {
        var existing = Persons.First(_ => _.Id == id);
        existing.FirstName = person.FirstName;
        existing.LastName = person.LastName;
        existing.Address = person.Address;
        existing.Age = person.Age;
        existing.City = person.City;
        existing.Email = person.Email;
        existing.Phone = person.Phone;
        existing.Title = person.Title;
    }

    public void Delete(int id)
    {
        var existing = Persons.First(_ => _.Id == id);
        Persons.Remove(existing);
    }
}

public interface IPersonService
{
  IEnumerable<Person> GetAll();
  Person Get(int id);
  Person Add(Person person);
  void Update(int id, Person person);
  void Delete(int id);
}
~~~

This Service needs to be registered in the `Startup.cs`:

~~~ csharp
services.AddScoped<IPersonService, PersonService>();
~~~

If this is done, we can create the test project.

## The unit test project

I always choose [xUnit](https://github.com/xunit/xunit) (or Nunit) over MSTest, but feel free to use MSTest. The used testing framework doesn't really matter.

![]({{ site.baseurl }}/img/testing-aspnetcore/newtestproject01.png)

Inside that project, I created two test classes: `PersonsControllerIntegrationTests` and  `PersonsControllerUnitTests`

We need to add some NuGet packages. Right click the project in VS2017 to edit the project file and to add the references manually, or use the NuGet Package Manager to add these packages: 

~~~ xml
<PackageReference Include="Microsoft.AspNetCore.All" Version="2.0.0" />
<PackageReference Include="Microsoft.AspNetCore.TestHost" Version="2.0.0" />
<PackageReference Include="FluentAssertions" Version="4.19.2" />
<PackageReference Include="Moq" Version="4.7.63" />
~~~

The first package contains the dependencies to ASP.NET Core. I use the same package as in the project to test. The second package is used for the integration tests, to build a test host for the project to test. FluentAssertions provides a more elegant way to do assertions. And Moq is used to create fake objects.

Let's start with the unit tests:

## Unit Testing the Controller

We start with an simple example, by testing the GET methods only:

~~~ csharp
public class PersonsControllerUnitTests
{
  [Fact]
  public async Task Values_Get_All()
  {
    // Arrange
    var controller = new PersonsController(new PersonService());

    // Act
    var result = await controller.Get();

    // Assert
    var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    var persons = okResult.Value.Should().BeAssignableTo<IEnumerable<Person>>().Subject;

    persons.Count().Should().Be(50);
  }

  [Fact]
  public async Task Values_Get_Specific()
  {
    // Arrange
    var controller = new PersonsController(new PersonService());

    // Act
    var result = await controller.Get(16);

    // Assert
    var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    var person = okResult.Value.Should().BeAssignableTo<Person>().Subject;
    person.Id.Should().Be(16);
  }
  
  [Fact]
  public async Task Persons_Add()
  {
    // Arrange
    var controller = new PersonsController(new PersonService());
    var newPerson = new Person
    {
      FirstName = "John",
      LastName = "Doe",
      Age = 50,
      Title = "FooBar",
      Email = "john.doe@foo.bar"
    };

    // Act
    var result = await controller.Post(newPerson);

    // Assert
    var okResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
    var person = okResult.Value.Should().BeAssignableTo<Person>().Subject;
    person.Id.Should().Be(51);
  }

  [Fact]
  public async Task Persons_Change()
  {
    // Arrange
    var service = new PersonService();
    var controller = new PersonsController(service);
    var newPerson = new Person
    {
      FirstName = "John",
      LastName = "Doe",
      Age = 50,
      Title = "FooBar",
      Email = "john.doe@foo.bar"
    };

    // Act
    var result = await controller.Put(20, newPerson);

    // Assert
    var okResult = result.Should().BeOfType<NoContentResult>().Subject;

    var person = service.Get(20);
    person.Id.Should().Be(20);
    person.FirstName.Should().Be("John");
    person.LastName.Should().Be("Doe");
    person.Age.Should().Be(50);
    person.Title.Should().Be("FooBar");
    person.Email.Should().Be("john.doe@foo.bar");
  }

  [Fact]
  public async Task Persons_Delete()
  {
    // Arrange
    var service = new PersonService();
    var controller = new PersonsController(service);

    // Act
    var result = await controller.Delete(20);

    // Assert
    var okResult = result.Should().BeOfType<NoContentResult>().Subject;

    // should throw an eception, 
    // because the person with id==20 doesn't exist enymore
    AssertionExtensions.ShouldThrow<InvalidOperationException>(
      () => service.Get(20));
  }
}
~~~

This snippets also shows the benefits of FluentAssertions. I really like the readability of this fluent API. 

BTW: Enabling live unit testing in Visual Studio 2017 is impressive:

![]({{ site.baseurl }}/img/testing-aspnetcore/liveunittesting.png)

With this unit test approach the invalid `ModelState` isn't tested. To get this tested we need to test more integrated. Also the `ModelBinder` should be executed, to validate the input and to set the `ModelState`

One last thing about unit test: I mentioned Moq before, because I propose to isolate the code to test. This means, wouldn't use a real service, when I test a controller. I also wouldn't use a real repository, when I test a service. And so on... That's why you should work with fake services instead of real ones. Moq is a tool to create such fake objects and to set them up:

~~~ csharp
[Fact]
public async Task Persons_Get_From_Moq()
{
  // Arrange
  var serviceMock = new Mock<IPersonService>();
  serviceMock.Setup(x => x.GetAll()).Returns(() => new List<Person>
  {
    new Person{Id=1, FirstName="Foo", LastName="Bar"},
    new Person{Id=2, FirstName="John", LastName="Doe"},
    new Person{Id=3, FirstName="Juergen", LastName="Gutsch"},
  });
  var controller = new PersonsController(serviceMock.Object);

  // Act
  var result = await controller.Get();

  // Assert
  var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
  var persons = okResult.Value.Should().BeAssignableTo<IEnumerable<Person>>().Subject;

  persons.Count().Should().Be(3);
}
~~~

To learn more about Moq, have a look into the repository: https://github.com/moq/moq4

## Integration testing the Controller

But let's start with the simple cases, by testing the GET methods first. To test the integration of a web API or any other web based API, you need to have a web server running. Even in this case a web server is needed, but fortunately there is a test server which can be used. With this host it is not needed to set-up a separate web server on the test machine:

~~~ csharp
public class PersonsControllerIntegrationTests
{
  private readonly TestServer _server;
  private readonly HttpClient _client;

  public PersonsControllerIntegrationTests()
  {
    // Arrange
    _server = new TestServer(new WebHostBuilder()
                             .UseStartup<Startup>());
    _client = _server.CreateClient();
  }
  // ... 
}
~~~

At first, the `TestServer` will be set up. This guy gets the `WebHostBuilder` - known from every ASP.NET Core 2.0 application - and uses the `Startup` of our project to test. At second, we are able to create a `HttpClient` out of that server. We'll use this `HttpClient` in the tests to create request to the server and to receive the responses.

~~~csharp
[Fact]
public async Task Persons_Get_All()
{
  // Act
  var response = await _client.GetAsync("/api/Persons");
  response.EnsureSuccessStatusCode();
  var responseString = await response.Content.ReadAsStringAsync();

  // Assert
  var persons = JsonConvert.DeserializeObject<IEnumerable<Person>>(responseString);
  persons.Count().Should().Be(50);
}

[Fact]
public async Task Persons_Get_Specific()
{
  // Act
  var response = await _client.GetAsync("/api/Persons/16");
  response.EnsureSuccessStatusCode();
  var responseString = await response.Content.ReadAsStringAsync();

  // Assert
  var person = JsonConvert.DeserializeObject<Person>(responseString);
  person.Id.Should().Be(16);
}
~~~

Now let's test the POST request:

~~~ csharp
[Fact]
public async Task Persons_Post_Specific()
{
  // Arrange
  var personToAdd = new Person
  {
    FirstName = "John",
    LastName = "Doe",
    Age = 50,
    Title = "FooBar",
    Phone = "001 123 1234567",
    Email = "john.doe@foo.bar"
  };
  var content = JsonConvert.SerializeObject(personToAdd);
  var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

  // Act
  var response = await _client.PostAsync("/api/Persons", stringContent);

  // Assert
  response.EnsureSuccessStatusCode();
  var responseString = await response.Content.ReadAsStringAsync();
  var person = JsonConvert.DeserializeObject<Person>(responseString);
  person.Id.Should().Be(51);
}
~~~

We need to prepare a little bit more. Here we create a `StringContent` object, which derives from `HttpContent`. This will contain the Person we want to add as JSON string. This get's sent via post to the `TestServer`.

To test the invalid `ModelState`, just remove a required field or pass a wrong formatted email address or telephone number and test against it. In this sample, I test against three missing required fields:

~~~ csharp
[Fact]
public async Task Persons_Post_Specific_Invalid()
{
  // Arrange
  var personToAdd = new Person { FirstName = "John" };
  var content = JsonConvert.SerializeObject(personToAdd);
  var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

  // Act
  var response = await _client.PostAsync("/api/Persons", stringContent);

  // Assert
  response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
  var responseString = await response.Content.ReadAsStringAsync();
  responseString.Should().Contain("The Email field is required")
    .And.Contain("The LastName field is required")
    .And.Contain("The Phone field is required");
}
~~~

This is almost the same pattern for the PUT and the DELETE requests:

~~~ csharp
[Fact]
public async Task Persons_Put_Specific()
{
  // Arrange
  var personToChange = new Person
  {
    Id = 16,
    FirstName = "John",
    LastName = "Doe",
    Age = 50,
    Title = "FooBar",
    Phone = "001 123 1234567",
    Email = "john.doe@foo.bar"
  };
  var content = JsonConvert.SerializeObject(personToChange);
  var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

  // Act
  var response = await _client.PutAsync("/api/Persons/16", stringContent);

  // Assert
  response.EnsureSuccessStatusCode();
  var responseString = await response.Content.ReadAsStringAsync();
  responseString.Should().Be(String.Empty);
}

[Fact]
public async Task Persons_Put_Specific_Invalid()
{
  // Arrange
  var personToChange = new Person { FirstName = "John" };
  var content = JsonConvert.SerializeObject(personToChange);
  var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

  // Act
  var response = await _client.PutAsync("/api/Persons/16", stringContent);

  // Assert
  response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
  var responseString = await response.Content.ReadAsStringAsync();
  responseString.Should().Contain("The Email field is required")
    .And.Contain("The LastName field is required")
    .And.Contain("The Phone field is required");
}

[Fact]
public async Task Persons_Delete_Specific()
{
  // Arrange

  // Act
  var response = await _client.DeleteAsync("/api/Persons/16");

  // Assert
  response.EnsureSuccessStatusCode();
  var responseString = await response.Content.ReadAsStringAsync();
  responseString.Should().Be(String.Empty);
}
~~~

## Conclusion

That's it. 

Thanks to the `TestServer`! With this it is really easy to write integration tests the controllers. Sure, it is still a little more effort than the much simpler unit tests, but you don't have external dependencies anymore. No external web server to manage. No external web server, which is out of control.

[Try it out](https://github.com/JuergenGutsch/unittesting-aspnetcore) and tell me about your opinion :)
