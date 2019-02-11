---
layout: post
title: "Thoughts about repositories, ORMs and so on"
teaser: "description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- EF Core
- Architecture
---

The last architectural brain dump I did in my blog was more than three years ago. At that time it was my German blog, which is shut down unfortunately. Anyway, this is another one. This time I want to write about the sense of the repository pattern in combination with an object relational mapper (ORM) like Entity Framework (EF) Core.

In the past years I had some great discussions about the sense of repository pattern. Mostly offline, but also online on twitter or in the comments of this blog. I also followed discussions on twitter, in Jeff Ftitz's stream. 

My idea is you don't need to use repositories, if you use a unit of work. It is not only my idea, but it makes absolutely sense to me and I also favor not to use repositories in case I use EF or EF Core. There are many reasons. Let us look at them. 

## YAGNI - you ain't gonna need it

In the classic architecture pattern you had three layers: UI, Business and Data. That made kinda sense in the past, in a world of monolithic applications without ORM. At that time you wrapped all of the SQL and data mapping stuff into the data layer. The actual work with the data was done in the business layer and the user interacted with the data in the UI. Later this data layers become more and more generic. 

> At least I created generic data layers in the past which generated the SQL based on the type the data need to get mapped to. I used the property information of the types as well as attributes. Just before ORM were a thing in .NET. Yes. I created OR mappers, but I didn't really care ;-)

Wait... What is a data layer for, if you already use a ORM?

To encapsulate the ORM? Why would you do that? To change the underlaying ORM, if needed? When did you ever changed the ORM and why? 

These days you don't need to change the ORM in case you change the database. You only need to change the date provider of the ORM. Because the ORM is generic and is able to access various database systems. 

To not have ORM dependencies in the business and UI layers? You'll ship your app including all dependencies anyway. 

To easier test the business logic in an isolated way, without the ORM? This is anyway possible and you need to test the repositories as well in an isolated way. Just mock the DbContext.

You ain't gone need an additional layer you need to maintain and to test. This is additional senseless code in the most cases. It just increases the lines of code and only makes sense, if you get paid for code instead of solutions

## KISS - keep it simple and stupid

In almost all cases, the simplest solution is the best one. Why? Because it is a solution and because it is simple ;-) 

Simple to understand, simple to test and simple to maintain. For the most of us, it is hard to create a simple solution, because our brains aren't working that way. That's the crux we have as software developers: We are able to understand complex scenarios, write complex programs, building software for self driving cars, video games and space stations. 

In reality our job is to make complex things as simple as possible. The most of us do that, by writing business software that helps a lot of people doing their work in an efficient way that saves a lot of time and money. But often we use rocket science, or we use sky scraper technology to just build a tiny house. 

Why? Because we are developers, we think in a complex way and we really, really love rocket science.

But sometimes we should look for a little simpler solution. Don't write code you don't need. Don't create a complex architecture, if you just need a tiny house. Don't use rocket science to build a car. Keep it simple and stupid. Your application just needs to work for the customer.

## SRP - Single responsibility prinziple

I think the SRP principle was confused a little in the past. What kind of responsibilities are we talking about? Should a business logic not fetch data or should a product service not create orders? Do you see the point? In my opinion we should split the responsibilities by topic first and later inside the service classes we are able to split on method level by abstraction or what ever we need to separate. 

This should keep the dependencies as small as possible and every service is a single isolated module, which is responsible for a specific topic. 

## Let write some code to describe what I mean

Back to the repositories, let's write some code and let's compare some code snippets. This is just some kind of fake code. But a saw something like this a lot in the past. In the first snippet we have a business layer, which needs three repositories to update some data. Mostly a repository is created per database table or per entity. This is why this business layer need to use two more repositories just to check for additional fields:

~~~ csharp
public class AwesomeBusiness
{
    private readonly AwesomeRepository _awesomeRepository;
    private readonly CoolRepository _coolRepository;
    private readonly SuperRepository _superRepository;

    public AwesomeBusiness(
        AwesomeRepository awesomeRepository,
        CoolRepository coolRepository,
        SuperRepository superRepository)
    {
        _awesomeRepository = awesomeRepository;
        _coolRepository = coolRepository;
        _superRepository = superRepository;
    }
    
    public void UpdateAwesomeness(int id)
    {
        var awesomeness = _awesomeRepository.GetById(id);
        awesomeness.IsCool = _coolRepository.HasCoolStuff(awesomeness.Id);
        awesomeness.IsSuper = _superRepository.HasSuperStuff(awesomeness.Id);
        awesomeness.LastCheck = DateTime.Now;
        _awesomeRepository.UpdateAwesomeness(awesomeness);
    }
}

public class AwesomeRepository
{
    private readonly AppDbContext _dbContext;

    public AwesomeRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    internal void UpdateAwesomeness(Awesomeness awesomeness)
    {
        var aw = _dbContext.Awesomenesses.FirstOrDefault(x => x.Id = awesomeness.Id);
        aw.IsCool = awesomeness.IsCool;
        aw.IsSuper = awesomeness.IsSuper;
        aw.LastCheck = awesomeness.LastCheck;
        _dbContext.SaveChanges();
    }
}

public class SuperRepository
{
    private readonly AppDbContext _dbContext;

    public SuperRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    internal bool HasSuperStuff(int id)
    {
        return _dbContext.SuperStuff.Any(x => x.AwesomenessId == id);
    }
}

public class CoolRepository
{
    private readonly AppDbContext _dbContext;

    public CoolRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    internal bool HasCoolStuff(int id)
    {
        return _dbContext.CoolStuff.Any(x => x.AwesomenessId == id);
    }
}

public class Awesomeness
{
    public int Id { get; set; }
    public bool IsCool { get; set; }
    public bool IsSuper { get; set; }
    public DateTime LastCheck { get; set; }
}
~~~

Usually the Repositories are much bigger that this small classes, they provide functionality for the default CRUD operations on the object and sometimes some more. 

I've seen a lot of repositories the last 15 years, some where kind generic or planned to be generic. Most of them are pretty individual depending on the needs of that object to work on. This is so much overhead for such a simple feature.

Why not just like this?

~~~ csharp
public class AwesomeService
{
    private readonly AppDbContext _dbContext;

    public AwesomeService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void UpdateAwesomeness(int id)
    {
        var awesomeness = _dbContext.Awesomenesses.First(x => x.Id == id);
        awesomeness.IsCool = _dbContext.CoolStuff.Any(x => x.AwesomenessId == id);
        awesomeness.IsSuper = _dbContext.SuperStuff.Any(x => x.AwesomenessId == id);
        awesomeness.LastCheck = DateTime.Now;

        _dbContext.SaveChanges();
    }
}

public class Awesomeness
{
    public int Id { get; set; }
    public bool IsCool { get; set; }
    public bool IsSuper { get; set; }
    public DateTime LastCheck { get; set; }
}
~~~

This is simple, less code, less dependencies, easy to understand and anyway working. Sure it uses EF directly. If there is really, really, really the need to encapsulate the ORM. Why don't you create repositories by topic instead of per entity? Let's have a look:

~~~ csharp
public class AwesomeService
{
    private readonly AwesomeRepository _awesomeRepository;

    public AwesomeService(AwesomeRepository awesomeRepository)
    {
        _awesomeRepository = awesomeRepository;
    }     

    public void UpdateAwesomeness(int id)
    {
        _awesomeRepository.UpdateAwesomeness(id);
    }   
}

public class AwesomeRepository
{
    private readonly AppDbContext _dbContext;

    public AwesomeRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    internal void UpdateAwesomeness(int id)
    {
        var awesomeness = _dbContext.Awesomenesses.First(x => x.Id == id);
        awesomeness.IsCool = _dbContext.CoolStuff.Any(x => x.AwesomenessId == id);
        awesomeness.IsSuper = _dbContext.SuperStuff.Any(x => x.AwesomenessId == id);
        awesomeness.LastCheck = DateTime.Now;
        _dbContext.SaveChanges();
    }
}

public class Awesomeness
{
    public int Id { get; set; }
    public bool IsCool { get; set; }
    public bool IsSuper { get; set; }
    public DateTime LastCheck { get; set; }
}
~~~

This looks clean as well and encapsulates EF pretty well. But why do we really need the AwesomeService in that case? It just calls the repository. It doesn't contain any real logic, but needs to be tested and maintained. I also saw this kind of services a lot the last 15 years. This also doesn't make sense to me. At the end I always end up with the second solution. 

We don't need to have a three layered architecture, because we had the last 20 years. It always depends.

## Architectural point of view

The point of view changed over the last years. I don't look at data objects anymore. If I do architecture, I don't use the OOP glasses anymore. I try to explore the data flows inside the solution. Where do the data come from, how do the data transform on the way to the target and where do the data go? I don't think in layers anymore. I try to figure out what is the best way to let the date flow into the right direction.

I'm looking for the main objects the application is working on. In that case object isn't a .NET object or any .NET Type. It is just a definition. If I'm working on a shopping card, the main object is the order. This produces money. This is the object that produces the most of the actions and contains and produces the most of the data.   

Depending on the size and the kind of the application, I end up using different kind architectural patterns. Despite what pattern is used, there's no repository anymore. There are services that provide the data in the way I need them in the UI. Sometimes the Services are called Handlers, depending what architectural pattern is used, but they work the same way. Mostly they are completely independent from each other. There's no think like a UserService or GroupService, but there's an AuthService or an ProfileService. There is no ProductService, CategoryService or CheckoutService, but an OrderService.

## What do you think?

Does this make sense to you? What do you think?

I know this is a topic that is always discussed in a controversy way. But it shouldn't. But tell me your opinion about that topic. I'm curious to learn more from you

For me it worked quite well. I reduced a lot of overhead and a lot of code.

