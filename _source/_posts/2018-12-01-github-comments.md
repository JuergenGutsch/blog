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

I already evaluated a solution to use: I will go with Utterance

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

This script will search for Issues with the same title as the current page. If there's no such issue, it will create a new one. If there is such an issue it will create an comment on that issue. This script also supports markdown.

## Open questions

One pretty important open question: 

1. Is it possible to import all the Disqus comments to GitHub Issues? 
   - This is what I need to figure out now.
   - Would be bad to not have the existing comments available in the new system.
2. What if Jeremys services are not available anymore?
   - He runs an authentication service and a service that posts the comments into GitHub Issues
   - I think in that case I just need to host it by myself. The codes are open source and [available on GitHub](https://github.com/utterance).

The second question is easy to solve. As I said I will just host the stuff by my own in case Jeremy will shut down his services. The first question is much more essential. It would be cool to get the comments somehow in a readable format. I would than write a small script or a small console app to import the comments as GitHub Issues.

## Exporting the Disqus comments to GitHub Issues

Fortunately there is an export feature on Disqus, in the administration settings of the site:

![](..\img\github-comments\disqus-export.png)

After clicking "Export Comment" the export gets scheduled and you'll get an email with the download link to the export. The Export is a gz compressed XML file including all threads and posts. A thread in this case is an entry per page where the comment form was visible. A thread actually doesn't need to contain comments. Post are comments related to a thread. Posts contain the actual comment as message, Author information and relations to the thread and the parent post if it is a reply to a comment.

![](..\img\github-comments\disqus-export-xml.png)

This is pretty clean XML and it should be easy to import that automatically into GitHub Issues. Now I need to figure out how the GitHub API works and to write a small C# Script to import all the comments.

This XML also includes the authors names and usernames. This is cool to know don't have any value for me, because Disqus user are no GitHub user. I can't set the comments in behalf of real GitHub users. So any migrated comment will be done by myself and I need to mark the comment, that it originally came from another reader.

So it will be something like this:

~~~ csharp
var comment = $@"Comment written by {post.Author} at {post.CreatedAt}:

{post.Message}
";
~~~

## Importing the comments

I decided to write a small console app and to do some initial tests on a test repo. I extracted the exported data and moved it into the .NET Core console app folder and tried to play around with it.

First I read all threads out of the file and than the posts afterwards. A only select the threads that are not marked as closed and not marked as deleted. I also check the blog post URL of the thread, because sometimes the thread was created by a local test run, sometimes I changed the publication date of a post afterwards, which also changed the URL and sometimes the thread was created by a post that was displayed via a proxying page. I tried to filter all that stuff out. The URL need to start with http://asp.net-hacker.rocks or https://asp.net-hacker.rocks to be valid. Also the posts shouldn't be marked as deleted or marked as spam

Than I assigned the posts to the specific threads using the provided thread id and ordered the posts by date. This breaks the dialogues of the Disqus threads, but should be ok for the first step.

Than I create the actual issue post it and post the assign comments to the new issue. 

That's it.

Reading the XML file is easy using the XmlDocument this is also available in .NET Core:

~~~ csharp
var doc = new XmlDocument();
doc.Load(path);
var nsmgr = new XmlNamespaceManager(doc.NameTable);
nsmgr.AddNamespace(String.Empty, "http://disqus.com");
nsmgr.AddNamespace("def", "http://disqus.com");
nsmgr.AddNamespace("dsq", "http://disqus.com/disqus-internals");

IEnumerable<Thread> threads = await FindThreads(doc, nsmgr);
IEnumerable<Post> posts = FindPosts(doc, nsmgr);

Console.WriteLine($"{threads.Count()} valid threads found");
Console.WriteLine($"{posts.Count()} valid posts found");
~~~

I need to use the XmlNamespaceManager here to use tags and properties using the Disqus namespaces. The XmlDocument as well as the XmlNamespaceManager need to get passed into the read methods then. The two find methods are than reading the threads and posts out of the XmlDocument.

In the next snippet I show the code to read the threads:

~~~ csharp
private static async Task<IEnumerable<Thread>> FindThreads(XmlDocument doc, XmlNamespaceManager nsmgr)
    {
        var xthreads = doc.DocumentElement.SelectNodes("def:thread", nsmgr);

        var threads = new List<Thread>();
        var i = 0;
        foreach (XmlNode xthread in xthreads)
        {
            i++;

            long threadId = xthread.AttributeValue<long>(0);
            var isDeleted = xthread["isDeleted"].NodeValue<bool>();
            var isClosed = xthread["isClosed"].NodeValue<bool>();
            var url = xthread["link"].NodeValue();
            var isValid = await CheckThreadUrl(url);

            Console.WriteLine($"{i:###} Found thread ({threadId}) '{xthread["title"].NodeValue()}'");

            if (isDeleted)
            {
                Console.WriteLine($"{i:###} Thread ({threadId}) was deleted.");
                continue;
            }
            if (isClosed)
            {
                Console.WriteLine($"{i:###} Thread ({threadId}) was closed.");
                continue;
            }
            if (!isValid)
            {
                Console.WriteLine($"{i:###} the url Thread ({threadId}) is not valid: {url}");
                continue;
            }

            Console.WriteLine($"{i:###} Thread ({threadId}) is valid");
            threads.Add(new Thread(threadId)
            {
                Title = xthread["title"].NodeValue(),
                Url = url,
                CreatedAt = xthread["createdAt"].NodeValue<DateTime>()

            });
        }

        return threads;
    }
~~~

I think there's nothing magic in it. Even assigning the posts to the threads is just some LINQ code.

To create the actual issues and comments, I use the Octokit.NET library which is available on NuGet and GitHub. 

~~~ shell
dotnet add package Octokit
~~~

This library is quite simple to use and well documented:

~~~ csharp
private static async Task PostIssuesToGitHub(IEnumerable<Thread> threads)
{
    var client = new GitHubClient(new ProductHeaderValue("DisqusToGithubIssues"));
    var basicAuth = new Credentials(repoOwner, "majuhekaph557!");
    client.Credentials = basicAuth;

    // load the existing issues
    var issues = await client.Issue.GetAllForRepository(repoOwner, repoName);
    foreach (var thread in threads)
    {
        // check for existing issues
        if (issues.Any(x => x.Title.Equals(thread.Title)))
        {
            continue;
        }

        var newIssue = new NewIssue(thread.Title);
        newIssue.Body = thread.Url;
		// create a new issue in github
        var issue = await client.Issue.Create(repoOwner, repoName, newIssue);
        Console.WriteLine($"New issue (#{issue.Number}) created: {issue.Url}");
        
        foreach (var post in thread.Posts)
        {
            var message = $@"Comment written by {post.Author} at {post.CreatedAt}:

{post.Message}
";
                    
			// create a new comment in github
            var comment = await client.Issue.Comment.Create(repoOwner, repoName, issue.Number, message); 
            Console.WriteLine($"New comment by {post.Author} at {post.CreatedAt}");
        }
    }
}
~~~

After I started that code, the console app starts to add issues and comments to GitHub:

![](..\img\github-comments\disqus-on-github.png)

The comments are set as expected:

![](..\img\github-comments\disqus-on-github-comment.png)

## Octokit.AbuseException

Unfortunately that run doesn't finish. After the first few issues were entered I got an exception like this.

~~~ text
Octokit.AbuseException: 'You have triggered an abuse detection mechanism and have been temporarily blocked from content creation. Please retry your request again later.'

~~~

I now need to find a way to add issues and comments in a more friendly way to not get blocked by GitHub.

