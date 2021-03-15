---
layout: post
title: "How to suppress dotnet whatch run to open a browser"
teaser: "An interesting question on Twitter leads me to write this small post. The question was how to suppress opening a browser when you run `dotnet watch run` in case you don't need it."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Tests
- Tag
---

An interesting question on Twitter leads me to write this small post. The question was how to suppress opening a browser when you run `dotnet watch run`.

The thing is, that you might not want to open a browser if you run `dotnet watch run` on an Web API project. Since the Web API projects have Swagger enabled by default, this might make sense but often you just want to run your backend project while your frontend project is open in a browser or whatever frontend you have.

## Using an environment variable

There are two options to change that behavior. You can set an environment variable, which sets the behavior globally or in a console session:

~~~bash
SET DOTNET_WATCH_SUPPRESS_LAUNCH_BROWSER=1
~~~

This will override the default behavior.

## Using the launchSettings.json

The better option is to change it project-wise for all the projects you want to suppress. This can be done in the `launchSettings.json` that you will find in the `Properties` folder of each project. The `launchsettings.json` contains `iisSettings` and two or more profiles that configures how the applications will be launched:

~~~json
{
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:32265",
      "sslPort": 44369
    }
  },
  "profiles": {
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "MyProject": {
      "commandName": "Project",
      "dotnetRunMessages": "true",
      "launchBrowser": true, 
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
~~~

The `launchBrowser` property in the `profiles` defines whether the browser should be opened or not. Set it to `false` in case you want to suppress it. 

In case you set the environment variable, it will override the setting of the `launchsettings.json`.

## Conclusion

In many cases you might want to see the Swagger UI in your browser to test your API but there are cases as well, where you just want to spin up up your backend and to work on your front end using the running API.