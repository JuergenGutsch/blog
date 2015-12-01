---
layout : layout
title : ASP.NET Hacker
---

{% assign posts = site.posts | where:'group', post.date %}

<ul class="posts">
{% for post in site.posts limit:5 %}
<li>
<div class="idea">
{% if forloop.first and post.layout == "post" %}
<h1><a href="{{ post.url }}">{{ post.title }}</a></h1>
{% else %}
<h2><a class="postlink" href="{{ post.url }}">{{ post.title }}</a></h2>
{% endif %}
<p class="postdate">{{ post.author }} - {{ post.date | date: "%e %B, %Y"  }}</p>
<ul class="postdate">
{% for tag in post.tags %}
<li><a href="#">{{ tag }}</a></li>
{% endfor %}
</ul>
				
<p>{{ post.teaser }}</p>
<p><a href="{{ post.url}}#disqus_thread">Comments</a></p>
</div>
</li>
{% endfor %}
</ul>

<h3>OLDER</h3>
<ul class="postArchive">
{% for post in site.posts offset:5 limit:10 %}
<li>
<span class="olderpostdate"> {{ post.date | date: "%d %b"  }} </span> <a class="postlink" href="{{ post.url }}">{{ post.title }}</a>
</li>
{% endfor %}
</ul>

<script id="dsq-count-scr" src="//aspnethacker.disqus.com/count.js" async></script>
