---
layout: post
title: "Creating an email form with ASP.NET Core Razor Pages"
teaser: "In the comments of my last post, I got asked to write about, how to create a email form using ASP.NET Core Razor Pages. Here it is :)"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- Razor Pages
---

In the comments of my last post, I got asked to write about, how to create a email form using ASP.NET Core Razor Pages. The reader also asked about a tutorial about authentication and authorization. I'll write about this in one of the next posts. This post is just about creating a form and sending an email with the form values.

## Creating a new project

To try this out, you need to have the latest Preview of Visual Studio 2017 installed. (I use 15.3.0 preview 3) And you need .NET Core 2.0 Preview installed (2.0.0-preview2-006497 in my case)

In Visual Studio 2017, use "File... New Project" to create a new project. Navigate to ".NET Core", chose the "ASP.NET Core Web Application (.NET Core)" project  and choose a name and a location for that new project.

![]({{ site.baseurl }}/img/razor-pages/new-project.PNG)

In the next dialogue, you probably need to switch to ASP.NET Core 2.0 to see all the new available project types. (I will write about the other ones in the next posts.) Select the "Web Application (Razor Pages)" and pressed "OK".

![]({{ site.baseurl }}/img/razor-pages/new-razor-pages.PNG)

That's it. The new ASP.NET Core Razor Pages project is created.

## Creating the form

It makes sense to use the `contact.cshtml` page to add the new contact form. The `contact.cshtml.cs` is the `PageModel` to work with. Inside this file, I added a small class called `ContactFormModel`. This class will contain the form values after the post request was sent.

~~~ csharp
public class ContactFormModel
{
  [Required]
  public string Name { get; set; }
  [Required]
  public string LastName { get; set; }
  [Required]
  public string Email { get; set; }
  [Required]
  public string Message { get; set; }
}
~~~

To use this class, we need to add a property of this type to the `ContactModel`:

~~~ csharp
[BindProperty]
public ContactFormModel Contact { get; set; }
~~~

This attribute does some magic. It automatically binds the `ContactFormModel` to the view and contains the data after the post was sent back to the server. It is actually the MVC model binding, but provided in a different way. If we have the regular model binding, we should also have a `ModelState`. And we actually do:

~~~ csharp
public async Task<IActionResult> OnPostAsync()
{
  if (!ModelState.IsValid)
  {
    return Page();
  }

  // create and send the mail here

  return RedirectToPage("Index");
}
~~~

This is an async OnPost method, which looks pretty much the same as a controller action. This returns a Task of IActionResult, checks the ModelState and so on.

Let's create the HTML form for this code in the `contact.cshtml`. I use bootstrap (just because it's available) to format the form, so the HTML code contains some overhead:

~~~ html
<div class="row">
  <div class="col-md-12">
    <h3>Contact us</h3>
  </div>
</div>
<form class="form form-horizontal" method="post">
  <div asp-validation-summary="All"></div>
  <div class="row">
    <div class="col-md-12">
      <div class="form-group">
        <label asp-for="Contact.Name" class="col-md-3 right">Name:</label>
        <div class="col-md-9">
          <input asp-for="Contact.Name" class="form-control" />
          <span asp-validation-for="Contact.Name"></span>
        </div>
      </div>
    </div>
  </div>
  <div class="row">
    <div class="col-md-12">
      <div class="form-group">
        <label asp-for="Contact.LastName" class="col-md-3 right">Last name:</label>
        <div class="col-md-9">
          <input asp-for="Contact.LastName" class="form-control" />
          <span asp-validation-for="Contact.LastName"></span>
        </div>
      </div>
    </div>
  </div>
  <div class="row">
    <div class="col-md-12">
      <div class="form-group">
        <label asp-for="Contact.Email" class="col-md-3 right">Email:</label>
        <div class="col-md-9">
          <input asp-for="Contact.Email" class="form-control" />
          <span asp-validation-for="Contact.Email"></span>
        </div>
      </div>
    </div>
  </div>
  <div class="row">
    <div class="col-md-12">
      <div class="form-group">
        <label asp-for="Contact.Message" class="col-md-3 right">Your Message:</label>
        <div class="col-md-9">
          <textarea asp-for="Contact.Message" rows="6" class="form-control"></textarea>
          <span asp-validation-for="Contact.Message"></span>
        </div>
      </div>
    </div>
  </div>
  <div class="row">
    <div class="col-md-12">
      <button type="submit">Send</button>
    </div>
  </div>
</form>
~~~

This also looks pretty much the same as in common ASP.NET Core MVC views. There's no difference.

> BTW: I'm still impressed by the tag helpers. This guys even makes writing and formatting code snippets a lot easier.

## Acessing the form data

As I wrote some lines above, there is a model binding working for you. This fills up the property `Contact` with data and makes it available in the OnPostAsync() method, if the attribute `BindProperty` is set. 

~~~ csharp
[BindProperty]
public ContactFormModel Contact { get; set; }
~~~

Actually, I expected to have a model, passed as argument to the OnPost, as I saw it the first time. But you are able to use the property directly, without any other action to do:

~~~ csharp
var mailbody = $@"Hallo website owner,

This is a new contact request from your website:

Name: {Contact.Name}
LastName: {Contact.LastName}
Email: {Contact.Email}
Message: ""{Contact.Message}""


Cheers,
The websites contact form";

SendMail(mailbody);
~~~

That's nice, isn't it?

## Sending the emails

Thanks to the pretty awesome .NET Standard 2.0 and the new APIs available for .NET Core 2.0, it get's even nicer: 

// irony on

**Finally in .NET Core 2.0, it is now possible to send emails directly to an SMTP server using the famous and pretty well known `System.Net.Mail.SmtpClient()`:**

~~~ csharp
private void SendMail(string mailbody)
{
  using (var message = new MailMessage(Contact.Email, "me@mydomain.com"))
  {
    message.To.Add(new MailAddress("me@mydomain.com"));
    message.From = new MailAddress(Contact.Email);
    message.Subject = "New E-Mail from my website";
    message.Body = mailbody;

    using (var smtpClient = new SmtpClient("mail.mydomain.com"))
    {
      smtpClient.Send(message);
    }
  }
}
~~~

Isn't that cool?

// irony off

It definitely works and this is actually a good thing. 

> In previews .NET Core versions it was recommended to use an external mail delivery service like SendGrid. This kind of services usually provide a REST based API , which can be used to communicate with that specific service. Some of them also provide various client libraries for the different platforms and languages to wrap that APIs and makes them easier to use.
>
> I'm anyway a huge fan of such services, because they are easier to use and I don't need to handle message details like encoding. I don't need to care about SMTP hosts and ports, because it is all HTTPS. I don't really need to care as much about spam handling, because this is done by the service. Using such services I just need to configure the sender mail address, maybe a domain, but the DNS settings are done by them.
>
> SendGrid could be bought via the Azure marketplace and contains huge number of free emails to send. I would propose to use such services whenever it's possible. The `SmtpClient` is good in enterprise environments where you don't need to go threw the internet to send mails. But maybe the Exchanges API is another or better option in enterprise environments.

## Conclusion

The email form is working and it is actually not much code written by myself. That's awesome. For such scenarios the razor pages are pretty cool and easy to use. There's no Controller to set-up, the views and the PageModels are pretty close and the code to generate one page is not distributed over three different folders as in MVC. To create bigger applications, MVC is for sure the best choice, but I really like the possibility to keep small apps as simple as possible.