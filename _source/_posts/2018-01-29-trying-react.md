---
layout: post
title: "Trying React the first time"
teaser: "The last two years I worked a lot with Angular. I learned a lot and I also wrote some blog posts about it. While I worked with Angular, I always had React in mind and wanted to learn about it. But I never head the time or a real reason to look at it. I still have no reason to try it, but a little bit of time left. So why not? This post is just a small overview of what I learned during the setup and in the very first tries."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- React
- Babel
- Webpack
- JavaScript
---

The last two years I worked a lot with Angular. I learned a lot and I also wrote some blog posts about it. While I worked with Angular, I always had [React](https://reactjs.org/) in mind and wanted to learn about that. But I never head the time or a real reason to look at it. I still have no reason to try it, but a little bit of time left. So why not? :-)

This post is just a small overview of what I learned during the setup and in the very first tries.

## The Goal

It is not only about developing using React, later I will also see how React works with ASP.NET and ASP.NET Core and how it behaves in Visual Studio. I also want to try the different benefits (compared to Angular) I heard and read about React:

* It is not a huge framework like Angular but just a library 
* Because It's a library, it should be easy to extend existing web-apps.
* You should be more free to use different libraries, since there is not all the stuff built in.

## Setup

My  first ideas was to follow the tutorials on [https://reactjs.org/](https://reactjs.org/). Using this tutorial some other tools came along and some hidden configuration happened. The worst thing from my perspective is that I need to use a package manager to install another package manager to load the packages. Yarn was installed using NPM and was used. Webpack was installed and used in some way, but there was no configuration, no hint about it. This tutorial uses the create-react-app starter kid. This thing hides many stuff.

### Project setup

What I like while working with Angular is a really transparent way of using it and working with it. Because of this I searched for a pretty simple tutorial to setup React in a simple, clean and lightweight way. I found this great tutorial by Robin Wieruch: [https://www.robinwieruch.de/minimal-react-Webpack-babel-setup/](https://www.robinwieruch.de/minimal-react-Webpack-babel-setup/)

This Setup uses NPM to get the packages. It uses Webpack to bundle the needed Javascript, Babel is integrated in to Webpack to transpile the JavaScripts from ES6 to more browser compatible JavaScript.

I also use the Webpack-dev-server to run the React app during development. Also react-hot-loader is used to speed up the development time a little bit. The main difference to Angular development is the usage of ES6 based JavaScript and Babel instead of using Typescript. It should also work with typescript, but it doesn't really seem to matter, because they are pretty similar. I'll try using ES6 to see how it works. The only thing I possibly will miss is the type checking.

As you can see, there is not really a difference to Typescript yet, only the JSX thing takes getting used to: 

~~~ javascript
// index.js
import React from 'react';
import ReactDOM from 'react-dom';

import Layout from './components/Layout';

const app = document.getElementById('app');

ReactDOM.render(<Layout/>, app);

module.hot.accept();
~~~

I can also uses classes in JavaScript:

~~~ javascript
// Layout.js
import React from 'react';
import Header from './Header';
import Footer from './Footer';

export default class Layout extends React.Component {
    render() {
        return (
            <div>
                <Header/>
                <Footer/>
            </div>
        );
    }
}
~~~

With this setup, I believe I can easily continue to play around with React.

### Visual Studio Code

To support ES6, React and JSX in VSCode I installed some extensions for it:

* **Babel JavaScript** by Michael McDermott
  * Syntax-Highlighting for modern JavaScripts
* **ESLint** by Dirk Baeumer
  * To lint the modern JavaScripts


* **JavaScript (ES6) code snippets** by Charalampos Karypidis
* **Reactjs code snippets** by Charalampos Karypidis

### Webpack

Webpack is configured to build a `bundle.js` to thde `./dist` folder. This folder is also the root folder for the Webpack dev server. So it will serve all the files from within this folder.

To start building and running the app, there is a start script added to the `packages.config`

~~~ json
"start": "Webpack-dev-server --progress --colors --config ./Webpack.config.js",
~~~

With this I can easily call `npm start` from a console or from the terminal inside VSCode. The Webpack dev server will rebuild the codes and reload the app in the browser, if a code file changes.

~~~ javascript
const webpack = require('webpack');

module.exports = {
    entry: [
        'react-hot-loader/patch',
        './src/index.js'
    ],
    module: {
        rules: [{
            test: /\.(js|jsx)$/,
            exclude: /node_modules/,
            use: ['babel-loader']
        }]
    },
    resolve: {
        extensions: ['*', '.js', '.jsx']
    },
    output: {
        path: __dirname + '/dist',
        publicPath: '/',
        filename: 'bundle.js'
    },
    plugins: [
      new webpack.HotModuleReplacementPlugin()
    ],
    devServer: {
      contentBase: './dist',
      hot: true
    }
};
~~~

### React Developer Tools

For Chrome and Firefox there are add-ins available to inspect and debug React apps in the browser. For Chrome I installed the **React Developer Tools**, which is really useful to see the component hierarchy:

![]({{site.baseurl}}/img/trying-react/react-dev-tools.PNG)

## Hosting the app

The react app is hosted in a `index.html`, which is stored inside the `./dist` folder. It references the `bundle.js`. The React process starts in the `index.js`. React putts the App inside a `div` with the Id `app` (as you can see in the first code snippet in this post.)

~~~ html
<!DOCTYPE html>
<html>
  <head>
      <title>The Minimal React Webpack Babel Setup</title>
  </head>
  <body>
    <div id="app"></div>
    <script src="bundle.js"></script>
  </body>
</html>
~~~

The `index.js` import the `Layout.js`. Here a basic layout is defined, by adding a Header and a Footer component, which are also imported from other components. 

~~~ javascript
// Header.js
import React from 'react';
import ReactDOM from 'react-dom';

export default class Header extends React.Component {
    constructor(props) {
        super(props);
        this.title = 'Header';
    }
    render() {
        return (
            <header>
                <h1>{this.title}</h1>
            </header>
        );
    }
}
~~~

~~~javascript
// Footer.js
import React from 'react';
import ReactDOM from 'react-dom';

export default class Footer extends React.Component {
    constructor(props) {
        super(props);
        this.title = 'Footer';
    }
    render() {
        return (
            <footer>
                <h1>{this.title}</h1>
            </footer>
        );
    }
}
~~~

The resulting HTML looks like this:

~~~ html
<!DOCTYPE html>
<html>
  <head>
    <title>The Minimal React Webpack Babel Setup</title>
  </head>
  <body>
    <div id="app">
      <div>
        <header>
          <h1>Header</h1>
        </header>
        <footer>
          <h1>Footer</h1>
        </footer>
      </div>
    </div>
    <script src="bundle.js"></script>
  </body>
</html>
~~~

## Conclusion

My current impression is that React is much more fast on startup than Angular. This is just a kind of a Hello world app, but even for such an app Angular need some time to start a few lines of code. Maybe that changes if the App gets bigger. But I'm sure it keeps to be fast, because of less overhead in the framework.

The setup was easy and works on the first try. The experience in Angular helped a lot here. I already know the tools. Anyway, Robins tutorial is pretty clear, simple and easy to read: [https://www.robinwieruch.de/minimal-react-Webpack-babel-setup/](https://www.robinwieruch.de/minimal-react-Webpack-babel-setup/)

To get started with React, there's also a nice Video series on YouTube, which tells you about the really basics and how to get started creating components and adding the dynamic stuff to the components: [https://www.youtube.com/watch?v=MhkGQAoc7bc&list=PLoYCgNOIyGABj2GQSlDRjgvXtqfDxKm5b](https://www.youtube.com/watch?v=MhkGQAoc7bc&list=PLoYCgNOIyGABj2GQSlDRjgvXtqfDxKm5b)
