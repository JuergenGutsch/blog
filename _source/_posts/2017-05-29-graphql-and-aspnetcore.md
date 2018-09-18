---
layout: post
title: "Exploring GraphQL and creating a GraphQL endpoint in ASP.NET Core"
teaser: "A few weeks ago, I found some time to have a look at GraphQL and even at the .NET implementation of GraphQL. It is pretty amazing to see it in actions and it is easier than expected to create a GraphQL endpoint in ASP.NET Core. In this post I'm going to show you how it works."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- GraphQL
---

A few weeks ago, I found some time to have a look at GraphQL and even at the .NET implementation of GraphQL. It is pretty amazing to see it in actions and it is easier than expected to create a GraphQL endpoint in ASP.NET Core. In this post I'm going to show you how it works.

## The Graph Query Language

The GraphQL was invented by Facebook in 2012 and released to the public in 2015. It is a query language to tell the API exactly about the data you wanna have. This is the difference between REST, where you need to query different resources/URIs to get different data. In GrapgQL there is one single point of access about the data you want to retrieve. 

That also makes the planning about the API a little more complex. You need to think about what data you wanna provide and you need to think about how you wanna provide that data. 

While playing around with it, I created a small book database. The idea is to provide data about books and authors.

Let's have a look into few examples. The query to get the book number and the name of a specific book looks like this.

~~~ json
{
  book(isbn: "822-5-315140-65-3"){
    isbn,
    name
  }
}
~~~

This look similar to JSON but it isn't. The property names are not set in quotes, which means it is not really a JavaScript Object Notation. This query need to be sent inside the body of an POST request to the server.

The Query gets parsed and executed against a data source on the server and the server should send the result back to the client:

~~~ json
{
  "data": {
    "book": {
      "isbn": "822-5-315140-65-3",
      "name": "ultrices enim mauris parturient a"
    }
  }
}
~~~

If we want to know something about the author, we need to ask about it:

~~~ json
{
  book(isbn: "822-5-315140-65-3"){
    isbn,
    name,
    author{
      id,
      name,
      birthdate
    }
  }
}
~~~

This is the possible result:

~~~ json
{
  "data": {
    "book": {
      "isbn": "822-5-315140-65-3",
      "name": "ultrices enim mauris parturient a",
      "author": {
        "id": 71,
        "name": "Henderson",
        "birthdate": "1937-03-20T06:58:44Z"
      }
    }
  }
}
~~~

You need a list of books, including the authors? Just ask for it:

~~~ json
{
  books{
    isbn,
    name,
    author{
      id,
      name,
      birthdate
    }
  }
}
~~~

The list is too large? Just limit the result, to get only 20 items:

~~~ json
{
  books(limit: 20) {
    isbn,
    name,
    author{
      id,
      name,
      birthdate
    }
  }
}
~~~

Isn't that nice?

To learn more about GraphQL and the specifications, visit [http://graphql.org/](http://graphql.org/) 

## The Book Database

The book database is just fake. I love to use GenFu to generate dummy data. So I did the same for the books and the authors and created a BookRepository:

~~~ csharp
public class BookRepository : IBookRepository
{
  private IEnumerable<Book> _books = new List<Book>();
  private IEnumerable<Author> _authors = new List<Author>();

  public BookRepository()
  {
    GenFu.GenFu.Configure<Author>()
      .Fill(_ => _.Name).AsLastName()
      .Fill(_=>_.Birthdate).AsPastDate();
    _authors = A.ListOf<Author>(40);

    GenFu.GenFu.Configure<Book>()
      .Fill(p => p.Isbn).AsISBN()
      .Fill(p => p.Name).AsLoremIpsumWords(5)
      .Fill(p => p.Author).WithRandom(_authors);
    _books = A.ListOf<Book>(100);
  }

  public IEnumerable<Author> AllAuthors()
  {
    return _authors;
  }

  public IEnumerable<Book> AllBooks()
  {
    return _books;
  }

  public Author AuthorById(int id)
  {
    return _authors.First(_ => _.Id == id);
  }

  public Book BookByIsbn(string isbn)
  {
    return _books.First(_ => _.Isbn == isbn);
  }
}

public static class StringFillerExtensions
{
  public static GenFuConfigurator<T> AsISBN<T>(
    this GenFuStringConfigurator<T> configurator) where T : new()
  {
    var filler = new CustomFiller<string>(
      configurator.PropertyInfo.Name, 
      typeof(T), 
      () =>
      {
        return MakeIsbn();
      });
    configurator.Maggie.RegisterFiller(filler);
    return configurator;
  }
  
  public static string MakeIsbn()
  {
    // 978-1-933988-27-6
    var a = A.Random.Next(100, 999);
    var b = A.Random.Next(1, 9);
    var c = A.Random.Next(100000, 999999);
    var d = A.Random.Next(10, 99);
    var e = A.Random.Next(1, 9);
    return $"{a}-{b}-{c}-{d}-{e}";
  }
}
~~~

GenFu provides a useful set of so called fillers to generate data randomly. There are fillers to generate URLs, emails, names, last names, states of US and Canada and so on. I also need a ISBN generator, so I created one by extending the generic GenFuStringConfigurator.

The BookRepository is registered as a singleton in the Dependency Injection container, to work with the same set of data while the application is running. You are able to add some more information to that repository, like publishers and so on.

## GraphQL in ASP.NET Core

Fortunately there is a .NET Standard compatible [implementation of the GraphQL on GitHub](https://github.com/graphql-dotnet/graphql-dotnet). So there's no need to parse the Queries by yourself. This library is also available as a NuGet package:

~~~ xml
<PackageReference Include="GraphQL" Version="0.15.1.678" />
~~~

The examples provided on GitHub, are pretty easy. They directly write the result to the output, which means the entire ASP.NET Applications is a GraphQL server. But I want to add GraphQL as a ASP.NET Core MiddleWare, to add the GraphQL implementation as a different part of the Application. Like this you are able to use REST based POST and PUT request to add or update the data and to use the GraphQL to query the data.

I also want that the middleware is listening to the sub path "/graph"

~~~ csharp
public class GraphQlMiddleware
{
  private readonly RequestDelegate _next;
  private readonly IBookRepository _bookRepository;

  public GraphQlMiddleware(RequestDelegate next, IBookRepository bookRepository)
  {
    _next = next;
    _bookRepository = bookRepository;
  }

  public async Task Invoke(HttpContext httpContext)
  {
    var sent = false;
    if (httpContext.Request.Path.StartsWithSegments("/graph"))
    {
      using (var sr = new StreamReader(httpContext.Request.Body))
      {
        var query = await sr.ReadToEndAsync();
        if (!String.IsNullOrWhiteSpace(query))
        {
          var schema = new Schema { Query = new BooksQuery(_bookRepository) };
          var result = await new DocumentExecuter()
            .ExecuteAsync(options =>
                          {
                            options.Schema = schema;
                            options.Query = query;
                          }).ConfigureAwait(false);
          CheckForErrors(result);
          await WriteResult(httpContext, result);
          sent = true;
        }
      }
    }
    if (!sent)
    {
      await _next(httpContext);
    }
  }

  private async Task WriteResult(HttpContext httpContext, ExecutionResult result)
  {
    var json = new DocumentWriter(indent: true).Write(result);
    httpContext.Response.StatusCode = 200;
    httpContext.Response.ContentType = "application/json";
    await httpContext.Response.WriteAsync(json);
  }

  private void CheckForErrors(ExecutionResult result)
  {
    if (result.Errors?.Count > 0)
    {
      var errors = new List<Exception>();
      foreach (var error in result.Errors)
      {
        var ex = new Exception(error.Message);
        if (error.InnerException != null)
        {
          ex = new Exception(error.Message, error.InnerException);
        }
        errors.Add(ex);
      }
      throw new AggregateException(errors);
    }
  }
}

public static class GraphQlMiddlewareExtensions
{
  public static IApplicationBuilder UseGraphQL(this IApplicationBuilder builder)
  {
    return builder.UseMiddleware<GraphQlMiddleware>();
  }
}
~~~

With this kind of MiddleWare, I can extend my applications Startup.cs with GraphQL:

~~~ csharp
app.UseGraphQL();
~~~

As you can see, the BookRepository gets passed into this Middleware via constructor injection. The most important part is that line:

~~~ csharp
var schema = new Schema { Query = new BooksQuery(_bookRepository) };
~~~

This is where we create a schema, which is used by the GraphQL engine to provide the data. The schema defines the structure of the data you wanna provide. This is all done in a root type called BooksQuery. This type gets the BookRepostory.

This Query is a GraphType, provided by the GraphQL library. You need to derive from a ObjectGraphType and to configure the schema in the constructor:

~~~ csharp
public class BooksQuery : ObjectGraphType
{
  public BooksQuery(IBookRepository bookRepository)
  {
    Field<BookType>("book",
                    arguments: new QueryArguments(
                      new QueryArgument<StringGraphType>() { Name = "isbn" }),
                      resolve: context =>
                      {
                        var id = context.GetArgument<string>("isbn");
                        return bookRepository.BookByIsbn(id);
                      });

    Field<ListGraphType<BookType>>("books",
                                   resolve: context =>
                                   {
                                     return bookRepository.AllBooks();
                                   });
  }
}
~~~

Using the GraphQL library all types used in the Query to define the schema are any kind of GraphTypes, even the BookType:

~~~ csharp
public class BookType : ObjectGraphType<Book>
{
  public BookType()
  {
    Field(x => x.Isbn).Description("The isbn of the book.");
    Field(x => x.Name).Description("The name of the book.");
    Field<AuthorType>("author");
  }
}
~~~

The difference is just the generic ObjectGraphType which is also used for the AuthorType. The properties of the Book, which are simple types like the name or the ISBN are mapped directly with the lambda. The complex typed properties like the Author are mapped via another generic ObjectGraphType, which is ObjectGraphType<Author> in that case.

Like this you need to create your Schema, which can be used to query the data.

## Conclusion

If you want to play around with this demo, I [pushed it to a repository on GitHub](https://github.com/JuergenGutsch/GraphQlDemo).

This are my first steps using GraphQL and I really like it. I think this is pretty useful and will reduce the effort on both the client side and the server side a lot. Even if the effort to create the schema is lot more than creating just a Web API controller, but usually you need to create a lot more than just one single Web API controller.

This also reduces the amount of data between the client and the server, because the client could just load the needed data and don't need to GET or POST all unneeded stuff.

I think, I'll use it a lot more in the future projects.

What do you think?  
