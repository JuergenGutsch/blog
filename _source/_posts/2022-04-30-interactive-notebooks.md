---
layout: post
title: "Interactive Notebooks with C#"
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

I already wrote about the .NET interactive Notebook two years ago, in July 2020. 2020-07-22-dotnet-notebooks.md I wrote about how to install it and run it with .NET und C# in the browser as well as in VSCode.

Back then it needed some manual steps to get it running. Because I saw a talk about it, I tried it again and it's now easily set up in VS Code by just installing the `Jupyter` add-in. I also installed the `Jupyter Keymap` and the `Jupyter Notebook Renderers` add-ins. The later helps to render graphical output like charts and diagrams in SVG, PNG, JPG, GIF and Plotly and other formats.

In this post I want to show the different output types.

To learn how to install it, read the post from 2020 or just install the add-in in VSCode.

display()

The easiest way to display data is to use the display() function. Tis function can write out a list of complex data into a human readable table.

While console.write just writes out the .NET type, the display method contains formatters that can write out the actual data in a formatted way. The default formatter is writing a table:

![image-20220502171034247](C:\Users\webma\AppData\Roaming\Typora\typora-user-images\image-20220502171034247.png)

What I now like to figure out is a way to tell the display method yo use a different formatter. Or to find a different method to display data in different formats, like charts, lists, etc.



