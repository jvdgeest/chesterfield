using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using MindTouch.Tasking;
using MindTouch.Dream;
using Chesterfield.Interfaces;
using Chesterfield.Support;

namespace Chesterfield.Tests
{
  [TestClass]
  public class TheCouchDatabaseShould
  {
    public class TestSubClass : CouchDocument
    {
      public string TESTVAL { get; set; }
    }

    private static string BASE_DB = "chesterfield-test";
    private CouchClient _client;
    private CouchDatabase _db;

    [TestInitialize]
    public void CreateDatabase()
    {
      _client = new CouchClient(
        host: ConfigurationManager.AppSettings["Host"],
        port: Int32.Parse(ConfigurationManager.AppSettings["Port"]),
        username: ConfigurationManager.AppSettings["Username"],
        password: ConfigurationManager.AppSettings["Password"]
      );

      DeleteDatabase();
      _db = _client.GetDatabase(BASE_DB);
    }

    [TestCleanup]
    public void DeleteDatabase()
    {
      if (_client.HasDatabase(BASE_DB))
        _client.DeleteDatabase(BASE_DB);
    }

    [TestMethod]
    public void ReturnDatabaseInformation()
    {
      // Act
      CouchDatabaseInfo couchDatabaseInfo = _db.GetInfo();

      // Assert
      Assert.AreEqual(BASE_DB, couchDatabaseInfo.Name);
      Assert.AreEqual(false, couchDatabaseInfo.CompactRunning);
      Assert.AreNotEqual(0, couchDatabaseInfo.DiskFormatVersion);
      Assert.AreNotEqual(0, couchDatabaseInfo.DiskSize);
      Assert.AreEqual(0, couchDatabaseInfo.DocCount);
      Assert.AreEqual(0, couchDatabaseInfo.DocDeletedCount);
      Assert.AreNotEqual(0, couchDatabaseInfo.InstanceStartTimeMs);
      Assert.AreNotEqual(DateTime.MinValue,
        couchDatabaseInfo.InstanceStartTime);
    }

    [TestMethod]
    public void CreateDocumentFromStringWithCustomId()
    {
      // Arrange
      string obj = @"{""test"": ""prop""}";
      string id = Guid.NewGuid().ToString("N");

      // Act
      var result = _db.CreateDocument(id, obj, new Result<string>()).Wait();

      // Assert
      Assert.IsNotNull(_db.GetDocument<CouchDocument>(id));
    }

    [TestMethod]
    public void CreateDocumentFromStringWithGeneratedId()
    {
      // Arrange
      JDocument obj = new JDocument(@"{""test"": ""prop""}");

      // Act
      var result = _db.CreateDocument(obj);

      // Assert
      Assert.IsNotNull(result.Id);
      Assert.AreEqual("prop", result.Value<string>("test"));
    }

    [TestMethod]
    public void SaveExistingDocument()
    {
      // Arrange
      JDocument obj = new JDocument(@"{""test"": ""prop""}");
      obj.Id = Guid.NewGuid().ToString("N");

      // Act
      var result = _db.CreateDocument(obj);
      var doc = _db.GetDocument<JDocument>(obj.Id);
      doc["test"] = "newprop";
      var newresult = _db.UpdateDocument(doc);

      // Assert
      Assert.AreEqual(newresult.Value<string>("test"), "newprop");
    }

    [TestMethod]
    public void DeleteDocument()
    {
      // Arrange
      string id = Guid.NewGuid().ToString("N");
      _db.CreateDocument(id, "{}", new Result<string>()).Wait();
      var doc = _db.GetDocument<CouchDocument>(id);

      // Act
      _db.DeleteDocument(doc);

      // Assert
      Assert.IsNull(_db.GetDocument<CouchDocument>(id));
    }

    [TestMethod]
    public void DetermineIfDocumentHasAttachment()
    {
      // Arrange
      string id = Guid.NewGuid().ToString("N");
      _db.CreateDocument(id, "{}", new Result<string>()).Wait();

      // Act
      using (MemoryStream ms = new MemoryStream(
        Encoding.UTF8.GetBytes("This is a text document")))
      {
        _db.AddAttachment(id, ms, "test.txt");
      }
      var doc = _db.GetDocument<CouchDocument>(id);

      // Assert
      Assert.IsTrue(doc.HasAttachment);
    }

    [TestMethod]
    public void DetermineIfDocumentHasNoAttachment()
    {
      // Arrange
      string id = Guid.NewGuid().ToString("N");
      _db.CreateDocument(id, "{}", new Result<string>()).Wait();
      var doc = _db.GetDocument<CouchDocument>(id);

      // Assert
      Assert.IsFalse(doc.HasAttachment);
    }

    [TestMethod]
    public void ReturnAttachmentNames()
    {
      // Arrange
      string id = Guid.NewGuid().ToString("N");
      _db.CreateDocument(id, "{}", new Result<string>()).Wait();

      // Act
      using (MemoryStream ms = new MemoryStream(
        Encoding.UTF8.GetBytes("This is a text document")))
      {
        _db.AddAttachment(id, ms, "test.txt");
      }
      var doc = _db.GetDocument<CouchDocument>(id);

      // Assert
      Assert.IsTrue(doc.GetAttachmentNames().Contains("test.txt"));
    }

    [TestMethod]
    public void CreateAttachment()
    {
      // Arrange
      _db.CreateDocument(@"{""_id"":""test_upload""}",
        new Result<string>()).Wait();
      var doc = _db.GetDocument<CouchDocument>("test_upload");
      var attachment = Encoding.UTF8.GetBytes("test");
      using (MemoryStream ms = new MemoryStream(
        Encoding.UTF8.GetBytes("This is a text document")))
      {
        _db.AddAttachment("test_upload", ms, "test_upload.txt");
      }

      // Act
      string result;
      using (Stream stream = _db.GetAttachment(doc, "test_upload.txt"))
      {
        using (StreamReader reader = new StreamReader(stream))
        {
          result = reader.ReadToEnd();
        }
      }

      // Assert
      Assert.IsTrue(result == "This is a text document");
    }

    [TestMethod]
    public void DeleteAttachment()
    {
      // Arrange
      _db.CreateDocument(@"{""_id"":""test_delete""}",
        new Result<string>()).Wait();
      _db.GetDocument<CouchDocument>("test_delete");
      using (MemoryStream ms = new MemoryStream(
        Encoding.UTF8.GetBytes("This is a text document")))
      {
        _db.AddAttachment("test_delete", ms, "test_upload.txt");
      }

      // Act
      _db.DeleteAttachment("test_delete", "test_upload.txt");
      var retrieved = _db.GetDocument<CouchDocument>("test_delete");

      // Assert
      Assert.IsFalse(retrieved.HasAttachment);
    }

    [TestMethod]
    public void CreateAttachmentInSubCouchDocumentClass()
    {
      // Arrange
      TestSubClass tsc = new TestSubClass { TESTVAL = "Hello" };
      tsc = _db.CreateDocument(tsc, new Result<TestSubClass>()).Wait();

      // Act
      var attachment = Encoding.UTF8.GetBytes("test");
      using (MemoryStream ms = new MemoryStream(
        Encoding.UTF8.GetBytes("This is a text document")))
      {
        _db.AddAttachment(tsc.Id, ms, "test_upload.txt");
      }
      tsc = _db.GetDocument(tsc.Id, new Result<TestSubClass>()).Wait();

      // Assert
      Assert.IsTrue(tsc.HasAttachment);
    }


    [TestMethod]
    public void ReturnEtagInViewResults()
    {
      // Act
      _db.CreateDocument(@"{""_id"":""eTag""}", new Result<string>()).Wait();
      ViewResult<string, JObject> result = _db.GetAllDocuments(
        new Result<ViewResult<string, JObject>>()).Wait();

      // Assert
      Assert.IsTrue(!string.IsNullOrEmpty(result.ETag));
    }

    [TestMethod]
    public void Return304IfEtagMatches()
    {
      // Act
      _db.CreateDocument(@"{""_id"":""test_eTag_exception""}",
        new Result<string>()).Wait();
      ViewResult<string, JObject> result =
        _db.GetAllDocuments(new Result<ViewResult<string, JObject>>()).Wait();
      ViewResult<string, JObject> cachedResult = _db.GetAllDocuments(
        new ViewOptions { Etag = result.ETag },
        new Result<ViewResult<string, JObject>>()).Wait();

      // Assert
      Assert.AreEqual(DreamStatus.NotModified, cachedResult.Status);
    }

    [TestMethod]
    public void CreateViewDocument()
    {
      // Act
      CouchDesignDocument view = new CouchDesignDocument("testviewitem");
      view.Views.Add("testview",
        new CouchView("function(doc) {emit(doc._rev, doc)}"));
      _db.CreateDocument(view);

      // Assert
      Assert.IsNotNull(view.Rev);
    }

    [TestMethod]
    public void ReturnViewResults()
    {
      // Arrange
      CreateViewDocument();
      _db.CreateDocument(new JDocument());
      _db.CreateDocument(new JDocument());
      _db.CreateDocument(new JDocument());

      // Act
      ViewResult<string, JObject> result = _db.GetView("testviewitem",
        "testview", new Result<ViewResult<string, JObject>>()).Wait();

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.TotalRows > 0);
    }

    [TestMethod]
    public void ReturnViewResultsWithDocuments()
    {
      // Arrange
      CreateViewDocument();
      _db.CreateDocument(new JDocument());
      _db.CreateDocument(new JDocument());
      _db.CreateDocument(new JDocument());

      // Act
      ViewResult<string, JObject, JDocument> result =
        _db.GetView<string, JObject, JDocument>("testviewitem", "testview");

      // Assert
      Assert.IsNotNull(result);
      foreach (ViewResultRow<string, JObject, JDocument> row in result.Rows)
      {
        Assert.IsNotNull(row.Doc);
        Assert.IsNotNull(row.Key);
        Assert.IsNotNull(row.Id);
        Assert.IsNotNull(row.Value);
      }
    }

    [TestMethod]
    public void ReturnViewResultsAsJObject()
    {
      // Arrange
      CreateViewDocument();
      _db.CreateDocument(new JDocument());
      _db.CreateDocument(new JDocument());
      _db.CreateDocument(new JDocument());

      // Act
      JObject result = _db.GetView("testviewitem", "testview",
        new Result<JObject>()).Wait();

      // Assert
      Assert.IsNotNull(result);
      Assert.IsNotNull(result["rows"]);
    }

    [TestMethod]
    public void RunTestView()
    {
      // Arrange
      _db.CreateDocument("id1", "{}", new Result<string>()).Wait();

      // Act
      CouchDesignDocument doc = new CouchDesignDocument("test_compactview");
      ViewResult<string, JObject> result = _db.GetTempView<string, JObject>(
        new CouchView("function(doc) { emit(null, doc) }"));

      // Assert
      Assert.IsNotNull(result);
      Assert.AreEqual(DreamStatus.Ok, result.Status);
    }

    [TestMethod]
    public void GetChanges()
    {
      // Arrange
      _db.CreateDocument(null, "{}", new Result<string>()).Wait();

      // Act
      CouchChanges changes = _db.GetChanges(new ChangeOptions(),
        new Result<CouchChanges>()).Wait();

      // Assert
      Assert.AreEqual(1, changes.Results.Length);
      Assert.IsNotNull(changes.Results[0].Changes);
      Assert.IsNotNull(changes.Results[0].Id);
      Assert.IsNotNull(changes.Results[0].Sequence);
    }

    [TestMethod]
    public void GetChangesWithDocument()
    {
      // Arrange
      _db.CreateDocument(null, "{}", new Result<string>()).Wait();

      // Act
      CouchChanges<JDocument> changes = _db.GetChanges(new ChangeOptions(),
        new Result<CouchChanges<JDocument>>()).Wait();

      // Assert
      Assert.AreEqual(1, changes.Results.Length);
      Assert.IsNotNull(changes.Results[0].Doc);
      Assert.IsNotNull(changes.Results[0].Changes);
      Assert.IsNotNull(changes.Results[0].Id);
      Assert.IsNotNull(changes.Results[0].Sequence);
    }

    [TestMethod]
    public void GetContiniousChanges()
    {
      // Arrange
      AutoResetEvent evt = new AutoResetEvent(false);
      string id = null;

      // Act
      using (CouchContinuousChanges ccc =
        _db.GetCoutinuousChanges(new ChangeOptions() { Since = 0 }, (x, y) =>
        {
          try
          {
            id = y.Id;

            // Assert
            Assert.IsNotNull(y.Id);
            Assert.IsTrue(y.Sequence > 0);
          }
          finally
          {
            evt.Set();
          }
        }, new Result<CouchContinuousChanges>()).Wait())
      {
        JDocument result = _db.CreateDocument(new JDocument(),
          new Result<JDocument>()).Wait();
        evt.WaitOne();

        // Assert
        Assert.AreEqual(result.Id, id);
      }
    }

    [TestMethod]
    public void GetContiniousChangesWithDocument()
    {
      // Arrange
      AutoResetEvent evt = new AutoResetEvent(false);
      string id = null;

      // Act
      using (CouchContinuousChanges<JDocument> ccc =
        _db.GetCoutinuousChanges<JDocument>(new ChangeOptions() { Since = 0 },
          (x, y) =>
          {
            try
            {
              id = y.Doc.Id;

              // Assert
              Assert.IsNotNull(y.Doc);
              Assert.IsNotNull(y.Id);
              Assert.IsTrue(y.Sequence > 0);
            }
            finally
            {
              evt.Set();
            }
          }, new Result<CouchContinuousChanges<JDocument>>()).Wait())
      {
        JDocument result = _db.CreateDocument(new JDocument(),
          new Result<JDocument>()).Wait();
        evt.WaitOne();

        // Assert
        Assert.AreEqual(result.Id, id);
      }
    }

    [TestMethod]
    public void RunUpdateHandlerWithHttpResponse()
    {
      // Arrange
      CouchDesignDocument view = new CouchDesignDocument("design");
      view.Updates["test"] = @"function(doc, req) { return [null, 'OK']; }";
      _db.CreateDocument(view);

      // Act
      UpdateHttpResponse response =
        _db.UpdateHandle<UpdateHttpResponse>("design", "test");

      // Assert
      Assert.IsNull(response.Rev);
      Assert.AreEqual("OK", response.HttpResponse);
    }

    [TestMethod]
    public void RunUpdateHandlerWithHttpResponseAndRevision()
    {
      // Arrange
      CouchDesignDocument view = new CouchDesignDocument("design");
      view.Updates["test"] = @"function(doc, req) { 
        doc = {'_id': req.uuid}; return [doc, 'OK']; }";
      _db.CreateDocument(view);

      // Act
      UpdateHttpResponse response =
        _db.UpdateHandle<UpdateHttpResponse>("design", "test");

      // Assert
      Assert.IsNotNull(response.Rev);
      Assert.AreEqual("OK", response.HttpResponse);
    }

    [TestMethod]
    public void RunUpdateHandlerWithJsonResponse()
    {
      // Arrange
      CouchDesignDocument view = new CouchDesignDocument("design");
      view.Updates["test"] = @"function(doc, req) { 
        doc = {'_id': req.uuid, 'test': 'OK'}; return [doc, toJSON(doc)]; }";
      _db.CreateDocument(view);

      // Act
      UpdateJsonResponse response =
        _db.UpdateHandle<UpdateJsonResponse>("design", "test");

      // Assert
      Assert.IsNotNull(response.Rev);
      Assert.IsNotNull(response.Response["_id"]);
      Assert.IsNotNull(response.Response["test"]);
      Assert.AreEqual("OK", response.Response["test"]);
    }

    [TestMethod]
    public void RunUpdateHandlerWithDocumentResponse()
    {
      // Arrange
      CouchDesignDocument view = new CouchDesignDocument("design");
      view.Updates["test"] = @"function(doc, req) { 
        doc = {'_id': req.uuid, 'testval': 'OK'}; return [doc, toJSON(doc)]; }";
      _db.CreateDocument(view);

      // Act
      UpdateDocResponse<TestSubClass> response =
        _db.UpdateHandle<UpdateDocResponse<TestSubClass>>("design", "test");

      // Assert
      Assert.IsNotNull(response.Rev);
      Assert.IsNotNull(response.Response.Id);
      Assert.IsNotNull(response.Response.TESTVAL);
      Assert.AreEqual("OK", response.Response.TESTVAL);
    }
  }
}
