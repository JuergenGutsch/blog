---
layout: post
title: "News about LightCore 2.0"
teaser: "I recently wrote about that it is pretty hard to move an existing more complex .NET Framework library to .NET Core or .NET Standard 1.x. More complex in this case just means, that this library uses reflection a little more than maybe some others. I'm talking about the LightCore IoC container. Some weeks ago the preview version of .NET Standard 2.0 was announced and I tried it again."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- Unit Test
- XUnit
- MSTest
---

I recently wrote about that it is pretty hard to move an existing more complex .NET Framework library to .NET Core or .NET Standard 1.x. More complex in this case just means, that this library uses reflection a little more than maybe some others. I'm talking about the LightCore IoC container. 

I started to move it to .NET Core in November 2015, but gave up halve a year later. Because it needs much more time to port it to NET Core and because it makes much more sense to port it to the .NET Standard than to .NET Core. And because of the announcement of .NET Standard 2.0 makes me much more optimistic to get it done with pretty less effort. So I stopped moving it to .NET Standard 1.x and was waiting for .NET standard 2.0.

Some weeks ago the preview version of .NET Standard 2.0 was announced and I tried it again. It works as expected. The API of .NET Standard 2.0 is big enough to get the old source of LightCore running. Also Rick Strahl did some pretty cool and detailed post about it:

* post 1
* post 2
* post 3

## The current status

The progress is good, but there is still something to do:

* I also ported the existing unit tests to .NET Core Xunit test libraries and the tests are running. That means LightCore is definitely running with .NET Core.
* We need to have the same tests in .NET Framework based unit tests to be sure
* We'll get one breaking change: I moved the file based configuration from XAML to JSON, because the XAML serializer is not yet supported in .NET Standard.
* We'll get another breaking change, because I don't want to support SilverLight anymore
  * Silverlight users should use the old packages instead. Sorry about that.
* We need to rebuild the ASP.NET integration package to get it running again in ASP.NET 4.x
  * Should be small step
* We need to complete the ASP.NET Core integration package to get it running in ASP.NET Core
  * Needs some more time, because it needs a generic and proper way to move the existing service registrations from the ASP.NET Core's IServiceCollection to LightCore
* Continues integration and deployment using AppVeyor should be set-up
* Documentation should be set-up on GitHub

But this is a good status and I don't really want to release the new version until the .NET Standard 2.0 is released. I don't want to have the preview versions of the .NET Standard libraries reverenced in a release version of LightCore. That means, there is still some time to get the open issues done.

