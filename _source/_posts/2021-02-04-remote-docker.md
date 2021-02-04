---
layout: post
title: "Working inside a Docker container using Visual Studio Code"
teaser: "Have you ever tried to develop inside the Docker container? I had to try it out and was impressed about how easy it is using Visual Studio Code. This tutorial shows you how to connect to a Docker container and how develop inside the Docker container."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET 5
- ASP.NET Core
- DOcker
- VSCode
---

As mentioned [in the last post]({% post_url remote-wsl.md%}) I want to write about remote working inside a docker container.  But at first, we should get an idea about why we should ever remote work inside a docker container.

## Why should I do that?

One of our customers is running an OpenShift/Kubernetes cluster and also likes to have the technology-specific development environments in a container that runs in Kubernetes. We had a NodeJS development container, a Python development container, and so on...  All the containers had an SSH server installed, Git, the specific SDKs, and all the stuff that is needed to develop. Using [VSCode](https://code.visualstudio.com/) we connected to the containers via SSH and developed inside the container.

Having the development environment in a container is one reason. Maybe not the most popular reason. But trying stuff inside a container because the local environment isn't the same makes a lot of sense. If you want to debug an application in a production-like environment makes absolute sense. 

## How does it work?

VSCode has a great set of tools to work remotely. In installed Remote WSL (used in the last post), Remote SSL was the one we used with OpenShift (maybe I will write about it, too), and with this post, I'm gonna use Remote Container. All three of them will work inside the remote explorer within VS Code. All three add-ins will work pretty similarly. 

If the remote machine doesn't have the VSCode Server Installed, the remote toll will install it and start it. The VSCode server is like a full VSCode without a user interface. It also needs to have add-ins installed to work with the specific technologies. The local VSCode will connect to the remote VSCode server and mirrors it in the user interface of your locally installed VSCode. It is like a remote session to the other machine but feels local.

## Setup the demo

I created a small ASP.NET Core MVC project:

~~~shell
dotnet new mvc -n RemoteDocker -o RemoteDocker
cd RemoteDocker
~~~

Than I added a `dockerfile` to it:

~~~dockerfile
FROM mcr.microsoft.com/dotnet/sdk:5.0

COPY . /app

WORKDIR /app

EXPOSE 5000 5001

# ENTRYPOINT ["dotnet", "run"] not needed to just work in the container
~~~

If you don't have the docker tool installed, VSCode will ask you to install it as soon you have the `dockerfile` open. If it's installed you can just right-click  the `dockerfile` in the VSCode Explorer and select "Build image...". 

![image-20210203220213602]({{site.baseurl}}/img/remote-docker/image-20210203220213602.png)

This will prompt you for an image name. You can use the proposed name which is "remotedocker:latest" in my case. It seems it uses the project name or the folder name which makes sense:

![image-20210203220356005]({{site.baseurl}}/img/remote-docker/image-20210203220356005.png)

Select the Docker tab in VSCode and you will find your newly built image in the list of images:

![image-20210203220705183]({{site.baseurl}}/img/remote-docker/image-20210203220705183.png) 

You can now right-click the tag `latest` and choose "Run Interactive". If you just choose "Run" the container stops, because we commented out the entry point. We need an interactive session. This will start-up the container and it will now appear as a running container in the container list:

![image-20210203220954330]({{site.baseurl}}/img/remote-docker/image-20210203220954330.png)

You can browse and open the files inside the container from this containers list, but editing will not work.  This is not what we want to do. We want to remotely connect  VSCode to this Docker container.

## Connecting to Docker

This can be done using two different ways:

1. Just right-click the running container and choose "Attach Visual Studio Code"

![image-20210203221956964]({{site.baseurl}}/img/remote-docker/image-20210203221956964.png)

2. Or select the Remote Explorer tab, ensure the Remote Containers add-in is selected in the upper-right dropdown box, wait for the containers to load. If all the containers are visible, choose the one you want to connect, right-click it and choose "Attach to container" or "Attach in New Window". It does the same thing as the previous way 

![image-20210203221546976]({{site.baseurl}}/img/remote-docker/image-20210203221546976.png)

Now you have a VSCode instance open that is connected to the container. You now can see the files in the project, you can sue the terminal inside the container and you can now edit the files inside the project. 

![image-20210203222354886]({{site.baseurl}}/img/remote-docker/image-20210203222354886.png) 

You can see that this is a different VSCode than your local instance by having a look at the tabs on the left side. Not all the add-ins are installed on that instance. In my case, the database tools are missing as well as the Kubernetes tools and some others.

## Working inside the Container

Since we disabled the entry point in the `dockerfile`  we are now able to start debugging by pressing F5. 

![image-20210204221414743]({{site.baseurl}}/img/remote-docker/image-20210204221414743.png)

This also opens the local browser and shows the application that is running inside the container. This is really awesome. It feels like really local development:

![image-20210204222047723]({{site.baseurl}}/img/remote-docker/image-20210204222047723.png)

Let's change something to see that this is really working. Like in the last demo, I'm going to change the page title. I would like to see the name "Remote Docker demo":

![image-20210204222347623]({{site.baseurl}}/img/remote-docker/image-20210204222347623.png)

Just save and restart debugging in VSCode:

![image-20210204222617177]({{site.baseurl}}/img/remote-docker/image-20210204222617177.png)

That's it.

## Conclusion

Isn't this cool?

You can easily start docker containers to test, debug and develop in a production-like environment. You can configure a production-like environment with all Docker containers you need using docker-compose on your machine. Then add your development, or your testing container to the composition and start it all up. Now you can connect to this container and start playing around within this environment. It is all fast, accessible, and on your machine.

This is cool! 

I'd like to see this is also working if the containers running on Azure. I will try it within the next weeks and maybe I can put the results in a new blog post.