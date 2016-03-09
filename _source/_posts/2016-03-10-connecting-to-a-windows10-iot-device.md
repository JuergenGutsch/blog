--- 
layout: post
title: "Connecting to a Windows 10 IoT device"
teaser: "While playing around with Windows 10 IoT on a Raspberry PI 2, I found different ways to connect to the device to setup and manage it. Some other tools, mentioned in the getting started tutorial seem not to work on my machine. This post shows, how I setup and manage my Windows IoT devices."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- Raspberry PI
- Windows 10 IoT
---

While playing around with Windows 10 IoT on a Raspberry PI 2, I found different ways to connect to the device to setup and manage it. Some other tools, mentioned in the getting started tutorial seem not to work on my machine. This post shows, how I setup and manage my Windows IoT devices.

## Setup the SD Card

In the getting started tutorial there is a tool mentioned called **Windows 10 IoT Core Dashboard**. This tool should show you all your running Windows 10 IoT devices in your network and with this tool you should be able to setup a new device. This tool looks almost like this (screenshot in German):

![](/img/winiot/setup.png)

This tool downloads the latest image of Windows 10 IoT and installed it on the SD Cards. Pretty useful and hopefully it doesn't download that image every time you need to setup a new SD Card ;) 

If you already downloaded and installed the latest Windows 10 IoT for the Raspberry PI (or even every other Board) on your machine, you are able to Setup the SD Card directly using. Using the Windows Explorer just go to `C:\Program Files (x86)\Microsoft IoT\` and start the `IoTCoreImageHelper.exe`. This is a small tool called **Windows IoT Core Image Helper** which uses the `dism.exe` to copy the FFU image to your SD card. 

If you like to use command line tools, you're able to use the dism.exe directly which is located in the `dism` folder under `C:\Program Files (x86)\Microsoft IoT\` ;)

![](/img/winiot/imagehelper.png)

## Setup the Raspberry PI

This is just about the hardware. Plug-in a screen, mouse, keyboard and network cable or alternatively a WiFi adapter. (I use the original Raspberry PI WiFi adapter). Put you SD card with the Windows 10 IoT image in the SD card slot and plugin the power cable to switch the device on.

Now you just need to follow the wizard to setup your device. Usually the device will find your network and you're able to connect to the device. In case of WiFi you need to enter the WiFi key to connect to your network.

## Connecting to the Raspberry PI

The already mentioned **Windows 10 IoT Core Dashboard** on your computer should find all the devices in your network, but it doesn't on my computer. 

![](/img/winiot/devices.png)

Maybe this happens also to you, or maybe this is really a problem on OSI Layer 8, as mentioned by [Hannes Preishuber](http://blog.ppedv.de/author/hannesp.aspx) in his always friendly manner ;)

<blockquote class="twitter-tweet" data-conversation="none" data-lang="en"><p lang="de" dir="ltr"><a href="https://twitter.com/sharpcms">@sharpcms</a> <a href="https://twitter.com/SCHWABENCODEcom">@SCHWABENCODEcom</a> bei uns läufts auf 2 Pcs mit 2 devices Ohne Prob. Fehler liegt im OSI Layer 8</p>&mdash; Hannes Preishuber (@HannesPreishub) <a href="https://twitter.com/HannesPreishub/status/707189454604214273">March 8, 2016</a></blockquote>
<script async src="//platform.twitter.com/widgets.js" charset="utf-8"></script>

Anyway, there is another option to see all your devices from your computer. Again, go to the folder `C:\Program Files (x86)\Microsoft IoT\` and start the `WindowsIoTCoreWatcher.exe`. This installs the **Windows IoT Core Watcher** which shows you all your devices and additionally the addresses, states and so on.

![](/img/winiot/iotwatcher.png)

A right click on a device enables you to copy the physical address, the IP address or to open the web dashboard on that device.

The web dashboard is one of the most important tools, where you can manage your apps, watch the performance, manage your network connections and many more.

Maybe you need to enter a user name and a password to connect to your device. This is initially set to `Administrator` and `p@ssw0rd`.

SSH is another option to connect to your device. I usually use Putty to connect connect via SSH: 

![](/img/winiot/putty.png)

An additional way is FTP. Using FileZilla it looks like this:

![](/img/winiot/ftp.png)

## Deploying an app using Visual Studio

You are able to deploy an already published app using the web dashboard of your device. Another easy way whyle you are developing your app, is to use Visual Studio 2015. This is pretty easy if you know the way to do it ;-)

Choose "ARM" as solution platform and Remote Machine as the target. The first time you choose the Remote Machine, the Remote Connections dialog will open. Type in the IP address of your PI and choose the authentication mode `Universal`. Click select and you are now able to deploy via F5 or via right click and `deploy` in the context menu of the current project.

![](/img/winiotcar/remoteconnections.png)

To change the Remote Machine settings, just go to the debug settings and reconfigure the settings. I had to change the configuration because I chose the wrong authentication at the first time I tried to deploy:

![](/img/winiotcar/debugsettings.png)

## Conclusion

The Windows 10 IoT Core Dashboard is useless for me, because it doesn't really work on my machine. And it doesn't really bother me why it doesn't work. Because there are some more  ways to connect and to deploy to your device. Hope this helps to get a short overview about the setup, about connecting and the deployment to your Windows 10 IoT device. 