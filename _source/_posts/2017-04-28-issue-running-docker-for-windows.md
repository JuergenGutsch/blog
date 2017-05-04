---
layout: post
title: "Error while starting Docker for Windows"
teaser: "The last couple of months I wanted to play around with Docker for Windows. It worked just twice. Once at the first try for just one or two weeks. Then I got an error, when Docker tries to initialize right after Windows starts."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- Docker
- Errors
---

The last couple of months I wanted to play around with Docker for Windows. It worked just twice. Once at the first try for just one or two weeks. Then I got an error, when Docker tries to initialize right after Windows starts. After I reinstalled Docker for Windows it runs the second time for a two or three weeks. I tried to reinstall all that stuff but I didn't get it running again on my machine.

![]({{ site.baseurl }}/img/docker-for-windows/object-reference-not-set.png)

The error shown on this dialog is not really meaningful:

> Object reference not set to an instance of an object...

Even the log didn't really help:

~~~ text
Version: 17.03.1-ce-win5 (10743)
Channel: stable
Sha1: b18e2a50ccf296bcd637b330c0ca9faaab9d790c
Started on: 2017/04/28 21:49:37.965
Resources: C:\Program Files\Docker\Docker\Resources
OS: Windows 10 Pro
Edition: Professional
Id: 1607
Build: 14393
BuildLabName: 14393.1066.amd64fre.rs1_release_sec.170327-1835
File: C:\Users\Juergen\AppData\Local\Docker\log.txt
CommandLine: "Docker for Windows.exe"
You can send feedback, including this log file, at https://github.com/docker/for-win/issues
[21:49:38.182][GUI            ][Info   ] Starting...
[21:49:38.669][GUI            ][Error  ] Object reference not set to an instance of an object.
[21:49:45.081][ErrorReportWindow][Info   ] Open logs
~~~



Today I found some time to search for a solution and fortunately I'm not the only one who faced this error. I found an issue on GitHub which described exactly this behavior: [https://github.com/docker/for-win/issues/464#issuecomment-277700901](https://github.com/docker/for-win/issues/464#issuecomment-277700901)

**The solution** is to delete all the files inside of that folder:

~~~ text
C:\Users\<UserName>\AppData\Roaming\Docker\
~~~

Now I just needed to restart docket for Windows, by calling the `Docker for Windows.exe` in `C:\Program Files\Docker\Docker\`

![]({{ site.baseurl }}/img/docker-for-windows/docker-for-windows.png)

Finally I can continue playing around with Docker :)  