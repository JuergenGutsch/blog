---
layout: post
title: "Do you know the GitHub Advisory Database?"
teaser: "Since a while I'm trying to get into the topics of application security. One of the huge topics that is critical is about vulnerabilities in dependencies. This post is about why you should know the GitHub Advisory Database."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET
- NuGet
- AppSec
- DevSecOps
---

For a while, I'm trying to get into the topics of application security. Application security is a really huge area and covers a lot of topics. It contains user authentication as well as CORS, various kinds of injections, and many other errors you can do during development. One of the huge topics is about vulnerabilities in dependencies.

Almost every developer is using third-party libraries and components directly, via NuGet, NPM, pip, or whatever package manager. I did and still do as well. 

While adding a dependency to your application, can you ensure that this dependency doesn't contain any vulnerabilities, or even ensure that the risk of adding vulnerabilities to your application can be reduced? Every code can contain an error that results in a critical issue, so third-party dependencies can do as well. Adding a dependency to your application can also mean adding a security problem to your application.

NPM already has a dependency audit tool integrated. `npm --audit`  checks the packages.config against a vulnerabilities database and tells you about flaws and which version should be safe. In Python, you can install pip package globally to do vulnerabilities checks based on the requirements.txt against an open-source database. 

In the .NET CLI you can use `dotnet list package --vulnerable` to check package dependencies of your project. The .NET CLI [is using the GitHub Advisory Database](https://devblogs.microsoft.com/nuget/how-to-scan-nuget-packages-for-security-vulnerabilities/) to check for vulnerabilities: [https://github.com/advisories](https://github.com/advisories)

## GitHub Advisory Database

The GitHub Advisory Database contains "the latest security vulnerabilities from the world of open-source software" as GitHub writes here [https://github.com/advisories](https://github.com/advisories). 

More about the GitHub Advisory Database: 
[https://docs.github.com/en/code-security/security-advisories/about-github-security-advisories](https://docs.github.com/en/code-security/security-advisories/about-github-security-advisories)

Actually, I see a problem with the GitHub Advisory Database in the .NET universe. There are more than 270 thousand unique packages registered on NuGet.org and there are more than 3.5 million package versions registered but only 140 reviewed advisories Advisory Database. 

It is great to have the possibility to check the packages for vulnerabilities but it doesn't make sense to check against a database that only contains 140 entries for that amount of packages.

There might be some reasons for that:

### 1st) .NET packages authors don't know about the Advisory Database

I'm sure many NuGet package authors don't know about the Advisory Database. Like me. I learned about the Advisory Database just a couple of weeks ago.

### 2nd) It is not common the the .NET universe to report vulnerabilities

Compared to the other stacks the .NET universe is pretty new in the open-source world. Sure, are some pretty cool projects that are almost 20 years old, but those projects are only exceptions. 

### 3rd) There are less vulnerabilities in .NET packages

Since the .NET packages are based on a good and almost complete framework there might be fewer vulnerabilities than on other stacks. From my perspective, this might be possible, but it is petty much dependent on the kind of package. The more the package is related to a frontend, the more vulnerabilities can occur in such a library.

## Why should I use such a advisory database?

There are important reasons why you should use the advisory database or even report to an advisory database:

### Don't completely trust your own code

You as a package author are not really save with your own code. Vulnerabilities can occur in every code. Every good developer is focusing on business logic and doesn't really think about side aspects like application security. It can always happen that you create a critical bug that results in more or less critical vulnerability. 

Every time you fix such a case in your code you should report that to the GitHub Advisory Database to tell your users about possible security issues in older versions of your package. This way you protect your package users. This way you tell your users to feel responsible for your users. It doesn't tell your users that you are a bad developer. The opposite is the case.

It is not your fault to produce packages with vulnerabilities accidentally, but it would be your fault if you don't do anything about it. 

### Don't completely trust the NuGet packages you use

Execute a vulnerability check on the packages you use whenever it is possible. Update the packages you use to the latest version whenever it is possible.

Even Microsoft packages contain vulnerabilities as you can see here:
[https://github.com/advisories/GHSA-q7cg-43mg-qp69](https://github.com/advisories/GHSA-q7cg-43mg-qp69)

GitHub does such checks on the code level for you using the [Dependabot](https://github.com/dependabot). In case you don't host your code on GitHub you should use different tools like the already mentioned CLI tools or commercial tools like [SonaSource](https://www.sonarsource.com/), [Snyk](https://snyk.io/), or similar.

You can execute such checks on the build server immediately before you actually build your code. You could continue building in case the vulnerabilities are of a low or moderate level. You could stop building the code in case there are high or critical vulnerabilities

### Safe your users

- Check your code
- Check your dependencies
- Accept reported vulnerabilities of your package and fix them
- Report vulnerabilities that occurred in your package after you fixed it.
- Don't report vulnerabilities for your packages that actually occur in used packages.
  - This should be done by the other package authors  
- Report vulnerabilities of used packages in their repositories or to their maintainers
  - To give them a chance to fix it

 ## How to report a vulnerability

If you own a repository on GitHib you can easily draft and propose a new security advisory to the GitHub database. In your repository on GitHub there is a "Security" tab. If you click on that tab, you'll find the "Security advisories" page on the left-hand menu. Here you see your already drafted advisories as well as a button to create a new one:

![image-20210913215021624]({{site.baseurl}}/img/gsa/gsa01.png)

If you don't own that repository, you will see the same page but without the button to draft an advisory.

If you click that button, you'll see a nice form to draft the advisory.

![image-20210913220736613]({{site.baseurl}}/img/gsa/gsa02.png)

![image-20210913220644670]({{site.baseurl}}/img/gsa/gsa03.png)

Once it is drafted, you can request a CVE identifier or just publish it. The GitHub team will then review it and add it to the advisory database:

![image-20210913221748858]({{site.baseurl}}/img/gsa/gsa04.png)

![image-20210913221830226]({{site.baseurl}}/img/gsa/gsa05.png)

That's it.

If you find a critical problem on a repository that you don't own. You should create an issue on that repository, describe what you found. The repository owner should now fix the problem and add an advisory to the database.

## Conclusion

You definitely should take care of your dependencies and should check them for vulnerabilities. And you definitely should have a look at the GitHub Advisory Database  and you should report your advisories there

This would help to keep the applications secure.
