--- 
layout: post
title: "To 'var' or not to 'var'"
teaser: "Why is it important to use 'var' instead of the concrete type, if you declare and assign a variable? While I was refactoring a legacy application I found some good reason to always use 'var' whenever it's possible."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- C#
- .NET
- Clean Code
---

There are many discussions out there about to write 'var' or the concrete type, if you declare a variable. Personally since it is possible to use 'var' in C#, I always write 'var' whenever it is possible and every time I am refactoring a legacy application I remember the reasons why it is important to write 'var' instead of the concrete type.

In the last months I worked a lot with legacy applications and had a lot of effort on refactoring some of the codes because of the concrete type was uses instead of 'var'.

Many people don't like 'var'. Some of them because of the variant type in the VB languages. These guys are using C# but they still don't now C# well, because 'var' is not a variant type but a kind of a placeholder for the concrete type.

~~~ csharp
var age = 37;
~~~

This doesn't mean to declare a variant type. This means to tell the compiler to place Int32 where we wrote 'var'. 'var' is simply syntactical sugar, but with many additional benefits.

The other people who don't like 'var' wanted to directly see the type of the variable. (From my perspective they don't know Visual Studio very well.) This guys opinion is, that 'age' could also be a string or a double. Or maybe a Boolean. Just kidding. But it seems they don't trust variable names and assignments.

## My thoughts about 'var'

If I read the code, I directly see that 'age' is a numeric. While reading the code, in the most cases, it is not really important to know what type of number it is. But in this case, a integer makes more sense, because it is about an age of something. Writing meaningful variable names is very important, with or without 'var'. But using meaningful names and the concrete type declaring a variable we have a three times redundancy in just three words:

~~~ csharp
int age = 37;
// numeric number = number
// we know that 'age' is always a number ;)
~~~

More cleaner, more readable and with less redundancy is something like this:

~~~ csharp
var age = 37;
// variable age is 37
~~~

I don't read 'var' as a type. I read 'var' as a shortcut for just 'variable'. The name and the assignment tells me about the type.

And what about this?

~~~ csharp
var productName  = _productService.GetNameFromID(123);
~~~

Because I trust the names, I know the variable is of type string. (Any kind of string, because it could be a custom implementation of string, but this doesn't matter in this line of code.)

While refactoring legacy code I also found something like this:

~~~ csharp
string product = _productService.GetNameFromID(123);
~~~

In the later usage/reading of the variable name 'product', I'm not really sure about the type of product and I would expect a 'Product' type instead of a string. This is not a reason to use the concrete type, this is a reason to change the variable name instead:

~~~ csharp
var productName = _productService.GetNameFromID(123);
~~~

Because names are strings in the most cases, I would also expect a string.

Let's have a look at this:

~~~ csharp
var product = _productService.GetFromID(123);
~~~

We are able to read a lot out of this simple line:

- It is a product
- The type could be Product, because we are working with products
- It has an ID which is numeric

Hopefully it is true ;) To be sure I can use the best tool to write C# code. In Visual Studio just place your mouse over the keyword 'var' to get the type information. VS knows the information from the return type of the method GetFromID(); That's simple, isn't it?

To see the type information is not a good reason to write the concrete type.

Another reason is readability. Lets have a nested generic type:

~~~ csharp
IDictionary<String, IEnumerable<Product>> productGroups = _productService.GetGroupedProducts();
~~~

Is this really a good and readable solution? What happened if you change the type of the  groups from IEnumerable<T> to something else? 

Doesn't look this pretty more cleaner and more readable?

~~~csharp
var productGroups = _productService.GetGroupedProducts();
~~~

I know, it is not always possible to write 'var', e.g. If you don't assign a value, you have to write a concrete type. In method arguments you always have to write the concrete type. A return value always needs to have a concrete type definition, even dynamic is a concrete type definition in this case. ;)

The most important reason to write 'var' is refactoring. To reduce code changes while refactor code, you should use this useful keyword, because it doesn't need to be changed. 

~~~ csharp
Product product = _productService.GetFromID(123);
~~~

If we need to change the type of the returning value of the method because of any reason, we also need to change the type of the variable definition. Let's simplify this only a little bit:

~~~ csharp
var product = _productService.GetFromID(123);
~~~

Now we don't need to change anything in this line of code.

On the customer side I had to mask a domain object and it's dependencies with interfaces to simpler do refactorings later on. Extracting the interface wasn't a big deal. But the most code changes are the replacing of the concrete type on the variable declarations. Sure ReSharper helps a lot, but this domain object was used in many different and huge solutions. This couldn't be done in one step. If they would have used 'var' in all possible cases, we would also have reduced the needed code changes a lot.

## Conclusion

The keyword 'var' helps you to easier maintain your code, it reduces code changes and redundancies and it makes your code more readable. Use it whenever it is possible. It is not a variant type, it is a shortcut of the concrete type. It doesn't hide type information, because the assignments and the variable name contains the needed information and Visual Studio helps you to know more about the variable if needed.