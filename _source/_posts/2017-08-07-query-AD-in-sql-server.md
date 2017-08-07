---
layout: post
title: "Querying AD in SQL Server via LDAP provider"
teaser: "This is kinda off-topic, because it's not about ASP.NET Core, but l really like it to share. I recently needed to import some additional user data from the Active Directory via a nightly run into a SQL Server Database. This was done via T-SQL and a SQL Server job."
author: "Jürgen Gutsch"
comments: true
image: /img/cardlogo-dark.png
tags: 
- SQL Server
- Active Directory
- LDAP
---

This is kinda off-topic, because it's not about ASP.NET Core, but l really like it to share. I recently needed to import some additional user data via a nightly run into a SQL Server Database. The base user data came from a SAP database via an CSV bulk import. But not all of the data. E.g. the telephone numbers are maintained mostly by the users itself in the AD. After SAP import, we need to update the telephone numbers with the data from the AD.

The bulk import was done with a stored procedure and executed nightly with an SQL Server job. So it makes sense to do the AD import with a stored procedure too. I wasn't really sure whether this works via the SQL server. 

My favorite programming languages are C# and JavaScript, and I'm not really a friend of T-SQL, but I tried it. I googled around a little bit and found a solution quick solution in T-SQL.

The trick is map the AD via an LDAP provider as a linked server to the SQL Server. This can even be done via a dialogue, but I never got it running like this, so I chose the way to use T-SQL instead:

~~~ sql
USE [master]
GO 
EXEC master.dbo.sp_addlinkedserver @server = N'ADSI', @srvproduct=N'Active Directory Service Interfaces', @provider=N'ADSDSOObject', @datasrc=N'adsdatasource'
EXEC master.dbo.sp_addlinkedsrvlogin @rmtsrvname=N'ADSI',@useself=N'False',@locallogin=NULL,@rmtuser=N'<DOMAIN>\<Username>',@rmtpassword='*******'
GO 
EXEC master.dbo.sp_serveroption @server=N'ADSI', @optname=N'collation compatible',  @optvalue=N'false'
GO 
EXEC master.dbo.sp_serveroption @server=N'ADSI', @optname=N'data access', @optvalue=N'true'
GO 
EXEC master.dbo.sp_serveroption @server=N'ADSI', @optname=N'dist', @optvalue=N'false'
GO 
EXEC master.dbo.sp_serveroption @server=N'ADSI', @optname=N'pub', @optvalue=N'false'
GO 
EXEC master.dbo.sp_serveroption @server=N'ADSI', @optname=N'rpc', @optvalue=N'false'
GO 
EXEC master.dbo.sp_serveroption @server=N'ADSI', @optname=N'rpc out', @optvalue=N'false'
GO 
EXEC master.dbo.sp_serveroption @server=N'ADSI', @optname=N'sub', @optvalue=N'false'
GO 
EXEC master.dbo.sp_serveroption @server=N'ADSI', @optname=N'connect timeout', @optvalue=N'0'
GO 
EXEC master.dbo.sp_serveroption @server=N'ADSI', @optname=N'collation name', @optvalue=null
GO 
EXEC master.dbo.sp_serveroption @server=N'ADSI', @optname=N'lazy schema validation',  @optvalue=N'false'
GO 
EXEC master.dbo.sp_serveroption @server=N'ADSI', @optname=N'query timeout', @optvalue=N'0'
GO 
EXEC master.dbo.sp_serveroption @server=N'ADSI', @optname=N'use remote collation',  @optvalue=N'true'
GO 
EXEC master.dbo.sp_serveroption @server=N'ADSI', @optname=N'remote proc transaction promotion', @optvalue=N'true'
GO
~~~

You can to use this script to set-up a new linked server to AD. Just set the right user and password to the second T-SQL statement. This user should have read access to the AD. A specific service account would make sense here. Don't save the script with the user credentials in it. Once the linked server is set-up, you don't need this script anymore.

This setup was easy. The most painful part, was to setup a working query. 

~~~ sql
SELECT * FROM OpenQuery ( 
  ADSI,  
  'SELECT cn, samaccountname, mail, mobile, telephonenumber, sn, givenname, co, company
  FROM ''LDAP://DC=company,DC=domain,DC=controller'' 
  WHERE objectClass = ''User'' and co = ''Switzerland''
  ') AS tblADSI
  WHERE mail IS NOT NULL AND (telephonenumber IS NOT NULL OR mobile IS NOT NULL)
ORDER BY cn
~~~

Any error in the Query to execute resulted in an generic error message, which told me that there was an problem to built this query. Not really helpful. 

It took me two hours to find the right LDAP connection string and some more hours to find the right properties the query. 

The other painful thing are the conditions. Because the where clause outside the OpenQuery couldn't be run inside the OpenQuery. Don't ask me why. My Idea was to limit the result set completely with the query inside the OpenQuery, but was only able to limit to the objectType "User" and to the country. Also the AD need to be maintained in a proper way: e.g. the field company btw. didn't return the company (which should be the same in the entire company) but the company units.

>  BTW: the column order in the result set is completely the other way round, than defined in the query. 

Later, I could limit the result set to existing emails (to find out whether this is a real user) and existing telephone numbers.

The rest is easy: Wrap that query in a stored procedure, iterate threw all of the users, find the related ones in the database (previously imported from SAP) and update the telephone numbers.