--- 
layout: post
title: "ASP.​NET Core and Angular2 - Part 2"
teaser: "In the last post, I prepared a ASP.NET Core project to use and build TypeScript and to host a Angular2 single page application. Now, in this second part of the ASP.NET Core and Angular2 series, I want to prepare the ASP.NET Core Web API to provide some data to Angular2."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- Web API
- Angular2
- TypeScript
---

In the last post, I prepared a ASP.NET Core project to use and build TypeScript and to host a Angular2 single page application. Now, in this second part of the ASP.NET Core and Angular2 series, I'm going to prepare the ASP.NET Core Web API to provide some data to Angular2.

I really like to separate the read and the write logic, to optimize the read and the write stuff in different ways and to keep the code clean and simple. To do this I use the "Command & Query Segregation" pattern and a small library I wrote, to support this pattern. This library provides some interfaces, a `QueryProcessor` to delegate the queries to the right `QueryHandler` and a `CommandDispatcher` to get the right `CommandHandler` for the specific command.

I also like to use the Azure Table Storage, which is a pretty fast NoSQL storage. This makes sense for the current application, because the data wont change so much. I 
'll write one or two newsletter per month. I add maybe three events per month, maybe two user groups per year and maybe one speaker every two months. I'll use four tables in the Azure Table Storage: Newsletters, Speakers, Usergroups and Events. The Events table is more like a relation table between the user group and a speaker, containing the date, a title and a short description. This is not an event database for all of the user group events, but a table to store the events, we have to pay travel expenses for the specific speaker.

I'll write a little more in detail about the "Command & Query Segregation" and the Azure Table Storage Client in separate posts. In this post, you'll see the `IQueryProcessor` and the `ICommandDispatcher` used in the API controller and simple Query and Command classes which are passed to that services. The queries and the commands will be delegated to the right handlers, which I need to implement and which will contain my business logic. Please look in the [GitHub repository](https://github.com/JuergenGutsch/InetaDatabase) to see more details about the handlers. (The details about getting the data from the data source is not really relevant in this post. You are able to use use any data source you want.)

This CQS engine is configured in the `Startup.cs` by calling `services.AddCqsEngine();`

~~~ csharp
services.AddCqsEngine(s =>
{
    s.AddQueryHandlers();
    s.AddCommandHandlers();
});
~~~

Registering the handlers in this lambda is optional, but this groups the registration a little bit. I'm also able to register the Handlers directly on the `services` object.

The methods used to register the Handlers are ExtensionMethods on the `ServiceCollection`, to keep the `Startup.cs` clean. I do all the handler registrations in this ExtensionMethod:

~~~ csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQueryHandlers(this IServiceCollection services)
    {
        services.AddTransient<IHandleQueryAsync<AllSpeakersQuery, IEnumerable<Speaker>>, AllSpeakersQueryHandler>();
        services.AddTransient<IHandleQueryAsync<SpeakerByIdQuery, Speaker>, SpeakerByIdQueryHandler>();

        services.AddTransient<IHandleQueryAsync<AllEventsQuery, IEnumerable<Event>>, AllEventsQueryHandler>();
        services.AddTransient<IHandleQueryAsync<EventByIdQuery, Event>, EventByIdQueryHandler>();

        // and many more registrations
        
        return services;
    }
}
~~~

## The Web API

To provide the fetched data to the Angular2 SPA, I want to use a Web API which is now completely included in ASP.NET Core MVC. Right click the `Controllers` folder and add a new item. Select "Server-side" and than the "Web API Controller Class". I called it `SpeakersController`:

![]({{ site.baseurl }}/img/angular2/newapicontroller.png)

~~~ csharp
[Route("api/[controller]")]
public class SpeakersController : Controller
{
    private readonly IQueryProcessor _queryProcessor;
    private readonly ICommandDispatcher _commandDispatcher;

    public SpeakersController(
        IQueryProcessor queryProcessor,
        ICommandDispatcher commandDispatcher)
    {
        _queryProcessor = queryProcessor;
        _commandDispatcher = commandDispatcher;
    }

    [HttpGet]
    public async Task<IEnumerable<Speaker>> Get()
    {
        var query = new AllSpeakersQuery();
        var speakers = await _queryProcessor.ProcessAsync(query);
        return speakers;
    }

    [HttpGet("{id}")]
    public async Task<Speaker> Get(Guid id)
    {
        var query = new SpeakerByIdQuery(id);
        var speakers = await _queryProcessor.ProcessAsync(query);
        return speakers;
    }

    [HttpPost]
    public async void Post([FromBody]Speaker value)
    {
        var command = new InsertSpeakerCommand(value);
        await _commandDispatcher.DispatchCommandAsync(command);
    }
        
    [HttpPut("{id}")]
    public async void Put(int id, [FromBody]Speaker value)
    {
        var command = new UpdateSpeakerCommand(id, value);
        await _commandDispatcher.DispatchCommandAsync(command);
    }
        
    [HttpDelete("{id}")]
    public async void Delete(int id)
    {
        var command = new DeleteSpeakerCommand(id);
        await _commandDispatcher.DispatchCommandAsync(command);
    }
}
~~~

As you can see in the Controller, I injected a `IQueryProcessor` and a `ICommandDispatcher` and I use this services by creating a query or a commend and passed it to the `DispatchAsync` or `ProcessAsync` methods

## The client side

Ho does it look like to access the Web APIs with Angular2?

First I need to create a service in Angular2. This Service is also a component and exactly this is what I really love with Angular2: Everything is a component and just needs to be stacked together :)

I create a Angular2 service for every entity in the project. First I need to import some Angular2 modules: 
- Http is to call remote resources. 
- Headers need to be send additionally to the server. 
- And we need to work with Responses and RequestsOptions. 
- We get an Observable type from the Http service 
- and we have to import our `Speaker` type:

~~~ typescript
import {Injectable, Component} from 'angular2/core';
import {Http, Response, HTTP_PROVIDERS, Headers, RequestOptions} from 'angular2/http';
import {Observable} from 'rxjs/Observable';

import {Speaker} from './speaker';

@Component({
    providers: [Http]
})
@Injectable()
export class SpeakerService {

    constructor(private _http: Http) { }

    private _speakersUrl: string = '/api/speakers/';

	// methods to access the data
}
~~~

The Http service gets injected via the constructor and can be used like this:

~~~ typescript
getSpeakers() {
    let data: Observable<Speaker[]> = this._http.get(this._speakersUrl)
        .map(res => <Speaker[]>res.json())
        .catch(this.handleError);

    return data;
}

getSpeaker(id: string) {
	let data: Observable<Speaker> = this._http.get(this._speakersUrl + id)
        .map(res => <Speaker>res.json())
        .catch(this.handleError);

    return data;
}

private handleError(error: Response) {
    console.error(error);
    return Observable.throw(error.json().error || 'Server error');
}
~~~

In both public methods we return an `Observable` object, which needs special handling in the specific consuming component, because all requests to the server are async. To consume the data, I need to subscribe to that Observable:

~~~ typescript
this._speakerService.getSpeakers()
    .subscribe(
        speakers => this.speakers = speakers,
        error => this.errorMessage = <any>error);
~~~

Subscribe calls the first delegate in case of success and assigns the speaker to the property of the current component. In case of errors the second delegate is executed and the error object gets assigned to the error property.

This is how a complete Angular2 speaker list component looks like:

~~~ typescript 
import {Component, OnInit} from 'angular2/core';
import {HTTP_PROVIDERS} from 'angular2/http';
import {ROUTER_DIRECTIVES} from 'angular2/router';

import {Speaker} from './speaker';
import {SpeakerService} from './speaker.service';

@Component({
    selector: 'speakers-list',
    templateUrl: 'app/speaker/speakers-list.template.html',
    directives: [
        ROUTER_DIRECTIVES
    ],
    providers: [SpeakerService, HTTP_PROVIDERS]
})
export class SpeakersListComponent implements OnInit {

    constructor(private _speakerService: SpeakerService) { }

    speakers: Speaker[];
    errorMessage: any;

    ngOnInit() {
        this._speakerService.getSpeakers()
            .subscribe(
                speakers => this.speakers = speakers,
                error => this.errorMessage = <any>error);
    }
}
~~~

To save an entity, I use the post or put method on the Http object, I need to specify the content type and to add the data to the body:

~~~ typescript
saveSpeaker(speaker: Speaker) {

    let body = JSON.stringify(speaker);

    let headers = new Headers({ 'Content-Type': 'application/json' });
    let options = new RequestOptions({ headers: headers });

    let temp = this._http.post(this._speakersUrl, body, options)
        .map(res => console.info(res))
        .catch(this.handleError);
}
~~~

## Conclusion

That's about how I provide the data to the client. Maybe the CQS part is not really relevant for you, but this is the way how I usually create the back-ends in my personal projects. The important part is the Web API and only you know the way how you need to access your data inside your API Controller. ;)

In the next blog post, I'm going to show you how I organize the Angular2 app and how I use the Angular2 routing to navigate between different components.