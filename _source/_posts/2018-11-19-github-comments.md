---
layout: post
title: "Removing Disqus and adding GitHub Issue Comments"
teaser: "In this post I write about why I removed Disqus from my blog and how I added GitHub issue based comments to my blog"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- Unit Test
- XUnit
- MSTest
---

I recently realized that I ran this new blog for almost exactly three years now and wrote almost 100 posts until yet. Running this blog is completely different compared to the the previous one based on the community server on ASP.NET Zone. I now write on markdown files which I commit and push to GitHub. I also switched the language. From January 2007 to November 2015 I wrote in German and since I run this GitHub based blog I switched completely to English, which is a great experience and improves the English writing and speaking skills a lot.

This blog is based on [Pretzel](https://github.com/Code52/pretzel), which is a .NET based [Jekyll](https://jekyllrb.com/) clone, that creates a static website. Pretzel as well as Jekyll is optimized for blogs or similar structured web sites. Both systems take markdown files and turn them based on the [Liquid template engine](https://jekyllrb.com/docs/step-by-step/02-liquid/) into static HTML pages. This works pretty well and I really like the way to push markdown files to the GitHub repo and get an updated blog a few seconds later on Azure. This is continuous delivery using GitHub and Azure for blog posts. It is amazing. And I really love blogging this way.

> Actually the blog is successful from my perspective. Around 6k visits per week is a good number, I guess. 

Because the blog is static HTML, at the end I need to extend it with software as a service solutions to create dynamic content or to track the success of that blog.

So I added [Disqus](https://disqus.com/) to enable comments on this blog. Disqus was quite popular at that time for this kind of blogs and I also get some traffic from Disqus. Anyway, now this service started to show some advertisement on my page and it also shows advertisement that is not really related to the contents of my page. 

I also added a small Google AdSense banner to the blog, but this is placed at the end of the page and doesn't really annoy you as a reader, I hope. I put some text upon this banner, to ask you as a reader to support my blog if you like it. A click on that banner doesn't really cost some time or money.

I don't get anything out of the annoying off-topic adds that Disqus shows here, except a free tool to collect blog post comments and store them somewhere outside in the cloud. I don't really "own" the comments, which is the other downside.

Sure Disqus is a free service and someone need to pay for it, but the ownership of the contents is an problem as well as the fact that I cannot influence the contents of the adds displayed on my blog:

![]({{site.baseurl}}/img/github-comments/disqus-add.png)

## Owning the comments

The comments are important contents you provide to me, to the other readers and to the entire developer community. But they are completely separated from the blog post they relate to. They are stored on a different cloud. Actually I have no idea where Disqus stores the comments.

How do I own the comments? 

My idea the was to use GitHub issues of the blog repository to collect the comments. Every first comment of a blog post should create a GitHub issue and any comment is a comment on this issue. With this solution the actual posts and the comments are in the same repository, they can be linked together and I own this comments a little more than previously.

I already asked on twitter about that and got some positive feedback.

## Evaluating a solution

There are already some JavaScript codes available which can be used to add GitHub Issues as comments. The GitHub API is well documented and it should be easy to do this. 

I already evaluated a solution to use and decided to go with Utterance

> "A lightweight comments widget built on GitHub issues"

Utterance was built by [Jeremy Danyow](https://www.danyow.net/author/jeremy/). I stumbled upon it on Jeremys blog post about [Using GitHub Issues for Blog Comments](https://www.danyow.net/using-github-issues-for-blog-comments/). Jeremy works as a Senior Software Engineer at Microsoft, he is member of the Aurelia core team and created also [gist.run](https://gist.run/). 

As far as I understood, Utterances is a light weight version of Microsofts comment system used with the new docs on [https://docs.microsoft.com](https://docs.microsoft.com). Also Microsoft stores the comments as Issues on GitHub, which is nice because they can create real issues out of it, in case there are real Problems with the docs, etc.

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

## Open questions until yet

Some important open question came up while evaluating the solution: 

1. Is it possible to import all the Disqus comments to GitHub Issues? 
   - This is what I need to figure out now.
   - Would be bad to not have the existing comments available in the new system.
2. What if Jeremys services are not available anymore?
   - He runs an authentication service and a service that posts the comments into GitHub Issues
   - I think in that case I just need to host it by myself. The codes are open source and [available on GitHub](https://github.com/utterance).
   - Actually there are self hosting instructions available: [https://github.com/utterance/utterances/issues/42#issuecomment-399723028](https://github.com/utterance/utterances/issues/42#issuecomment-399723028) 

The second question is easy to solve. As I wrote, I will just host the stuff by my own in case Jeremy will shut down his services. The first question is much more essential. It would be cool to get the comments somehow in a readable format. I would than write a small script or a small console app to import the comments as GitHub Issues.

## Exporting the Disqus comments to GitHub Issues

Fortunately there is an export feature on Disqus, in the administration settings of the site:

![]({{site.baseurl}}/img/github-comments/disqus-export.png)

After clicking "Export Comment" the export gets scheduled and you'll get an email with the download link to the export. 

The exported file is a GZ compressed XML file including all threads and posts. A thread in this case is an entry per blog post where the comment form was visible. A thread actually doesn't need to contain comments. Post are comments related to a thread. Posts contain the actual comment as message, Author information and relations to the thread and the parent post if it is a reply to a comment.

![]({{site.baseurl}}/img/github-comments/disqus-export-xml.png)

This is pretty clean XML and it should be easy to import that automatically into GitHub Issues. Now I needed to figure out how the GitHub API works and to write a small C# Script to import all the comments.

This XML also includes the authors names and usernames. This is cool to know, but it doesn't have any value for me anymore, because Disqus users are no GitHub users. I can't set the comments in behalf of real GitHub users. So any migrated comment will be done by myself and I need to mark the comment, that it originally came from another reader.

So it will be something like this:

~~~ csharp
var message = $@"Comment written by **{post.Author}** on **{post.CreatedAt}**

{post.Message}
";
~~~

## Importing the comments

I decided to write a small console app and to do some initial tests on a test repo. I extracted the exported data and moved it into the .NET Core console app folder and tried to play around with it.

First I read all threads out of the file and than the posts afterwards. A only selected the threads which are not marked as closed and not marked as deleted. I also checked the blog post URL of the thread, because sometimes the thread was created by a local test run, sometimes I changed the publication date of a post afterwards, which also changed the URL and sometimes the thread was created by a post that was displayed via a proxying page. I tried to filter all that stuff out. The URL need to start with http://asp.net-hacker.rocks or https://asp.net-hacker.rocks to be valid. Also the posts shouldn't be marked as deleted or marked as spam

Than I assigned the posts to the specific threads using the provided thread id and ordered the posts by date. This breaks the dialogues of the Disqus threads, but should be ok for the first step.

Than I created the actual issue post it and posted the assigned comments to the new issue. 

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

I need to use the `XmlNamespaceManager` here to use tags and properties using the Disqus namespaces. The `XmlDocument` as well as the `XmlNamespaceManager` need to get passed into the read methods then. The two find methods are than reading the threads and posts out of the `XmlDocument`.

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

To create the actual issues and comments, I use the [Octokit.NET](https://github.com/octokit/octokit.net) library which is [available on NuGet](https://www.nuget.org/packages/Octokit/) and GitHub. 

~~~ shell
dotnet add package Octokit
~~~

This library is quite simple to use and well documented. You have the choice between basic authentication and token authentication to connect to GitHub. I chose the token authentication which is the proposed way to connect. To get the token you need to go to the settings of your GitHub account. Choose a personal access token and specify the rights the for the token. The basic rights to contribute to the specific repository are enough in this case:

~~~ csharp
private static async Task PostIssuesToGitHub(IEnumerable<Thread> threads)
{
    var client = new GitHubClient(new ProductHeaderValue("DisqusToGithubIssues"));
    var tokenAuth = new Credentials("secret personal token from github");
    client.Credentials = tokenAuth;

    var issues = await client.Issue.GetAllForRepository(repoOwner, repoName);
    foreach (var thread in threads)
    {
        if (issues.Any(x => !x.ClosedAt.HasValue && x.Title.Equals(thread.Title)))
        {
            continue;
        }

        var newIssue = new NewIssue(thread.Title);
        newIssue.Body = $@"Written on {thread.CreatedAt} 

URL: {thread.Url}
";

        var issue = await client.Issue.Create(repoOwner, repoName, newIssue);
        Console.WriteLine($"New issue (#{issue.Number}) created: {issue.Url}");
        await Task.Delay(1000 * 5);

        foreach (var post in thread.Posts)
        {
            var message = $@"Comment written by **{post.Author}** on **{post.CreatedAt}**

{post.Message}
";

            var comment = await client.Issue.Comment.Create(repoOwner, repoName, issue.Number, message);
            Console.WriteLine($"New comment by {post.Author} at {post.CreatedAt}");
            await Task.Delay(1000 * 5);
        }
    }
}
~~~

This method gets the list of Disqus threads, creates the GitHub client and inserts one thread by another. I also read the existing Issues from GitHub in case I need to run the migration twice because of an error. After the Issue is created, I only needed to create the comments per Issue.

After I started that code, the console app starts to add issues and comments to GitHub:

![]({{site.baseurl}}/img/github-comments/disqus-on-github.png)

The comments are set as expected:

![]({{site.baseurl}}/img/github-comments/disqus-on-github-comment.png)

Unfortunately the import breaks after a while with a weird exception.

## Octokit.AbuseException

Unfortunately that run didn't finish. After the first few issues were entered I got an exception like this.

~~~ text
Octokit.AbuseException: 'You have triggered an abuse detection mechanism and have been temporarily blocked from content creation. Please retry your request again later.'
~~~

This Exception happens because I reached the creation rate limit (user.creation_rate_limit_exceeded). This limit is set by GitHub on the public API. It is not allowed to do more than 5000 requests per hour: [https://developer.github.com/v3/#rate-limiting](https://developer.github.com/v3/#rate-limiting)

You can see such security related events in the security tap of your GitHub account settings.

There is no real solution to solve this problem, except to add more checks and fallbacks to the migration code. I checked which issue already exists and migrate only the issues that don't exist. I also added a five second delay between each request to GitHub. This only increases the migration time, and helps to start the migration only two times. Without the delay I got the exception more often during the tests.

## Using Utterances

Once the Issues are migrated to GutHub, I need to use Utterances to the blog. At first you need to install the [utterances app](https://github.com/apps/utterances) on your repository. The repository needs to be public and the issues should be enabled obviously. 

On (https://utteranc.es/)[https://utteranc.es/] there is a kind of a configuration wizard that creates the HTML snippet for you, which you need to add to your blog. In my case it is the small snippet I already showed previously:

~~~ html
<script src="https://utteranc.es/client.js"
        repo="juergengutsch/blog"
        issue-term="title"
        theme="github-light"
        crossorigin="anonymous"
        async>
</script>
~~~

This loads the Uttereances client script, configures my blog repository and the way the issued will be found in my repository. You have different options for the issue-term. Since I set the blog post title as GitHub issue title, I need to tell Utterances to look at the tile. The theme I want to use here is the GitHub light theme.  The dark theme doesn't fit the blog style. I was also able to override the CSS by overriding the following two CSS classes:

~~~ css
.utterances {}
.utterances-frame {}
~~~

## The result

At the end it worked pretty cool. After the migration and after I changed the relevant blog template I tried it locally using the `pretzel taste` command.

![]({{site.baseurl}}/img/github-comments/utterances-comments.png)

If you want to add a comment as a reader, you need to logon with your GitHub account and you need to grand the utterances app to post to my repo with our name. 

No every new commend will be stored in the repository of my blog. All the contents are in the same repository. There will be an issue per post, so it is almost directly linked. 

What do you think? Do you like it? Tell me about your opinion :-)

BTW: You will find the migration tool [on GitHub](https://github.com/JuergenGutsch/disqus-to-github-issues).
