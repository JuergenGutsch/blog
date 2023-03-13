---
layout: post
title: "Problems using a custom Authentication Cookie in classic ASP.​NET"
teaser: "While working an a new application for a customer, which needs to work in their on premise environment and uses their custom authentication service, I stuck in a problem with the custom authentication cookie in combination with ASP.NET forms authentication. In this post I descript that problem and a solution I finally found."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET
- Authentication
- Cookie
---

A customer of mine created an own authentication service that combines various login mechanisms in their on-premise application environment. On this central service combines authentication via Active Directory, classic ASP.NET Forms Authentication and a custom login via a number of an access card.

* Active Directory 
  (For employees only) 
* Forms Authentication 
  (Against a user store in the database for extranet users and and against the AD for employees via extranet)
* Access badges 
  (for employees, this authentication results in lower access rights)

This worked pretty nice in their environment until I created a new application which needs to authenticate against this service and was build using ASP.NET 4.7.2

> **BTW:** Unfortunately I couldn't use ASP.NET Core here because I needed to reuse specific MVC components that are shared between all the applications.
>
> I also wrote "classic ASP.NET" which feels a bit wired. I worked with ASP.NET for a long time (.NET 1.0) and still work with ASP.NET for specific customers. But it really is kinda classic since ASP.NET Core is out and since I worked with ASP.NET Core a lot as well.

## How the customer solution works

I cannot go into the deep details, because this is the customers code, you only need to get the idea.

The problem because it didn't work with the new ASP.NET Framework is, that they use a custom authentication cookie that was based on [ASP.NET forms authentication](https://referencesource.microsoft.com/#System.Web/Security/FormsAuthentication.cs,a820aab5aa1ac27c). I'm pretty sure, when the authentication service was created they didn't know about ASP.NET Identity or it didn't exist. They created a custom Identity, that stores all the user information as properties. They build an authentication ticket out of it and use forms authentication to encrypt and store that cookie. The cookie name is customized in the web.config which is not an issue. All the apps share the same encryption information.

The client applications that uses the central authentication service, read that cookie, decrypt the information using forms authentication, de-serialize the data into that custom authentication ticket that contains the user information. The user than gets created and stored into the `User` property of the current `HttpContext` and is authenticated in the application.

This sounds pretty straight foreword is working well, except in newer ASP.NET versions.

## How it should work

The best way to use the authentication cookie would be to use the ASP.NET Identity mechanisms to create that cookie. After the authentication happened on the central service, the needed user information should have been stored as claims inside the identity object, instead of properties in a custom `Identity` object. The authentication cookie should have been stored using the forms authentication mechanism only, without an custom authentication ticket. The forms authentication is able to create that ticket including all the claims.

On the client applications forms authentication would have been red the cookie and would have been created a new `Identity` including all the claims that are defined in the central authentication service. The forms authentication module would have stored the user in the current `HttpContext` as well. 

Less code, more easy. IMHO.

## What is the actual problem?

The actual problem is, that the client applications reads the authentication cookie from the `CookieCollection` on `Application_PostAuthenticateRequest`:

~~~ csharp
// removed logging and other overhead

protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
{
    var serMod = default(CustomUserSerializeModel);

	var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
	if (authCookie != null || Request.IsLocal)
	{
		var ticket = FormsAuthentication.Decrypt(authCookie.Value); 
		var serializer = new JavaScriptSerializer();
		serMod = serializer.Deserialize<CustomUserSerializeModel>(ticket.UserData);
    }
    
    // some fallback code ...
    
    if (serMod != null)
	{
		var user = new CustomUser(serMod);
		var cultureInfo = CultureInfo.GetCultureInfo(user.Language);

		HttpContext.Current.User = user;
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
	}
    
    // some more code ...
}
~~~

**In newer ASP.NET Frameworks the authentication cookie gets removed from the cookie collection after the user was authenticated.**

Actually I have no idea since what version the cookie will be removed, but this is anyway a good thing because of security reasons, but there are no information in the release notes since ASP.NET 4.0.

Anyway the cookie collection doesn't contain the authentication cookie anymore and the cookie variable is null if I try to read it out of the collection.

> **BTW:** The cookie is still in the request headers and could be read manually. But including the encryption it could be difficult to read it.

I tried to solve this problem by reading the cookie on `Application_AuthenticateRequest`. This is also not working, because the `FormsAuthenticationModule` reads the cookie previously. 

The next try was on to read it on `Application_BeginRequest`. This in generally woks, I get the cookie and I can read it. But, because the cookie is configured as authentication cookie, the `FormsAuthModule` tries to read it and fails. It'll set the `User` to null because there is an authentication cookie available which doesn't contain valid information. Which also makes kinda sense.

So this is not the right solution as well. 

I worked on that problem almost four months. (Not completely four months, but for many hours within this four months.) I compared applications and other solutions. Because there was no hint about the removal of the authentication cookie and because it was working on the old applications I was pretty confused about the behavior. 

I studied the [source code of ASP.NET](https://referencesource.microsoft.com/#System.Web/Security/FormsAuthenticationModule.cs,114) to get the solution. And there is one.

## And finally the solution

The solution is to read the cookie on `FormsAuthentication_OnAuthenticate` in the `global.asax` and not to store the user in the current context, but in the event arguments `User` property. The user than gets stored in the context by the `FormsAutheticationModule`, that also executes this event handler.

~~~ csharp
// removed logging and other overhead

protected void FormsAuthentication_OnAuthenticate(Object sender, FormsAuthenticationEventArgs args)
{
	AuthenticateUser(args);
}

public void AuthenticateUser(FormsAuthenticationEventArgs args)
{    
	var serMod = default(CustomUserSerializeModel);

	var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
	if (authCookie != null || Request.IsLocal)
	{
		var ticket = FormsAuthentication.Decrypt(authCookie.Value); 
		var serializer = new JavaScriptSerializer();
		serMod = serializer.Deserialize<CustomUserSerializeModel>(ticket.UserData);
    }
    
    // some fallback code ...
    
    if (serMod != null)
	{
		var user = new CustomUser(serMod);
		var cultureInfo = CultureInfo.GetCultureInfo(user.Language);

		args.User = user; // <<== this does the thing!
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
	}
    
    // some more code ...
}
~~~

That's it.

## Conclusion

Pleas don't create custom authentication cookies, try the `FormsAuthentication` and ASP.NET Identity mechanisms first. This is much simpler and won't break that way because of future changes.

Also please don't write a custom authentication service, because there is already a good one out there that is the almost the standard. Have a look into the [IdentityServer](https://identityserver.io/), that also provides the option to handle different authentications mechanisms using common standards and technologies. 

If you really need to create a custom solution, be carefully and know what you are doing. 
