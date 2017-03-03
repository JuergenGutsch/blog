---
layout: post
title: "ActiveRoute TagHelper"
teaser: "I recently read the pretty cool blog post by Ben Cull about the IsActiveRoute TagHelper. Inspired by this idea, I created a different TagHelper, which shows or hide contents, if the specified route or route parts are in the current route. This could be useful, e.g. if you don't want to have a link in an active menu item."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET Core
- TagHelper
---

I recently read the pretty cool blog post by [Ben Cull](http://benjii.me/) about the IsActiveRoute TagHelper: [http://benjii.me/2017/01/is-active-route-tag-helper-asp-net-mvc-core/](http://benjii.me/2017/01/is-active-route-tag-helper-asp-net-mvc-core/). This TagHelper adds a css class to an element, if the specified route or route parts are in the current active route. This is pretty useful, if you want to highlight an active item in a menu.

Inspired by this idea, I created a different TagHelper, which shows or hide contents, if the specified route or route parts are in the current route. This could be useful, e.g. if you don't want to have a link in an active menu item.

> From the perspective of an semantic web, it doesn't make sense to link to the current page. That means, the menu item that points to the current page should not be a link.

The usage of this TagHelper will look like this:

~~~ html
<ul class="nav navbar-nav">
  <li>
    <a asp-active-route asp-action="Index" asp-controller="Home" asp-hide-if-active="true">
      <span>Home</span>
    </a>
    <span asp-active-route asp-action="Index" asp-controller="Home">Home</span>
  </li>
  <li>
    <a asp-active-route asp-action="About" asp-controller="Home" asp-hide-if-active="true">
      <span>About</span>
    </a>
    <span asp-active-route asp-action="About" asp-controller="Home">About</span>
  </li>
  <li>
    <a asp-active-route asp-action="Contact" asp-controller="Home" asp-hide-if-active="true">
      <span>Contact</span>
    </a>
    <span asp-active-route asp-action="Contact" asp-controller="Home">Contact</span>
  </li>
</ul>
~~~

As you may see on the a-Tag, multiple TagHelper can work on a single Tag. In this case the built in AnchorTagHelper and the ActiveRouteTagHelper are manipulating the Tag. The a-Tag will be hidden if the specified route is active and the span-Tag is shown in that case.  

If you now navigate to the About page, the a-Tag is removed from the specific menu item and the span-Tag is shown. The HTML result of the menu now looks pretty clean:

~~~ html
<ul class="nav navbar-nav">
  <li>
    <a href="/">
      <span>Home</span>
    </a>
  </li>
  <li>
    <span>About</span>
  </li>
  <li>
    <a href="/Home/About">
      <span>Contact</span>
    </a>
  </li>
</ul>
~~~

Using this approach for the menu, we don't need Ben Culls TagHelper here to add a special CSS class. The style for the active item can be set via the selection of that list item with just the span in it:

~~~ css
.nav.navbar-nav li > a { ... }
.nav.navbar-nav li > a > span { ... }
.nav.navbar-nav li > span { ... } /* this is the active item*/
~~~

This CSS is based on the default Bootstrap based template in a new ASP.NET Core project. If you use another template, just replace the CSS class which identifies the menu with your specific identifier.

That means, to get that active menu item looking nice, you may just add a CSS like this:

~~~ css
.navbar-nav li > span {
    padding: 15px;
    display: block;
    color: white;
}
~~~

This results in the following view:

![]({{ site.baseurl }}/img/active-route-taghelper/active-route.png)

To get this working, we need to implement the TagHelper. I just created a new class in the project and called it ActiveRouteTagHelper and added the needed properties:

~~~ csharp
[HtmlTargetElement(Attributes = "asp-active-route")]
public class ActiveRouteTagHelper : TagHelper
{
  [HtmlAttributeName("asp-controller")]
  public string Controller { get; set; }

  [HtmlAttributeName("asp-action")]
  public string Action { get; set; }

  [HtmlAttributeName("asp-hide-if-active")]
  public bool HideIfActive { get; set; }
  
  
}
~~~

That class inherits the TagHelper base class. To use it on any HTML tag, I defined a attribute name which is needed to on the HTML we want to manipulate. I used the name "asp-active-route". Also the attributes getting a specific name. I could use the default name, without the leading "asp" prefix, but I thouhgt it would make sense to share the Controller and Action properties with the built-in AnchorTagHelper. And to be consistent, I use the prefix in all cases.

Now we need to override the Process method to actually manipulate the specific HTML tag:

~~~ csharp
public override void Process(TagHelperContext context, TagHelperOutput output)
{
  if (!CanShow())
  {
    output.SuppressOutput();
  }

  var attribute = output.Attributes.First(x => x.Name == "asp-active-route");
  output.Attributes.Remove(attribute);
}
~~~

If I cannot show the Tag because of the conditions in the CahShow() method, I completely suppress the output. Nothing is generated in that case. Not the contents and not the HTML tag itself.

At the end of the method, I remove the identifying attribute, which is used to activate this TagHelper, because this attribute will be kept usually.

To get the RouteData of the current route, we cant use the TagHelperContext or the TagHelperOutput. We need to add the inject the ViewContext:

~~~ csharp
[HtmlAttributeNotBound]
[ViewContext]
public ViewContext ViewContext { get; set; }
~~~

Now we are able to access the route data and get the needed information about the current route:

~~~ csharp
private bool CanShow()
{
  var currentController = ViewContext.RouteData.Values["Controller"].ToString();
  var currentAction = ViewContext.RouteData.Values["Action"].ToString();

  var show = false;
  if (!String.IsNullOrWhiteSpace(Controller) &&
      Controller.Equals(currentController, StringComparison.CurrentCultureIgnoreCase))
  {
    show = true;
  }
  if (show &&
      !String.IsNullOrWhiteSpace(Action) &&
      Action.Equals(currentAction, StringComparison.CurrentCultureIgnoreCase))
  {
    show = true;
  }
  else
  {
    show = false;
  }

  if (HideIfActive)
  {
    show = !show;
  }

  return show;
}
~~~

One last step you need to do, is to register your own TagHelpers. In Visual Studio open the _ViewImports.cshtml and add the following line of code:

~~~ razor
@addTagHelper *, CoreWebApplication
~~~

Where CoreWebApplication is the assembly name of your project. * means use all TagHelpers in that library

## Conclusion

I hope this makes sense to you and helps you a little more to get into the TagHelpers.

I always have fun, creating a new TagHelper.  With less code, I'm able to extend the View engine the way I need. 

I always focus on semantic HTML, if possible. Because it makes the Web a little more accessible to other devices and engines than we usually use. This could be screen readers for blind people, as well as search engines. Maybe I can do some more posts about accessibility in ASP.NET Core applications.