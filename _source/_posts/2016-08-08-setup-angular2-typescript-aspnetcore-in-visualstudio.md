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
  "name": "dotnetpro-ecollector",
  "version": "1.0.0",
  "scripts": {
    "start": "tsc && concurrently \"npm run tsc:w\" \"npm run lite\" ",
    "lite": "lite-server",
    "postinstall": "typings install && gulp restore",
    "tsc": "tsc",
    "tsc:w": "tsc -w",
    "typings": "typings"
  },
  "license": "ISC",
  "dependencies": {
    "@angular/common": "2.0.0-rc.4",
    "@angular/compiler": "2.0.0-rc.4",
    "@angular/core": "2.0.0-rc.4",
    "@angular/forms": "0.2.0",
    "@angular/http": "2.0.0-rc.4",
    "@angular/platform-browser": "2.0.0-rc.4",
    "@angular/platform-browser-dynamic": "2.0.0-rc.4",
    "@angular/router": "3.0.0-beta.1",
    "@angular/router-deprecated": "2.0.0-rc.2",
    "@angular/upgrade": "2.0.0-rc.4",
    "systemjs": "0.19.27",
    "core-js": "^2.4.0",
    "reflect-metadata": "^0.1.3",
    "rxjs": "5.0.0-beta.6",
    "zone.js": "^0.6.12",
    "angular2-in-memory-web-api": "0.0.14",
    "es6-promise": "^3.1.2",
    "es6-shim": "^0.35.0",
    "jquery": "^2.2.4",
    "bootstrap": "^3.3.6"
  },
  "devDependencies": {
    "gulp": "^3.9.1",
    "concurrently": "^2.0.0",
    "lite-server": "^2.2.0",
    "typescript": "^1.8.10",
    "typings": "^1.0.4"
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

gulp.task('default', function () {
    // place code for your default task here
});

gulp.task('restore', function() {
    gulp.src([
        'node_modules/@angular/**/*.js',
        'node_modules/angular2-in-memory-web-api/*.js',
        'node_modules/rxjs/**/*.js',
        'node_modules/systemjs/dist/*.js',
        'node_modules/zone.js/dist/*.js',
        'node_modules/core-js/client/*.js',
        'node_modules/reflect-metadata/reflect.js',
        'node_modules/jquery/dist/*.js',
        'node_modules/bootstrap/dist/**/*.*'
    ]).pipe(gulp.dest('./wwwroot/libs'));
});
~~~

The task restore, copies all the needed files to the Folder ./wwwroot/libs

TypeScript needs some type definitions to get the types and API definitions of the libraries, which are not written in TypeScript or not available in TypeScript. To load this, we use another tool, called "typings". This is already installed with NPM. This tool is a package manager for type definition files. We need to configure this tool with a `typings.config`

~~~ json
{
  "globalDependencies": {
    "es6-shim": "registry:dt/es6-shim#0.31.2+20160317120654",
    "jquery": "registry:dt/jquery#1.10.0+20160417213236"
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

That is all to prepare a ASP.NET Core project in Visual Studio 2015. Let's start to create the Angular app. The first step is to create a index.html in the folder `wwwroot`:

~~~ html
<html>
<head>
    <title>dotnetpro eCollector</title>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <link rel="stylesheet" href="css/styles.css">
    <!-- 1. Load libraries -->
    <!-- Polyfill(s) for older browsers -->
    <script src="libs/shim.min.js"></script>
    <script src="libs/zone.js"></script>
    <script src="libs/Reflect.js"></script>
    <script src="libs/system.src.js"></script>
    <!-- 2. Configure SystemJS -->
    <script src="systemjs.config.js"></script>
    <script>
        System.import('app')
            .catch(function (err) { console.error(err); });
    </script>
</head>
<!-- 3. Display the application -->
<body>
    <my-app>Loading...</my-app>
</body>
</html>
~~~

As you can see, we load almost all JavaScript files from the libs folder. Except a systemjs.config.js. This file is needed to configure Angular2, to define which module is needed, where to find dependencies an so on. Create a new JavaScript file, call it systemjs.config.js and paste the following content into it:

~~~ js
(function (global) {

    // map tells the System loader where to look for things
    var map = {
        'app': 'app', 
        'rxjs': 'lib/rxjs',
        '@angular': 'lib/@angular'
    };

    // packages tells the System loader how to load when no filename and/or no extension
    var packages = {
        'app': { main: 'main.js', defaultExtension: 'js' },
        'rxjs': { defaultExtension: 'js' },
        'angular2-in-memory-web-api': { defaultExtension: 'js' },
    };

    var packageNames = [
      '@angular/common',
      '@angular/compiler',
      '@angular/core',
      '@angular/http',
      '@angular/platform-browser',
      '@angular/platform-browser-dynamic',
      '@angular/router',
      '@angular/router-deprecated',
      '@angular/upgrade'
    ];

    packageNames.forEach(function (pkgName) {
        packages[pkgName] = { main: 'index.js', defaultExtension: 'js' };
    });

    var config = {
        map: map,
        packages: packages
    }

    // filterSystemConfig - index.html's chance to modify config before we register it.
    if (global.filterSystemConfig) { global.filterSystemConfig(config); }

    System.config(config);

})(this);

~~~

This file also defines a main entry point which is a main.js. This file is the transpiled TypeScript file main.ts we need to create in the next step. The main.ts bootstraps our Angular2 app:

~~~ typescript
import { bootstrap } from '@angular/platform-browser-dynamic';
import { AppComponent } from './app.component';

bootstrap(AppComponent);
~~~

We also need to create our first Angular2 component. Create a TypeScript file with the name app.component.ts inside the app folder:

~~~ typescript
import { Component } from '@angular/core';

@Component({
  selector: 'my-app',
  template: '<h1>My first Angular App in Visual Studio</h1>'
})
export class AppComponent { }
~~~

If all works fine, Visual Studio should have created a JavaScript file for each TypeScript file. Also the build should run. Pressing F5 should start the Application and a Browser should open. 

A short moment the `Loading...` is visible in the browser. After the app is initialized and all the Angular2 magic happened, you'll see the contents of the template defined in the app.component.ts.

## Conclusion
I propose to use VisualStudio just for small single page applications, because it gets slower the more dynamic files need to be handled. ASP.NET Core is pretty cool to handle dynamically generated files, but Visual Studio still is not. VS tries to track and manage all the files inside the project, which slows down a lot. One solution is to disable source control in Visual Studio and use an external tool to manage the sources. 
 
Another - even better - solution is not to use Visual Studio for front-end development. In a new project, I propose to separate front-end and back-end development and to use Visual Studio Code for the front-end development or even both. You need to learn a few things about NPM, Gulp and you need to use a console in this case, but web development will be a lot faster and a lot more lightweight with this approach. In one of the next posts, I'll show how I currently work with Angular2.
