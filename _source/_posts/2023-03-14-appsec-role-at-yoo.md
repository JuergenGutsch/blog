---
layout: post
title: "Application Security at YOO"
teaser: "Description"
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- .NET Core
- ASP.NET Core
- Tests
- Tag
---

Since bout a year, I was working on a pretty exciting project. I defined and created a new role for our company that is responsible for application security. Actually, application security was never a missing aspect in our development process. All the colleagues were great developers and highly motivated to create secure applications. 

The actual problem was a missing standard, that sets up every project in the same secure way, that helps the QA to also test security aspects, that uses the same tools to improve the software quality and software security and that keeps the awareness about security during the entire development process.

## Defining the new role

Defining and creating this new role also meant that I will take over this new role and to be responsible, to ensure and to maintain the security standards through the entire company. I will also be responsible to keep awareness about security through the entire company and to train the developers, the DevOps responsible responsible person as well as the QA. I am not the only person that is responsible for application security in general. My job will be to make all collogues feel responsible to create secure applications. That they all have security in mind, that all features will be analyzed from the security perspective as well.

And this is why the process of secure software development doesn't start at development or DevOps. 

## Adding security to the company

Actually, the secure software development process starts at the sales phase. The sales person need to know what type of customer he is selling our services, what type of data the potential customer will be handling with the new project and what type of possible risks he has. The sales person need to know what level of security he needs to sell. (Exactly, we need to sell security. See next section why)

The process continues with the requiremence engineering, even the UX and UI specialists need to take care about security. DevOps is following by setting up the secure software development infrastructure and secure deployments per project. DevOps is also supporting the developers with the right tools that checks for software quality and possible vulnerabilities and code flaws while building and delivering the software. In the development phase, required security aspects need to be implemented and QA needs to know how to test this. Maybe tooling will support the QA to make automated security tests.

## Selling security

True, we need to sell levels of security because ensuring application security is some effort. The more secure the more effort during development and afterwards, while ensuring and testing the application security. A potential customer in a sensitive or risky environment should know that security will cost something. A bank, a power plant, and other big and risky industry are paying for security person that keeps unauthorized people outside of their restricted areas. Such companies should be aware and willing to pay for implementing higher security mechanisms to keep unauthorized people out of the digital restricted areas as well.

Actually, application security need to be ensured in every project and a basic level security won't be charged separately.

## What are the standards we use?

It is the [OWASP](https://owasp.org) foundation that helps me diving into the new topics. We are going to implement the OWAS Application Security Verification Standard ([ASVS](https://owasp.org/www-project-application-security-verification-standard/)) and the mobile version of it (MASVS). ASVS already is divided into three levels of security. Level 1 is the basic level of security that all projects need to implement. Actually Level 1 is quite basic and despite of just a few topics, this is all stuff we as developers almost already knew, used and implemented in the past. If we have thought about it and if the project pressure wasn't that high. The levels 2 and 3 are adding more security mechanisms to the projects to handle sensitive and critical data and infrastructure.

This standards is helping us like a blueprint for all our projects to keep the levels of security and it helps our QA to know what to test from the security point of view..

Actually, since ASVS is adopting and covering many other standards as well, we will be save with future security audits, no matter what standard will be used during the audit.

## Will YOO be a secure software company?

The company already creates secure software. Since it was more or less a side aspect in the past, we'll now focus on security by following the standards and the process we have implemented.

So, yes, we can now call ourself a secure software company. But we are not certified somehow. OWASP and ASVS are no badges nor provide certificates we can put proudly and high nosed on our website. But we can proudly mention that we are following a standard that was created by well known and independent security experts.

## The main role is still a software engineer :-)

The application security role is only an additional role to my position as a software engineer. In a midsized company like the YOO ensuring secure software software is not such an high effort that it is needed to create a new position like an application security engineer. And therefore it is just a new additional role for me. 

Despite that, my job title will change a little bit and will be called like **Software & Application Security Engineer**.

## Learning about application security

Actually, application security as a global topic was kinda new to me and I never expected that it is needed to have the entire company involved. But that also was the fun part: Talking to other disciplines and talking to people that are not really involved into my day to day business. It is not completely technical to implement application security to a software company.

As mentioned, the website of the [OWASP](https://owasp.org) Foundation points me to various learning resources. OWAS is full of projects to learn about application security. You might know the OWASP Top 10 list of security risks. I already mentioned the ASVS. But there is a ton more.

Another great learning resource is the twitter feed of [Tanya Janka](https://twitter.com/shehackspurple). Her talks about application security are amazing and her book is a great read, even on vacation at the beaches of Greece while the kids are playing. If you want to learn about application security, follow her on twitter, [read her blog](https://shehackspurple.ca), read her book and watch her talks. You also need to dive into the OWASP website and the various projects of the foundation.

IMAGE Book on Beach 

## Further more

And maybe, you as a software producing company would like to adopt the same standards and processes. If you would like to know, how we created and implemented the secure software process, feel free to ask. If you need help to make your software development process more secure, we would be happy to help as well.

LINK to yoo appsec post: https://yoo.digital/

## Conclusion

I'm happy to start working on my new role officially this week and I'm happy to bring the YOO a step further in creating and delivering high quality and secure software. I'm also pretty excited about how it will go and grow over time. The implementation of a secure software process isn't complete and need to be adjusted whenever it's needed. It is a living process that needs reviews and adjustments regularly.
