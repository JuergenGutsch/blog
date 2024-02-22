---
layout: post
title: "Develop cloud native applications using .NET Aspire"
teaser: "At the .NET Conf 2023, Microsoft announced a kind of a toolset to build cloud-native applications. In this post I'm going to introduce .NET Aspire and show it in action with a small demo solution."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Cloud Native
- Microservices
- Aspire
---

At the .NET Conf 2023, Microsoft announced a kind of a toolset to build cloud-native applications. That announcement was kind of [hidden in a talk](https://youtu.be/z1M-7Bms1Jg?si=JCSi-dqZYptF_ZEf) done by [Glenn Condron](https://twitter.com/condrong) and [David Fowler](https://twitter.com/davidfowl) about building cloud-native applications using .NET 8, which was also announced at that conference. This talk actually was about .NET Aspire, which I will quickly introduce with this post.

Let's start first by answering a question.

When I did a talk about .NET Aspire recently [at the .NET user group in Basel (CH)](https://www.meetup.com/basel-net-user-group/events/297394460/), one individual in the audience asked me the following question:  

## What is a cloud-native application?

Let's as the internet to find the right answer:

> **Amazon:**
> "Cloud native is the software approach of building, deploying, and managing modern applications in cloud computing environments. Modern companies want to build highly scalable, flexible, and resilient applications that they can update quickly to meet customer demands. To do so, they use modern tools and techniques that inherently support application development on cloud infrastructure. These cloud-native technologies support fast and frequent changes to applications without impacting service delivery, providing adopters with an innovative, competitive advantage."
> (https://aws.amazon.com/what-is/cloud-native/)

> **Google:**
> "A cloud-native application is specifically designed from the ground up to take advantage of the elasticity and distributed nature of the cloud. "
> (https://cloud.google.com/learn/what-is-cloud-native)

> **RedHat:**
> "Cloud-native applications are a collection of small, independent, and loosely coupled services."
> (https://www.redhat.com/en/topics/cloud-native-apps)

> **Oracle:**
> "The term cloud native refers to the concept of building and running applications to take advantage of the distributed computing offered by the cloud delivery model. Cloud-native apps are designed and built to exploit the scale, elasticity, resiliency, and flexibility the cloud provides."
> (https://www.oracle.com/cloud/cloud-native/what-is-cloud-native/)

> **Microsoft:**
> "*Cloud-native architecture and technologies are an approach to designing, constructing, and operating workloads that are built in the cloud and take full advantage of the cloud computing model.*"
> (https://learn.microsoft.com/en-us/dotnet/architecture/cloud-native/definition)

> **Cloud Native Computing Foundation (CNCF):**
> "Cloud native technologies empower organizations to build and run scalable applications in modern, dynamic environments such as public, private, and hybrid clouds. Containers, service meshes, microservices, immutable infrastructure, and declarative APIs exemplify this approach.
>
> These techniques enable loosely coupled systems that are resilient, manageable, and observable. Combined with robust automation, they allow engineers to make high-impact changes frequently and predictably with minimal toil."
> (https://github.com/cncf/toc/blob/main/DEFINITION.md)

Every answer is a little different. Basically, it means a cloud-native application is built for the cloud and uses the services the cloud provides to be scalable and resilient.

## Why is .NET Aspire needed?

Actually, it is not needed. There are good tools out there to set up the local development environment the way you can develop cloud-native applications locally. There are also tools that set up your development environment inside the cloud to develop in the same environment where your application will live. This is great and super helpful. Unfortunately, these possibilities are sometimes hard to set up and some teams can't use it for some reason. The easiest way to set up an environment locally for me as a developer on Windows using .NET was to use Docker Compose or to load or emulate the services I needed locally or to be connected to the cloud environment all the time and to use the cloud services directly. Both options are not perfect.

So, you see that .NET Aspire is not needed. But it is super helpful for me as a C# developer. 

## What is .NET Aspire doing?

.NET Aspire helps **tooling** in VS and the CLI to create and interact with .NET Aspire apps. It also brings some project templates to create new .NET Aspire apps. .NET Aspire helps with **orchestrating**, means running and connecting to multi-project applications and their dependencies. It also provides **components** that connect to cloud dependencies like queues, caches, databases, or even prebuild containers. All those components can be orchestrated and connected to your applications. .NET Aspire creates a deployment-ready development environment. Using the Azure Development CLI (azd) you can easily deploy your cloud native application to Azure.

.NET Aspire is made for local development and it is made for Microsoft Azure. Developments and deployments to other clouds might be possible in the future with the support of the developer community. In the first stage, Microsoft will not support other cloud providers. Which makes sense since Azure is the number one platform for Microsoft. 

.NET Aspire uses Docker Desktop to run your cloud-native application. When you press F5 in VS, your apps will be deployed to containers and will run on Docker Desktop locally. When you deploy your cloud-native application, a Bycep script will be created and your apps will be deployed to a new Azure Resource Group inside Azure Container Apps. App Service Containers are not supported. AKS is only supported via the community tool Aspirate.

Currently, .NET Aspire is Preview 3. Which means some features might not work or are not yet implemented.

But those limitations are absolutely fine for the moment.

## Let's have a quick look at .NET Aspire in action

Therefore, I created a frontend app using the new Blazor Web App and a backend that provides me with the data via a Web API endpoint. Both apps are just the default templates with the weather data demos. I just did a small modification: Instead of generating the weather data in the front end, it now loads them from the API. 

When doing a right-click on one of the projects and select "Add", you will see two new entries in the context menu:

* ".NET Aspire Component..."
* ".NET Aspire Orchestration Support..."

![image-20240222214405599](C:\Users\JürgenGutsch\AppData\Roaming\Typora\typora-user-images\image-20240222214405599.png)

Selecting ".NET Aspire Orchestration Support...", it creates two new projects in your solution:

![image-20240222214727408](C:\Users\JürgenGutsch\AppData\Roaming\Typora\typora-user-images\image-20240222214727408.png)

The AppHost is the project where you will do the actual composition, we will have a more detailed look later. The ServiceDefaults contains one single code file with extension methods that configure default services and Middlewares the actual projects need to use. Mainly Telemetry and HelthChecks. Actually, these service defaults are added to the actual projects when adding the Aspire Orchestration support. The following code shows the usage of the default in lines 5 and 17:

![image-20240222215145900](C:\Users\JürgenGutsch\AppData\Roaming\Typora\typora-user-images\image-20240222215145900.png)

As you can see, I also configured a HttpClient that connects to the backend API. 

I also added the Aspire orchestration support to the backend API and the service defaults are added to that project as well. In this project, I configured a distributed Redis cache in line 14:

![image-20240222220527042](C:\Users\JürgenGutsch\AppData\Roaming\Typora\typora-user-images\image-20240222220527042.png)

This application contains three components: A frontend which is a Blazor web app, a backend which is a minimal API and a Redis cache. These three components need to be orchestrated to run and debug it locally. The problem is, that I don't have a local instance of Redis yet.

This is where Aspire can help us. Let's have a look into the `Program.cs` of the AppHost Project:

~~~csharp
var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var backend = builder.AddProject<Projects.WeatherApp_Backend>("backend")
    .WithReference(cache)
    .WithReplicas(2);

builder.AddProject<Projects.WeatherApp_Frontend>("frontend")
    .WithReference(backend);

builder.Build().Run();
~~~

This looks pretty similar to a regular minimal API without any ASP.NET Core stuff. The first line defines a DistributedApplicationBuilder which is the orchestrator. 

Line 3 adds Redis to the orchestration with the name "cache".  Remember that we configured the distributed cache with the exact same name in the backend project. 

Line 5 adds a project reference to the orchestration with the name backend. It references the cache and it should start two instances of the backend.

Line 9 adds a project reference to the frontend. This one needs the backend and adds it as a reference.

How does the frontend know the backend address when the apps are running in orchestration? I do have the same problem when I use docker-compose to orchestrate apps. In this case, i just need to read the endpoint URL from the environment variables:

~~~csharp
Ibuilder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration.GetValue<string>("services:backend:0"))
});
~~~

You will see why this is working a little later.

Let's start the application but ensure Docker Desktop is running first.





https://learn.microsoft.com/de-de/dotnet/aspire/



