---
layout: post
title: "Title"
teaser: "Description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Tests
- Tag
---

Since almost a year I do a lot of Python projects. Actually Python isn't that bad. Python and Flask to build web applications work almost similar to NodeJS and ExpressJS. Similarly to NodeJS, Python development is really great using Visual Studio Code. 

People who are used to use Python know Jupyter Notebooks to create interactive documentations. Interactive documentation means that the code snippets are executable and that you can use Python code to draw charts or to calculate and display data.

If I got it right, Jupyter Notebook was IPython in the past. Now Jupyter Notebook is a standalone project and the IPython project focuses on Python Interactive and Python kernels for Jupyter Notebook. 

The so called kernels extend Jupyter Notebook to execute a specific language. The Python kernel is default. You are able to install a lot more kernels. There are kernels for NodeJS and more.

Microsoft is also working on .NET Interactive and kernels for Jupyter Notebook.  You are now able to write interactive documentations in Jupyter Notebook using C#, F# and PowerShell.

In this blog post I'll try to show you how to install and to use it.

## Install Jupyter Notebook 

You need to have Python3 installed on your machine. The best way to install Python on Windows is to use [Chocolatey](https://chocolatey.org/):

```shell
choco install python
```

https://chocolatey.org/packages/python/3.8.4

If Python is installed you can install Jupyter Notebook using the Python package manager PIP:

```shell
pip install notebook
```

You now can use Jupyter by just type `jupyter notebook` in the console. This would start the Notebook with the default Python3 kernel. The following command shows the installed kernels:

```shell
jupyter kernelspec list
```
We'll see the Python3 kernel in the console output:

![](../img/dotnet-notebook/listkernels01.png)

## Install .NET Interactive

The goal is to have the .NET Interactive kernel running in Jupyter. To get this done you first need to install the latest build of .NET Interactive from MyGet:

```shell
dotnet tool install -g --add-source "https://dotnet.myget.org/F/dotnet-try/api/v3/index.json" Microsoft.dotnet-interactive
```

Or the latest version from NuGet:

```shell
dotnet tool install -g Microsoft.dotnet-interactive
```

If this is installed you can use `dotnet interactive` to install the Jupyter kernels:

```shell
dotnet interactive jupyter install
```

![](../img/dotnet-notebook/installonjupyter.png)

Let's see if the kernels are installed:

```
jupyter kernelspec list
```

![listkernels02](../img/dotnet-notebook/listkernels02.png)

That's it. We now have four different kernels installed.

## Run Jupyter Notebook

Let's run Jupyter by calling the next command. Be sure to navigate into a folder where your notebooks are or where you want to save your notebooks:

```shell
jupyter notebook
```

![startnotebook](../img/dotnet-notebook/startnotebook.png)

It now starts a webserver and opens a Browser. The current folder will be the working folder for the currently running Jupyter instance. I don't have any files in that folder. I now want to start playing around with a C# notebook:

![notebook01](../img/dotnet-notebook/notebook01.png)

Here we have the Python3 and the three new .NET notebook types available.

## Try .NET Interactive

Let's add some content and a code snippet. At first I added a Markdown cell. Cells are content elements the support specific content types. A Markdown cell is one type as well as a Code cell. The later executes a code snippet and shows the output underneath:

![notebook02](../img/dotnet-notebook/notebook02.png)

