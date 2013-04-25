using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MindTouch.Tasking;

namespace Chesterfield.IntegrationTest
{
  [TestClass]
  public class TheCouchClientShould
  {
    private static string BASE_DB = "chesterfield-test";
    private CouchClient _client;

    [TestInitialize, TestMethod]
    public void CreateDatabase()
    {
      // Arrange
      _client = new CouchClient(
        host: ConfigurationManager.AppSettings["Host"],
        port: Int32.Parse(ConfigurationManager.AppSettings["Port"]),
        username: ConfigurationManager.AppSettings["Username"],
        password: ConfigurationManager.AppSettings["Password"]
      );
      DeleteDatabase();

      // Act
      _client.GetDatabase(BASE_DB);

      // Assert
      Assert.IsTrue(_client.HasDatabase(BASE_DB));
    }

    [TestCleanup, TestMethod]
    public void DeleteDatabase()
    {
      // Act
      if (_client.HasDatabase(BASE_DB))
        _client.DeleteDatabase(BASE_DB);

      // Assert
      Assert.IsFalse(_client.HasDatabase(BASE_DB));
    }

    [TestMethod]
    public void ReturnListWithDatabases()
    {
      // Act
      IEnumerable<string> databases = _client.GetAllDatabases();

      // Assert
      Assert.IsNotNull(databases);
      Assert.IsTrue(databases.Contains(BASE_DB));
    }

    [TestMethod]
    public void ReturnConfigValue()
    {
      // Arrange
      _client.SetConfigValue("chesterfield", "key", "value");

      // Assert
      Assert.AreEqual("value", _client.GetConfigValue("chesterfield", "key"));
    }

    [TestMethod]
    public void DeleteConfigValue()
    {
      // Arrange
      _client.SetConfigValue("chesterfield", "key", "value");

      // Act
      _client.DeleteConfigValue("chesterfield", "key");

      // Assert
      Assert.IsNull(_client.GetConfigValue("chesterfield", "key"));
    }

    [TestMethod]
    public void ReadConfigSection()
    {
      // Arrange
      _client.SetConfigValue("chesterfield", "key", "value");

      // Act
      Dictionary<string, string> section = _client.GetConfigSection(
        "chesterfield", new Result<Dictionary<string, string>>()).Wait();

      // Assert
      Assert.AreEqual(1, section.Count);
      Assert.IsTrue(section.ContainsKey("key"));
      Assert.AreEqual("value", section["key"]);
    }

    [TestMethod]
    public void ReadConfigs()
    {
      // Act
      Dictionary<string, Dictionary<string, string>> config = _client.GetConfig(
        new Result<Dictionary<string, Dictionary<string, string>>>()).Wait();

      // Assert
      Assert.IsTrue(config.Count > 0);
    }
  }
}