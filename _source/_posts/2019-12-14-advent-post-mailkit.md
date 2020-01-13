---
layout: post
title: "ASP.NET Hack Advent Post 14: MailKit"
teaser: "This is the fourteenth post of the ASP.NET Hack Advent. Until December 24th I'm going to post a link to a good community resource per day and a few lines about it."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- ASP.NET Hack Advent
- OpenSource
---

![]({{site.baseurl}}/img/advent/advent.jpg)

This fourteenth post is about a cross-platform .NET library for IMAP, POP3, and SMTP.

On Twitter I got asked about [sending emails out from a worker service](https://twitter.com/johnObasi7/status/1205155810860523520). So I searched for the documentation about [System.Net.Mail](https://docs.microsoft.com/en-us/dotnet/api/system.net.mail?view=netcore-3.1) and the [SmtpClient Class](https://docs.microsoft.com/en-us/dotnet/api/system.net.mail.smtpclient?view=netcore-3.1) and was really surprised that this class was marked as obsolete. It seems I missed the announcement about this.

![]({{site.baseurl}}/img/advent/smtpclient.png)

The .NET team recommend to use the MailKit and MimeKit to send emails out. 

Both libraries are open sourced under the MIT license and free to use in commercial projects. It seems that this libraries are really complete and provide a lot of useful features.

Website: [http://www.mimekit.net/](http://www.mimekit.net/)

**MailKit:**

GitHub: [https://github.com/jstedfast/MailKit](https://github.com/jstedfast/MailKit)

NuGet: [https://www.nuget.org/packages/MailKit/](https://www.nuget.org/packages/MailKit/)

**MimeKit:**

GitHub: [https://github.com/jstedfast/MimeKit](https://github.com/jstedfast/MimeKit)

NuGet: [https://www.nuget.org/packages/MimeKit/](https://www.nuget.org/packages/MimeKit/)