---
layout: post
title: "Creating a Word document OutputFormatter in ASP.NET Core"
teaser: "In one of the ASP.NET Core projects we did in the last year, we created an OutputFormatter to provide a Word documents as printable reports via ASP.NET Core Web API."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- OutputFormatter
- Web API
---

In one of the ASP.NET Core projects we did in the last year, we created an OutputFormatter to provide a Word documents as printable reports via ASP.NET Core Web API.  Well, this formatter wasn't done by me, but done by a fellow software developer [Jakob Wolf](https://twitter.com/wolf85wolf) at the [yooapps.com](http://yooapps.com). I told him to write about it, but he hadn't enough time to do it yet, so I'm going to do it for him. Maybe you know about him on Twitter. Maybe not, but he is one of the best ASP.NET and Angular developers I ever met.

## About OutputFormatters

In ASP.NET you are able to have many different formatters. The best known built-in formatter is the `JsonOutputFormatter` which is used as the default `OutputFormatter` in ASP.NET Web API. 

By using the `AddMvcOptions()` you are able to add new Formatters or to manage the existing formatters:

~~~ csharp
services.AddMvc()
    .AddMvcOptions(options =>
    {
        options.OutputFormatters.Add(new WordOutputFormatter());
        options.FormatterMappings.SetMediaTypeMappingForFormat(
          "docx", MediaTypeHeaderValue.Parse("application/ms-word"));
    })
~~~
As you can see in the snippet above, we add the Word document formatter (called `WordOutputFormatter` to provide the Word documents if the requested type is "application/ms-word".

You are able to add whatever formatter you need, provided on whatever media type you want to support.

Let's have a look how a output formatter looks like:

~~~ csharp
public class MyFormatter : IOutputFormatter
{
  public bool CanWriteResult(OutputFormatterCanWriteContext context)
  {
    // check whether to write or not
    throw new NotImplementedException();
  }

  public async Task WriteAsync(OutputFormatterWriteContext context)
  {
    // write the formatted contents to the response stream.
    throw new NotImplementedException();
  }
}
~~~

You have one method to check whether the data can be written to the expected format or not. The other async method does the job to format and output the data to the response stream, which comes with the context.

This way needs to do some things manually. A more comfortable way to implement an OutputFormatter is to inherit from the `OutputFormatter` base class directly:

~~~ csharp
public class WordOutputFormatter : OutputFormatter
{
  public string ContentType { get; }

  public WordOutputFormatter()
  {
    ContentType = "application/ms-word";
    SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ContentType));
  }

  // optional, but makes sense to restrict to a specific condition
  protected override bool CanWriteType(Type type)
  {
    if (type == null)
    {
      throw new ArgumentNullException(nameof(type));
    }

    // only one ViewModel type is allowed
    return type == typeof(DocumentContentsViewModel);
  }

  // this needs to be overwritten
  public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
  {
    // Format and write the document outputs here
    throw new NotImplementedException();
  }
}
~~~

The base class does some things for you. For example to write the correct HTTP headers.

## Creating Word documents

To create Word documents you need to add a reference to the Open XML SDK. We used the [OpenXMLSDK-MOT](https://www.nuget.org/packages/openxmlsdk-mot/) with the version 2.6.0, which cannot used with .NET Core. This is why we run that specific ASP.NET Core project on .NET 4.6. 

Version 2.7.0 is available as a .NET Standard 1.3 library and can be used in .NET Core. Unfortunately this version isn't yet available in the default NuGet Feed. To install the latest Version, follow the instructions on GitHub: [https://github.com/officedev/open-xml-sd](https://github.com/officedev/open-xml-sdk/) Currently there is a mess with the NuGet package IDs and versions on NuGet and MyGet. Use the MyGet feed, mentioned on the GitHub page to install the latest version. The package ID here is DocumentFormat.OpenXml and the latest stable Version is 2.7.1

In this post, I don't want to go threw all the word processing stuff , because it is too specific to our implementation. I just show you how it works in general. The Open XML SDK is pretty well documented, so you can use this as an entry point to create your own specific `WordOutputFormatter`:

~~~ csharp
public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
{
  var response = context.HttpContext.Response;
  var filePath = Path.GetTempFileName();

  var viewModel = context.Object as DocumentContentsViewModel;
  if (viewModel == null)
  {
    throw new ArgumentNullException(nameof(viewModel));
  }

  using (var wordprocessingDocument = WordprocessingDocument
         .Create(filePath, WordprocessingDocumentType.Document))
  {
    // start creating the documents and the main parts of it
    wordprocessingDocument.AddMainDocumentPart();

    var styleDefinitionPart = wordprocessingDocument.MainDocumentPart
      .AddNewPart<StyleDefinitionsPart>();
    var styles = new Styles();
    styles.Save(styleDefinitionPart);

    wordprocessingDocument.MainDocumentPart.Document = new Document
    {
      Body = new Body()
    };
    var body = wordprocessingDocument.MainDocumentPart.Document.Body;

    // call a helper method to set default styles
    AddStyles(styleDefinitionPart); 
    // call a helper method set the document to landscape mode
    SetLandscape(body); 

    foreach (var institution in viewModel.Items)
    {
      // iterate threw some data of the viewmodel 
      // and create the elements you need
      
      // ... more word processing stuff here

    }

    await response.SendFileAsync(filePath);
  }
}
~~~

The VewModel with the data to format, is in the `Object` property of the `OutputFormatterWriteContext`. We do a save cast and check for null before we continue. The Open XML SDK works based on files. This is why we need to create a temp file name and let the SDK use this file path. Because of that fact - at the end - we send the file out to the response stream using the `response.SendFileAsync()` method. I personally prefer to work on the `OutputStream` directly, to have less file operations and to be a little bit faster. The other thing is, we need to cleanup the temp files.

After the file is created, we work on this file and create the document, custom styles and layouts and the document body, which will contain the formatted data. Inside the loop we are only working on that `Body` object. We created helper methods to add formatted values, tables and so on...

## Conclusion

OutputFormatters are pretty useful to create almost any kind of content out of any kind of data. Instead of hacking around in the specific Web API actions, you should always use the OutputFormatters to have reusable components.

The OutputFormatter we build, is not really reusable or even generic, because it was created for a specific kind of a report. But with this starting point, we are able to make it generic. We could pass a template document to the formatter, which knows the properties of the ViewModel, this way it is possible to create almost all kind of Word documents.