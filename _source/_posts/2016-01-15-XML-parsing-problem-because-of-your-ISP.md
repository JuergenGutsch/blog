--- 
layout: post
title: "XML parsing problem while trying to query SharePoint Online"
teaser: "A few days ago I lost more than 5 hours, because I couldn't query user information from SharePoint Online. The reason was a pretty awful problem caused by my ISP."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- Office 365
- SharePoint Online
- XML Exception
---

Yesterday it works and today it don't work. You possibly know that. Usually there are some code changes, if something like this happens. Some days ago there are now code changes, now new libraries referenced. I just didn't work. And I didn't know what happened here. 

I got a XmlException which tells me that my application can't parse a XML result because of the DTD:

> System.Xml.XmlException: For security reasons DTD is prohibited in this XML document. To enable DTD processing set the DtdProcessing property on XmlReaderSettings to Parse and pass the settings into XmlReader.Create method.

This Exception was thrown deep inside a SharePoint Client library, when my Application wants to query some user information from SharePoint. The SharePoint Context was fine. I lost 3 hours to find out what changed until the last day. I had a second look to some pull request, I checked the Git history and I checked my .NET environment. I also asked the available team members. But it happened only to me. This was pretty confusing and annoying.

There were no relevant code changes from the last day, to the day when this problem happens. But there was another huge difference: At this day I worked at home. The day before I was in the Office in Basel. A team member asked me about that and sends me a Link to a StackOverflow thread, where some other developers almost had the same problems: [DTD is prohibited” error when accessing sharepoint 2013/office365 list](http://stackoverflow.com/questions/23443316/dtd-is-prohibited-error-when-accessing-sharepoint-2013-office365-list-but-not). All of them wanted to query information from Office 365 and SharePoint Online. One of them got this exception using a WiFi extender. Another one got that because, his ISP (Internet Service Provider) provides a custom error page, if he can't resolve a specific domain.

I started Fiddler, to see what happened here. I don't use a WiFi extender, but at home I use a different ISP than in the Office in Basel.

Sniffing the HTTP Traffic shows me what happened. **I had exactly the same problem as the second person on StackOverflow**. In my case it was the `msoid.[companyname].emea.microsoftonline.com` which couldn't be resolved by my ISP:

My ISP provides a feature called "navigation help", which is a custom error page, which includes a web search for the not resolved host header. That means if the ISP can't resolve a domain name (host header) he provides a page which some help to solve the problem. Which is a good feature in general. But the real issue is they send the page with HTTP Status 200 which means it is all fine with the result and the SharePoint client library tries to parse the returning HTML result, but expects a XML result. **Exactly this throws that XML Parsing Exception**.

![]({{ site.baseurl }}/img/isp-o365/navigation-help.PNG)

The guy on stack overflow could solve this, by switching this feature off. I fortunately found a hint on that page of my ISP too, to switch this "navigation help" feature off. And this solves the problem. 

**Switching it off and restarting the router solves an issue that costs me more than 5 hours.**

My application is a web application based on ASP.NET, it will be hosted on Azure. This issue will not happen in production. But if you are developing a client application, which needs to connect to SharePoint Online this could definitely happen, if your users changing the work space (working at home, at a restaurant or somewhere else) to a space with a different ISP, which also provides something like this.

Getting exactly this exception while querying information from SharePoint Online means the returning result is not the expected XML what can only be happen if XML Parser don't get valid XML. The not resolvable host header is not the real problem, because the client library seems to use a fallback in this case. The problem is, that the ISP possibly returns a wrong HTTP Status, if the host header can't be resolved. 