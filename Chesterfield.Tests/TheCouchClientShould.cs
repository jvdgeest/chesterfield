using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MindTouch.Tasking;
using MindTouch.Dream;
using Newtonsoft.Json.Linq;
using Chesterfield.Interfaces;
using Chesterfield.Support;

namespace Chesterfield.IntegrationTest
{
  [TestClass]
  public class TheCouchClientShould
  {
    private CouchClient client;
    private string baseDatabase = "chesterfield-test";
    private string replicateDatabase = "chesterfield-replicate-test";

    [TestInitialize, TestMethod]
    public void CreateDatabases()
    {
      // Arrange
      client = new CouchClient(
        aHost: ConfigurationManager.AppSettings["Host"],
        aPort: Int32.Parse(ConfigurationManager.AppSettings["Port"]),
        aUserName: ConfigurationManager.AppSettings["Username"],
        aPassword: ConfigurationManager.AppSettings["Password"]
      );
      DeleteDatabases();

      // Act
      client.CreateDatabase(baseDatabase);
      client.CreateDatabase(replicateDatabase);

      // Assert
      Assert.IsTrue(client.HasDatabase(baseDatabase));
      Assert.IsTrue(client.HasDatabase(replicateDatabase));
    }

    [TestCleanup, TestMethod]
    public void DeleteDatabases()
    {
      // Act
      if (client.HasDatabase(baseDatabase))
      {
        client.DeleteDatabase(baseDatabase);
      }
      if (client.HasDatabase(replicateDatabase))
      {
        client.DeleteDatabase(replicateDatabase);
      }

      // Assert
      Assert.IsFalse(client.HasDatabase(baseDatabase));
      Assert.IsFalse(client.HasDatabase(replicateDatabase));
    }

    [TestMethod]
    public void ReturnListWithDatabases()
    {
      // Arrange
      IEnumerable<string> databases = client.GetAllDatabases();

      // Assert
      Assert.IsNotNull(databases);
      Assert.IsTrue(databases.Contains(baseDatabase));
      Assert.IsTrue(databases.Contains(replicateDatabase));
    }

    [TestMethod]
    public void ReturnDatabaseInformation()
    {
      // Arrange
      CouchDatabase db = client.GetDatabase(baseDatabase);
      CouchDatabaseInfo couchDatabaseInfo = db.GetInfo();

      // Assert
      Assert.AreEqual(baseDatabase, couchDatabaseInfo.Name);
      Assert.AreEqual(false, couchDatabaseInfo.CompactRunning);
      Assert.AreNotEqual(0, couchDatabaseInfo.DiskFormatVersion);
      Assert.AreNotEqual(0, couchDatabaseInfo.DiskSize);
      Assert.AreEqual(0, couchDatabaseInfo.DocCount);
      Assert.AreEqual(0, couchDatabaseInfo.DocDeletedCount);
      Assert.AreNotEqual(0, couchDatabaseInfo.InstanceStartTimeMs);
      Assert.AreNotEqual(DateTime.MinValue, couchDatabaseInfo.InstanceStartTime);
    }

    [TestMethod]
    public void CreateDocumentFromStringWithCustomId()
    {
      // Arrange
      CouchDatabase db = client.GetDatabase(baseDatabase);
      string obj = @"{""test"": ""prop""}";
      string id = Guid.NewGuid().ToString("N");

      // Act
      var result = db.CreateDocument(id, obj, new Result<string>()).Wait();

      // Assert
      Assert.IsNotNull(db.GetDocument<CouchDocument>(id));
    }

    [TestMethod]
    public void CreateDocumentFromStringWithGeneratedId()
    {
      // Arrange
      CouchDatabase db = client.GetDatabase(baseDatabase);
      JDocument obj = new JDocument(@"{""test"": ""prop""}");

      // Act
      var result = db.CreateDocument(obj);

      // Assert
      Assert.IsNotNull(result.Id);
      Assert.AreEqual("prop", result.Value<string>("test"));
    }

    [TestMethod]
    public void SaveExistingDocument()
    {
      // Arrange
      CouchDatabase db = client.GetDatabase(baseDatabase);
      JDocument obj = new JDocument(@"{""test"": ""prop""}");
      obj.Id = Guid.NewGuid().ToString("N");

      // Act
      var result = db.CreateDocument(obj);
      var doc = db.GetDocument<JDocument>(obj.Id);
      doc["test"] = "newprop";
      var newresult = db.UpdateDocument(doc);
      
      // Assert
      Assert.AreEqual(newresult.Value<string>("test"), "newprop");
    }

    [TestMethod]
    public void DeleteDocument()
    {
      // Arrange
      CouchDatabase db = client.GetDatabase(baseDatabase);
      string id = Guid.NewGuid().ToString("N");
      db.CreateDocument(id, "{}", new Result<string>()).Wait();
      var doc = db.GetDocument<CouchDocument>(id);
      
      // Act
      db.DeleteDocument(doc);

      // Assert
      Assert.IsNull(db.GetDocument<CouchDocument>(id));
    }

    [TestMethod]
    public void DetermineIfDocumentHasAttachment()
    {
      // Arrange
      CouchDatabase db = client.GetDatabase(baseDatabase);
      string id = Guid.NewGuid().ToString("N");
      db.CreateDocument(id, "{}", new Result<string>()).Wait();

      // Act
      using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes("This is a text document")))
      {
        db.AddAttachment(id, ms, "test.txt");
      }
      var doc = db.GetDocument<CouchDocument>(id);

      // Assert
      Assert.IsTrue(doc.HasAttachment);
    }
  }
}