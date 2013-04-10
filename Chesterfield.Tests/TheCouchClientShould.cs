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
      CouchDatabase database = client.GetDatabase(baseDatabase);
      CouchDatabaseInfo couchDatabaseInfo = database.GetInfo();

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
  }
}