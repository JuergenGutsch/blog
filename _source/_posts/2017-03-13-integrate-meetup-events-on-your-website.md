---
layout: post
title: "Integrate Meetup events on your website"
teaser: "This is a guest post, written by Olivier Giss about integrating Meetup events on your website. Olivier is working as a web developer at algacom AG in Basel and also one of the leads of the .NET User Group Nordwest-Schweiz."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- Meetup
- AngularJS
- JSONP
---

This is a guest post, written by Olivier Giss about integrating Meetup events on your website. Olivier is working as a web developer at [algacom AG](http://www.algacom.ch) in Basel and also one of the leads of the [.NET User Group Nordwest-Schweiz](http://www.dotnet-nordwest.ch/)

---

For two years, I am leading the .NET User Group Nordwest-Schweiz with Jürgen Gutsch that owns this nice blog. After a year, we decided also [to use Meetup](https://www.meetup.com/Basel-NET-User-Group/) to get more participants.

## Understanding the problem

But with each added platform where we post our events, we increased the workload to keep it all up to date. Jürgen had the great idea to read the Meetup events and list them on our own website to lower the work.

This is exactly what I want to show you.

## The Meetup API

Before we start coding we should understand how the API of Meetup is working and what it does offer. [The API of Meetup is well documented](https://www.meetup.com/meetup_api/) and supports a lot of data. What we want is to get a list of upcoming events for our meetup group and display it on the website without to be authenticated on meetup.com.

For our goal, we need the following meetup API method:

GET [https://api.meetup.com/:urlname/events](https://api.meetup.com/:urlname/events)

The parameter “:urlname” is the meetup group name. In the request body we could sort, filter and control paging what we don’t need. If we would execute that query, you get an authorization error. 

However, we don’t want that the user must be authenticated to get the events. To get it to work we need to use a [JSONP](https://en.wikipedia.org/wiki/JSONP) request.

## Let’s getting it done

The simplest way doing a JSONP request is using jQuery:

~~~ javascript
$.ajax({
  url: "https://api.meetup.com/Basel-NET-User-Group/events",
  jsonp: "callback",
  dataType: "jsonp",
  data: {
    format: "json"
  },
  success: function(response) {
    var events = response.data;
  }
});

~~~

> Be aware: JSONP has some security implications. As JSONP is really JavaScript, it can do everything that is possible in the context. You need to trust the provider of the JSONP data!

After that call, we are getting the data from the Meetup API which can be used with simple data binding to display it on our website. You can choose any kind of MV* JS framework to do that. I used AngularJS.

~~~ html
<div class="row" ng-repeat="model in vm.Events track by model.Id" ng-cloak>
  <a href="{{::model.Link}}" target="_blank" title="Öffnen auf meetup.com"><h3>{{::model.DisplayName}}</h3></a>
  <label>Datum und Uhrzeit</label>
  <p>{{::model.Time}}</p>
  <label>Description</label>
  <div ng-bind-html="model.Description"></div>
  <label>Ort</label>
  <p>{{::model.Location}}</p>
</div>
~~~

As you can see everything is One-Way bound because the data is never changed. The “ng-bind-html” binds HTML content from the meetup event description.

The Angular controller is simple, it uses the "\$sce” service to ensure that the provided HTML content from the meetup API is marked as secure. When we change a model outside of angular, we must notify our changes with “vm.scope.$apply()”. 

~~~ javascript
(function () {
  var module = angular.module('app', []);

  module.controller('MeetupEventsController', ['$scope', '$sce', MeetupEventsController]);

  MeetupEventsController.$inject = ['$scope', '$sce'];

  function MeetupEventsController($scope, $sce) {

    var vm = this;
    vm.Events = [];
    vm.scope = $scope;
    vm.loaded = false;

    vm.Refresh = function() {
      $.ajax({
        url: "https://api.meetup.com/Basel-NET-User-Group/events",
        jsonp: "callback",
        dataType: "jsonp",
        data: {
          format: "json"
        },
        success: function(response) {
          var events = response.data;

          for (var i = 0; i < events.length; i++) {
            var item = events[i];

            var eventItem = {
              Id: i,
              DisplayName: item.name,
              Description: $sce.trustAsHtml(item.description),
              Location: item.venue.name + " " + item.venue.address_1 + " " + item.venue.city,
              Time: new Date(item.time).toLocaleString(),
              Link :item.link,
            };
            vm.Events.push(eventItem)
          }
          vm.loaded = true;
          vm.scope.$apply();
        }
      });
    };
    function activate() {
      vm.Refresh();
    };
    activate();
  };
})();

~~~

Finally, we are finish. Not that complicated, right? Feel free to ask question or share your experience.

---

Just visit the website of the [.NET User Group Nordwest-Schweiz](http://www.dotnet-nordwest.ch/) to see the Meetup integration in action.