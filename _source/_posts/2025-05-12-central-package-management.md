---
layout: post
title: "NuGet, Security, and Central Package Management"
teaser: "For a while, I took over two more roles in the company. Besides being a software engineer, I'm also responsible for sharing knowledge, keeping awareness, and supporting our projects regarding quality assurance and application security. The latter is the topic why I'm writing these lines about how central package management solves a security problem with transitive NuGet packages."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Visual Studio
- NuGet
---

I took over two more roles in the company for a while. Besides being a software engineer, I'm also responsible for sharing knowledge, raising awareness, and supporting projects regarding quality assurance and application security. The latter is the topic for which I'm writing these lines. 

## Package Vulnerabilities

In Visual Studio, NuGet is checking the referenced packages for vulnerabilities, which is a great new feature. We now see a notification in VS if a referenced or transitive package has vulnerabilities. We can also run a dotnet CLI command to see if a package has vulnerabilities, which is also great. The CLI command can be used in build pipelines to check for vulnerabilities during build time automatically. Awesome.

For a while now, our company has been using a separate tool that scans our repositories during code changes on the main branches and PRs. This tool does not use NuGet but reads all files that have NuGet references, such as project files and other package reference files. It checks the package names and version against CVE Databases for published vulnerabilities and reports them directly to us via Slack notifications. We can also create Jira tickets directly within the tool and assign them to the right project and the right person to solve the problem. Also, this tool is scanning transitive packages, which is cool in general.

## Vulnerable Transitive Packages

In the previous section, I mentioned transitive packages two times. These are packages referenced by the packages you are referencing on your projects directly—they are kind of second—or third-level references.

What's the problem with those? 
Counter question: 
Who would you solve vulnerabilities in that kind of package? 
Exactly! That's the problem!

Vulnerabilities in directly referenced packages can be updated really simply. Just update to a patched package version, and the problem is solved, right?

> In most cases, CVE databases list vulnerabilities that have already been patched. Otherwise, you would make a vulnerability publicly known that can't be patched, which is dangerous for the user of a vulnerable package. The tool we use and the NuGet audit feature check against CVE databases. 
>
> Vulnerabilities that are not listed in those databases cannot be found using those tools. These are called Zero-Day vulnerabilities.

Since transitive packages are not directly referenced, you can't easily increase the version number to a patched version. 

We need a solution for it.

## Central Package Management (CPM)

A quick research (=googling) points me to Central Package Management for NuGet. This is a little bit hidden feature in the .NET ecosystem. It is supported by SDK-style projects in VS, VS Code, and the dotnet CLI. 

Imagine you can manage your packages and package versions in a central place for all the projects in your solution. This solves several problems:

* All projects use the same package version.
* You can manage the package version in one place.

Projects than reference the packages without a version number. VS supports it, NuGet supports it, and the dotnet CLI supports it. On the other hand, when I wrote it is a kind of hidden feature, I mean it like this. You can't change to CPM in VS. You can't create a file in VS to manage your packages centrally. 

CPM is basically yet another XML file called `Directoy.Packages.props` that need to be located in the same folder as your solution file. 

What you can do to create such a file is to create a new XML file and rename it to `Directoy.Packages.props`, google for the docs, and add the base structure of the XML into the file. Or you can use an easier way using the dotnet CLI to create such a file:

```shell
dotnet new packagesprops
```

Type dotnet new list to find the file in the list of templates:

![image-20250509191258128]({{site.baseurl}}/img/cpm/cpm.png)

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

Even if the NuGet explorer supports this file, you will probably need to touch it from time to time. Therefore, I'd propose to add it to a solution folder in VS to have quick access to it while developing. Usually I create a solution folder called `_default` or `_shared` to every solution that contains files like this, or the `.gitignore` or whatever file that is not part of any projects and needs to be edited from time to time.

Now the work starts, and you should add all the packages referenced in your projects within this file without having duplications. Duplications lead to NuGet errors when adding references or at build time.

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

You can do this manually on small projects. Unfortunately, the dotnet CLI or VS does not support automatically converting all NuGet references in a solution to CPM. 

As an alternative, I propose to use a dotnet tool called `centralisedpackageconverter` ([more information](https://github.com/Webreaper/CentralisedPackageConverter)) that you can install with a single command:

```shell
dotnet tool install CentralisedPackageConverter --global
```

After it's installed, run it with the following command and it does its job:

```shell
central-pkg-converter .
```

What about vulnerabilities in transitive packages? 

## Transitive Pinning

If you run the previous command with the option `-t` or `--transitive-pinning`, it adds an XML tag to the `PropertyGroup` that I was looking for:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Aspire.Hosting.AppHost" Version="9.1.0" />

```

This setting enables transitive pinning with CPM. This allows you to add entries for transitive packages to pin them to a specific version. 

For example: You are using the latest version of a package 4.0.0 that references a transitive vulnerable package 3.3.13 that is already patched in version 3.3.14 . You cannot update the direct reference to solve the problem because you have already used the latest version of the directly referenced package. You can add an entry for the transitive package and set it to version 3.3.14. This way, you are pinning that transitive package to a patched later version. This package doesn't need to be referenced in any project. When NuGet tries to solve the transitive references, it finds the entry and loads the patched package.

This will solve the problem with vulnerable transitive packages.

Again, VS and NuGet are supporting this feature in general. You can still use VS and the NuGet package explorer to manage and update your packages. If you reference a new package to a project, it will add the reference without a version number to the project file when it comes to migrating to CPM.

## Conclusion

CPM is great for managing your package versions in a central place. Sure, you can do it with the NuGet package explorer on the solution level as well, but it actually sets the package versions on each project file, which will work until a team member updates a package on the project level instead of the solution level. CPM is always on the solution level.

CPM also solves the security problem of vulnerable transitive package references by using transitive pinning, which allows you to update a transitive package reference to a patched version.

## Lastly

One last thing to mention: If you really want to use different versions of packages within your solution's projects, you cannot use CPM. CPM and regular NuGet references can't be mixed yet. Managing the package versions centrally really means for all projects in your solution. 



