--- 
layout: post
title: "A small library to support the CQS pattern."
teaser: "The last years, I loved to use the Command and Query Segregation pattern. Using this pattern in every new project, requires to have the same infrastructure classes in this projects. This is why I started to create a small and reusable library, which now supports ASP.NET Core and matches the .NET Standard 1.6."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- CQS
- Patterns
- OSS
- Libraries
- ASP.NET Core
- .NET Standard
---

The last years, I loved to use the Command and Query Segregation pattern. Using this pattern in every new project, requires to have the same infrastructure classes in this projects. This is why I started to create a small and reusable library, which now supports ASP.NET Core and is written to match .NET Standard 1.6.

## About that CQS

The idea behind CQS is to separate the query part (the read part / fetching-the-data-part) from the command part (the write part / doing-things-with-the-data-part). This enables you to optimize both parts in different ways. You are able to split the data flow into different optimized pipes.

From my perspective, the other most important benefit of it is, that this approach enforce you, to split your business logic into pretty small peaces of code. This is because each command and each query only does one single thing:
- fetching a specific set of data
- executing a specific command

E. g. if you press a button, you probably want to save some data. You will create a SaveDataCommand with the data to save in it. You'll pass that command to the CommandDispatcher and this guy will delegate the command to the right CommandHandler, which is just responsible to save that specific data to the database, or what ever you want to do with that data. You think you'll also add a log entry with the same command? No problem: Just create another CommandHandler using the same command. With this approach you'll have two small components, one to save the data and another one to add a log entry, which are completely independent and can be tested separately.

What about fetching the data? Just create a "Query" with the data used as a filter criteria. Pass the Query to the QueryProcessor, which delegates the Query to the right QueryHandler. in this QueryHandler, you are able to select the data from the data source, map it to the result you expect and return it back.

Sounds easy? It really is as easy.

Each Handler, both the QuerHandlers and the CommandHandlers, are isolated peaces of code, if you use Dependency Injection in it. This means unit tests are as easy as the implementation itself.
 
## What is inside the library?

This library contains a CommandDispatcher and a QueryProcessor to delegate commands and queries to the right handlers. The library helps you to write your own commands and queries, as well as your own command handlers and query handlers. There are two main NameSpaces inside the library: Command and Query

The Command part contains the `CommandDispatcher`, an `ICommand` interface and two more interfaces to define command handlers (`ICommandHandler<in TCommand>`) and async command handlers (`IAsyncCommandHandler<in TCommand>`):

The `CommandDispatcher` interface looks like this:

~~~ csharp
public interface ICommandDispatcher
{
    void DispatchCommand<TCommand>(TCommand command) where TCommand : ICommand;
    Task DispatchCommandAsync<TCommand>(TCommand command) where TCommand : ICommand;
}
~~~

The Query part contains the `QueryProcessor`, a generic `IQuery`, which defines the result in the generic argument and two different `QueryHandlers`. It also contains two more interfaces to define query handlers (`IHandleQuery<in TQuery, TResult>`) and async query handlers (`IHandleQueryAsync<in TQuery, TResult>`)

~~~ csharp
public interface IQueryProcessor
    {
        TResult Process<TResult>(IQuery<TResult> query);
        Task<TResult> ProcessAsync<TResult>(IQuery<TResult> query);
    }
~~~

## Using the library

For the following examples, I'll reuse the speaker database I already used in previous blog posts.

After you installed the library using Nuget, you need to register the the `QueryProcessor` and the `CommandDispatcher` to the dependency injection. You can do it manually in the `ConfigureSerices` method or just by using `AddCqsEngine()`

~~~ csharp
public void ConfigureServices(IServiceCollection services)
{
	services.AddMvc(),

	services.AddCqsEngine();

	services.AddQueryHandlers();
	services.AddCommandHandlers();
}
~~~

The methods `AddQueryHandlers` and `AddCommandHandlers` are just methods to encapsulate the registrtion of your Handlers, and are propably written by you as a user of this library. The method could look like this:

~~~ csharp
public static IServiceCollection AddQueryHandlers(this IServiceCollection services)
{
	services.AddTransient<IHandleQueryAsync<AllSpeakersQuery, IEnumerable<Speaker>>, SpeakerQueryHandler>();
	services.AddTransient<IHandleQueryAsync<SpeakerByIdQuery, Speaker>, SpeakerQueryHandler>();

	services.AddTransient<IHandleQueryAsync<AllEventsQuery, IEnumerable<Event>>, EventQueryHandler>();
	services.AddTransient<IHandleQueryAsync<SingleEventByIdQuery, Event>, EventQueryHandler>();

	services.AddTransient<IHandleQueryAsync<AllUsergroupsQuery, IEnumerable<Usergroup>>, UsergroupQueryHandler>();
	services.AddTransient<IHandleQueryAsync<SingleUsergroupByIdQuery, Usergroup>, UsergroupQueryHandler>();

	services.AddTransient<IHandleQueryAsync<AllNewslettersQuery, IEnumerable<Newsletter>>, NewsletterQueryHandler>();
	services.AddTransient<IHandleQueryAsync<SingleNewsletterByIdQuery, Newsletter>, NewsletterQueryHandler>();
	
	return services;
}
~~~

Usually you will place this method near you handlers.

The method `AddCqsEngine` is overloaded to add your `QueryHandlers` and yor `CommandHandlers` to the dependnecy injection. There is no real magic behind that method. It is just to group the additional dependencies:

~~~ csharp
services.AddCqsEngine(s =>
	{
		s.AddQueryHandlers();
		s.AddCommandHandlers();
	});
~~~

The parameter `s` is the same Seervicecollection as the one in the `ConfigureServices` method.

This library makes heavily use of dependency injection and uses the `IServiceProvider`, which is used and provided in ASP.NET Core. If you replace the built in DI container with different one, you should ensure that the `IServiceProvider` is implemented and registered with that container. 

## Query the data

Getting all the speakers out of the storage is a pretty small example. I just need to create a small class called `AllSpeakersQuery` and to implement the generic interface `IQuery`:

~~~ csharp
public class AllSpeakersQuery : IQuery<IEnumerable<Speaker>>
{
}
~~~

The generic argument of the `IQuery` interface defines the value we want to retrieve from the storage. In this case it is a `IEnumerable` of speakers.

Querying a single speaker looks like this:

~~~ csharp
public class SpeakerByIdQuery : IQuery<Speaker>
{
    public SingleSpeakerByIdQuery(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; private set; }
}
~~~

The query contains the speakers Id and defines the return value of a single Speaker. 

Once you got the `QueryProcessor` from the dependency injection, you just need to pass the queries to it and retrieve the data:

~~~ csharp
// sync
var speaker = _queryProcessor.Process(new SpeakerByIdQuery(speakerId));
// async
var speakers = await _queryProcessor.ProcessAsync(new AllSpeakersQuery());
~~~

Now let's have a look into the `QueryHandlers`, which are called by the `QueryProcessor`. This handlers will contain your business logic. This are small classes, implementing the `IHandleQuery<in TQuery, TResult>` interface or the `IHandleQueryAsync<in TQuery, TResult>` interface, where `TQuery` is a `IQuery<TResult>`. This class usually retrieves a data source via dependency injection and an `Execute` or `ExecuteAsync` method, with the specific Query as argument:

~~~ csharp
public class AllSpeakersQueryHandler :
    IHandleQuery<AllSpeakersQuery, IEnumerable<Speaker>>
{
    private readonly ITableClient _tableClient;

    public SpeakerQueryHandler(ITableClient tableClient)
    {
        _tableClient = tableClient;
    }

    public Task<IEnumerable<Speaker>> Execute(AllSpeakersQuery query)
    {
        var result = _tableClient.GetItemsOf<Speaker>();
        return result;
    }
}

public class SpeakerByIdQueryQueryHandler :
    IHandleQueryAsync<SpeakerByIdQuery, Speaker>
{
    private readonly ITableClient _tableClient;

    public SpeakerQueryHandler(ITableClient tableClient)
    {
        _tableClient = tableClient;
    }
    
    public async Task<Speaker> ExecuteAsync(SpeakerByIdQuery query)
    {
        var result = await _tableClient.GetItemOf<Speaker>(query.Id);
        return result;
    }
}
~~~

Sometimes I handle multiple queries in a single class, this is possible by just implementing multiple `IHandleQuery` interfaces. I would propose to do this only, if you have really small Execute methods.

## Executing Commands

Let's have a quick look into the commands too.

Let's assume we need to create a new speaker and we need to update a speakers email address. To do this we need to define two specific commands

~~~ csharp
public class AddSpeakerCommand : ICommand
{
    AddSpeakerCommand(Speaker speaker)
    {
        Speaker = speaker;
    }

    public Speaker Speaker { get; private set; }
]

public class UpdateSpeakersEmailCommand : ICommand
{
    UpdateSpeakersEmailCommand(int speakerId, string email)
    {
        SpeakerId = speakerId;
        Email = email;
    }

    public int SpeakerId { get; private set; }

    public string Email { get; private set; }
}

~~~

As equal to the queries, the commands need to be passed to the `CommandDispatcher`, which is registered in the DI container.

~~~ csharp
// sync
_commandDispatcher.DispatchCommand(new AddSpeakerCommand(myNewSpeaker));
// async
await _commandDispatcher.DispatchCommandasync(new UpdateSpeakersEmailCommand(speakerId, newEmail));
~~~

The CommandHandlers are small classes which are implementing the ICommandHandler<in TCommand> or the IAsyncCommandHandler<in TCommand> where TCommand is a ICommand. Thise handlers contain a Handle or a HandleAync method with the specific Command as argument. As equal to the query part, you usually will also get a data source from the dependency injection:

~~~ csharp
public class AddSpeakerCommandHandler : ICommandHandler<AddSpeakerCommand>
{
	private readonly ITableClient _tableClient;

	public AddSpeakerCommandHandler(ITableClient tableClient)
	{
		_tableClient = tableClient;
	}

	public void Handle(AddSpeakerCommand command)
	{
		_tableClient.SaveItemOf<Speaker>(command.Speaker);
	}
}
~~~

## Command validation

What about validatig the commands? Sometimes it is needed to check authorization or to validate the command values before executing the commands. You can do the checks inside the handlers, but this is not always a good idea. This increases the size and the complexity of the handlers and the validation logic is not reusable like this.

This is why the CommandDispatcher supports precondition checks. As equal to the command handlers, you just need to write command preconditions (`ICommandPrecondition<in TCommand>`) od async command preconditions (`ICommandPrecondition<in TCommand>`). This interfaces contain a Chack or ChackAsync method which will be executed before the command handlers are executed. You can hava as many preconditions as you want for a single command. If you register the preconditions to the DI container, the command dispatcher will find and execute them:

~~~ csharp
public class ValidateChangeUsersNameCommandPrecondition : ICommandPrecondition<ChangeUsersNameCommand>
{
    public void Check(ChangeUsersNameCommand command)
    {
        if (command.UserId == Guid.Empty)
        {
            throw new ArgumentException("UserId cannot be empty");
        }
        if (String.IsNullOrWhiteSpace(command.Name))
        {
            throw new ArgumentNullException("Name cannot be null");
        }
    }
}
~~~

In case of errors, the command dispatcher will throw an AggregateException with all the possible exceptions in it.

## Conclusion

The whole speaker database application is built like this: Using handlers to create small components, which are handling queries to fetch data or which are executing commands to do something with the data.

What do you think? Does it make sense to you? Would it be useful for your projects? Please drop some lines and tell me about your opinion :)

This library is [hosted on GitHub](https://github.com/JuergenGutsch/Gos.Tools). I would be happy about any type contribution on GitHub. Feel free to try id out and let me know about issues, tips and improvements :)

