---
layout: post
title: "ASP.NET Core 3.0 Weather Application - The gRPC Client"
teaser: ""
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Tests
- Tag
---

The next couple of posts will be a series that describes how to build a kind of a microservice app using the latest ASP.NET Core 3.0 features.

I'm going to use gRPC, Worker Services, SignalR, Blazor and maybe the Identity Server to secure all the services. If some time is left, I'll put all the stuff into docker containers.

## Introduction

I'm going to write an application that reads weather data in, stores them and provides statistical information about that weather. In this case I use downloaded data from a weather station in Kent (WA). I'm going to simulate a day in two seconds. 

I will write a small gRPC services which will be our weather station in Kent. I'm also goin to write a worker service that hosts a gRPC Client to connect to the weather station to fetch the data every day. This worker service also stores the date into a database. The third application is a Blazor app that fetches the data from the database and displays the data in a chart and in a table.

In this post I will continue with the client that fetches the data from the server.

## Setup the app

``` shell
dotnet new worker -n WeatherStats.Worker -o WeatherStats.Worker

```





## The gRPC endpoint





## Conclusion



