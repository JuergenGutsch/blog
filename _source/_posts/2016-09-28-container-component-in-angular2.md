--- 
layout: post
title: "Creating a container component in Angular2"
teaser: "In one of the last projects, I needed a shared reusable component, which needs to be extended with additional contents or functionality by the view who uses this component. In our case, it was a kind of a menu bar used by multiple views."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- Angular2
---

In one of the last projects, I needed a shared reusable component, which needs to be extended with additional contents or functionality by the view who uses this component. In our case, it was a kind of a menu bar used by multiple views. (View in this case means routing targets.) 

Creating such components was easier than expected. I anyway spent almost a whole day to find that solution, I played around with view and template providers, tried to access the template and to manipulate the template. I also tried to create an own structural directive.

But you just need to use the <ng-content> directive in the container component.

~~~ html
<nav>
  <div class="navigation pull-left">
    <ul>
      <!-- the menu items --->
    </ul>
  </div>
  <div class="pull-right">
    <ng-content></ng-content>
  </div>
</nav
~~~

That's all. You don't need to write any TypeScript code to get this working. Using this component is now pretty intuitive:

~~~ html
<div class="nav-bar">
  <app-navigation>
    <button (click)="printDraft($event)">print draft</button>
    <button (click)="openPreview($event)">Show preview</button>
  </app-navigation>
</div>
~~~

The contents of the <app-navigation> - the buttons - will now be placed to the <ng-content> place holder.

After spending almost a whole day to get this working my first question was: Is it really that easy? Yes it is. That's all.

Maybe you knew about it. But I wasn't able to find any hint in the docs, on StackOverflow or in any Blog about it. Maybe this requirement isn't used needed often. At least I stumbled upon [a documentation where ng-content as used](https://angular.io/docs/ts/latest/guide/component-styles.html#!#loading-styles) and I decided to write about it. Hope it will help someone else. :)