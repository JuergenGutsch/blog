--- 
layout: post
title: "Unit testing DNX libraries with NUnit"
teaser: "Just a few weeks ago, there was a release of NUnit which adds support for DNX libraries.  But how does it look like with DNX libraries? Is it as easy as with Xunit? This post shows you how unit testing of DNX libraries works with NUnit."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags:
- .NET Core
- Unit Tests
- NUnit
---

For this blog post I will reuse the Visual Studio solution of the [last post]({% post_url unit-testing-dnx-libraries-with-xunit.md %}). I only added a new test project called "UnitTestDemo.NunitTests" which is a DNX console application.

## The test runner

Nunit doesn't yet provide a DNX test runner as it is provided by Xunit, this is why we need to have a console application which can be called via the DNX command. But this is not a big deal and contains only a few lines of code and a little bit of configuration in the project.json. Lets add two new dependencies:

~~~ json
"dependencies": {
    "UnitTestDemo": "",
    "nunit": "3.0.0",
    "NUnitLite": "3.0.0-*",
    "FluentAssertions": "4.0.1"
},
~~~

NUntLight is a lightweight NUnit runner implementation and needs to be used to execute unit test libraries. A NUnit test project usually is a DNX console application with a program.cs inside. Let's add some lines of code to the Main method:

~~~ csharp
public class Program
{
    public static void Main(string[] args)
    {
#if DNX451
        new AutoRun().Execute(args);
#else
        new AutoRun().Execute(typeof(Program).GetTypeInfo().Assembly, Console.Out, Console.In, args);
#endif
    }
}
~~~

This method calls the Execute method of the NUnitLigts AutoRun class. If we are running on .NET Core, we need to pass in the console in and out streams and the Assembly which contains the unit tests. On .NET Framework this can be automatically resolved.

## Writing test

If this is done we are able to start writing NUnit tests in a usual way:

~~~ csharp
[TestFixture]
public class DivideTests
{
    [Test]
    public void Divide10By5ShouldResultIn2()
    {
        var expected = 2F;
        var actual = Calculate.Devide(10, 5);

        actual.Should().Be(expected);
    }

    [Test]
    public void DevideAnyNumberBy0ShouldResultInAnExeption()
    {
        Action act = () => Calculate.Devide(10, 0);

        act.ShouldThrow<ArgumentOutOfRangeException>();
    }
}
~~~

## Excuting the tests

To execute the tests we can go the same way as using Xunit. We only need to use another command:

~~~ json
"commands": {
  "test": "UnitTestDemo.NunitTests"
},
~~~

This calls the program.cs of the current project. All the other things are equal to the Xunit tests. We are able to use the Unit Test Explorer in Visual Studio or we can press the run test button:

![]({{ site.baseurl }}/img/unittestxunit/runtestinvs.png)

Using the command prompt we also have to ensure that we have all the dependencies and we need start the command:

~~~ batch
dnu restore
dnx test
~~~

This will show you the test results in the console like this: 

![]({{ site.baseurl }}/img/unittestxunit/dnxtestnunit.png)

As you can see, unit testing DNX libraries is pretty easy. Sure this could be improved a lot, but there's currently no reason not to test the your code ;)