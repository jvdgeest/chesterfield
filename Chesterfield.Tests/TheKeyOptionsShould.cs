using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Chesterfield.Support;

namespace Chesterfield.Tests
{
	[TestClass]
	public class TheKeyOptionsShould
	{
		[TestMethod]
		public void RespondCorrectlyToZeroItems()
		{
            // Arrange
			KeyOptions keyOptions = new KeyOptions();

            // Assert
			Assert.IsFalse(keyOptions.HasValues);
			Assert.IsTrue(keyOptions.Count == 0);
			Assert.AreEqual(String.Empty, keyOptions.ToString());
		}

		[TestMethod]
		public void RespondCorrectlyToOneItem()
		{
            // Arrange
			KeyOptions keyOptions = new KeyOptions("somevalue");

            // Assert
			Assert.IsTrue(keyOptions.HasValues);
			Assert.AreEqual(1, keyOptions.Count);
			Assert.AreEqual("\"somevalue\"", keyOptions.ToString());
		}

		[TestMethod]
		public void RespondCorrectlyToTwoItems()
		{
            // Arrange
			KeyOptions keyOptions = new KeyOptions("somevalue", 1);

            // Assert
			Assert.IsTrue(keyOptions.HasValues);
			Assert.AreEqual(2, keyOptions.Count);
			Assert.AreEqual("[\"somevalue\",1]", keyOptions.ToString());
		}

		[TestMethod]
		public void RespondCorrectlyToSubArrays()
		{
            // Arrange
			KeyOptions keyOptions = new KeyOptions("somevalue", 
                                                   new JArray(1, 2, 3));

            // Assert
			Assert.AreEqual("[\"somevalue\",[1,2,3]]",keyOptions.ToString());
		}

		[TestMethod]
		public void RespondCorrectlyToEmptyObjects()
		{
            // Arrange
			KeyOptions keyOptions = new KeyOptions("somevalue", new JObject());

            // Assert
			Assert.AreEqual("[\"somevalue\",{}]", keyOptions.ToString());
		}
	}
}