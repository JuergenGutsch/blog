---
layout: post
title: "Using xUnit, MSTest or NUnit to test .NET Core libraries"
teaser: "MSTest was just announced to be open sourced, but was already moved to .NET Core some months ago. It seems it makes sense to write another blog post about unit testing .NET Core applications and .NET Standard libraries using .NET Core tools. In this post I'm going to use the dotnet CLI and Visual Studio Code."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- Unit Test
- XUnit
- MSTest
---

MSTest was just announced to be open sourced, but was already moved to .NET Core some months ago. It seems it makes sense to write another blog post about unit testing .NET Core applications and .NET Standard libraries using .NET Core tools.

In this post I'm going to use the dotnet CLI and Visual Studio Code completely. Feel free to use Visual Studio 2017 instead, if you want to and if you don't like to use the console. Visual Studio 2017 is using the same dotnet CLI and almost the same commands in the background.

## Setup the system under test

Our SUT is a pretty complex class, that helps us a lot to do some basic math operation. This class will be a part of a super awesome library:

~~~ csharp
namespace SuperAwesomeLibrary
{
  public class MathTools
  {
    public decimal Add(decimal a, decimal b) =>  a + b;
    public decimal Substr(decimal a, decimal b) => a - b;
    public decimal Multiply(decimal a, decimal b) => a * b;
    public decimal Divide(decimal a, decimal b) => a / b;
  }
}
~~~

I'm going to add this class to the "SuperAwesomeLibrary" and a solution I recently added like this:

~~~ shell
mkdir unit-tests & cd unit-tests
dotnet new sln -n SuperAwesomeLibrary
mkdir SuperAwesomeLibrary & cd SuperAwesomeLibrary
dotnet new classlib
cd ..
dotnet sln add SuperAwesomeLibrary\SuperAwesomeLibrary.csproj
~~~

The cool thing about the dotnet CLI is, that you are really able to create Visual Studio solutions (line 2). This wasn't possible with the previous versions. The result is a Visual Studio and MSBuild compatible solution and you can use and build it like any other solution in your continuous integration environment. Line 5 creates a new library, which will be added to the solution in line 7.

After this is done, the following commands will complete the setup, by restoring the NuGet packages and building the solution:

~~~ shell
dotnet restore
dotnet build
~~~

## Adding xUnit tests 

The dotnet CLI directly supports to add XUnit tests:

~~~ shell
mkdir SuperAwesomeLibrary.Xunit & cd SuperAwesomeLibrary.Xunit
dotnet new xunit
dotnet add reference ..\SuperAwesomeLibrary\SuperAwesomeLibrary.csproj
cd ..
dotnet sln add SuperAwesomeLibrary.Xunit\SuperAwesomeLibrary.Xunit.csproj
~~~

This commands are creating the new xUnit test project, adding a reference to the SuperAwesomeLibrary and adding the test project to the solution.

If this was done, I created the xUnit tests for our MathHelper using VSCode:

~~~ csharp
public class MathToolsTests
{
  [Fact]
  public void AddTest() 
  {
    var sut = new MathTools();
    var result = sut.Add(1M, 2M);
    Assert.True(3M == result);
  }
  [Fact]
  public void SubstrTest() 
  {
    var sut = new MathTools();
    var result = sut.Substr(2M, 1M);
    Assert.True(1M == result);
  }
  [Fact]
  public void MultiplyTest() 
  {
    var sut = new MathTools();
    var result = sut.Multiply(2M, 1M);
    Assert.True(2M == result);
  }
  [Fact]
  public void DivideTest() 
  {
    var sut = new MathTools();
    var result = sut.Divide(2M, 2M);
    Assert.True(1M == result);
  }
}
~~~

This should work and you need to call your very best dotnet CLI friends again:

~~~shell
dotnet restore
dotnet build
~~~

The cool thing about this commands is, it works in your solution directory and restores the packages of all your solution and it builds all your projects in your solution. You don't need to go threw all of your projects separately.

But if you want to run your tests, you need to call the library or the project directly, if you are not in the project folder:

~~~ shell
dotnet test SuperAwesomeLibrary.Xunit\SuperAwesomeLibrary.Xunit.csproj
~~~

If you are in the test project folder just call `dotnet test` without the project file. 

This command will run all your unit tests in your project.

## Adding MSTest tests

Adding a test library for MSTest works the same way:

~~~ shell
mkdir SuperAwesomeLibrary.MsTest & cd SuperAwesomeLibrary.MsTest
dotnet new mstest
dotnet add reference ..\SuperAwesomeLibrary\SuperAwesomeLibrary.csproj
cd ..
dotnet sln add SuperAwesomeLibrary.MsTest\SuperAwesomeLibrary.MsTest.csproj
~~~

Event the test class looks almost the same:

~~~ csharp
[TestClass]
public class MathToolsTests
{
  [TestMethod]
  public void AddTest()
  {
    var sut = new MathTools();
    var result = sut.Add(1M, 2M);
    Assert.IsTrue(3M == result);
  }
  [TestMethod]
  public void SubstrTest()
  {
    var sut = new MathTools();
    var result = sut.Substr(2M, 1M);
    Assert.IsTrue(1M == result);
  }
  [TestMethod]
  public void MultiplyTest()
  {
    var sut = new MathTools();
    var result = sut.Multiply(2M, 1M);
    Assert.IsTrue(2M == result);
  }
  [TestMethod]
  public void DivideTest()
  {
    var sut = new MathTools();
    var result = sut.Divide(2M, 2M);
    Assert.IsTrue(1M == result);
  }
}
~~~

And again our favorite commands:

~~~ shell
dotnet restore
dotnet build
~~~

> The command `dotnet restore` will fail in offline mode, because MSTest is not delivered with the runtime and the default NuGet packages, but xUnit is. This means, it needs to fetch the latest packages from NuGet.org. Kinda weird, isn't it? 

The last task to do, is to run the unit tests:

~~~ shell
dotnet test SuperAwesomeLibrary.MsTest\SuperAwesomeLibrary.MsTest.csproj
~~~

This doesn't really look hard.

## What about Nunit?

Unfortunately there is no default template for a Nunit test projects. I really like Nunit and I used it for years. It is anyway possible to use NUnit with .NET Core, but you need to do some things manually. The first steps seem to be pretty similar to the other examples, except we create a console application and add the NUnit dependencies manually:

~~~ shell
mkdir SuperAwesomeLibrary.Nunit & cd SuperAwesomeLibrary.Nunit
dotnet new console
dotnet add package Nunit
dotnet add package NUnitLite
dotnet add reference ..\SuperAwesomeLibrary\SuperAwesomeLibrary.csproj
cd ..
~~~

The reason why we need to create a console application is, that there is not yet a runner for visual studio available for NUnit. This also means `dotnet test` will not work. The NUnit Devs are working on it, but this seems to need some more time. Anyway, there is an option to use NUnitLite to create a self executing test library.

We need to use NUnitLight and to change the `static void Main` to a `static int Main`:

~~~ csharp
static int Main(string[] args)
{
  var typeInfo = typeof(Program).GetTypeInfo();
  return new AutoRun(typeInfo.Assembly).Execute(args);
}
~~~

This lines automatically executes all TestClasses in the current assembly. It also passes the NUnit arguments to NUnitLite, to e. g. setup the output log file, etc.

Let's add a NUnit test class:

~~~ csharp
[TestClass]
public class MathToolsTests
{
  [Test]
  public void AddTest() 
  {
    var sut = new MathTools();
    var result = sut.Add(1M, 2M);
    Assert.That(result, Is.EqualTo(3M));
  }
  [Test]
  public void SubstrTest() 
  {
    var sut = new MathTools();
    var result = sut.Substr(2M, 1M);
    Assert.That(result, Is.EqualTo(1M));
  }
  [Test]
  public void MultiplyTest() 
  {
    var sut = new MathTools();
    var result = sut.Multiply(2M, 1M);
    Assert.That(result, Is.EqualTo(2M));
  }
  [Test]
  public void DivideTest() 
  {
    var sut = new MathTools();
    var result = sut.Divide(2M, 2M);
    Assert.That(result, Is.EqualTo(1M));
  }
}
~~~

Finally we need to run the tests. But this time we cannot use dotnet test.

~~~ shell
dotnet restore
dotnet build
dotnet run -p SuperAwesomeLibrary.Nunit\SuperAwesomeLibrary.Nunit.csproj
~~~

Because it is a console application, we need to use `dotnet run` to execute the app and the NUnitLite test runner. 

## What about mocking?

Currently creating mocking frameworks is a little bit difficult using .NET Standard, because there is a lot of reflection needed, which is not completely implemented in .NET Core or even .NET Standard. 

My Favorite tool Moq is anyway available for .NET Standard 1.3, which means it should work here. Let's see how it works.

Lets assume we have a PersonService in the SuperAwesomeLibrary that uses a IPersonRepository to fetch Persons from a data storage:

~~~ csharp
using System;
using System.Collections.Generic;

public class PersonService
{
  private readonly IPersonRepository _personRepository;

  public PersonService(IPersonRepository personRepository)
  {
    _personRepository = personRepository;
  }

  public IEnumerable<Person> GetAllPersons()
  {
    return _personRepository.FetchAllPersons();
  }
}

public interface IPersonRepository
{
  IEnumerable<Person> FetchAllPersons();
}

public class Person
{
  public string Firstname { get; set; }
  public string Lastname { get; set; }
  public DateTime DateOfBirth { get; set; }
}
~~~

After building the library, I move to the NUnit test project to add Moq and [GenFu](https://github.com/MisterJames/GenFu/). 

~~~ shell
cd SuperAwesomeLibrary.Nunit
dotnet add package moq
dotnet add package genfu
dotnet restore
~~~

GenFu is a really great library to create the test data for unit tests or demos. I really like this library and use it a lot.

Now we need to write the actual test using this tools. This test doesn't really makes sense, but it shows how Moq works:

~~~ csharp
using System;
using System.Linq;
using NUnit.Framework;
using SuperAwesomeLibrary;
using GenFu;
using Moq;

namespace SuperAwesomeLibrary.Xunit
{
  [TestFixture]
  public class PersonServiceTest
  {
    [Test]
    public void GetAllPersons() 
    {
      var persons = A.ListOf<Person>(10); // generating test data using GenFu
      
      var repo = new Mock<IPersonRepository>();
      repo.Setup(x => x.GetAllPersons()).Returns(persons);

      var sut = new PersonService(repo.Object);
      var actual = sut.GetAllPersons();

      Assert.That(actual.Count(), Is.EqualTo(10));
    }
  }
}
~~~

As you can see the, Moq works the same was in .NET Core as in the full .NET Framework.

Now let's start the NUnit tests again:

~~~ shell
dotnet build
dotnet run
~~~

Et voilà:

![]({{ site.baseurl }}/img/unit-tests/dotnetrun.png)

## Conclusion

Running unit tests within .NET Core isn't really a big deal and it is really a good thing that it is working with different unit testing frameworks. You have the choice to use your favorite tools. It would be nice to have the same choice even with the mocking frameworks.

In one of the next post I'll write about unit testing a ASP.NET Core application. Which includes testing MiddleWares, Controllers, Filters and View Components.

You can play around with the code samples on GitHub: [https://github.com/juergengutsch/dotnetcore-unit-test-samples/](https://github.com/juergengutsch/dotnetcore-unit-test-samples/)