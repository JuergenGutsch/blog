---
layout: post
title: "Removing Disqus and adding GitHub Issue Comments"
teaser: "description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- Unit Test
- XUnit
- MSTest
---

I ran this blog for around three years now and wrote almost 100 posts until yet. This blog is based on Pretzel, which is a .NET based Jekyll clone, that creates a static website. Pretzel as well as Jekyll is optimized for blogs or similar structured web sites. Both systems take markdown files and turn them based on the Liquid template engine into static HTML pages. This works pretty well and I really like the way to push markdown files to the GitHub repo and get an updated blog a few seconds later on Azure. This is continuous delivery using GitHub and Azure for blog posts. It is amazing. And I really love blogging this way.

Because the blog is static HTML at the end I need to extend it with software as a service solutions to create dynamic content or to track the success of that blog.

> Actually the blog is successful from my perspective. Around 6k visits per week is a good number, I guess. 

So I added Disqus to enable comments on this blog. Disqus was quite popular at that time for this kind of blogs and I also get some traffic from Disqus. Anyway, this service started to show some advertisement on my page and it also shows advertisement that is not really related to the contents of my page. 

I also added a small Google AdSense banner to the blog, but this is placed at the end of the page and doesn't really annoy you as a reader, I hope. I put some text upon this banner, to ask you as a reader to support my blog if you like it. A click on that banner doesn't really cost some time or money.

I don't get anything out of the annoying off-topic adds that Disqus shows here, except a free tool to collect blog post comments and store it somewhere outside in the cloud. I don't really "own" the comments, which is the other downside.

Sure Disqus is a free service and someone need to pay for it, but the ownership of the contents is an problem as well that I cannot influence the contents of the adds displayed on my blog:

![](..\img\github-comments\disqus-add.png)

## Owning the comments

The comments are important contents, you provided to me. But they are completely separated from the blog post they relate to. They are stored on a different cloud. Actually I have no idea where Disqus stores the comments.

How do I own the comments? My idea is to use GitHub Issues of the blog repository to collect the comments. Every first comment to a post creates a GitHub Issue and any comment is a comment on this issue. With this solution the actual posts and the comments are in the same repository, they can be linked together and I own this comments a little more than previously.

I already asked on twitter about that and got some positive feedback.

> "For my blog comments I'm thinking about moving back from disqus to github issue based comments. Mainly because of the ads I don't want to show. What do you think?" 

## Implementing the GitHub comments

There are already some JavaScript codes available which can be used to add GitHub Issues as comments. The GitHub API is well documented and it should be easy to do this. 

I already evaluated a solution to use. I will go with Utterance

> "A lightweight comments widget built on GitHub issues"

Utterance was built by [Jeremy Danyow](https://www.danyow.net/author/jeremy/). I stumbled upon it on Jeremys blog post about [Using GitHub Issues for Blog Comments](https://www.danyow.net/using-github-issues-for-blog-comments/)

More Links about it: https://utteranc.es/ and https://github.com/utterance

At the end I just need to add a small HTML snippet to my blog:

~~~ html
<script src="https://utteranc.es/client.js"
        repo="juergengutsch/blog"
        issue-term="title"
        theme="github-light"
        crossorigin="anonymous"
        async>
</script>
~~~

## Open questions

One pretty important open question: 

1. Is it possible to import all the Disqus comments to GitHub Issues? 
   - This is what I need to figure out now.
   - Would be bad to not have the existing comments available in the new system.
2. What if Jeremys services are not available anymore?
   - He runs an authentication service and a service that posts the comments into GitHub Issues
   - I think in that case I just need to host it by myself. The codes are open source and [available on GitHub](https://github.com/utterance).

The second question is easy to solve. As I said I will just host the stuff by my own in case Jeremy will shut down his services. The first question is much more essential. It would be cool to get the comments somehow in a readable format. I would than write a small script to import the comments as GitHub Issues.

## Importing the Disqus comments to GitHub Issues

Fortunately there is an export feature on Disqus, in the administration settings of the site:

![](..\img\github-comments\disqus-export.png)

After clicking "Export Comment" the export gets scheduled and you'll get an email with the download link to the export. The Export is a gz compressed XML file including all threads and posts. A thread in this case is an entry per page where the comment form was visible. A thread actually doesn't need to contain comments. Post are comments related to a thread. Posts contain the actual comment as message, Author information and relations to the thread and the parent post if it is a reply to a comment.

![](..\img\github-comments\disqus-export-xml.png)

This is pretty clean XML and it should be easy to import that automatically into GitHub Issues. Now I need to figure out how the GitHub API works and to write a small C# Script to import all the comments.



