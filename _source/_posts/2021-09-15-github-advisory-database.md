---
layout: post
title: "Do you know the GitHub Advisory Database?"
teaser: "Description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET
- NuGet
- AppSec
- DevSecOps
---

Since a while I'm trying to get into all topics of application security. Application security is a really huge area and covers a lot of topics. It contains user authentication as well as CORS, various kind of injections and many other errors you can do during development. One of the huge topics that is critical is about vulnerabilities in dependencies.

Almost every developer is using third party libraries and components directly, via NuGet, NPM, pip or whatever package manager. I did and still do as well. 

While adding a dependency to your application, can you ensure that this dependency doesn't contain any vulnerabilities, or even ensure that the risk of adding vulnerabilities to your application can be reduced? Every code can contain an error that results in a critical issue, so third party dependencies can do as well. Adding a dependency to your application can also mean adding a security problem to your application.

NPM already has an dependency audit tool integrated. `npm --audit`  checks the packages.config against a vulnerabilities database and tells you about flaws and which version should be safe. In python you can install pip package globally to do vulnerabilities checks based in the requirements.txt against an open source database. 

 In the .NET CLI you can use `dotnet list package --vulnerable` to check package dependencies of you project. The .NET CLI [is using the GitHub Advisory Database](https://devblogs.microsoft.com/nuget/how-to-scan-nuget-packages-for-security-vulnerabilities/) to check for vulnerabilities: [https://github.com/advisories](https://github.com/advisories)

## GitHub Advisory Database

The GitHub Advisory Database contains "the latest security vulnerabilities from the world of open source software" as GitHub writes here [https://github.com/advisories](https://github.com/advisories). 

More about the GitHub Advisory Database: 
[https://docs.github.com/en/code-security/security-advisories/about-github-security-advisories](https://docs.github.com/en/code-security/security-advisories/about-github-security-advisories)

Actually I see a problem with the GitHub Advisory Database in the .NET universe. There are more than 270 thousand unique packages registered on NuGet.org and there are more than 3.5 million package version registered but only 140 reviewed advisories Advisory Database. 

It is great to have the possibility to check the packages for vulnerabilities but it doesn't make sense to check against a database that only contains 140 entries for that amount of packages.

There might be some reasons for that

### 1st) .NET packages authors don't know about the Advisory Database

I'm sure many NuGet package authors don't know about the Advisory Database. Like me. I learned about the Advisory Database just a couple of weeks ago.

### 2nd) It is not common the the .NET universe to report vulnerabilities

Compared to the other stacks the .NET universe is pretty new in the open source world. Sure, are some pretty cool projects that are almost 20 years old, but those project are only exceptions. 

### 3rd) There are less vulnerabilities in .NET packages

Since the .NET packages are based on a good and almost complete framework there might be less vulnerabilities that on other stacks. From my perspective this might be possible, but it is petty much depended on the kind of a package. The more the package is related to a frontend, the more vulnerabilities can occur in such a library.

## Don't completely trust your own code

You as a package author are not really save with your own code. Vulnerabilities can occur in every code. Every good developer is focusing on business logic and doesn't really think about side aspects like application security. It can always happen that you create a critical bug that results in more or less critical vulnerability. 

Every time you fix such a case in your code you should report that to the GitHub Advisory Database to tell your users about possible security issues in older versions of your package. This way you protect your package users. This way you tell your users to feel responsible for your users. It doesn't tell your users that you are a bad developer. The opposite is the case.

It is not your fault to produce packages with vulnerabilities accidently, but it would be you fault if you don't do anything about it. 

## Don't completely trust packages you use

Execute a vulnerability check on the packages you use whenever it is possible. Update the packages you use to the latest version whenever it is possible.

GitHub does such checks on the code level for you using the [Dependabot](https://github.com/dependabot). In case you don't host your code on GitHub you should use different tools like the already mentioned CLI tools or commercial tools like [SonaSource](https://www.sonarsource.com/), [Snyk](https://snyk.io/) or similar  

## Safe your users

- Check your code
- Check your dependencies
- Accept reported vulnerabilities of your package and fix them
- Report vulnerabilities that occurred in your package after you fixed it.
- Don't report vulnerabilities for your packages that actually occur in used packages.
  - This should be done by the other package authors  
- Report vulnerabilities of used packages in their repositories or to their maintainers
  - To give them a chance to fix it

 
