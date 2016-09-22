--- 
layout: post
title: "Authentication in ASP.​NET Core for your Web API and Angular2"
teaser: "
Authentication in a single page application is a bit special, if you just know the traditional ASP.NET way. To imagine that the app is a completely independent app like a mobile app helps. Token based authentication is the best solution for this kind of apps. In this post I'm going to try to describe a high level overview and to show a simple solution."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- Angular2
- Web API
- Authentication
---

Authentication in a single page application is a bit more special, if you just know the traditional ASP.NET way. To imagine that the app is a completely independent app like a mobile app helps. Token based authentication is the best solution for this kind of apps. In this post I'm going to try to describe a high level overview and to show a simple solution.

## Intro

As written in my last posts about Angular2 and ASP.NET Core, I reduced ASP.NET Core to just a HTTP Service, to provide JSON based data to an Angular2 client. Some of my readers, asked me about how the Authentication is done in that case. I don't use any server generated log-in page, registration page or something like this. So the ASP.NET Core part only provides the web API and the static files for the client application.

There are many ways to protect your application out there. The simplest one is using an [Azure Active Directory](https://azure.microsoft.com/en-us/documentation/services/active-directory/). You could also setup a separate authentication server, using [IdentityServer2](https://identityserver4.readthedocs.io/en/dev/), to manage the users, roles and to provide a token based authentication.

And that's the key word: A Token Based Authentication is the solution for that case.

With the token bases authentication, the client (the web client, the mobile app, and so on) gets a string based encrypted token after a successful log-in. The token also contains some user info and an info about how long the token will be valid. This token needs to be stored on the client side and needs to be submitted to the server every time you request a resource. Usually you use a HTTP header to submit that token. If the token is not longer valid you need to perform a new log-in.

In one of our smaller projects, didn't set-up a different authentication server and we didn't use Azure AD, because we needed a fast and cheap solution. Cheap from the customers perspective.

## The Angular2 part

On the client side we used [angular2-jwt](https://github.com/auth0/angular2-jwt/), which is a Angular2 module that handles authentication tokens. It checks the validity, reads meta information out of it and so on. It also provides a wrapper around the Angular2 HTTP service. With this wrapper you are able to automatically pass that token via a HTTP header back to the server on every single request.

The work flow is like this. 

1. If the token is not valid or doesn't exist on the client, the user gets redirected to the log-in route
2. The user enters his credentials and presses the log-in button
3. The date gets posted to the server where a special middle-ware handles that request
    1. The user gets authenticated on the server side
    2. The token, including a validation date and some meta date, gets created 
    3. The token gets returned back to the client
4. the client stores the token in the local storage, cookie or whatever, to use it on every new request.

The angular2-jwt does the most magic on the client for us. We just need to use it, to check the availability and the validity, every time we want to do a request to the server or every time we change the view.

This is a small example (copied from the Github readme) about how the HTTP wrapper is used in Angular2:
~~~ typescript
import { AuthHttp, AuthConfig, AUTH_PROVIDERS } from 'angular2-jwt';

...

class App {

  thing: string;

  constructor(public authHttp: AuthHttp) {}

  getThing() {
    // this uses authHttp, instead of http
    this.authHttp.get('http://example.com/api/thing')
      .subscribe(
        data => this.thing = data,
        err => console.log(err),
        () => console.log('Request Complete')
      );
  }
}
~~~

More samples and details can be found directly on github [https://github.com/auth0/angular2-jwt/](https://github.com/auth0/angular2-jwt/) and there is also a detailed blog post about using angular2-jwt: [https://auth0.com/blog/introducing-angular2-jwt-a-library-for-angular2-authentication/](https://auth0.com/blog/introducing-angular2-jwt-a-library-for-angular2-authentication/)

## The ASP.NET part

On the server side we also use a, separate open source project, called [SimpleTokenProvider](https://github.com/nbarbettini/SimpleTokenProvider). This is really a pretty simple solution to authenticate the users, using his credentials and to create and provide the token. I would not recommend to use this way in a huge and critical solution, in that case you should choose the [IdentiyServer](https://leastprivilege.com/2016/05/20/identityserver4-on-asp-net-core-rc2/) or any other authentication like [Azure AD](https://azure.microsoft.com/en-us/documentation/articles/active-directory-whatis/) to be more secure. The sources of that project need to be copied into your project and you possibly need to change some lines e. g. to authenticate the users against your database, or whatever you use to store the user data. This project provides a middle-ware, which is listening on a defined path, like /api/tokenauth/. This URL is called with a POST request by the log-in view of the client application.

The authentication for the web API, is just using the token, sent with the current request. This is simply done with the built-in IdentiyMiddleware. That means, if ASP.NET MVC gets a request to a Controller or an Action with an AuthorizeAttribute, it checks the request for incoming Tokens. If the Token is valid, the user is authenticated. If the user is also in the right role, he gets authorized.

We put the users role information as additional claims into the Token, so this information can be extracted out of that token and can be used in the application.

To find the users and to identify the user, we use the given UserManager and SignInManager. These managers are bound to the IdentityDataContext. This classes are already available, when you create a new project with Identiy in Visual Studio.

This way we can authenticate a user on the server side:
~~~ csharp
public async Task<ClaimsIdentity> GetIdentity(string email, string password)
{
    var result = await _signInManager.PasswordSignInAsync(email, password, false, lockoutOnFailure: false);
    if (result.Succeeded)
    {
        var user = await _userManager.FindByEmailAsync(email);
        var claims = await _userManager.GetClaimsAsync(user);

        return new ClaimsIdentity(new GenericIdentity(email, "Token"), claims);
    }

    // Credentials are invalid, or account doesn't exist
    return null;
}
~~~

And this claims will be used to create the Jwt-Token in the [TokenAuthentication middle-ware](https://github.com/nbarbettini/SimpleTokenProvider/blob/master/src/SimpleTokenProvider/TokenProviderMiddleware.cs):

~~~ csharp
var username = context.Request.Form["username"];
var password = context.Request.Form["password"];

var identity = await identityResolver.GetIdentity(username, password);
if (identity == null)
{
    context.Response.StatusCode = 400;
    await context.Response.WriteAsync("Unknown username or password.");
    return;
}

var now = DateTime.UtcNow;

// Specifically add the jti (nonce), iat (issued timestamp), and sub (subject/user) claims.
// You can add other claims here, if you want:
var claims = new[]
{
    new Claim(JwtRegisteredClaimNames.Sub, username),
    new Claim(JwtRegisteredClaimNames.Jti, await _options.NonceGenerator()),
    new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(now).ToString(), ClaimValueTypes.Integer64)
};

// Create the JWT and write it to a string
var jwt = new JwtSecurityToken(
    issuer: _options.Issuer,
    audience: _options.Audience,
    claims: claims,
    notBefore: now,
    expires: now.Add(_options.Expiration),
    signingCredentials: _options.SigningCredentials);
var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

var response = new
{
    access_token = encodedJwt,
    expires_in = (int)_options.Expiration.TotalSeconds,
    admin = identity.IsAdministrator(),
    fullname = identity.FullName(),
    username = identity.Name
};

// Serialize and return the response
context.Response.ContentType = "application/json";
await context.Response.WriteAsync(JsonConvert.SerializeObject(response, _serializerSettings));
~~~

This code will not work, if you copy and past it in your application, but shows you what needs to be done to create a token and how the token is created and sent to the client. Nate Barbattini wrote a detailed article about how this SimpleTokenProvider is working and how it needs to bes used in his Blog: [https://stormpath.com/blog/token-authentication-asp-net-core](https://stormpath.com/blog/token-authentication-asp-net-core)

## Conclusion

This is jsut a small overview. If you want to learn more and detailed information about how ASP.NET Identity works, you should definetly subscribe to the blogs of [Dominick Baier](https://leastprivilege.com/) and [Brock Allen](https://brockallen.com/). Even the [ASP.NET Docs](https://docs.asp.net/) are good resources to [learn more about the ASP.NET Security](https://docs.asp.net/en/latest/security/index.html).