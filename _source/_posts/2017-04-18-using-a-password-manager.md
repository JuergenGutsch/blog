---
layout: post
title: "Using a password manager"
teaser: "Since years, I'm using a password manager to store and manage all my credentials for all the accounts I use. The usage of such a tool is pretty common for me and it is a normal flow for me to create a new entry in the password manager first before I create the actual account. But there are still people out there, who don't use any tool to store and manage their credentials."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- Password Manager
- KeePass
---

Since years, I'm using a password manager to store and manage all my credentials for all the accounts I use. The usage of such a tool is pretty common for me and it is a normal flow for me to create a new entry in the password manager first before I create the actual account.

I am always amazed that there are still people, who don't use any tool to store and manage their credentials.  They use the same password or the same bunch of passwords everywhere. This is dangerous and most of them already know about that. Maybe they are to lazy to spend one or two ours to think about the benefits and to search for the right tool or they simply don't know about such tools

For me the key benefits are pretty clear: 

* I don't need to remember all the passwords.
* I don't need to think about secure passwords while creating new one
* I just need to remember one single passphrase

Longer passwords are more secure than short ones, even if the short ones include special characters, numbers and upper case characters. But longer passwords are pretty hard to remember. This is why [Edward Snowden proposed to use passphrases instead of passwords](https://www.youtube.com/watch?v=yzGzB-yYKcc). Using passphrases like this, are easy to remember. But would it really makes sense to create a different pass-phrase for every credentials you use? I don't think so. 

![]({{ site.baseurl }}/img/keepass/login.png)

I just created a single pass-phrase which is used with the password manager and I use the password manager to generate the long passwords for me. Most of the generated password are like this:

~~~ tex
I4:rs/pQO8Ir/tSk*`1_KSG"~to-xMI/Gf;bNP7Qi3s8RuqIzl0r=-JL727!cgwq
~~~

![]({{ site.baseurl }}/img/keepass/generator.png)

The tool I use is KeePass 2, which is available on almost all devices and OS I use. As small desktop app on Windows, Mac and Linux as an app on Windows phones and Android. (Probably on iOS too, but I don't use such a device). KeePass stores the passwords on a encrypted file. This file could be stored on a USB drive or on a shared folder in the cloud. I use the cloud way to share the file to every device. The cloud store itself is secured with a password I don't know and which is stored in that KeePass file.

> What the F***, you really put the key to lock your house into a safe which is inside your house? 
> Yes!

The KeyPass file is synced offline and offline accessible on all devices. If the file changes it will be synched to all devices. I'm still able to access all credentials.

With the most password mangers, you are able to copy the username and the password, using shortcuts to the login forms. Some of them are able to automatically fill in the credentials. KeePass uses the clipboard, but deletes the values from the clipboard after a couple of seconds. Like this, no one else can access or reuse the credentials within the clipboard.

There are many more password manager out there. But KeePass - with the encrypted file - works best for me. I'm responsible to store the file on a save location and I can do that wherever I want. 

## Summary

Use a password manager! Spend the time to try the tools and choose the tools that fits you best. It is important! Once you use a password manager, you never want to work without one. It will make you live easier and hopefully more secure.