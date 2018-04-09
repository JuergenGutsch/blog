---
layout: post
title: "Playing around with GenFu"
teaser: "If you write unit tests or UI mock-ups you will probably need some test data. In the past I used NBuilder to build list of objects. But unfortunately NBuilder is not compatible with .NET Core and I need to find another tool, if I want to use .NET Core."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- GenFu
---

In the past I used [NBuilder](http://nbuilder.org/) (by Gareth Down) to create test data for my unit tests, demos and UI mock-ups. I really liked NBuilder, I used it for many years and I wrote about it in my [old blog (ger)](http://www.aspnetzone.de/blogs/juergengutsch/) and in the dotnetpro (a German .NET magazine)

Unfortunately [NBuilder](https://github.com/garethdown44/nbuilder/) is not compatible with .NET Core and there was no new release since 2011. Currently I play around with ASP.NET 5 and .NET Core, so compatibility to .NET Core and the latest dotnet platform standard is needed. 

Good I attended the MVP Summit 2015 and the Hackaton at the last day, because I heard about [GenFu](https://github.com/MisterJames/GenFu/), written by James Chambers, David Paquette and Simon Timms. They used that Hackathon to move this library to .NET Core. I did the same with LightCore at the same event.

GenFu also was a test data generator with some more features than NBuilder. GenFu includes some random data generators to create real looking data. 

> "GenFu is a library you can use to generate realistic test data. It is composed of several property fillers that can populate commonly named properties through reflection using an internal database of values or randomly created data. You can override any of the fillers and give GenFu hints on how to fill properties."

~~~ powershell
PM> Install-Package GenFu
~~~

To learn more about GenFu, I need to play around with it. I did this by writing a small ASP.NET 5 application which shows us user groups and their meetings and speakers and their topics. I also pushed that application to GitHub. So let me show what I found while playing around:

### Setup the project

I created a new ASP.NET Core 1 web application (without the authentication stuff) and added `"GenFu": "1.0.4"` to the dependencies in the project.json.

After that I created a set of types like UserGroup, Lead, Meeting, Speaker and so on.

E. g. the UserGroup looks like this:

~~~ csharp
public class UserGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public IEnumerable<Leader> Leaders { get; set; }
    public DateTime Founded { get; set; }
    public int Members { get; set; }
    public IEnumerable<Meeting> Meetings { get; set; }
}
~~~

### Using GenFu

Let's start creating a List of user groups to show on the start page. Like NBuilder, GenFu is using a fluent API to create the a single instance or a list of a specific type:

~~~csharp
var userGroup = A.New<UserGroup>();

var usergroups = A.ListOf<UserGroup>(20);
~~~

The second line of code generates a List of 20 user groups. The DateTime, Guid and String properties are already filled with randomly created values.

![]({{ site.baseurl }}/img/genfu/objects01.png)

What I want to have is a list with some more real looking data. The good thing about GenFu is, it already includes some sample data and a pretty cool fluent API to configure the types:

~~~ csharp
A.Configure<UserGroup>()
    .Fill(x => x.Members).WithinRange(10, 250)
    .Fill(x => x.Name).AsMusicGenreName()
    .Fill(x => x.Description).AsMusicGenreDescription()
    .Fill(x => x.Founded).AsPastDate();
~~~

The configuration needs to be done before retrieving the list or the single object. The result now is much better than before:

![]({{ site.baseurl }}/img/genfu/objects02.png)

We now have a list of music genre user groups :)

To fill the properties Leaders and Meetings, I created lists before the configuration of the UserGroup and created a extension method on IEnumerable<T> to get a almost random list out of the source list:

~~~ csharp
var leaders = A.ListOf<Leader>(20);
var meetings = A.ListOf<Meeting>(100);

A.Configure<UserGroup>()
    .Fill(x => x.Members).WithinRange(10, 250)
    .Fill(x => x.Name).AsMusicGenreName()
    .Fill(x => x.Description).AsMusicGenreDescription()
    .Fill(x => x.Founded).AsPastDate()
    .Fill(x => x.Leaders, leaders.GetRandom(1, 4))
    .Fill(x => x.Meetings, meetings.GetRandom(20,100));
            
var usergroups = A.ListOf<UserGroup>(20);
~~~

Now we can start to create the leaders, speakers, meetings in the same way to get the full set of data. E. g. to get a list of speakers we can also use same methods as to generate the user groups: 

~~~ csharp
var speakers = A.ListOf<Speaker>(20);
~~~

![]({{ site.baseurl }}/img/genfu/speakers.png)

But wait! Did I really configure the Speakers? 

I did not! 

I just created the list, but I get well looking names, twitter handles, email addresses and I get a nice phone number. Only the website, the description and the topics list are not well configured. Sure, the names, twitter handles and email addresses don't match, but this is not really important.

This is another pretty cool feature of GenFu. Depending of the property name, it finds the right thing called Filler. We are able to configure the speakers, to assign the Filler we want to have, but in many cases GenFu is able to find the right one, without any configuration.

Just type `A.Defaults` or `GenFu.Defaults` to get a list of constants to see what data are already included in GenFu.

Lets extend GenFu to create our own Filler to generate random website addresses. A small look into the EmailFiller shows me how easy it is to create our own PropertyFiller. A string based PropertyFiller can inherit base functionality from the PropertyFiller<String>:

~~~ csharp
public class WebAddressFiller : PropertyFiller<string>
{
    public WebAddressFiller()
        : base(
                new[] { "object" },
                new[] { "website", "web", "webaddress" })
    {
    }

    public override object GetValue(object instance)
    {
        var domain = Domains.DomainName();

        return $"https://www.{domain}";
    }
}
~~~

The first argument we pass into the base constructor is a list of type names of the objects we want to fill. "object" in this case means any kind of type based on Object. In GenFu there are different Fillers to fill the property title, because a person title is a different thing than an article title. Like this you can create different fillers for the same property name.

The second argument are the property names to fill. 

In the method GetValue we can generate the value and return them back. Because there already is a EmailFiller which generates domain names too, I reuse the ValueGenerator DomainName to get a random domain name out of GenFus resources. 

No we need to register the new Filler to GenFu and to use it:

~~~ csharp
A.Default().FillerManager.RegisterFiller(new WebAddressFiller());
var speakers = A.ListOf<Speaker>(20);
~~~

The result is as expect. We get well formed web addresses:

![]({{ site.baseurl }}/img/genfu/website.png)

That was pretty easy with only a few lines of code :)

![]({{ site.baseurl }}/img/genfu/speakers2.png)

In one the first snippets at the beginning of this post, I created an extension method to create a random length list out of a source list. Wouldn't it be better, if we could create a ListFiller to do that automatically? There is already a configuration extension for list properties called WithRandom, but this thing whats to have a list of lists to select a list out of it randomly. I would like to have it a little more different. I would like to have an extension method, where I pass the source list and a min and a max count of list entries:

~~~ csharp
public static GenFuConfigurator<TType> AsRandom<TType, TData>(
    this GenFuComplexPropertyConfigurator<TType, IEnumerable<TData>> configurator,
    IEnumerable<TData> data, int min, int max)
    where TType : new()
{
    configurator.Maggie.RegisterFiller(
        new CustomFiller<IEnumerable<TData>>(
            configurator.PropertyInfo.Name, typeof(TType),
            () => data.GetRandom(min, max)));

    return configurator;
}
~~~

This isn't really a Filler. This is an ExtensionMethod on the GenFuComplexPropertyConfiguration which registers a CustomFilleer to get random data out of the source list. As you can see, I reused the initially created extension method to generate the random lists, but I needed to modify that extension method to use the randomizer of GenFu instead of a separate one:

~~~ csharp
private static IEnumerable<T> GetRandom<T>(this IEnumerable<T> source, int min, int max)
{
    var length = source.Count();
    var index = A.Random.Next(0, length - 1);
    var count = A.Random.Next(min, max);

    return source.Skip(index).Take(count);
}
~~~

I also made this method private because of the dependency to GenFu.

Now I can use this method in the GenFu configuration of the UserGroup to randomly fill the leaders and the meetings of a user group:

~~~ cSharp
var leaders = A.ListOf<Leader>(20);
var meetings = A.ListOf<Meeting>(100);

A.Configure<UserGroup>()
    .Fill(x => x.Members).WithinRange(10, 250)
    .Fill(x => x.Name).AsMusicGenreName()
    .Fill(x => x.Description).AsMusicGenreDescription()
    .Fill(x => x.Founded).AsPastDate()
    .Fill(x => x.Leaders).AsRandom(leaders, 1, 4)
    .Fill(x => x.Meetings).AsRandom(meetings, 5, 100);
~~~

This is not really much code to automatically generate test data for your test or the dummy data of your mock-up. Just a bit of configuration which can be placed somewhere in a central  place.

![]({{ site.baseurl }}/img/genfu/usergroups.png)

### I think ...
... GenFu becomes my favorite library to create test and dummy data. I like the way GenFu generates well looking random dummy data. GenFu is really easy to use and to extend. 

BTW: You'll find the small play around application on GitHub: [https://github.com/JuergenGutsch/GenFuUserGroups/](https://github.com/juergengutsch/GenFuUserGroups/)