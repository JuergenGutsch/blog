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

I recently wrote about that it is pretty hard to move an existing more complex .NET Framework library to .NET Core or .NET Standard 1.x. More complex in this case just means, e. g. that this library uses reflection a little more than maybe some others. I'm talking about the LightCore DI container. 

I started to move it to .NET Core in November 2015, but gave up halve a year later. Because it needs much more time to port it to NET Core and because it makes much more sense to port it to the .NET Standard than to .NET Core. And the announcement of .NET Standard 2.0 makes me much more optimistic to get it done with pretty less effort. So I stopped moving it to .NET Core and .NET Standard 1.x and was waiting for .NET standard 2.0.

Some weeks ago the preview version of .NET Standard 2.0 was announced and I tried it again. It works as expected. The API of .NET Standard 2.0 is big enough to get the old source of LightCore running. Also Rick Strahl did some pretty cool and detailed post about it:

* post 1
* post 2
* post 3

## The current status

* I created .NET Standard 2.0 libraries for the most of the projects. I didn't change most parts of the code. The XAML configuration stuff was an exception. 
* I also ported the existing unit tests to .NET Core Xunit test libraries and the tests are running. That means LightCore is definitely running with .NET Core 2.0.
* We'll get one breaking change: I moved the file based configuration from XAML to JSON, because the XAML serializer is not (maybe not yet) supported in .NET Standard.
* We'll get another breaking change: I don't want to support Silverlight anymore.
  - Silverlight users should use the old packages instead. Sorry about that.

## Roadmap

I don't really want to release the new version until the .NET Standard 2.0 is released. I don't want to have the preview versions of the .NET Standard libraries referenced in a final version of LightCore. That means, there is still some time to get the open issues done.

| Version        | Date               | Comment                                  |
| -------------- | ------------------ | ---------------------------------------- |
| 2.0.0-preview1 | End of July 2017   | uses the preview versions of .NET Standard 2.0 and .NET Core 2.0 |
| 2.0.0-preview2 | End of August 2017 | uses the preview versions of .NET Standard 2.0 and .NET Core 2.0. Maybe the finals, if they are released until that. |
| 2.0.0          | September 2017     | depends on the release of .NET Standard 2.0 and .NET Core 2.0 |

## Open issues

The progress is good, but there is still something to do, before the release of the first preview:

* We need to have the same tests in .NET Framework based unit tests to be sure
  * [https://github.com/JuergenGutsch/LightCore/issues/4](https://github.com/JuergenGutsch/LightCore/issues/4)
* We need to finalize the ASP.NET integration package to get it running again in ASP.NET 4.x
  * Should be small step, because it is almost done: The WebAPI integration and the sample application is needed.
  * [https://github.com/JuergenGutsch/LightCore/issues/4](https://github.com/JuergenGutsch/LightCore/issues/4)
* We need to complete the ASP.NET Core integration package to get it running in ASP.NET Core
  * Needs some more time, because it needs a generic and proper way to move the existing service registrations from the ASP.NET Core's IServiceCollection to LightCore
  * [https://github.com/JuergenGutsch/LightCore/issues/6](https://github.com/JuergenGutsch/LightCore/issues/6)
* Continuous integration and deployment using AppVeyor should be set-up
  * [https://github.com/JuergenGutsch/LightCore/issues/2](https://github.com/JuergenGutsch/LightCore/issues/2)
  * [https://github.com/JuergenGutsch/LightCore/issues/3](https://github.com/JuergenGutsch/LightCore/issues/3)
* Documentation should be set-up on GitHub
  * [https://github.com/JuergenGutsch/LightCore/issues/3](https://github.com/JuergenGutsch/LightCore/issues/3)

## Contributions

I'd like to call for contributions. Try LightCore, raise Issues, create PRs and whatever is needed to get LightCore back to life and whatever is needed to make it a lightweight, fast and easy to use DI container.