---
layout: post
title: "Setup Angular2 & TypeScript in a ASP.​NET Core project using Visual Studio"
teaser: "In this post I try to explain, how to setup a ASP.NET Core project with Angular2 and typescript in Visual Studio 2015."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.​NET Core
- .NET Core
- Angular2
---

In this post I try to explain, how to setup a ASP.NET Core project with Angular2 and typescript in Visual Studio 2015. 

> **UPDATE** 
> This post is now updated to ASP.NET Core 1.0 and Angular2 final. I had troubles to create a ASP.NET Core app using .NET Core 1.0.1 in Visual Studio. This is why it still uses 1.0. The most changes are done in the Angular2 part with the new Module and some other Angular2 dependencies. I also changed the `gulpfile.js` to move the needed data in a cleaner way.
> You will find a working project on GitHub: [https://github.com/JuergenGutsch/angular2-aspnetcore-vs](https://github.com/JuergenGutsch/angular2-aspnetcore-vs)

There are two ways to setup an Angular2 Application: The most preferred way is to use angular-cli, which is pretty simple. Unfortunately the Angular CLI doesn't use the latest version . The other way is to follow the tutorial on angular.io, which sets-up a basic starting point, but this needs a lot of manually steps. There are also two ways to setup the way you want to develop your app with ASP.NET Core: One way is to separate the client app completely from the server part. It is pretty useful to decouple the server and the client, to create almost independent applications and to host it on different machines. The other way is to host the client app inside the server app. This is useful for small applications, to have all that stuff in one place and it is easy to deploy on a single server.

In this post I'm going to show you, how you can setup Angular2 app, which will be hosted inside an ASP.NET Core application using Visual Studio 2015. Using this way, the Angular-CLI is not the right choice, because it already sets up a development environment for you and all that stuff is configured a little bit different. The effort to move this to Visual Studio would be to much. I will almost follow the tutorial on [http://angular.io/](http://angular.io/). But we need to change some small things to get that stuff working in Visual Studio 2015.

## Configure the ASP.NET Core project

Let's start with a new ASP.NET Core project based on .NET Core. (The name doesn't matter, so "WebApplication391" is fine). We need to choose a Web API project, because the client side Angular2 App will probably communicate with that API and we don't need all the predefined MVC stuff.

A Web API project can't serve static files like JavaScripts, CSS styles, images, or even HTML files. Therefore we need to add a reference to `Microsoft.AspNetCore.StaticFiles` in the `project.json`:

~~~ json
"Microsoft.AspNetCore.StaticFiles": "1.0.0 ",
~~~

And in the startup.cs, we need to add the following line, just before the call of `UseMvc()

~~~ csharp
app.UseStaticFiles();
~~~

Another important thing we need to do in the `startup.cs`, is to support the Routing of Angular2. If the Browser calls a URL which doesn't exists on the server, it could be a Angular route. Especially if the URL doesn't contain a file extension.

This means we need to handle the 404 error, which will occur in such cases. We need to serve the `index.html` to the client, if there was an 404 error, on requests without extensions. To do this we just need a simple lambda based MiddleWare, just before we call `UseStaticFiles()`:

~~~ csharp
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == 404
        && !Path.HasExtension(context.Request.Path.Value))
    {
        context.Request.Path = "/index.html";
        await next();
    }
});
~~~

Inside the properties folder we'll find a file called `launchSettings.json`. This file is used to configure the behavior of visual Studio 2015, when we press F5 to run the application. Remove all strings "`api/values`" from this file. Because Visual Studio will always call that specific Web API, every time you press F5.

Now we prepared the ASP.NET Core application to start to follow the angular.io tutorial.:

Let's start with the NodeJS packages. Using Visual Studio we can create a new "npm Configuration file" called `package.json`. Just copy the stuff from the tutorial:

~~~ json
{
  "name": "angular-quickstart",
  "version": "1.0.0",
  "scripts": {
    "start": "tsc && concurrently \"tsc -w\" \"lite-server\" ",
    "lite": "lite-server",
    "postinstall": "typings install && gulp restore",
    "tsc": "tsc",
    "tsc:w": "tsc -w",
    "typings": "typings"
  },
  "licenses": [
    {
      "type": "MIT",
      "url": "https://github.com/angular/angular.io/blob/master/LICENSE"
    }
  ],
  "dependencies": {
    "@angular/common": "2.0.2",
    "@angular/compiler": "2.0.2",
    "@angular/core": "2.0.2",
    "@angular/forms": "2.0.2",
    "@angular/http": "2.0.2",
    "@angular/platform-browser": "2.0.2",
    "@angular/platform-browser-dynamic": "2.0.2",
    "@angular/router": "3.0.2",
    "@angular/upgrade": "2.0.2",
    "angular-in-memory-web-api": "0.1.5",
    "bootstrap": "3.3.7",
    "core-js": "2.4.1",
    "reflect-metadata": "0.1.8",
    "rxjs": "5.0.0-beta.12",
    "systemjs": "0.19.39",
    "zone.js": "0.6.25"
  },
  "devDependencies": {
    "concurrently": "3.0.0",
    "lite-server": "2.2.2",
    "gulp": "^3.9.1",
    "typescript": "2.0.3",
    "typings":"1.4.0"
  }
}
~~~

In this listing, I changed a few things:
- I added "&& gulp restore" to the postinstall script
- I also added Gulp to the devDependency to typings

After the file is saved, Visual Studio tryies to load all the packages. This works, but VS shows a yellow exclemation mark because of any arror. Until yet, I didn't figure out what is going wrong here. To be sure all packages are propery installed, use the console, change directory to the current project and type `npm install`

The post install will possibly faile because gulp is not yet configured. We need gulp to copy the dependencies to the right location inside the wwwroot folder, because static files will only be loaded from that location. This is not part of the tutorial on angular.io, but is needed to fit the client stuff into Visual Studio. Using Visual Studio we need to create a new "gulp Configuration file" with the name `gulpfile.js`:

~~~ javascript
var gulp = require('gulp');

var libs = './wwwroot/libs/';

gulp.task('default', function () {
    // place code for your default task here
});

gulp.task('restore:core-js', function() {
    gulp.src([
        'node_modules/core-js/client/*.js'
    ]).pipe(gulp.dest(libs + 'core-js'));
});
gulp.task('restore:zone.js', function () {
    gulp.src([
        'node_modules/zone.js/dist/*.js'
    ]).pipe(gulp.dest(libs + 'zone.js'));
});
gulp.task('restore:reflect-metadata', function () {
    gulp.src([
        'node_modules/reflect-metadata/reflect.js'
    ]).pipe(gulp.dest(libs + 'reflect-metadata'));
});
gulp.task('restore:systemjs', function () {
    gulp.src([
        'node_modules/systemjs/dist/*.js'
    ]).pipe(gulp.dest(libs + 'systemjs'));
});
gulp.task('restore:rxjs', function () {
    gulp.src([
        'node_modules/rxjs/**/*.js'
    ]).pipe(gulp.dest(libs + 'rxjs'));
});
gulp.task('restore:angular-in-memory-web-api', function () {
    gulp.src([
        'node_modules/angular-in-memory-web-api/**/*.js'
    ]).pipe(gulp.dest(libs + 'angular-in-memory-web-api'));
});

gulp.task('restore:angular', function () {
    gulp.src([
        'node_modules/@angular/**/*.js'
    ]).pipe(gulp.dest(libs + '@angular'));
});

gulp.task('restore:bootstrap', function () {
    gulp.src([
        'node_modules/bootstrap/dist/**/*.*'
    ]).pipe(gulp.dest(libs + 'bootstrap'));
});

gulp.task('restore', [
    'restore:core-js',
    'restore:zone.js',
    'restore:reflect-metadata',
    'restore:systemjs',
    'restore:rxjs',
    'restore:angular-in-memory-web-api',
    'restore:angular',
    'restore:bootstrap'
]);
~~~

The task restore, copies all the needed files to the Folder ./wwwroot/libs

TypeScript needs some type definitions to get the types and API definitions of the libraries, which are not written in TypeScript or not available in TypeScript. To load this, we use another tool, called "typings". This is already installed with NPM. This tool is a package manager for type definition files. We need to configure this tool with a `typings.json`

~~~ json
{
  "globalDependencies": {
    "core-js": "registry:dt/core-js#0.0.0+20160725163759",
    "jasmine": "registry:dt/jasmine#2.2.0+20160621224255",
    "node": "registry:dt/node#6.0.0+20160909174046"
  }
}
~~~

No we have to configure typescript itself. We can also add a new item, using Visual Studio to create a TyoeScript configuration file. I would suggest not to use the default content, but the contents from the angular.io tutorial.

~~~ json
{
  "compileOnSave": true,
  "compilerOptions": {
    "target": "es5",
    "module": "commonjs",
    "moduleResolution": "node",
    "sourceMap": true,
    "emitDecoratorMetadata": true,
    "experimentalDecorators": true,
    "removeComments": false,
    "noImplicitAny": false
  },
  "exclude": [
    "node_modules"
  ]
}
~~~

The only things I did with this file, is to add the "compileOnSave" flag and to exclude the "node_modules" folder from the TypeScript build, because we don't need to build containing the TypeScript files and because we moved the needed JavaScripts to `./wwwroot/libs`.

> If you use Git or any other source code repository, you should ignore the files generated  out of our TypeScript files. In case of Git, I simply add another .gitignore to the `./wwwroot/app` folder
> ~~~ text
> #remove generated files
> *.js
> *.map
> ~~~
> We do this becasue the JavaScript files are only relevant to run the applicaiton and should be created automatically in the development environment or on a build server, befor deploying the app. 

## The first app

That is all to prepare a ASP.NET Core project in Visual Studio 2015. Let's start to create the Angular app. The first step is to create a `index.html` in the folder `wwwroot`:

~~~ html
<html>
<head>
    <title>Angular QuickStart</title>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <link rel="stylesheet" href="styles.css">
    <!-- 1. Load libraries -->
    <!-- Polyfill(s) for older browsers -->
    <script src="libs/core-js/shim.min.js"></script>
    <script src="libs/zone.js/zone.js"></script>
    <script src="libs/reflect-metadata/Reflect.js"></script>
    <script src="libs/systemjs/system.src.js"></script>
    <!-- 2. Configure SystemJS -->
    <script src="systemjs.config.js"></script>
    <script>
        System.import('app').catch(function (err) { console.error(err); });
    </script>
</head>
<!-- 3. Display the application -->
<body>
    <my-app>Loading...</my-app>
</body>
</html>
~~~

As you can see, we load almost all JavaScript files from the libs folder. Except a `systemjs.config.js`. This file is needed to configure Angular2, to define which module is needed, where to find dependencies an so on. Create a new JavaScript file, call it `systemjs.config.js` and paste the following content into it:

~~~ js
/**
 * System configuration for Angular samples
 * Adjust as necessary for your application needs.
 */
(function (global) {
    System.config({
        paths: {
            // paths serve as alias
            'npm:': 'libs/'
        },
        // map tells the System loader where to look for things
        map: {
            // our app is within the app folder
            app: 'app',
            // angular bundles
            '@angular/core': 'npm:@angular/core/bundles/core.umd.js',
            '@angular/common': 'npm:@angular/common/bundles/common.umd.js',
            '@angular/compiler': 'npm:@angular/compiler/bundles/compiler.umd.js',
            '@angular/platform-browser': 'npm:@angular/platform-browser/bundles/platform-browser.umd.js',
            '@angular/platform-browser-dynamic': 'npm:@angular/platform-browser-dynamic/bundles/platform-browser-dynamic.umd.js',
            '@angular/http': 'npm:@angular/http/bundles/http.umd.js',
            '@angular/router': 'npm:@angular/router/bundles/router.umd.js',
            '@angular/forms': 'npm:@angular/forms/bundles/forms.umd.js',
            // other libraries
            'rxjs': 'npm:rxjs',
            'angular-in-memory-web-api': 'npm:angular-in-memory-web-api',
        },
        // packages tells the System loader how to load when no filename and/or no extension
        packages: {
            app: {
                main: './main.js',
                defaultExtension: 'js'
            },
            rxjs: {
                defaultExtension: 'js'
            },
            'angular-in-memory-web-api': {
                main: './index.js',
                defaultExtension: 'js'
            }
        }
    });
})(this);
~~~

This file also defines a main entry point which is a main.js. This file is the transpiled TypeScript file `main.ts` we need to create in the next step. The `main.ts` bootstraps our Angular2 app:

~~~ typescript
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { AppModule } from './app.module';

const platform = platformBrowserDynamic();

platform.bootstrapModule(AppModule);
~~~

Since Angular2 RC6, there is an app-Module needed, which should be placed inside an `app.module.ts` file:

~~~ typescript
import { NgModule }      from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppComponent }   from './app.component';

@NgModule({
    imports:      [ BrowserModule ],
    declarations: [ AppComponent ],
    bootstrap:    [ AppComponent ]
})
export class AppModule { }
~~~

We also need to create our first Angular2 component. Create a TypeScript file with the name app.component.ts inside the app folder:

~~~ typescript
import { Component } from '@angular/core';

@Component({
    selector: 'my-app',
    template: '<h1>My First Angular App</h1>'
})
export class AppComponent { }
~~~

If all works fine, Visual Studio should have created a JavaScript file for each TypeScript file. Also the build should run. Pressing F5 should start the Application and a Browser should open. 

A short moment the `Loading...` is visible in the browser. After the app is initialized and all the Angular2 magic happened, you'll see the contents of the template defined in the app.component.ts.

Checkout the working project on GitHub: [https://github.com/JuergenGutsch/angular2-aspnetcore-vs](https://github.com/JuergenGutsch/angular2-aspnetcore-vs)

## Conclusion
I propose to use VisualStudio just for small single page applications, because it gets slower the more dynamic files need to be handled. ASP.NET Core is pretty cool to handle dynamically generated files, but Visual Studio still is not. VS tries to track and manage all the files inside the project, which slows down a lot. One solution is to disable source control in Visual Studio and use an external tool to manage the sources. 

Another - even better - solution is not to use Visual Studio for front-end development. In a new project, I propose to separate front-end and back-end development and to use Visual Studio Code for the front-end development or even both. You need to learn a few things about NPM, Gulp and you need to use a console in this case, but web development will be a lot faster and a lot more lightweight with this approach. In one of the next posts, I'll show how I currently work with Angular2.
