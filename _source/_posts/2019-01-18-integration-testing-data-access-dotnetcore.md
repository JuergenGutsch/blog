---
layout: post
title: "Integration testing data access in ASP.​NET Core"
teaser: "In the last post, I wrote about unit testing data access in ASP.NET Core. This time I'm going to go into integration tests. This post shows you how to write an end-to-end test using a WebApplicationFactory and hot to write specific integration test."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- Unit Test
- XUnit
---

In [the last post]({% post_url unit-testing-data-access-dotnetcore.md %}), I wrote about unit testing data access in ASP.NET Core. This time I'm going to go into integration tests. This post shows you how to write an end-to-end test using a `WebApplicationFactory` and hot to write specific integration test.

## Unit tests vs. Integration tests

I'm sure most of you already know the difference. In a few discussions I learned that some developers don't have a clear idea about the difference. At the end is doesn't really matter, because every test is a good test. Both the unit tests and the integration tests are coded test, they look similar and use the same technology. The difference is in the concepts of how and what to test and in the scope of the test:

* A **unit test** tests a logical unit, a single isolated component, a function or a feature. A unit test isolates this component to test it without any dependencies, like I did in the last post. First I tested the actions of a controller, without testing the actual service in behind. Than I tested the service methods in an isolated way with a faked `DbContext`. Why? Because unit tests shouldn't break because of a failing dependency. A unit test should be fast in development and in execution. It is a development tool. So it shouldn't cost a lot of time to write one. And in fact, setting up a unit test is much cheaper than setting up an integration test. Usually you write a unit test during or immediately after implementing the logic. In the best case you'll write a unit test before implementing the logic. This would be the TDD way, test driven development or test driven design.

* An **integration tests** does a lot more. It tests the composition of all units. It ensures that all units are working together in the right way. This means it may need a lot more effort to setup a test because you need to setup the dependencies. An integration test can test a feature from the UI to the database. It integrates all the dependencies. On the other hand an integrations test can be isolated on a hot path of a feature. It is also legit to fake or mock aspects that don't need to be tested in this special case. For example, if you test a user input from the UI to the database, you don't need to test the logging. Also an integration test shouldn't fail because on an error outside the context. This also means to isolate an integration test as much as possible, maybe by using an in-memory database instead of a real one.

Let's see how it works:

## Setup

I'm going to reuse the solution created for the last post to keep this section short.

I only need to create another XUnit test project, to add it to the existing solution and to add a reference to the WebToTest and some NuGet packages:

~~~ shell
dotnet new xunit -o WebToTest.IntegrationTests -n WebToTest.IntegrationTests
dotnet sln add WebToTest.IntegrationTests
dotnet add WebToTest.IntegrationTests reference WebToTest

dotnet add WebToTest.IntegrationTests package GenFu
dotnet add WebToTest.IntegrationTests package moq
dotnet add WebToTest.IntegrationTests package Microsoft.AspNetCore.Mvc.Testing
~~~

At the next step I create a test class for a web integration test. This means I setup a web host for the application-to-test to call the web via a web client. This is kind of an UI test than, not based on UI events, but I'm able to get and analyze the HTML result of the page to test.

ASP.NET Core since 2.0 has the possibility to setup a test host to run the a web in the test environment. This is pretty cool. You don't need to setup an actual web server to run a test against the web. This gets done automatically by using the generic `WebApplicationFactory`. You just need to specify the type of the Startup class of the web-to-test:

~~~ csharp
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace WebToTest.IntegrationTests
{
    public class PersonTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public PersonTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }
        
        // put test methods here
    }
}
~~~

Also the XUnit `IClassFixture` is special here. This generic interface tells XUnint to create an instance of the generic argument per test to run. In this case I get a new instance of the `WebApplicationFactory` of `Startup` per test. Wo this test class created its own web server every time a test method gets executed. This is a isolated test environment per test.

## End-to-end tests

Our first integration tests will ensure the MVC routes are working. This tests create a web host and calls the web via HTTP. It tests parts of the application from the UI to the database. This is an end-to-end test.

Instead of a XUnit `Fact`, we create a `Theory` this time. A `Theory` marks a test method which is able to retrieve input data via attribute. The `InlineDataAttribute` defines the data we want to pass in. In this case the MVC route URLs:

~~~ csharp
[Theory]
[InlineData("/")]
[InlineData("/Home/Index")]
[InlineData("/Home/Privacy")]
public async Task BaseTest(string url)
{
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync(url);

    // Assert
    response.EnsureSuccessStatusCode(); // Status Code 200-299
    Assert.Equal("text/html; charset=utf-8",
        response.Content.Headers.ContentType.ToString());
}
~~~

Let's try it

~~~ shell
dotnet test WebToTest.IntegrationTests
~~~

This actually creates 3 test results as you can see in the output window:

![]({{site.baseurl}}/img/unit-test-data/dotnettest1.png)

We'll now need to do the same thing for the API routs. Why in a separate method? Because the first integration test also checks the content type which is the type of a HTML document. The content type of the API results is `application/json`:

~~~ csharp
[Theory]
[InlineData("/api/person")]
[InlineData("/api/person/1")]
public async Task ApiRouteTest(string url)
{
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync(url);

    // Assert
    response.EnsureSuccessStatusCode(); // Status Code 200-299
    Assert.Equal("application/json; charset=utf-8",
        response.Content.Headers.ContentType.ToString());
}
~~~

This also works and we have to more successful tests now:

![]({{site.baseurl}}/img/unit-test-data/dotnettest2.png)

This isn't completely isolated, because it uses the same database as the production or the test web. At least it is the same file based SQLite database as in the test environment. Because a test should be as fast as possible, wouldn't it make sense to use a in-memory database instead? 

Usually it would be possible to override the service registration of the `Startup.cs` with the `WebApplicationFactory` we retrieve in the constructor. It should be possible to add the `ApplicationDbContext` and to configure an in-memory database:

~~~ csharp
public PersonTests(WebApplicationFactory<Startup> factory)
{
    _factory = factory.WithWebHostBuilder(config =>
    {
        config.ConfigureServices(services =>
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("InMemory"));
        });
    });
}
~~~

Unfortunately, I didn't get the seeding running for the in-memory database using the current preview version of ASP.NET Core 3.0. This will result in an failing test for the route URL `/api/person/1` because the `Person` with the `Id` 1 isn't available. This is a known issue on GitHub: https://github.com/aspnet/EntityFrameworkCore/issues/11666

To get this running we need to ensure seeding explicitly every time we create an instance of the DbContext. 

~~~ csharp
public PersonService(ApplicationDbContext dbContext)
{
    _dbContext = dbContext;
    _dbContext.Database?.EnsureCreated();
}
~~~

This hopefully gets fixed, because it is kinda bad to add this line only for the integration tests. Anyway, it works this way. Maybe you find a way to call `EnsureCreated()` in the test class.

## Specific integration tests

Sometimes it makes sense to test more specific parts of the application, without starting a web host and without accessing a real database. Just to be sure that the individual units are working together. This time I'm testing the `PersonController` together with the `PersonService`. I'm going to mock the `DbContext`, because the database access isn't relevant for the test. I just need to ensure the service provides the data to the controller in the right way and to ensure the controller is able to handle these data.

At first I create a simple test class that is able to create the needed test data and the `DbContext` mock:

~~~ csharp
public class PersonIntegrationTest
{
    // put the tests here

    private Mock<ApplicationDbContext> CreateDbContextMock()
    {
        var persons = GetFakeData().AsQueryable();

        var dbSet = new Mock<DbSet<Person>>();
        dbSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(persons.Provider);
        dbSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(persons.Expression);
        dbSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(persons.ElementType);
        dbSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(persons.GetEnumerator());

        var context = new Mock<ApplicationDbContext>();
        context.Setup(c => c.Persons).Returns(dbSet.Object);
        
        return context;
    }

    private IEnumerable<Person> GetFakeData()
    {
        var i = 1;
        var persons = A.ListOf<Person>(26);
        persons.ForEach(x => x.Id = i++);
        return persons.Select(_ => _);
    }
}
~~~

At next I wrote the tests, which look similar to to the `PersonControllerTests` I wrote in the last blog post. Only the arrange part differs a little bit. This time I don't pass the mocked service in, but an actual one that uses a mocked `DbContext`:

~~~ csharp
[Fact]
public void GetPersonsTest()
{
    // arrange
    var context = CreateDbContextMock();

    var service = new PersonService(context.Object);

    var controller = new PersonController(service);

    // act
    var results = controller.GetPersons();

    var count = results.Count();

    // assert
    Assert.Equal(26, count);
}

[Fact]
public void CreateDbContextMock()
{
    // arrange
    var context = CreateDbContext();

    var service = new PersonService(context.Object);

    var controller = new PersonController(service);

    // act
    var result = controller.GetPerson(1);
    var person = result.Value;

    // assert
    Assert.Equal(1, person.Id);
}
~~~

Let's try it by using the following command:

~~~ shell
dotnet test WebToTest.IntegrationTests
~~~

Et voilà:

![]({{site.baseurl}}/img/unit-test-data/dotnettest3.png)

At the end we should run all the tests of the solution at once to be sure not to break the existing tests and the existing code. Just type `dotnet test` and see what happens:

![]({{site.baseurl}}/img/unit-test-data/dotnettest4.png)

## Conclusion

I wrote that integration test will cost a lot more effort than unit test. This isn't completely true since we are able to use the `WebApplicationFactory`. In many other cases it will be a little more expensive, depending how you want to test and how many dependencies you have.. You need to figure out how you want to isolate a integration test. More isolation sometimes means more effort, less isolation means more dependencies that may break your test.

Anyway. Writing integration tests in my point of view is more important than writing unit tests, because it tests that the parts of the application are working together. And it is not that hard and doesn't cost that much. 

Just do it. If you never wrote tests in the past: Try it. It feels great to be on the save way, to be sure the code is working as expected.