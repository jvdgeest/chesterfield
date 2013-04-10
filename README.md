Chesterfield
========

Chesterfield is a [CouchDB](http://couchdb.apache.org/) client for the 
.NET platform. It is based on [LoveSeat](https://github.com/soitgoes/LoveSeat) 
from [Martin Murphy](https://github.com/soitgoes) and 
[DreamSeat](https://github.com/soitgoes/DreamSeat) from 
[Vincent Daron](https://github.com/vdaron).

Chesterfield is also based on:

 * [Mindtouch Dream](https://github.com/MindTouch/DReAM).
 * [Newtonsoft.Json](http://json.codeplex.com/)

Thanks to Mindtouch Dream, all the API calls can be executed asychronously or 
sychronously.

Tested compatibility
====================

 * CouchDB 1.0 and 1.1
 * .NET Framework 4.0 or Mono 2.9 (compiled master branch from Nov 20 2010)

Main features
=============

 * Complete synchronous/asynchronous API
 * Manage databases, documents, attachments, views, users, replication, 
   change notifications, ...

Chesterfield usage
==================

## Basics

### Synchronous

    // assumes localhost:5984 and Admin Party if constructor is left blank
    var client = new CouchClient();
    var db = client.GetDatabase("Northwind");
    
    // get document by ID (return a object derived from [JObject](http://james.newtonking.com/projects/json/help/html/T_Newtonsoft_Json_Linq_JObject.htm))
    var doc = GetDocument<JDocument>(string id);

    // get document by ID (strongly typed POCO version)
    var myObj = db.GetDocument<MyObject>("12345");

    // You can also use the asynchronous method signatures asking to Wait()
    var db2 = client.GetDatabase("Northwind", new Result<CouchDatabase>()).Wait();

### Asynchronous

    // assumes localhost:5984 and Admin Party if constructor is left blank
    var client = new CouchClient();
    client.GetDatabase("Northwind", new Result<CouchDatabase>()).WhenDone(
        a => DatabaseOpened(a),
        e => ProcessException(e)
        );
    }