---
layout: post
title: "Title"
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

Post your content here





~~~shell
dotnet new mvc -n PlayWithPlaywrite
dotnet new nunit -n PlayWithPlaywrite.Tests
dotnet new sln -n PLayWithPlaywrite
dotnet sln add .\PlayWithPlaywrite\
dotnet sln add .\PlayWithPlaywrite.Tests\
dotnet add .\PlayWithPlaywrite.Tests\ reference .\PlayWithPlaywrite\

dotnet add .\PlayWithPlaywrite\ package Microsoft.Playwright.NUnit
~~~



~~~
dotnet build
~~~

Install required browsers

~~~
.\PlayWithPlaywrite.Tests\bin\Debug\net7.0\playwright.ps1 install
~~~





~~~
.\PlayWithPlaywrite.Tests\bin\Debug\net7.0\playwright.ps1 codegen https://asp.net-hacker.rocks/codegen https://asp.net-hacker.rocks/
~~~

![image-20230125212422560](C:\Users\webma\AppData\Roaming\Typora\typora-user-images\image-20230125212422560.png)





~~~
dotnet add .\PlayWithPlaywrite\ package Microsoft.AspNetCore.Mvc.Testing
~~~

