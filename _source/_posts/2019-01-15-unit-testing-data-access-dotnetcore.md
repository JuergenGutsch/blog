---
layout: post
title: "Unit testing data access in .NET Core"
teaser: "Recently I got asked to explain how to unit test a controller that retrieves data using an entity Framework Core DbContext. In this post I'm going to show you how to use GenFu, Moq and XUnit to create small and isolated unit tests for your ASP.NET Core application."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- Unit Test
- XUnit
- MSTest
---

I really like to be in contact with the dear readers of my blog. I get a lot of positive feedback about my posts  via twitter or within the comments. That's awesome and that really pushed me forward to write more posts like this. Some folks also create PRs for my blog posts on GitHub to fix typos and other errors of my posts. You also can do this, by clicking the link to the related markdown file on GitHub at the end of every post.

Many thanks for this kind of feedback :-)

The reader Mohammad Reza recently asked me via twitter to write about unit testing an controller that connects to a database and to fake the data for the unit tests.

> [@sharpcms](https://twitter.com/sharpcms) Hello jurgen, thank you for your explanation of unit test :
> Unit Testing an ASP.NET Core Application.
> it's grate. can you please explain how to use real data from entity framwork and fake data for test in a controller?

> @Mohammad: First of all: I'm glad you like this post and I would be proud to write about that. Here it is:

## Setup the solution using the .NET CLI

First of all, let's create the demo solution using the .NET CLI

~~~ shell
mkdir UnitTestingAspNetCoreWithData & cd UnitTestingAspNetCoreWithData
dotnet new mvc -n WebToTest -o WebToTest
dotnet new xunit -n WebToTest.Tests -o WebToTest.Tests
dotnet new sln -n UnitTestingAspNetCoreWithData

dotnet sln add WebToTest
dotnet sln add WebToTest.Tests
~~~

This lines are creating a solution directory adding a web to test and a XUnit test project. Also a solution file gets added and the two projects will be added to the solution file. 

~~~ shell
dotnet add WebToTest.Tests reference WebToTest
~~~

This command wont work in the current version of the .NET Core, because the XUnit project still targets netcoreapp2.2. You cannot reference a higher target version. It should be equal or lower than the target version of the referencing project. You should change the the target to netcoreapp3.0 in the csproj of the test project before executing this command:

> ~~~ xml
> <TargetFramework>netcoreapp3.0</TargetFramework>
> ~~~

Now we need to add some NuGet references:

~~~ shell
dotnet add WebToTest package GenFu
dotnet add WebToTest.Tests package GenFu
dotnet add WebToTest.Tests package moq
~~~

At first we add [GenFu](http://genfu.io/), which is a dummy data generator. We need it in the web project to seed some dummy data initially to the database and we need it in the test project to generate test data. We also need [Moq](https://github.com/moq/moq) to create fake objects, e.g. fake data access in the test project.

Because the web project is an empty web project it also doesn't contain any data access libraries. We need to add Enitity Framework Core to the project.

~~~ shell
dotnet add WebToTest package Microsoft.EntityFrameworkCore.Sqlite -v 3.0.0-preview.18572.1
dotnet add WebToTest package Microsoft.EntityFrameworkCore.Tools -v 3.0.0-preview.18572.1
dotnet add WebToTest package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore -v 3.0.0-preview.18572.1
~~~

I'm currently using the preview version of .NET Core 3.0. The version number will change later on.

Now we can start Visual Studio Code 

~~~ shell
code .
~~~

In the same console window we can call the following command to execute the tests:

~~~ shell
dotnet test WebToTest.Tests
~~~

## Creating the controller to test

The Controller we want to test is an API controller that only includes two GET actions. It is only about the concepts. Testing additional actions, POST and PUT actions is almost the same. This is the complete controller to test.

~~~ csharp
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebToTest.Data.Entities;
using WebToTest.Services;

namespace WebToTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController()]
    public class PersonController : Controller
    {
        private readonly IPersonService _personService;

        public PersonController(IPersonService personService)
        {
            _personService = personService;
        }
        // GET: api/Person
        [HttpGet]
        public IEnumerable<Person> GetPersons()
        {
            return _personService.AllPersons();
        }

        // GET: api/Person/5
        [HttpGet("{id}")]
        public ActionResult<Person> GetPerson(int id)
        {
            var todoItem = _personService.FindPerson(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
        }
    }
}

~~~

As you can see, we don't use entity framework directly in the controller. I would propose to encapsulate the data access in service classes, which prepare the data as you need it.

> Some developers prefer to encapsulate the actual data access in an additional repository layer. From my perspective this is not needed, if you use an OR mapper like Entity Framework. One reason is that EF already is the additional layer that encapsulates the actual data access. And the repository layer is also an additional layer to test and to maintain.

So the service layer contains all the EF stuff and is used here. This also makes testing much easier because we don't need to mock the EF `DbContext`. The Service gets passed in via dependency injection.

Let's have a quick look into the `Startup.cs` where we need to configure the services:

~~~ csharp
public void ConfigureServices(IServiceCollection services)
{
    // [...]

    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

    services.AddTransient<IPersonService, PersonService>();
    
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
}
~~~

What I added to the `ConfigureServices` method is one line to register and configure the `DbContext` and one line to register the `PersonService` used in the controller.  Both types are not created yet. Before we create them we also need to add a few lines to the config file. Open the `appsettings.json` and add the connection string to the [SQLite](https://www.sqlite.org/) database:

~~~ json
{
  "ConnectionStrings": {
    "DefaultConnection": "DataSource=app.db"
  },
  // [...]
}
~~~

That's all about the configuration. Let's go back to the implementation. The next step is the `DbContext`. To keep the demo simple, I just use one `Person` entity here:

~~~ csharp
using GenFu;
using Microsoft.EntityFrameworkCore;
using WebToTest.Data.Entities;

namespace WebToTest.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {}
        
        public ApplicationDbContext() { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // seeding
            var i = 1;
            var personsToSeed = A.ListOf<Person>(26);
            personsToSeed.ForEach(x => x.Id = i++);
            modelBuilder.Entity<Person>().HasData(personsToSeed);
        }

        public virtual DbSet<Person> Persons { get; set; }
    }
}
~~~

We only have one `DbSet of Person` here. In the `OneModelCreating` method we use the new seeding method `HasData()` to ensure we have some data in the database. Usually you would use real data to seed to the database. In this case I use [GenFu](http://genfu.io/) do generate a list of 26 persons. Afterwards I need to ensure the IDs are unique, because by default [GenFu](http://genfu.io/) generates random numbers for the ids which may result in a duplicate key exception.

The person entity is simple as well:

~~~ csharp
using System;

namespace WebToTest.Data.Entities
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Address { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
    }
}
~~~

Now let's add the `PersonService` which uses the `ApplicationDbContext` to fetch the data. Even the `DbContext` gets injected into the constructor via dependency injection:

~~~ csharp
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebToTest.Data;
using WebToTest.Data.Entities;

namespace WebToTest.Services
{
    public class PersonService : IPersonService
    {
        private readonly ApplicationDbContext _dbContext;
        public PersonService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<Person> AllPersons()
        {
            return _dbContext.Persons
            	.OrderBy(x => x.DateOfBirth)
            	.ToList();
        }

        public Person FindPerson(int id)
        {
            return _dbContext.Persons
            	.FirstOrDefault(x => x.Id == id);
        }
    }

    public interface IPersonService
    {
        IEnumerable<Person> AllPersons();
        Person FindPerson(int id);
    }
}
~~~

We need the interface to register the service using a contract and to create a mock service later on in the test project.

If this is done, don't forget to create an initial migration to create the database:

~~~ shell
dotnet ef migrations add Initial -p WebToTest -o Data\Migrations\
~~~

This puts the migration into the `Data` folder in our web project. No we are able to create and seed the database:

~~~ shell
dotnet ef database update -p WebToTest
~~~

In the console you will now see how the database gets created and seeded:

![](/img/unit-test-data/seeding.png)

Now the web project is complete and should run. You can try it by calling the following command and calling the URL https://localhost:5001/api/person in the browser:

~~~ shell
dotnet run -p WebToTest
~~~

You now should see the 26 persons as JSON in the browser:

![](/img/unit-test-data/json.png)

## Testing the controller

In the test project I renamed the initially scaffolded class to `PersonControllerTests`. After that I created a small method that creates the test data we'll return to the controller. This is exactly the same code we used to seed the database:

~~~ csharp
private IEnumerable<Person> GetFakeData()
{
    var i = 1;
    var persons = A.ListOf<Person>(26);
    persons.ForEach(x => x.Id = i++);
    return persons.Select(_ => _);
}
~~~

We now can create out first test to test the controllers `GetPersons()` method:

~~~ csharp
[Fact]
public void GetPersonsTest()
{
    // arrange
    var service = new Mock<IPersonService>();

    var persons = GetFakeData();
    service.Setup(x => x.AllPersons()).Returns(persons);

    var controller = new PersonController(service.Object);

    // Act
    var results = controller.GetPersons();

    var count = results.Count();

    // Assert
    Assert.Equal(count, 26);
}
~~~

In the first line we use `Moq` to create a mock/fake object of our `PersonService`. This is why we need the interface of the service class. `Moq` creates proxy objects out of interfaces or abstract classes. Using `Moq` we are now able to setup the mock object, by telling `Moq` we want to return this specific list of persons every time we call the `AllPersons()` method.

If the setup is done we are able to inject the proxy object of the `IPersonService` into the controller. Our controller now works with a fake service instead using the original one. Inside the unit test we don't need a connection to the database now. That makes the test faster and more independent from any infrastructure outside the code to test

In the act section we call the `GetPersons()` method and will check the results afterwards in the assert section.

How does it look like with the `GetPerson()` method that returns one single item?

The second action to test returns an `ActionResult of Person`, so we only need to get the result a little bit different:

~~~ csharp
[Fact]
public void GetPerson()
{
    // arrange
    var service = new Mock<IPersonService>();

    var persons = GetFakeData();
   	var firstPerson = persons.First();
    service.Setup(x => x.FindPerson(1)).Returns(firstPerson);

    var controller = new PersonController(service.Object);

    // act
    var result = controller.GetPerson(1);
    var person = result.Value;

    // assert
    Assert.Equal(1, person.Id);
}
~~~

Also the setup differs, because we setup another method that returns a single `Person` instead of a `IEnumerable of Person`. 

To execute the tests run the next command in the console:

~~~ shell	
dotnet test WebToTest.Tests
~~~

This should result in the following output if all is done right:

![](/img/unit-test-data/dotnetrun.png)



## Testing the service layer

How does it look like to test the service layer? In that case we need to mock the `DbContext` to feed the service with fake data.

In the test project I created a new test class called `PersonServiceTests` and a test method that tests the `AllPersons()` method of the `PersonService`:

~~~ csharp
[Fact]
public void AllPersonsTest()
{
    // arrange
    var context = CreateDbContext();

    var service = new PersonService(context.Object);

    // act
    var results = service.AllPersons();

    var count = results.Count();

    // assert
    Assert.Equal(26, count);
}
~~~

This looks pretty simple at the first glance, but the magic is inside the method `CreateDbContext`, which created the mock object of the `DbContext`. I don't return the actual object, in case I need to extend the mock object in the current test method. Let's see how the `DbContext` is created:

~~~ csharp
private Mock<ApplicationDbContext> CreateDbContext()
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
~~~

The `DbSet` cannot be easily created, it is a bit special. This is why I need to mock the `DbSet` and to setup the Provider, the `Expression` the `ElementType` and the `Enumerator` using the values from the persons list. If this is done, I can create the `ApplicationDbContext` mock and setup the `DbSet` of Person on that mock. For every `DbSet` in your `DbContext` you need to add this four special setups on the mock `DbSet`. This seems to be a lot of overhead, but it is worth the trouble to test the service in an isolated way.

Sure you could use a in memory database with a real `DbContext`, but in this case the service isn't really isolated and and we anyway have a kind of a unit test.

The second test of the `PersonService` is pretty similar to the first one:

~~~ csharp
[Fact]
public void FindPersonTest()
{
    // arrange
    var context = CreateDbContext();

    var service = new PersonService(context.Object);

    // act
    var person = service.FindPerson(1);

    // assert
    Assert.Equal(1, person.Id);
}
~~~

Let's run the tests and see if it's all working as expected:

~~~ shell
dotnet test WebToTest.Tests
~~~

![](/img/unit-test-data/dotnetrun2.png)

Also this four tests passed.

## Summary

In this tutorial the setup took the biggest part, just to get a running API controller that we can use to test.

* We created the solution in the console using the .NET CLI.
* We added a service layer to encapsulate the data access.
* We added a EF DbContext to use in the service layer.
* We registered the services and the DbContext in the DI.
* We used the service in the controller to create two actions which return the data.
* We started the application to be sure all is running fine.
* We created one test on an action that doesn't return an ActionResult
* We created another test on an action that returns an ActionResult
* We ran the tests successfully in the console using the .NET CLI

Not using the CbContext in the Controller directly makes it a lot easier to test the controller by passing in a mock service. Why? Because it is easier to fake the service instead of the DB context. It also keeps the controller small which makes maintenance a lot easier later on.

Faking the DbContext is a bit more effort, but also possible as you saw in the last section. 

## Conclusion

> @Mohammad I hope this post will help you and answer your questions :-)

Using ASP.NET Core there is no reason not to unit test the most important and critical parts of your application. If needed you are able to unit test almost all in your ASP.NET Core application. 

Unit test is no magic but it is also not the general solution to ensure the quality of your app. To ensure that all tested units are working together you definitely need to have some some integration tests. 

I'll do another post about integration tests using ASP.NET Core 3.0 soon. 