---
layout: post
title: "ASP.NET Hack Advent Post 10: Wasmtime"
teaser: "This is the tenth post of the ASP.NET Hack Advent. Until December 24th I'm going to post a link to a good community resource per day and a few lines about it."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- ASP.NET Hack Advent
- Blogs
---

![]({{site.baseurl}}/img/advent/advent.jpg)

WebAssembly is pretty popular this time for .NET Developers. With Blazor we have the possibility to run .NET assemblies inside the WebAssembly inside a browser.

But did you know that you can run WebAssembly outside the web and that you can run WebAssembly code without a browser? This can be done with the open-source, and cross-platform application runtime called Wasmtime. With Wasmtime you are able to load and execute WebAssemby code directly from your program. Wasmtime is programmed and maintained by the [Bytecode Alliance](https://bytecodealliance.org/)

> The Bytecode Alliance is an open source community dedicated to creating secure new software foundations, building on standards such as [WebAssembly](https://webassembly.org/) and [WebAssembly System Interface (WASI)](https://wasi.dev/).

Website: [https://wasmtime.dev/](https://wasmtime.dev/)

GitHub: [https://github.com/bytecodealliance/wasmtime/](https://github.com/bytecodealliance/wasmtime/)

I wouldn't write about it, if it wouldn't be somehow related to .NET Core. The Bytecode Alliance just added an preview of an API for .NET Core. That means that you now can execute WebAssembly code from your .NET Core application. For more details see this blog post by [Peter Huene](https://hacks.mozilla.org/author/phuenemozilla-com/):

[https://hacks.mozilla.org/2019/12/using-webassembly-from-dotnet-with-wasmtime/](https://hacks.mozilla.org/2019/12/using-webassembly-from-dotnet-with-wasmtime/)

He wrote a pretty detailed blog post about Wasmtime and how to use it within a .NET Core application. Also the Bytecode Alliance added a .NET Core sample and created a NuGet package:

[https://github.com/bytecodealliance/wasmtime-demos/tree/master/dotnet](https://github.com/bytecodealliance/wasmtime-demos/tree/master/dotnet)

[https://www.nuget.org/packages/Wasmtime](https://www.nuget.org/packages/Wasmtime)

So Wasmtime is the opposite of Blazor. Instead of running .NET Code inside the WebAssembly, you are now also able to run WebAssembly in .NET Core.