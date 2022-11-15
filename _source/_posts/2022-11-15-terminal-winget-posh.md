---
layout: post
title: "Windows Terminal, PowerShell, oh-my-posh, and Winget "
teaser: "I'm thinking about changing the console setup I use for some development tasks on Windows. I was playing around with Windows Terminal, PowerShell, oh-my-posh, and Winget and I really like it."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- Terminal
- PowerShell
- Winget
---

I'm thinking about changing the console setup I use for some development tasks on Windows. The readers of this block already know that I'm a console guy. I'm using git and docker in the console only. I'm navigating my folders using the console. I even used the console to install, update or uninstall tools using Chocolatey (https://chocolatey.org/).

This post is not a tutorial on how to install and use the tools I'm going to mention here. It is just a small portrait of what I'm going to use. Follow the links to learn more about the tools.

## PowerShell and oh-my-posh

Actually, working in the console doesn't work for me with the regular `cmd.exe` and I completely understand why developers on Windows still prefer using windows based tools for git and docker, and so on. Because of that, I was using `cmder` (https://cmder.app/), a great terminal with useful Linux commands and great support for git. The git support not only integrates the git CLI, but it also shows the current branch in the prompt:

![Cmder in action]({{site.baseurl}}/img/terminal/cmder.png)

The latter is a great help when working with git; I missed that in the other terminals. Commander also supports adding different shells like git bash, WSL, or PowerShell but I used the cmd shell which has been enriched with a lot more useful commands. This worked great for me.

For a couple of weeks, I'm playing around with the Windows Terminal a little more. The reason why I looked into the Windows Terminal is, that I like the more lightweight settings. 

The Windows Terminal ([download it from the windows store](https://apps.microsoft.com/store/detail/windows-terminal/9N0DX20HK701)) and `oh-my-posh` (https://ohmyposh.dev/) are out for a while and I followed [Scott Handelman's blog posts about it](https://www.hanselman.com/blog/a-nightscout-segment-for-ohmyposh-shows-my-realtime-blood-sugar-readings-in-my-git-prompt) for a long time but wasn't able to get it running on my machine. Two weeks ago I [got some help](https://github.com/JanDeDobbeleer/oh-my-posh/discussions/2994) by [Jan De Dobbeleer](https://twitter.com/jandedobbeleer) to get it running. It just turned out that I had too many posh versions installed on my machine, and the path environment variable was messed up. After cleaning my system and reinstalling oh-my-posh on my machine by [following the installation guide](https://ohmyposh.dev/docs/installation/windows) it is working quite well:

![Terminal and posh in action]({{site.baseurl}}/img/terminal/terminal.png)

I still need to configure the prompt a little bit to match my needs 100% but the current theme is great for now and does more as `cmder` does. I'd like to display the latest tag of the current git repository and the currently used dotnet SDK version, but this will be another story.

## Windows Terminal

In the Windows Terminal, I configured oh-my-posh for both, the Windows PowerShell 5 and the new PowerShell 7 and set the PowerShell 7 as my default console. I also added configurations to use PowerShell 5, WSL (both Ubuntu 18 and Ubuntu 20), git bash, and the Azure Cloud Shell. I did almost the same with `cmder` but I like the way how it gets configured in Windows Terminal.

## Winget

Winget is basically an apt-get for windows and I like it.

As mentioned, Chocolatey is the tool I used to install the tools I need, like git, cmder, etc. I tried it for a while, winget was mentioned on Twitter (unfortunately I forgot the link). Actually, it is much better than Chocolatey because it uses the application registry used by windows, which means it can update and uninstall programs that have been installed without using winget.

Winget is the console version of installing and managing installed programs on Windows and it is natively installed on Windows 10 and Windows 11.

## Conclusion

So I'm going to change my setup from this ...

* cmder
  * cmd
  * chocolatey

... to that ... 

* Windows Terminal
  * Powershell7
  * oh-my-posh
  * Winget

... and it seems to work great for me.

Any other tools that I should have a look at? Just drop me a comment :-)
