---
layout: post
title: "ASP.​NET Core in .NET 6 - Preserve prerendered state in Blazor apps"
teaser: "This is the next part of the ASP.NET Core on .NET 6 series. In this post, I'd like to have a look into preserve prerendered state in Blazor apps."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- .NET 6
- Blazor

---

This is the next part of the [ASP.NET Core on .NET 6 series]({% post_url aspnetcore6-01.md %}). In this post, I'd like to have a look into preserve prerendered state in Blazor apps.

In Blazor apps can be prerendered on the server to optimize the load time. The app gets rendered immediately in the browser and is available for the user. Unfortunately, the state that is used on while prerendering on the server is lost on the client and needs to be recreated if the page is fully loaded and the UI may flicker, if the state is recreated and the prerendered HTML will be replaced by the HTML that is rendered again on the client.

To solve that, Microsoft adds support to persist the state into the prerendered page using the `<preserve-component-state />` tag helper. This helps to set a stage that is identically on the server and on the client.

> Actually, I have no idea why this isn't implemented as a default behavior in case the app get's prerendered. It should be done easily and wouldn't break anything, I guess. 

## Try to preserve prerendered states

I tried it with a new Blazor app and it worked quite well on the `FetchData` page. The important part is, to add the `preserve-component-state` tag helper after all used components in the `_Host.cshtml`. I placed it right before the script reference to the `blazor.server.js`:

~~~html
<body>
    <component type="typeof(App)" render-mode="ServerPrerendered" />

    <div id="blazor-error-ui">
        <environment include="Staging,Production">
            An error has occurred. This application may no longer respond until reloaded.
        </environment>
        <environment include="Development">
            An unhandled exception has occurred. See browser dev tools for details.
        </environment>
        <a href="" class="reload">Reload</a>
        <a class="dismiss">🗙</a>
    </div>

    <persist-component-state /> <!-- <== relevant tag helper -->
    <script src="_framework/blazor.server.js"></script>
</body>
~~~

The next snippet is more or less the same as on Microsoft's blog post, except that the `forecast` variable is missing there and `System.Text.Json` should be in the `usings` as well

~~~csharp
@page "/fetchdata"
@implements IDisposable

@using PrerenderedState.Data
@using System.Text.Json
@inject WeatherForecastService ForecastService
@inject ComponentApplicationState ApplicationState

...

@code {
    private WeatherForecast[] forecasts;
    protected override async Task OnInitializedAsync()
    {
        ApplicationState.OnPersisting += PersistForecasts;
        if (!ApplicationState.TryTakePersistedState("fetchdata", out var data))
        {
            forecasts = await ForecastService.GetForecastAsync(DateTime.Now);
        }
        else
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            };
            forecasts = JsonSerializer.Deserialize<WeatherForecast[]>(data, options);
        }
    }

    private Task PersistForecasts()
    {
        ApplicationState.PersistAsJson("fetchdata", forecasts);
        return Task.CompletedTask;
    }

    void IDisposable.Dispose()
    {
        ApplicationState.OnPersisting -= PersistForecasts;
    }
}
~~~

What is the tag helper doing?

It renders an HTML comment to the page that contains the state in an encoded format:

~~~HTML
<!--Blazor-Component-State:CfDJ8IZEzFk/KP1DoDRucCE
6nSjBxhfV8XW7LAhH9nkG90KnWp6A83ylBVm+Fkac8gozf2hBP
DSQHeh/jejDrmtDEesKaoyjBNs9G9EDDyyOe1o1zuLnN507mK0
Bjkbyr82Mw83mIVl21n8mxherLqhyuDH3QoHscgIL7rQKBhejP
qGqQLj0WvVYdvYNc6I+FuW4v960+1xiF5XZuEDhKJpFODIZIE7
tIDHJh8NEBWAY5AnenqtydH7382TaVbn+1e0oLFrrSWrNWVRbJ
QcRUR5xpa+yWOZ7U52iudA27ZZr5Z8+LrU9/QVre3ehO+WSW7D
Z/kSnvSkpSnGRMjFDUSgWJp3WE/y9ZKIqzmnOymihJARThmUUM
ewmU2oKkb6alKJ9SabJ0Dbj/ZLwJiDpIt1je5RpZGQvEp7SWJy
VMGieHgGL9lp2UIKwCX2HMiVB+b7UpYSby5+EjLW6FB8Yh5yY3
7IK90KVzl/45UDIJWWXpltHMhJqX2eiFxT7QS3p7tbG08jeBBf
6d74Bb7q6yxfgfRuPigERZhM1MEpqYvkHsugj7TC/z1mN2RF2l
yqjbF3VG/bpATkQyVkcZq4ll/zg+98PcXS18waisz7gntG3iwM
u/sf8ugqaFWQ1hS8CU3+JtvINC7bRDfg4g4joJjlutmmlMcttQ
GCCkt+hkGKxeAyMzHbnRkv8pVyPr4ckCjLdW02H5QhgebOWGGZ
etGlFih1Dtr5cidHT0ra72pgWNoSb7jqk4wVE+E5gmEOiuX0N2
/avvuwAnAifY9Sha1cY27ZxcNJQ5ZOejTXwquuitAdotatdk89
id3WDiTt6T0LvUywvMoga8qWIPqeZw+0VmBKJjFOwQRqx1dy9E
qq4zpTBOECcinKTsbnSb5KkRLQkrCQi4MJCkh/JzvKXP+/bksd
8B3ife7ad1aFgYwX/jvAtO8amzGiMaQvgYQyHsOQwqfrYUSFZm
9hGsdXUmWlE/g8VejWlSUiforHpVjPJojsfYfmeLOjRoSPBTQZ
Q0LL4ie/QFmKXY/TI7GjJCs5UuPM=-->
~~~

(I added some line breaks here)

This reminds me of the ViewState we had in ASP.NET WebForms. Does this make Blazor Server the successor of ASP.NET WebForms? Just kidding.

Actually, it is not really the ViewState, because it not gets sent back to the server. It just helps the client restore the state created on the server initially while it was prerendered.

## What's next?

In the next part In going to look into the support for [HTTP/3 endpoint TLS configuration]({% post_url aspnetcore6-http3-tls %}) in ASP.NET Core.

