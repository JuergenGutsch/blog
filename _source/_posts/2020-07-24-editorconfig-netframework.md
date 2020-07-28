---
layout: post
title: "Getting the .editorconfig working with the .NET Framework and MSBuild"
teaser: "The team was quite happy about the fact that it is possible to fails the build on a code style error via the .editorconfig. But there was one question I couldn't really answer: Does this also work for .NET Framework? It should, but does it really work? Let's see. And also see me stumbling upon a mistake inside the last post."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Tests
- Tag
---

I demonstrated my [results of the last post]({% post_url editorconfig-msbuild.md %}) about the `.editorconfig` to the team last week. They were quite happy about the fact that the build fails on a code style error but there was one question I couldn't really answer. The Question was:

> Does this also work for .NET Framework?

It should, because it is Roslyn who analyses the code. It is not any framework who does it.

To try it out I created three different class libraries that have the same class file linked into it, with the same code style errors:

~~~csharp
    using System;

namespace ClassLibraryNetFramework
{
    public class EditorConfigTests
    {
    public int MyProperty { get; } = 1;
    public EditorConfigTests() { 
    if(this.MyProperty == 2){
        Console.WriteLine("Hallo Welt");
        }        }
    }
}
~~~

This code file has at least eleven code style errors in it:

![]({{site.baseurl}}/img/editorconfig/codestyleerrors.png)

I created a .NET Standard library, a .NET Core library, and a .NET Framework library in VS2019 this time. The solution in VS2019 now looks like this:

![]({{site.baseurl}}/img/editorconfig/solution.png) 

I also added the MyGet Roslyn NuGet Feed to the NuGet sources and referenced the code style analyzers:

![]({{site.baseurl}}/img/editorconfig/nugetsettings.png)

This is the URL and the package name for you to copy:

* https://dotnet.myget.org/F/roslyn/api/v3/index.json
* Microsoft.CodeAnalysis.CSharp.CodeStyle Version: 3.8.0-1.20330.5

I also set the global.json to the latest preview of the .NET 5 SDK to be sure to use the latest tools:

~~~json
{
  "sdk": {
    "version": "5.0.100-preview.6.20318.15"
  }
}

~~~

## It didn't really work - My fault in the last blog post!

I saw some code style errors in VS2019 but not all the eleven errors I expected. I tried a build and the build didn't fail. Because I knew it worked the last time I tried it using the the dotnet CLI. I did the same here. I ran `dotnet build` and `dotnet msbuild` but the build didn't fail. 

> This is exactly what you don't need as a software developer: Doing things exactly the same twice and one time it works and on the other time it fails and you have no idea why. 

I tried a lot of things and compared project files, solution files, and `.editorconfig` files. Actually I compared it with the Weather Stats application I used in the last post. At the end I found one line in the the `PropertyGroup` of the project files of the weather application that shouldn't be there but actually was the reason why it worked. 

~~~xml
<CodeAnalysisRuleSet>..\editorconfig.ruleset</CodeAnalysisRuleSet>
~~~

While trying to get it running for the [last post]({% post_url editorconfig-msbuild.md %}), I also experimented with a ruleset file. The ruleset file is a XML file that can be used to enable or disable analysis rules in VS2019. I added a ruleset file to the solution and linked it into the projects, but forgot about that.

So it seemed the failing builds of the last post wasn't because of the `.editorconfig` but because of this ruleset file.

It also seemed the ruleset file is needed to get it working. That shouldn't be the case and I asked the folks via the GitHub Issue about that. The answer was fast:

* Fakt #1: The ruleset file isn't needed

* Fakt #2: The regular `.editorconfig` entries don't work yet

# The solution

Currently the ruleset entries where moved to the `.editorconfig` this means you need to add IDE specific entries to the `.editorconfig` to get it running, which also means you will have redundant entries until all the code style analyzers are moved to Roslyn and are mapped to the `.editorconfig`:

~~~ini
# IDE0007: Use 'var' instead of explicit type
dotnet_diagnostic.IDE0007.severity = error

# IDE0055 Fix formatting
dotnet_diagnostic.IDE0055.severity = error

# IDE005_gen: Remove unnecessary usings in generated code
dotnet_diagnostic.IDE0005_gen.severity = error

# IDE0065: Using directives must be placed outside of a namespace declaration
dotnet_diagnostic.IDE0065.severity = error

# IDE0059: Unnecessary assignment
dotnet_diagnostic.IDE0059.severity = error

# IDE0003: Name can be simplified
dotnet_diagnostic.IDE0003.severity = error  
~~~

As mentioned, these entries are already in the `.editorconfig` but written differently. 

In the GitHub Issue they also wrote to add a specific line, in case you don't know all the IDE numbers. This line writes out warnings for all the possible code style failures. You'll see the numbers in the warning output and you can now configure how the code style failure should be handled:

~~~ini
# C# files
[*.cs]
dotnet_analyzer_diagnostic.category-Style.severity = warning
~~~

This solves the problem and it actually works really good.

![]({{site.baseurl}}/img/editorconfig/allerrors.png)

## Conclusion

Even if it solves the problem, I really hope this is a intermediate solution only, because of the redundant entries in the `.editorconfig`. I would prefer to not have the IDE specific entries, but I guess this needs some more time and a lot work done by Microsoft.





