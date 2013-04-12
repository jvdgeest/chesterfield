using System;
using MindTouch.Dream;
using MindTouch.Tasking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Chesterfield.Support;

namespace Chesterfield
{
  /* This class has the following partial classes:
   * > CouchDatabase.cs
   * > CouchDatabase.AllDocuments.cs 
   * > CouchDatabase.Attachments.cs
   * > CouchDatabase.Changes.cs
   * > CouchDatabase.Documents.cs
   * > CouchDatabase.Views.cs 
   */
  public partial class CouchDatabase : CouchBase
  {
    public CouchDatabase(XUri databaseUri)
      : base(databaseUri)
    {
    }

    public CouchDatabase(XUri databaseUri, string username, string password)
      : base(databaseUri, username, password)
    {
    }

    public CouchDatabase(Plug databasePlug)
      : base(databasePlug)
    {
    }

    /* =========================================================================
     * Asynchronous methods 
     * =======================================================================*/

    /// <summary>
    /// Retrieves information about the CouchDB database, such as the disk size
    /// and document count. See CouchDatabaseInfo for more information.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<CouchDatabaseInfo> GetInfo(Result<CouchDatabaseInfo> result)
    {
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .Get(DreamMessage.Ok(), new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Ok)
              result.Return(JsonConvert.DeserializeObject<CouchDatabaseInfo>(
                a.ToText()));
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      return result;
    }

    /// <summary>
    /// Compacts the database.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result Compact(Result result)
    {
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(Constants._COMPACT)
        .Post(DreamMessage.Ok(MimeType.JSON, String.Empty), 
          new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Accepted)
              result.Return();
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      return result;
    }

    /// <summary>
    /// Compacts a design document.
    /// </summary>
    /// <param name="designName">Name of the design document to compact.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result CompactDesignDocument(string designName, Result result)
    {
      if (String.IsNullOrEmpty(designName))
        throw new ArgumentException("designName cannot be null nor empty");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(Constants._COMPACT)
        .At(XUri.EncodeFragment(designName))
        .Post(DreamMessage.Ok(MimeType.JSON, String.Empty), 
          new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Accepted)
              result.Return();
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      return result;
    }

    public Result<JObject> UpdateHandle(string designName, string functionName, 
      Result<JObject> result)
    {
      if (String.IsNullOrEmpty(designName))
        throw new ArgumentNullException("designName");
      if (String.IsNullOrEmpty(functionName))
        throw new ArgumentNullException("functionName");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(Constants._DESIGN)
        .At(XUri.EncodeFragment(designName))
        .At(Constants._UPDATE)
        .At(XUri.EncodeFragment(functionName))
        .Post(new Result<DreamMessage>())
        .WhenDone(
          a => result.Return(JObject.Parse(a.ToText())),
          result.Throw
        );
      return result;
    }

    /* =========================================================================
     * Synchronous methods 
     * =======================================================================*/

    /// <summary>
    /// Retrieves information about the CouchDB database, such as the disk size
    /// and document count. See CouchDatabaseInfo for more information.
    /// </summary>
    /// <returns>CouchDatabaseInfo object holding the database info.</returns>
    public CouchDatabaseInfo GetInfo()
    {
      return GetInfo(new Result<CouchDatabaseInfo>()).Wait();
    }

    /// <summary>
    /// Compacts the database.
    /// </summary>
    public void Compact()
    {
      Compact(new Result()).Wait();
    }

    /// <summary>
    /// Compacts a design document.
    /// </summary>
    /// <param name="designName">Name of the design document to compact.</param>
    public void CompactDesignDocument(string designName)
    {
      CompactDesignDocument(designName, new Result()).Wait();
    }

    public JObject UpdateHandle(string designName, string functionName)
    {
      return GetView(designName, functionName, new Result<JObject>()).Wait();
    }
  }
}