---
layout: post
title: "Develop cloud native applications using .NET Aspire"
teaser: "At the .NET Conf 2023, Microsoft announced a kind of toolset to build cloud-native applications. In this post, I'm going to introduce .NET Aspire and show it in action with a small demo solution."
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

At the .NET Conf 2023, Microsoft announced a kind of toolset to build cloud-native applications. That announcement was kind of [hidden in a talk](https://youtu.be/z1M-7Bms1Jg?si=JCSi-dqZYptF_ZEf) done by [Glenn Condron](https://twitter.com/condrong) and [David Fowler](https://twitter.com/davidfowl) about building cloud-native applications using .NET 8, which was also announced at that conference. This talk actually was about .NET Aspire, which I will quickly introduce with this post.

Let's start first by answering a question.

When I did a talk about .NET Aspire recently [at the .NET user group in Basel (CH)](https://www.meetup.com/basel-net-user-group/events/297394460/), one individual in the audience asked me the following question:  

## What is a cloud-native application?

Let's ask the internet to find the right answer:

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

## What is .NET Aspire doing?

.NET Aspire helps with **tooling** in VS and the CLI to create and interact with .NET Aspire apps. It also brings some project templates to create new .NET Aspire apps. .NET Aspire helps with **orchestrating**, means running and connecting to multi-project applications and their dependencies. It also provides **components** that connect to cloud dependencies like queues, caches, databases, or even prebuild containers. All those components can be orchestrated and connected to your applications using C#. .NET Aspire creates a deployment-ready development environment. Using the Azure Development CLI (azd) you can easily deploy your cloud native application to Azure.

.NET Aspire is made for local development and it is made for Microsoft Azure. Developments and deployments to other clouds might be possible in the future with the support of the developer community. In the first stage, Microsoft will not support other cloud providers. Which makes sense since Azure is the number one platform for Microsoft. 

.NET Aspire uses Docker Desktop to run your cloud-native application. When you press F5 in VS, your apps will be deployed to containers and will run on Docker Desktop locally. When you deploy your cloud-native application, a Bycep script will be created and your apps will be deployed to a new Azure Resource Group inside Azure Container Apps. App Service Containers are not supported yet. AKS is only supported via the community tool Aspirate.

Currently, .NET Aspire is Preview 3. Which means some features might not work or are not yet implemented.

But those limitations are absolutely fine for the moment.

## Why is .NET Aspire needed?

Actually, it is not needed. There are good tools out there to set up the local development environment the way you can develop cloud-native applications locally. There are also tools that set up your development environment inside the cloud to develop in the same environment where your application will live. This is great and super helpful. Unfortunately, these possibilities are sometimes hard to set up and some teams can't use it for some reason. The easiest way to set up an environment locally for me as a developer on Windows using .NET was to use Docker Compose or to load or emulate the services I needed locally or to be connected to the cloud environment all the time and to use the cloud services directly. Both options are not perfect.

So, you see that .NET Aspire is not needed. But it is super helpful for me as a C# developer. 

## Let's have a quick look at .NET Aspire in action

Therefore, I created a frontend app using the new Blazor Web App and a backend that provides me with the data via a Web API endpoint. Both apps are just the default templates with the weather data demos. I just did a small modification: Instead of generating the weather data in the front end, it now loads them from the API. 

When doing right-click on one of the projects and select "Add", you will see two new entries in the context menu:

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

Line 9 adds a project reference to the front end. This one needs the backend and adds it as a reference.

How does the frontend know the backend address when the apps are running in orchestration? I do have the same problem when I use docker-compose to orchestrate apps. In this case, i just need to read the endpoint URL from the environment variables:

~~~csharp
IIbuilder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration.GetValue<string>("services:backend:0"))
});
~~~

You will see why this is working a little later.

Let's start the application but ensure Docker Desktop is running first. Since it is all in preview at the moment, you may need to start the application two times. Once the app is started you'll see the URL in the console that pops up. In case no browser opens automatically copy the URL and open it in a browser.:

![image-20240223094108726](C:\Users\JürgenGutsch\AppData\Roaming\Typora\typora-user-images\image-20240223094108726.png)

You will see the really cool Aspire portal in the browser that shows you all the running apps:

![image-20240223094441698](C:\Users\JürgenGutsch\AppData\Roaming\Typora\typora-user-images\image-20240223094441698.png)

This Portal is built with the new ASP.NET Core Blazor web. 

On the start screen, you see all the running services. Two instances of the backend app and one instance of the frontend app. You will also recognize the instance of the Redis cache. This is coming from a docker image that got pulled by Aspire and is running as a container now. You will also see that the backends have two endpoint URLs. One is equal to both instances and the other one is the individual URL for that specific container. The one that is equal to both is routed through a kid of a proxy.

This portal doesn't show you only the running services. Because of the Service defaults that got injected into the apps, it can read the health states, the logs, and the telemetry information of your apps.  This will help you to debug your locally running apps. Just click through the portal to see the logs, the traces, and the metrics.

When you click on the details link of a specific running service, you can also see the environment variables that got passed to the service.  In the next screenshot, you can see that the URL of the backend app will be passed as an environment variable to the frontend. This is the environment variable we used in the frontend to connect to the backend:

![image-20240223094932947](C:\Users\JürgenGutsch\AppData\Roaming\Typora\typora-user-images\image-20240223094932947.png)

The orchestration makes the services to know each other this way. The backend gets the connection string to Redis via the environment variable. This is why the services can interact. So there is almost no magic here. Just C# to orchestrate and environment variables to connect the services to each other.

## Deployment

As mentioned your cloud-native app will be orchestrated to be cloud-ready. You can easily deploy your application to your Azure subscription. The tool that helps you with that is the Azure Developer CLI (`azd`). This CLI is super easy to use prepares your app for you and can do the deployment. After the installation of `azd` you just use it.

With the console of your choice, cd to your solution folder and type `azd login`. This will open up a browser that you can use to log in with your Azure account.

The following command will prepare your application to be ready for deployment:

~~~shell
azd init
~~~

It creates some configuration files and a `Bycep` script to set up your environment on Azure. Take a look at it to learn about `Bycep`. 

The next command does the deployment:

~~~shell
azd up
~~~

If you own more than one subscription you are asked which one to use. The CLI is super awesome. It is an interactive one that guides you through the entire deployment. Just follow the instructions. 

If the deployment is done your app is up and running on Azure. It is really that easy.

It sets all up on Azure. A Redis is up and running. Your apps are running in Azure Container Apps and if you would have a SQL Server configured in .NET Aspire, it would also set up a SQL Azure for you 

Just don't use preview versions of .NET. That won't run on Azure and it took me some time to figure out why my cloud native app is not running on Azure. The easiest way to not stumble into that issue is to create a `global.json` and pin your solution to an SDK version of .NET that is supported on Azure. 

## Conclusion 

This is just an introduction post about .NET Aspire. I hope it gives you a good overview of it.

I will definitely follow the releases of .NET Aspire and I'm really looking forward to using the final release for the development of real applications that will go into production. 

I really like it and will - for sure - write more deep dive about it. I also did a [talk at the .NET user group Basel](https://www.meetup.com/basel-net-user-group/events/297394460/) and would also do it at your user group, if you like. I'm also open to conference talks.

Just one thing I would really like to have is the Aspire portal to be deployed as well. I think this will be super helpful to monitor applications in production. As far as I know, there are no plans yet to have this portal as a tool for production. On the other hand, if you don't properly secure this portal, it could be a really dangerous security risk and all the information that the portal provides is also available on the Azure portal. So there isn't a real need for that.

Do you want to learn more about .NET Aspire? Follow the docs that are super complete and also contain super helpful tutorials about all the built in components: https://learn.microsoft.com/de-de/dotnet/aspire/



