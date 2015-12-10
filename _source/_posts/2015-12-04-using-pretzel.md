--- 
layout: post
title: "Using Pretzel"
teaser: "With this new blog I switched to a Jekyll like blogging system called Pretzel. Pretzel is Jekyll written in .NET and a lightweight, easy to use and funny blogging system. This blog post is about the pretzel setup I did to get this blog running."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- Pretzel
- Blog
---

The first time I played around with pretzel was more than a year ago. Since than I tried to find a new blog system what is flexible, lightweight, what supports offline editing and runs on my favorite hosing platform: On Microsoft Azure. I tried a lot of blog systems and CMS, like Ghost, Wordpress, Umbraco. I also tried to write an own Markdown based blog system. But at the end Pretzel is the system which matches my requirements at most.

Pretzel is a Jekyll like blogging system, which uses the same template engine and the same Markdown syntax. Pretzel adds some additional features like Azure support and a Razor template engine. I use the liquid engine which is also used in Jekyll. Almost like Jekyll it is creating a static website based on that template engine and the Markdown contents.

To work with Pretzel you need to use a console. PowerShell should also work, but I prefer to use cmder

To install Pretzel locally, you just need to use Chocolatey:

~~~ batch
choco install pretzel -y
~~~

> Just a view days ago they released a new version 0.3.0, which is not yet deployed to Chocolatey. I downloaded the latest release and copied the bits to the Chocolatey install folder, which is `C:\tools\pretzel\` in my case. This versions contains important big fixes.

After Pretzel is installed you can easily start baking a pretzel blog. Just create a working folder somewhere on your machine cd into that folder and type

~~~ batch
mkdir pretzelblog
cd prezelblog
pretzel create --azure
~~~

This command creates a new pretzel blog. That argument `--azure` adds support to bake/compile pretzel blogs directly on a Azure Webiste.

![]({{ site.baseurl }}/img/pretzel/create.png)

The contents of your folder should look like this:

![]({{ site.baseurl }}/img/pretzel/createfolder.png)

That's all to create a new blog. The blog sources, the posts and the templates are inside the `_source` folder:

![]({{ site.baseurl }}/img/pretzel/insidesource.png)

Now lets show you how it looks like in the browser. To start the blog locally you need to cd into the `_source` folder **taste** the Pretzel blog:

~~~ batch 
cd _source
pretzel taste --drafts
~~~

Add the argument `--drafts` to also see the drafted posts. The command `taste` starts a small web server listening to `localhost:8080` and opens the default browser where you can start using the blog.

If it's all fine you can **bake** your Pretzel blog

~~~ batch
pretzel bake
~~~

This command creates a folder called `_site` inside the inside the `_source` folder with the compiled blog inside. This is the baked Pretzel blog which is a static website, built with the templates and the posts and pages written in markdown. You can put the contents of this folder to any web server.

## Setup the deployment

The idea was to have the blog under Git source control and automatic deploy it to an Azure website. I use GitHub to host the repository and I linked that repository to an Azure website to automatic deploy it every time I push changes to the repository. In the first time I didn't use the argument `--azure` to create my blog, because the idea was to just push the compiled blog every time I change something or every time I create a new blog post. This means I only had the folder `_site` under source control. This works really great, because there is nothing special. Only the compiled static web was pushed and deployed.

But this wasn't really a good idea and I had some problems with this:

1. The Markdown contents and the templates are not under source control
2. I always need to bake the Pretzel blog before I can push.
3. Because the Markdown is not on GitHub pull requests are made on the compiled web and I need to merge on GitHub AND locally in the Markdown contents.

Using the argument `--azure` while creating the Pretzel blog adds a Visual Studio solution and a dummy project to the working folder. The Solution also contains a Visual Studio website project which points to the `_site` folder. The dummy project contains an empty class and a post build command which just bakes the Pretzel blog:

~~~ xml
<Target Name="AfterBuild">
	<Exec Command="pretzel.exe bake -d _source" />
</Target>
~~~

Every time I push changes to the repository the Azure website pulls the changes, compiles the dummy project, executes that command and than some Azure-Kudu-magic happens: Kudu copies the contents of the website project to the `wwwroot` folder of the Azure website. It magically does exactly what I want to have. If I call the Azure website within the browser I get the expected result.

In the current version 0.3.0 the argument `--azure` doesn't work completely correct, because it only copies the pretzel.exe to the working folder. Since they changed the output to deploy also some additional libraries, all the depended libraries also need to be copied to the root:
 
![]({{ site.baseurl }}/img/pretzel/shimcomplete.png)
 
This looks a bit messy, but it works. They need to ILMerge all the libraries or they need to fix the `--azure` argument

The next step to do is to git ignore the contents of the folder `_site` and the contents of the folder `_drafts` if you don't want to publish drafts to a public repository. To ensure the empty folder `_site` will be deployed to Azure (this is needed to don't get a compile error) I just added the `index.html` of the compiled web to the repository.

As you can see, it is pretty simple to setup a Pretzel blog. The real work starts now changing the templates to add a nice layout and creating the first blog posts :)