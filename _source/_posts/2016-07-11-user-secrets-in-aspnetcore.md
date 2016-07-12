--- 
layout: post
title: "Working with user secrets in ASP.​NET Core applications."
teaser: "user secrets shouldn't be stored in config files, and shouldn't be pushed to any source code repository. The SecretManager is a tool, that helps you to manage and use your secrets without saving them in a file inside your project."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.​NET Core
- .NET Core
- Security
- Azure
---

In the past there was a study about critical data in GitHub projects. They wrote a crawler to find passwords, user names and other secret stuff in projects on GitHub. And they found a lot of such data in public projects, even in projects of huge companies, which should pretty much care about security. 

The most of this credentials are stored in .config files. For sure, you need to configure the access to a database somewhere, you also need to configure the credentials to storages, mail servers, ftp, what ever. In many cases this credentials are used for development, with lot more rights than the production credentials.

Fact is: Secret information shouldn't be pushed to any public source code repository. Even better: not pushed to any source code repository.

But what is the solution? How should we tell our app where to get this secret information?

On Azure, you are able to configure your settings directly in the application settings of your web app. This overrides the settings of your config file. It doesn't matter if it's a web.config or an appsettings.json.

But we can't do the same on the local development machine. There is no configuration like this. How and where do we save secret credentials?

With .Core, there is something similar now. There is a SecretManager tool, provided by the .NET Core SDK (Microsoft.Extensions.SecretManager.Tools), which you can access with the dotnet CLI. 

This tool stores your secrets locally on your machine. This is not a high secure password manager like keypass. It is not really high secure, but on your development machine, it provides the possibility NOT to store your secrets in a config file inside your project. And this is the important thing here.

To use the SecretManager tool, you need to add that tool in the "Tools" section of your project.json, like this:

~~~ json
"Microsoft.Extensions.SecretManager.Tools": {
  "version": "1.0.0-preview2-final",
  "imports": "portable-net45+win8+dnxcore50"
},
~~~

Be sure you have a userSecretsId in your project.json. With this ID the SecretManager tool assigns the user secrets to your app:

~~~ json
"userSecretsId": "aspnet-UserSecretDemo-79c563d8-751d-48e5-a5b1-d0ec19e5d2b0",
~~~

If you create a new ASP.NET Core project with Visual Studio, the SecretManager tool is already added. 

Now you just need to access your secrets inside your app. In a new Visual Studio project, this should also already done and look like this:

~~~ csharp
public Startup(IHostingEnvironment env)
{
    _hostingEnvironment = env;

    var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

    if (env.IsDevelopment())
    {
        // For more details on using the user secret store see 
        // http://go.microsoft.com/fwlink/?LinkID=532709
        builder.AddUserSecrets();

        // This will push telemetry data through Application 
        // Insights pipeline faster, allowing you to view results 
        // immediately.
        builder.AddApplicationInsightsSettings(developerMode: true);
    }

    builder.AddEnvironmentVariables();
    Configuration = builder.Build();
}
~~~

If not arr a NuGet reference to `Microsoft.Extensions.Configuration.UserSecrets 1.0.0` in your `project.json` and add `builder.AddUserSecrets();` as shown here.


The Extension Method `AddUserSecrets()` loads the secret information of that project into the `ConfigurationBuilder`. If the keys of the secrets are equal to the keys in the previously defined `appsettings.json`, the app settings will be overwritten.

If this all is done you are able to use the tool to store new secrets:

~~~ batch
dotnet user-secrets set key value
~~~

If you create a separate section in your `appsettings.config` as equal to the existing settings, you need to combine the user secret key with the sections name and the settings name, separated by a colon.

I created settings like this:

~~~ json
"AppSettings": {
    "MySecretKey": "Hallo from AppSettings",
    "MyTopSecretKey": "Hallo from AppSettings"
},
~~~

To overwrite the keys with the values from the SecretManager tool, I need to create entries like this:

~~~ batch
dotnet user-secrets set AppSettings:MySecretKey "Hello from UserSecretStore"
dotnet user-secrets set AppSettings:MyTopSecretKey "Hello from UserSecretStore"
~~~

![]({{ site.baseurl }}/img/usersecrets/usersecrets.png)

> BTW: to override existing keys with new values, just call set the secret again with the same key and the new value.

This way to handle secret data works pretty fine for me. 

The SecretManager tool knows three more commands:
- `dotnet user-secrets clear`: removes all secrets from the store
- `dotnet user-secrets list`: shows you all existing keys
- `dotnet user-secrets remove <key>`: removes the specific key

Just type `dotnet user-secrets --help` to see more information about the existing commands.

If you need to handle some more secrets in your project, it possibly makes sense to create a small batch file to add the keys, or to share the settings with build and test environments. But never ever push this file to the source code repository ;)