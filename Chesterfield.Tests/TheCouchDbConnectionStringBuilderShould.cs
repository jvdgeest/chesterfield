using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Chesterfield.Support;

namespace Chesterfield.Tests
{
  [TestClass]
  public class TheCouchDbConnectionStringBuilderShould
  {
    [TestMethod]
    public void ParseConnectionString()
    {
      // Arrange
      CouchDbConnectionStringBuilder builder = 
        new CouchDbConnectionStringBuilder(
          "Host=test;port=10;username=un;Password=coucou;SslEnabled=true");

      // Assert
      Assert.AreEqual("test", builder.Host);
      Assert.AreEqual("un", builder.Username);
      Assert.AreEqual(10, builder.Port);
      Assert.AreEqual("coucou", builder.Password);
      Assert.AreEqual(true, builder.SslEnabled);
    }

    [TestMethod]
    public void UseCorrectDefaultValues()
    {
      // Arrange
      CouchDbConnectionStringBuilder builder = 
        new CouchDbConnectionStringBuilder(String.Empty);

      // Assert
      Assert.AreEqual("localhost", builder.Host);
      Assert.AreEqual(String.Empty, builder.Username);
      Assert.AreEqual(5984, builder.Port);
      Assert.AreEqual(String.Empty, builder.Password);
      Assert.AreEqual(false, builder.SslEnabled);
    }
  }
}