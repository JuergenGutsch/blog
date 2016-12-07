--- 
layout: post
title: "Contributing to OSS projects on GitHub using fork and upstream"
teaser: "This blog post is a simple guideline on how to contribute to OSS projects hosted on GitHub. This Blog post is a 'fork' of Damien Bowdens post about contributing to an OSS project, but uses a different tool to work with Git."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- ASP.NET
- OSS
- Community
---

## Intro
Some days ago, [Damien Bowden](https://damienbod.com/) wrote a pretty [cool post about, how to contribute to an open source software project](https://damienbod.com/2016/11/25/contributing-to-oss-projects-on-github-using-fork-and-upstreams/) hosted on GitHub, like the [AspLabs](https://github.com/aspnet/AspLabs). He uses Git Extensions in his great and pretty detailed post. Also a nice fact about this post is, that he uses a AspLabs as the demo project. Because we both worked on that on the hackathon at the MVP Summit 2016, together with [Glen Condron](https://twitter.com/condrong) and [Andrew Stanton-Nurse](https://twitter.com/anurse) from the ASP.NET Team.

At that Hackathon we worked on the HealthChecks for ASP.NET Core. The HealthChecks can be used to check the heath state of dependent sub systems in an e. g. micro service environment, or in any other environment where you need to know the health of depending systems. A depending systems could be a SQL Server, an Azure Storage service, the Hard drive, a Web-/REST-Service, or anything else you need to run your application. Using the HealthChecks you are able to do something, if a service is not available or unhealthy.

> BTW: The HealthChecks are mentioned by [Damian Edwards](https://twitter.com/DamianEdwards) in this ASP.NET Community Standup: [https://youtu.be/hjwT0av9gzU?list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF](https://youtu.be/hjwT0av9gzU?list=PL0M0zPgJ3HSftTAAHttA3JQU4vOjXFquF)

Because Damien Bowden also worked on that project, my idea was to do the same post. So asked him to "fork" the original post, but use the Git CLI in the console instead of Git Extensions. Because this is a fork, some original words are used in this post ;)

Why using the console? Because I'm a console junkie since a few years and from my perspective, no Git UI is as good as the simple and clean Git CLI :) Anyway, feel free to use the tool that fits your needs. Maybe someone will write the same post using SourceTree or using the Visual Studio Git integration. ;)

As a result this post is a also simple guideline on how you could contribute to OSS projects hosted on GitHub using fork and upstream. This is even not the only way to do it. In this demo I'm going to use the console and the basic git commands. As same as Damien did, I'll also use the [aspnet/AspLabs](https://github.com/aspnet/AspLabs) project from Microsoft as the target Repository.

> True words by Damien:
> So you have something to contribute, cool, that’s the hard part.

## Setup your fork

Before you can make your contribution, you need to create a fork of the repository where you want to make your contribution. Open the project on GitHub, and click the "Fork" button in the top right corner.

![]({{ site.baseurl }}/img/oss-contrib/ossfork.png)

Now clone your forked repository. Click the "Clone and download" button and copy the clone URL to the clipboard.

![]({{ site.baseurl }}/img/oss-contrib/ossclone.png)

Open a console and `cd` to the location where you want to place your projects. It is c:\git\ in my case. Write `git clone` followed by the URL to the repository and press enter.

![]({{ site.baseurl }}/img/oss-contrib/ossclonegit.png)

Now you have a local master branch and also a server master branch (remote) of your forked repository. The next step is to configure the remote upstream branch to the original repository. This is required to synchronize with the parent repository, as you might not be the only person contributing to the repository. This is done by adding another `remote` to that git repository. On GitHub copy the clone URL the the original repository aspnet/AspLabs. Go back to the console and type `git remote add upstream` followed by the URL of the original repository:

![]({{ site.baseurl }}/img/oss-contrib/ossupstream.png)

To check if anything is done right, type `git remote -v`, to see all existing remotes. It should look like this:

![]({{ site.baseurl }}/img/oss-contrib/ossremotes.png)

Now you can pull from the upstream repository. You pull the latest changes from the upstream/master branch to your local master branch. Due to this you should NEVER work on your master branch. Then you can also configure your git to rebase the local master with the upstream master if preferred.

## Start working on the code

Once you have pulled from the upstream, you can push to your remote master, i. e. the forked master. Just to mention it again, NEVER WORK ON YOUR LOCAL FORKED MASTER, and you will save yourself hassle.

Now you’re ready to work. Create a new branch. A good recommendation is to use the following pattern for naming:

    <gitHub username>/<reason-for-the-branch>

Here’s an example:

    JuergenGutsch/add-healthcheck-groups

By using your GitHub username, it makes it easier for the person reviewing the pull request.

To create that branch in the console, use the `git checkout -b` command followed by the branch name. This creates the branch and checks it out immediately:

![]({{ site.baseurl }}/img/oss-contrib/ossfeaturebranch.png)

## Creating pull requests

When your work is finished on the branch, you need to push your branch to your remote repository by calling `git push` Now you are ready to create a pull request. Go to your repository on GitHub, select your branch and and click on the "Compare & pull request" button:

![]({{ site.baseurl }}/img/oss-contrib/ossprcreate.png)

Check if the working branch and the target branch are fine. The target branch is usually the master of the upstream repo.

> NOTE: If your branch was created from an older master commit than the actual master on the parent, you need to pull from the upstream and rebase your branch to the latest commit. This is easy as you do not work on the local master. Or update your local master with the latest changes from the upstream, push it to your remote and merge your local master into your feature branch.

![]({{ site.baseurl }}/img/oss-contrib/ossgitpush.png)

If you are contributing to any Microsoft repository, you will need to [sign an electronic contribution license agreement](https://cla2.dotnetfoundation.org/) before you can contribute. This is pretty easy and done in a few minutes.

If you are working together with a maintainer of the repository, or your pull request is the result of an issue, you could add a comment with the GitHub name of the person that will review and merge, so that he or she will be notified that you are ready. They will receive a notification on GitHub as soon as you save the pull request.

Add a meaningful description. Tell the reviewer what they need to know about your changes. and save the pull request.

![]({{ site.baseurl }}/img/oss-contrib/ossprsave.png)

Now just wait and fix the issues as required. Once the pull request is merged, you need to pull from the upstream on your local forked repository and rebase if necessary to continue with you next pull request.

And who knows, you might even get a coin from Microsoft. ;)

<blockquote class="twitter-tweet" data-lang="de"><p lang="en" dir="ltr">I got a .NET challenge coin in recognition of my contributions to .NET <a href="https://twitter.com/hashtag/dotnet?src=hash">#dotnet</a> <a href="https://twitter.com/hashtag/AspNetCore?src=hash">#AspNetCore</a> <a href="https://twitter.com/hashtag/mvpsummit?src=hash">#mvpsummit</a> <a href="https://t.co/ikWXE6b8dD">pic.twitter.com/ikWXE6b8dD</a></p>&mdash; Ben Adams (@ben_a_adams) <a href="https://twitter.com/ben_a_adams/status/795816640067686400">8. November 2016</a></blockquote>
<script async src="//platform.twitter.com/widgets.js" charset="utf-8"></script>

## The console I use

I often get the question what type console I use. I have four consoles installed on my machine, in addition to the cmd.exe and PowerShell. I also installed the bash for Windows. But my favorite console is the [Cmder](http://cmder.net/), which is a pretty nice [ConEmu](https://conemu.github.io/) implementation. I like this console because it is easy to use, easy to customize and it has a nice color theme too.

## Thanks

Thanks to Andrew Stanton-Nurse for his tips. Thanks to Glen Condron for the reviews. thanks Damien Bowden for the original blog post ;)

I'd also be happy for tips from anyone on how to improve this guideline.
