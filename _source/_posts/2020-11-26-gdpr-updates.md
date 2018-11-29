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

After three years of running this blog, there were some updates needed. Nothing special, but some small changes on this blog. I moved away from the Disqus comments. I already wrote about that. And I need to change some small things to make this blog ready for GDPR. In this post I'm going to write about what I changed to make this blog ready for GDPR

## GDPR

In the last year I learned a lot about the GDPR, the good things and the bad things. From my perspective the general Idea is a good thing for the most of users and it makes the usage of the personal user date more transparent. There is almost nothing bad to say about the GDPR. All the bad things about GDPR are mostly communication failures doe by the German (and Swiss) government. 

> Why Switzerland? Because there was almost no communication about the GDPR to the companies, associations or any other website owner. Maybe because they thought it is a EU thing and it doesn't relate to a Swiss institution. But actually it does. It does in the same way or even more as it relates to the US and Canada or even Australia or China.

* The GDPR is more than two years old. But why is it an important thing since May this year? There was no big headline about that two years earlier. 
* Especially in Germany there was no good resource about the GDPR and what it means to a institution or person that runs a public website, a webservice or any other thing that collects data. There was no help about that. 
  * Actually the UK and France created well documented Website about the GDPR and all the things you need to know about it.
* No one in Germany (guessed 99%) knows anything about GDPR and panic happens since May 20th. Websites and blogs of schools and small soccer clubs shut down, because of this. 
* What if you are German and write a blog in English for an international audience? There was no help about that. Some of them added a privacy policy in German to be save. But what about the EU citizens which don't speak German? 

> Actually my family had some troubles because of GDPR an a offline scenario. My wife as a representative of the parents of the school grade of our first son, wrote a analogue letter including a small survey to all the parents of that grade. We than got told that she used a closed list (owned by the school) of all parents illegally to get the addresses of all of the parents. This caused a lot of trouble done by folks in panic because of the GDPR. Actually we live in a small city with 2100 inhabitants and we know all the parents of that grade and we know where they live. We don't need any list. In that case we didn't violate the GDPR.

Did you know that there are different levels of GDPR violations? Actually there are. If you run a website of a small soccer club, if you run a blog or a website without any business. It is not a big violation, if you use Google Analytics to track the traffic. You will be a little more save, if you add a small privacy policy. If you are a bigger institution and really work and deal with personal data, it will do a huge violation, if you don't tell the user about what you do with his data.

For a website or blog owner the GDPR in general means (as far as I understood):

* The users OWNS their personal data, you as a institution or person who collects the data don't
* The users MUST have control over their personal data
  * Able to see, to manipulate and to delete as well as to control the amount of data you will collect
* You as a institution or person who collects the data need to be super transparent
  * You need to tell the user how and why you collect the data, how you store and how you use the data and you need to tell the user whether or to whom you share the data.
* You are also responsible for indirect collection of personal data e.g. via Google Analytics, Adsense, web server logging, etc.

## Personal Data

What are personal data in that case?

All kind of data that can be directly matched to a person are personal data. Obviously phone numbers and email addresses are. Also IP addresses and cookies are personal data. In general: Every kind of data that cam be directly or indirectly used to identify a individual person.

## Privacy Policy

It was pretty hard to find the right resources to learn what needs to be changed. At the end it isn't that difficult. You need to tell the reader what data you will collect, why you want to collect the data and how the user can opt-out and delete the data you collected. This is usually done in a privacy policy. I also created one: [Privacy Policy](). 

Finally I found a blog post that describes the privacy policy pretty well that is GDPR conform. You need to answer a bunch of questions to put all the relevant information in such a privacy policy:

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

I also extended the cookie consent banner to add opt-out options as well as a link to the privacy policy. Instead of just informing the readers about cookies and other systems which will collect data in some way, I disabled all third party tools until the reader accepts the privacy policy. Unfortunately this is needed, because it is not allowed to collect the users data, if he doesn't allow it explicitly. 

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

Google adds some options to their services to be GDPR compliant. To do this there is an option to anonymize the personal data. In this case the IP address. They only store that part of the IP address that may contain regional information. To be more exact: They only store the first two numbers of an IP address. Doing this it shouldn't be possible to find the individual person with the data they collect. 

> Maybe it is using the overall footprint of the collected data based on. region, language, browser, OS, add-ins, cookies, etc.

Until now I wasn't able to find such an option in Disqus which is also a reason why I removed it