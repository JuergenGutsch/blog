---
layout: post
title: "NuFet, Security, and Central Package Management"
teaser: "Description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Visual Studio
- NuGet
---

Since a while I took over two more roles in the company. Besides being a software engineer, I'm also responsible sharing knowledge, keeping awareness, and supporting projects regards quality assurance and application security. The later is the topic why I'm writing this lines.   

Package Vulnerabilities

In Visual Studio, NuGet is checking for the referenced packages for vulnerabilities which is a great new feature. We now see in VS if a referenced package or a transitive package has vulnerabilities. We can also run a dotnet CLI command to see if a packages has vulnerabilities also great. This can be used in build pipelines to automatically check during build time. Awesome.

For a while now, we are using a separate tool that is scanning our our repositories on a code change on the main branches and PRs. This tool is not using NuGet but reads out all files that have NuGet references, like project files and other package reference files. It checks the package names and version against CVE Databases for published vulnerabilities and reports them directly to us via Slack notifications. We can also create Jira tickets directly within the tool and assign them to the right project and the right person to solve the problem. Also this tool is scanning transitive packages. which is cool in general.

Vulnerable Transitive Packages

In the previous section I mentioned transitive packages two times. Those are packages referenced by packages you are referencing on your projects. Kind of second or third level references.

What's the problem with those? Counter question: Who would you solve vulnerabilities in those kind of packages? That's the problem!

Vulnerabilities in direct referenced packages can be updated really simple. Just update to a package version where the problem is solved, right?

> In most cases CVE databases list vulnerabilities that have a patch already. Otherwise you would make a vulnerability publicly known that can't be patched. Which is dangerous for the user of a vulnerable package. The tool we use, as well as the NuGet audit feature are checking against CVE databases. Vulnerabilities that are not listed in those databases, can't be found using those tools. Those vulnerabilities are called Zero-Day-Vulnerability

Since transitive packages are not directly reverenced you can't easily increase the version number to a patched version. We need a solution for it.

Central Package Management

A quick research (=googling) points me to Central Package Management for NuGet. This is a little bit hidden feature in the .NET ecosystem. It is supported by SDK style project. Imagine you can manage your package and package versions on a cetral place for all the projects in your solution. This solves several problems:

* All projects use the same package version
* You can manage the package version on one place

Project than reference the packages without a version number. VS support it, NuGet supports it, and the dotnet CLI is supporting it. On the other hand, when I wrote it is a kind of hidden feature, I mean it like this. You can't change to CPM in VS. You can't create a file in VS to manage your packages centrally. 

CPM is basically yet another XML file called Directoy.Packages.props that need to be located in the same folder as your solution file. 

What you can do to create such a file is to create a new XML file and rename it to Directoy.Packages.props, google for the docs and add the base structure of the XML into the file.

An easier solution is to use the dotnet CLI to create such a file:

```shell
dotnet new packagesprops
```

Type dotnet new list to find the file in the list of templates:

![image-20250509191258128](C:\Users\JürgenGutsch\AppData\Roaming\Typora\typora-user-images\image-20250509191258128.png)

This will create a file like this:

```xml
<Project>
  <PropertyGroup>
    <!-- Enable central package management, https://learn.microsoft.com/en-us/nuget/consume-packages/Central-Package-Management -->
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
  </ItemGroup>
</Project>
```

Even if the NuGet explorer is supporting this file, you will need to put your hands on it from time to time. Therefor, I'd propose to add it to a solution folder in VS to have quick access to it while developing. 

No the work starts and you should add all the packages referenced in your projects within this file without having duplications. Duplications leads to NuGet errors.

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Aspire.Hosting.AppHost" Version="9.1.0" />
    <PackageVersion Include="CodeHollow.FeedReader" Version="1.2.6" />
    <PackageVersion Include="coverlet.collector" Version="6.0.4" />
    <PackageVersion Include="Duende.IdentityServer" Version="7.2.0" />
    <PackageVersion Include="EfCore.SchemaCompare" Version="9.0.0" />
    <PackageVersion Include="FluentAssertions" Version="8.2.0" />
    <PackageVersion Include="Microsoft.AspNetCore.Authentication.Google" Version="9.0.3" />
    <PackageVersion Include="Microsoft.AspNetCore.Authentication.OpenIdConnect" Version="9.0.3" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.3" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.3" />
     
```

You can do this manually on small projects. Unfortunately there is no support in the dotnet CLS or in VS. 

As an alternative I propose to use a dotnet tool called `centralisedpackageconverter` ([more information](https://github.com/Webreaper/CentralisedPackageConverter)) that you can install with a single command:

```shel
dotnet tool install CentralisedPackageConverter --global
```

After its installed run it with the following command and it does it's job:

```
central-pkg-converter .
```

Transitive Pinning

If you run it with the option `-t` or `--transitive-pinning` it adds a XML tag to the ProperyGroup that I was looking for:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Aspire.Hosting.AppHost" Version="9.1.0" />

```

This allows you to add entries for transitive packages to pin them to a specific version. 

For example: You are using the latest version of a package.4.0.0 that references to a transitive vulnerable package.3.3.13 that is already patched in version 3.3.14 . You cannot update the direct reference because you are already using the latest version. What you can do is to add an entry for the transitive package and set it yo version 3.3.14. This way you are pinning that transitive package to a later version.

This will solve the problem with vulnerable transitive packages.

Again: VS and NuGet is supporting this feature. You can still use VS and the NuGet package explorer to manage and update your packages. If you reference a new package to a project, it will add the reference without a version number to the project file and it will add entry to the Directoy.Packages.props including a version number.

Conclusion



Lastly

One thing to mention: If you want to use different version of packages within projects of your solution you cannot use CPM. CPM and regular NuGet references can't be mixed now. Managing the package versions centrally really means for all projects in your solution. 



