---
layout: post
title: "Blog: GDPR updates"
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

After three years of running this blog, there were some updates needed. Nothing special, but some small changes on this blog. I moved away from the Disqus comments. I already wrote about that. And I need to change some small things to make this blog ready for the GDPR.

## Privacy Policy

It was pretty hard to find the right resources to learn what needs to be changed. At the end it isn't that difficult. You need to tell the reader what data you will collect, why you want to collect the data and how the user can opt-out and delete the data you collected. This is usually done in a privacy policy. I also created one. 

I found a blog post that describes the privacy policy pretty well. You need to answer a bunch of questions to put all the relevant information in such a policy:

* What information is being collected?
* Who is collecting it?
* How is it collected?
* Why is it being collected?
* How will it be used?
* Who will it be shared with?
* What will be the effect of this on the individuals concerned?
* Is the intended use likely to cause individuals to object or complain?

If you answer all the questions as detailed as possible and if you put a small summery of all of your answers at the beginning of the file your should be save.

## Cookie consent banner

I also extended the cookie consent banner to add opt-out options as well as a link to the privacy policy. Instead of just informing the readers about cookies and other systems which will collect data in some way, I disabled all third party tools until the reader accepts the privacy policy. Unfortunately this is needed, because it is not allowed to collect the users readers, if he doesn't allow it explicitly. 

The cookie consent banner I use has some options to set a cookie if the readers accepts the privacy policy and to check whether the policy is accepted or not. I use this last feature to enable the third'party add-ins. 

* Google Analytics
* Google Absence
* Utterances GitHub comments

You should disable third party features, if you are not sure about their GDPR Settings and if you cannot control their data collection until the reader accepts your privacy policy.

For this blog I use the [jQuery Cookie Bar by primebox](https://www.primebox.co.uk/projects/jquery-cookiebar/) 

The cookie bar allows me to read out the current state to see whether the reader accepted the privacy policy or not. I can use this result to enable e.g. the Utterances comment form:

~~~ javascript
$(function() {
    if (pp_accepted) { // cookie consent state
        (function() {
            var d = document,
                s = d.createElement('script');
            s.src = 'https://utteranc.es/client.js';
            s.setAttribute('repo', 'juergengutsch/blog');
            s.setAttribute('issue-term', 'title');
            s.setAttribute('theme', 'github-light');
            s.setAttribute('crossorigin', 'anonymous');
            s.setAttribute('async', true);

            d.getElementById('articlefooter').appendChild(s);
        })();
    }
});
~~~

I did the same thing for Google Analytics and Google Adsense

## Other GDPR settings

