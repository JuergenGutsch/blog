---
layout: post
title: "Software Architecture decisions. A personal approach."
teaser: "Description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Tests
- Tag
---

Software Architecture is a really controversy topic. Sometimes discussions about software architecture are not really possible because some architecture patterns seem become a religious flavor. Sometimes there are so many personal feelings involved in some architecture decisions, that the result will be a bad decision in any way.

Just because pragmatism will be missing in the decisions.

## Pragmatism and laziness

Pragmatism and laziness are quite important for software engineers and on software architectural decisions. 

I'm a software engineer because of the delete key and because I'm a super lazy asshole. (Just ask my ex-wife she will prove.) If I need to do things twice, I programmed it wrong once. As mentioned in another blog post, you job as a software engineer is to make complex things simple. That means we need to think about how simplify and and automate processes that are too complex or take to long. Bei Lazy and get more time to do the fun things. Actually, being a lazy developer is fun and that makes me to make more complex stuff easy and fun. A kind of a chain reaction or recursion.

**How to use pragmatism and laziness in software architectural decisions?**

I'd say, at first you really need to know the customer and the user and the way how they work and what they need to achieve.. This is what a software engineer can do together with a usability expert. Than start to think about what can be done in a more efficient way. Try to be a lazy user. What helps you to get your job done in just a glimpse.

If you forget to know the user, you will build an easy to use and really efficient peace of crap, in case you are a pragmatic and lazy developer. If you are not like this, it will end up like a money burning software nightmare created by one of the biggest German software companies with three capital letters starting with S and ending with P.

Also knowing your user and his needs is really interesting and fun. This is why I know, how to fix broken used water pipes in huge buildings and what it needs to do it in an efficient way.

## Clean Code in mind

If you don't know Clean Code you definitely should. You will learn a lot about principles and practices a software engineer should know. Try to get the book by Robert C. Martin. If you have learned all the stuff, follow the role of being a pragmatic and lazy developer and don't use the the Clean Code principles and practices.

Absolutely! Don't use them without thinking pragmatic and think about whether you really need that principles and practices or not. 

Once I did a Clean Code training at a quite cool software company with a lot of highly motivated software engineers. I taught them all the principles and practices and at the end I asked them to solve a quite simple coding challenge called **FizzBuzz Kata** within one hour. (Google for it with Bing, if you like to learn more about that Kata.) The software engineers at that company are really great and really knew the stuff I taught them. They used all the stuff they learned to solve that simple challenge. Really Awesome. But they failed completely, because they created an **Enterprise FizzBus** that wasn't working at the end.

They weren't pragmatic and lazy, even if there are two Clean Code principles for being like this:

* KISS = Keep it simple and stupid
* YAGNI = You ain't gonna need it

Both that principles are the most important Clean Code from my perspective. Those principles make you think about simplicity and about things you might not need, because nobody asked about it and nobody will pay for it. Keeping that principles in mind also helps you if you need to do architectural decisions.

That Back to software architecture.

Many classy architectural patterns contain ideas and concepts that are not or not yet needed for the challenge you need to solve. As a software engineer you don't need to think about requirements that maybe come somewhen in the future. Why? Because the customer doesn't pay you for thing he never requested for.

One other important Clean Code principle came in to solve a problem of features that might come in the future.

* Open/Close principle => Being open for extensions, but close for manipulation

Sometimes software engineers are thinking about problems that might come in the future and try to solve them. Actually, this is burned money and wasted time. Some software engineers told me, the customer often doesn't know what he really needs. Actually, I think this is wrong in the almost all cases. The software engineer who said this, doesn't know the customer, isn't pragmatic and even not lazy. This kind of software engineer puts a lot of effort into getting as much money from the customer as possible, he will not solve anything and will loose the customer sooner than later.

Let's go back to software architectural decisions.

After more than 20 years in the industry, I came to the conclusion that there is only one single architecture pattern that is true on every kind of software you want to build. 

I call it the IDA-Pattern which stands for the following

# The "It Depends Architecture Pattern"

I guess you already recognized that it's not a real pattern, but just a try to tell you that there is no single solution for all problems.  

Even I have a favorite architectural design pattern but I know that I can't use it on every application I starting to build. My favorite architecture pattern is the CQRS pattern (Command & Query Responsibility Segregation). This is a technically fascinating pattern and actually solves a lot of problems you will have when you build larger applications. That's the point and the reason why I only used it once in in a real project. Why? Because it solves a lot of problems, I never had in the most applications I ever built. Having KISS and YAGNI in mind, I would never use CQRS for smaller or less complex applications.

To know what is a large and what is a less complex application, let's talk about project sizes:

## Project sizes 

It is hard to define what is a smaller or a larger applications. The following four aspects help me to get an idea about the size:

- Thinking of line of codes seems to be wrong.
- Thinking of the amount of features seems to be wrong as well. 
- Thinking of time spend on implementing, even seems to be kind of wrong.

* Actually, I am more thinking of triviality and complexity but even this won't be true in all cased.

At the end it can be the combination of all four aspects. A complex application with a ton of code, a huge amount of features that needs a lot of time to implement, is a complex application in the most cases and can be called a large application. I guess you get the point. It depends.

To choose the right architectural pattern, means to estimate the size of the application. To know the size of the application you you need to know your users, how they work and the problems the new solutions need to solve for them.

## Pasta patterns

I love lasagna, but don't like layered architecture anymore. Slicing applications into layers is the stuff you learn in school of university. Usually, it adds at least one layer of complexity that is not really needed. I'm not talking about MVC, MVP, etc. Those are UI patterns and techniques to clearly separate UI technique from UI logic and to structure UI code in a way to match the Open/Close principle. Actually I was talking about layering between UI, business layer and data layer as you learned in school.

This kind of three layer architecture doesn't make sense and is completely against the Open/Close principle and YAGNI. Slicing an application horizontally won't scale. The better way would be to slice an application vertically. On the other hand vertical isolated slices can produce a lot of duplicate code.

We all learned in school that we should avoid spaghetti patterns. I just wrote that lasagna won't scale, no matter if we create classy horizontal lasagna or the fancy vertical lasagna.

What about ravioli pattern? Each ravioli is a single isolated module. There are no layers anymore. No Slices. Just modules.

You can now start thinking about a generic layer between all the modules to keep all raviolis in a uniform shape and to stick them all together as you like. But this isn't really needed, since every module will have a specific logic, it can also have a specific API. Being to generic is against the YAGNI principle. Try to be lazy and ask yourself: Do I really need that? Does it actually solve the customers problem?

To be concrete: A single ravioli is basically a business logic module that either talks to the database directly or to another business logic module. Talking to the database directly can either mean using the native drivers to connect or to use a OR-Mapper directly. What makes more sense is not part of that story. There is no need for a specific database layer. There is no need for a repository pattern. 

I call a business logic module just a services. A service provides me the data I need to see or handles the data I provide to the application. Just that. A service should be as isolated as possible and not call any other service, except side aspects like logging or maybe authorization and stuff like this.

For sure to keep a module testable and isolated, you should inject the OR-Mapper, the logger, even the native db access client or any other interface to an external resource that is not under your control. 

Ravioli seems to be a good pattern and at the end it is not contraire to CQRS if I would ever get a chance to use it again in another real world project.

And because of that I really prefer the ravioli architecture pattern. It's just simple and it works

## Patterns I use



* The service pattern described in the section above for smaller applications.

* - Roughly, I create a service per topic and a service method per functionality
  - This is a more data driven approach. 

* CQS (Command & Query Segregation) if the application may get a larger.

* - One command per user action and a query per view, handled by one or more handlers that directly use the EF context.
  - This also is a data driven approach, but thinks in commands (user actions) and queries (user requests).
  - The handlers actually, are services as well, but specialized for commands and queries.

* CQRS (Command & Query Responsibility Segregation) for large applications

* - Almost the same as above, but uses an event store on the main domain object to store the states of that object. It completely doesn't think in relational models anymore. 

* Microservice / Distributed Service oriented approach

* - This is a special case, that handles another aspect that I didn't mention before: Each service can be built in a different technology and the implementation is completely decoupled, while the first three architecture design patterns can also work in monolithic applications, this can't be monolithic by design.
  - In that case, I have many small applications that mainly follow the service approach. But could also a mix of the previously described options, depending on the size and the complexity of each services.

 

 

