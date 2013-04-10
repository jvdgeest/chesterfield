using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MindTouch.Tasking;
using MindTouch.Dream;
using Newtonsoft.Json.Linq;
using System.Configuration;
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
  }
}