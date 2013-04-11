using System;
using System.IO;
using MindTouch.Dream;
using MindTouch.Tasking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Chesterfield.Interfaces;
using Chesterfield.Support;

namespace Chesterfield
{
  public partial class CouchDatabase : CouchBase
  {
    public CouchDatabase(XUri aDatabaseUri)
      : base(aDatabaseUri)
    {
    }

    public CouchDatabase(XUri aDatabaseUri, string aUsername, string aPassword)
      : base(aDatabaseUri, aUsername, aPassword)
    {
    }

    public CouchDatabase(Plug aDatabasePlug)
      : base(aDatabasePlug)
    {
    }

    /// <summary>
    /// Retrieve DatabaseInformation
    /// </summary>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<CouchDatabaseInfo> GetInfo(Result<CouchDatabaseInfo> aResult)
    {
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      BasePlug.Get(DreamMessage.Ok(), new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if (a.Status == DreamStatus.Ok)
            aResult.Return(JsonConvert.DeserializeObject<CouchDatabaseInfo>(a.ToText()));
          else
            aResult.Throw(new CouchException(a));
        },
        aResult.Throw
      );
      return aResult;
    }
    public CouchDatabaseInfo GetInfo()
    {
      return GetInfo(new Result<CouchDatabaseInfo>()).Wait();
    }

    /// <summary>
    /// Request compaction of the specified database. Compaction compresses the disk database file
    /// </summary>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result Compact(Result aResult)
    {
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      BasePlug.At(Constants.COMPACT).Post(DreamMessage.Ok(MimeType.JSON, String.Empty), new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if (a.Status == DreamStatus.Accepted)
          {
            aResult.Return();
          }
          else
          {
            aResult.Throw(new CouchException(a));
          }
        },
        aResult.Throw
      );
      return aResult;
    }
    public void Compact()
    {
      Compact(new Result()).Wait();
    }

    /// <summary>
    /// Compacts the view indexes associated with the specified design document. You can use this in place of the full database compaction if
    /// you know a specific set of view indexes have been affected by a recent database change
    /// </summary>
    /// <param name="aDocumentViewId">Design Document id to compact</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result CompactDocumentView(string aDocumentViewId, Result aResult)
    {
      if (String.IsNullOrEmpty(aDocumentViewId))
        throw new ArgumentException("aDocumentViewId cannot be null nor empty");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      BasePlug.At(Constants.COMPACT).At(XUri.EncodeFragment(aDocumentViewId)).Post(DreamMessage.Ok(MimeType.JSON, String.Empty), new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if (a.Status == DreamStatus.Accepted)
          {
            aResult.Return();
          }
          else
          {
            aResult.Throw(new CouchException(a));
          }
        },
        aResult.Throw
      );
      return aResult;
    }
    public void CompactDocumentView(string aDocumentViewId)
    {
      CompactDocumentView(aDocumentViewId, new Result()).Wait();
    }

    #region Change Management
    /// <summary>
    /// Request database changes
    /// </summary>
    /// <param name="aChangeOptions">Change options</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<CouchChanges> GetChanges(ChangeOptions aChangeOptions, Result<CouchChanges> aResult)
    {
      if (aChangeOptions == null)
        throw new ArgumentNullException("aOptions");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      aChangeOptions.Feed = ChangeFeed.Normal;

      BasePlug.At(Constants._CHANGES).With(aChangeOptions).Get(new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if (a.Status == DreamStatus.Ok)
          {
            ObjectSerializer<CouchChanges> serializer = new ObjectSerializer<CouchChanges>();
            aResult.Return(serializer.Deserialize(a.ToText()));
          }
          else
          {
            aResult.Throw(new CouchException(a));
          }
        },
        aResult.Throw
      );

      return aResult;
    }
    /// <summary>
    /// Request database changes including documents
    /// </summary>
    /// <typeparam name="T">Type of document used while returning changes</typeparam>
    /// <param name="aOptions">Change options</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<CouchChanges<T>> GetChanges<T>(ChangeOptions aOptions, Result<CouchChanges<T>> aResult) where T : ICouchDocument
    {
      if (aOptions == null)
        throw new ArgumentNullException("aOptions");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      aOptions.Feed = ChangeFeed.Normal;
      aOptions.IncludeDocs = true;

      BasePlug.At(Constants._CHANGES).With(aOptions).Get(new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if (a.Status == DreamStatus.Ok)
          {
            ObjectSerializer<CouchChanges<T>> serializer = new ObjectSerializer<CouchChanges<T>>();
            aResult.Return(serializer.Deserialize(a.ToText()));
          }
          else
          {
            aResult.Throw(new CouchException(a));
          }
        },
        aResult.Throw
      );

      return aResult;
    }
    /// <summary>
    /// Request continuous changes from database
    /// </summary>
    /// <param name="aChangeOptions">Change options</param>
    /// <param name="aCallback">Callback used for each change notification</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<CouchContinuousChanges> GetCoutinuousChanges(
      ChangeOptions aChangeOptions,
      CouchChangeDelegate aCallback,
      Result<CouchContinuousChanges> aResult)
    {
      if (aChangeOptions == null)
        throw new ArgumentNullException("aChangeOptions");
      if (aCallback == null)
        throw new ArgumentNullException("aCallback");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      aChangeOptions.Feed = ChangeFeed.Continuous;
      BasePlug.At(Constants._CHANGES).With(aChangeOptions).InvokeEx(Verb.GET, DreamMessage.Ok(), new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if (a.IsSuccessful)
          {
            aResult.Return(new CouchContinuousChanges(a, aCallback));
          }
          else
          {
            aResult.Throw(new CouchException(a));
          }
        },
        aResult.Throw
      );

      return aResult;
    }
    /// <summary>
    /// Request continuous changes from database including Documents
    /// </summary>
    /// <typeparam name="T">>Type of document used while returning changes</typeparam>
    /// <param name="aChangeOptions">Change options</param>
    /// <param name="aCallback">Callback used for each change notification</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<CouchContinuousChanges<T>> GetCoutinuousChanges<T>(
      ChangeOptions aChangeOptions,
      CouchChangeDelegate<T> aCallback,
      Result<CouchContinuousChanges<T>> aResult) where T : ICouchDocument
    {
      if (aChangeOptions == null)
        throw new ArgumentNullException("aChangeOptions");
      if (aCallback == null)
        throw new ArgumentNullException("aCallback");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      aChangeOptions.Feed = ChangeFeed.Continuous;
      aChangeOptions.IncludeDocs = true;

      BasePlug.At(Constants._CHANGES).With(aChangeOptions).InvokeEx(Verb.GET, DreamMessage.Ok(), new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if (a.IsSuccessful)
          {
            aResult.Return(new CouchContinuousChanges<T>(a, aCallback));
          }
          else
          {
            aResult.Throw(new CouchException(a));
          }
        },
        aResult.Throw
      );

      return aResult;
    }

    /// <summary>
    /// Request database changes
    /// </summary>
    /// <param name="options">Change options</param>
    /// <returns></returns>
    public CouchChanges GetChanges(ChangeOptions aChangeOptions)
    {
      return GetChanges(aChangeOptions, new Result<CouchChanges>()).Wait();
    }

    /// <summary>
    /// Request database changes including documents
    /// </summary>
    /// <typeparam name="T">Type of document used while returning changes</typeparam>
    /// <param name="aChangeOptions">Change options</param>
    /// <returns></returns>
    public CouchChanges<T> GetChanges<T>(ChangeOptions aChangeOptions) where T : ICouchDocument
    {
      return GetChanges(aChangeOptions, new Result<CouchChanges<T>>()).Wait();
    }
    /// <summary>
    /// Request continuous changes from database
    /// </summary>
    /// <param name="aChangeOptions">Change options</param>
    /// <param name="aCallback">Callback used for each change notification</param>
    /// <returns></returns>
    public CouchContinuousChanges GetCoutinuousChanges(ChangeOptions aChangeOptions, CouchChangeDelegate aCallback)
    {
      return GetCoutinuousChanges(aChangeOptions, aCallback, new Result<CouchContinuousChanges>()).Wait();
    }
    /// <summary>
    /// Request continuous changes from database including Documents
    /// </summary>
    /// <typeparam name="T">>Type of document used while returning changes</typeparam>
    /// <param name="aChangeOptions">Change options</param>
    /// <param name="aCallback">Callback used for each change notification</param>
    /// <returns></returns>
    public CouchContinuousChanges<T> GetCoutinuousChanges<T>(ChangeOptions aChangeOptions, CouchChangeDelegate<T> aCallback) where T : ICouchDocument
    {
      return GetCoutinuousChanges(aChangeOptions, aCallback, new Result<CouchContinuousChanges<T>>()).Wait();
    }

    #endregion

    #region Update Handlers
    #region Asynchronous methods

    public Result<JObject> UpdateHandle(string designId, string functionName, Result<JObject> result)
    {
      if (String.IsNullOrEmpty(designId))
        throw new ArgumentNullException("designId");
      if (String.IsNullOrEmpty(functionName))
        throw new ArgumentNullException("functionName");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug.At(Constants.DESIGN, XUri.EncodeFragment(designId), Constants.UPDATE, XUri.EncodeFragment(functionName)).Post(new Result<DreamMessage>()).WhenDone(
          a => result.Return(JObject.Parse(a.ToText())),
          result.Throw
      );
      return result;
    }

    #endregion

    #region Synchronous methods

    public JObject UpdateHandle(string designId, string functionName)
    {
      return GetView(designId, functionName, new Result<JObject>()).Wait();
    }

    #endregion
    #endregion
  }
}