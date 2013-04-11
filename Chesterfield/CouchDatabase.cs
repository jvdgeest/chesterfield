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

    #region Documents Management

    #region Primitives methods
    /// <summary>
    /// Creates a document when you intend for Couch to generate the id for you.
    /// </summary>
    /// <param name="aJson">Json for creating the document</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<string> CreateDocument(string aJson, Result<string> aResult)
    {
      return CreateDocument(null, aJson, aResult);
    }
    /// <summary>
    /// Creates a document using the json provided. 
    /// No validation or smarts attempted here by design for simplicities sake
    /// </summary>
    /// <param name="anId">Id of Document, if null or empty, id will be generated by the server</param>
    /// <param name="aJson"></param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<string> CreateDocument(string anId, string aJson, Result<string> aResult)
    {
      if (String.IsNullOrEmpty(aJson))
        throw new ArgumentNullException("aJson");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      JObject jobj = JObject.Parse(aJson);
      if (jobj.Value<object>(Constants._REV) != null)
        jobj.Remove(Constants._REV);

      Plug p = BasePlug;
      string verb = Verb.POST;
      if (!String.IsNullOrEmpty(anId))
      {
        p = p.AtPath(XUri.EncodeFragment(anId));
        verb = Verb.PUT;
      }

      p.Invoke(verb, DreamMessage.Ok(MimeType.JSON, jobj.ToString(Formatting.None)), new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if (a.Status == DreamStatus.Created)
          {
            aResult.Return(a.ToText());
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
    /// 
    /// </summary>
    /// <param name="anId"></param>
    /// <param name="aRev"></param>
    /// <param name="aJson"></param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<string> UpdateDocument(string anId, string aRev, string aJson, Result<string> aResult)
    {
      if (String.IsNullOrEmpty(anId))
        throw new ArgumentNullException("anId");
      if (String.IsNullOrEmpty(aRev))
        throw new ArgumentNullException("aRev");
      if (String.IsNullOrEmpty(aJson))
        throw new ArgumentNullException("aJson");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      JObject jobj = JObject.Parse(aJson);
      BasePlug.AtPath(XUri.EncodeFragment(anId)).With(Constants.REV, aRev).Put(DreamMessage.Ok(MimeType.JSON, jobj.ToString(Formatting.None)), new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if (a.Status == DreamStatus.Created)
          {
            aResult.Return(a.ToText());
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
    /// Delete the specified document
    /// </summary>
    /// <param name="anId">id of the document</param>
    /// <param name="aRev">revision</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<string> DeleteDocument(string anId, string aRev, Result<string> aResult)
    {
      if (String.IsNullOrEmpty(anId))
        throw new ArgumentNullException("anId");
      if (String.IsNullOrEmpty(aRev))
        throw new ArgumentNullException("aRev");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      BasePlug.AtPath(XUri.EncodeFragment(anId)).With(Constants.REV, aRev).Delete(new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if (a.Status == DreamStatus.Ok)
            aResult.Return(a.ToText());
          else
            aResult.Throw(new CouchException(a));
        },
        aResult.Throw
      );
      return aResult;
    }
    /// <summary>
    /// Retrive a document based on doc id
    /// </summary>
    /// <param name="anId">id of the document</param>
    /// <param name="aResult">Jobject or </param>
    /// <returns></returns>
    public Result<string> GetDocument(string anId, Result<string> aResult)
    {
      if (String.IsNullOrEmpty(anId))
        throw new ArgumentNullException("anId");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      BasePlug.AtPath(XUri.EncodeFragment(anId)).Get(new Result<DreamMessage>()).WhenDone(
        a =>
        {
          switch (a.Status)
          {
            case DreamStatus.Ok:
              aResult.Return(a.ToText());
              break;
            case DreamStatus.NotFound:
              aResult.Return((string)null);
              break;
            default:
              aResult.Throw(new CouchException(a));
              break;
          }
        },
        aResult.Throw
      );
      return aResult;
    }
    /// <summary>
    /// Check if a document exists
    /// </summary>
    /// <param name="anId">id of the document</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<bool> DocumentExists(string anId, Result<bool> aResult)
    {
      if (String.IsNullOrEmpty(anId))
        throw new ArgumentNullException("anId");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      BasePlug.AtPath(XUri.EncodeFragment(anId)).Head(new Result<DreamMessage>()).WhenDone(
        a => aResult.Return(a.IsSuccessful),
        aResult.Throw
      );
      return aResult;
    }
    #endregion

    /// <summary>
    /// Create a document based on object based on ICouchDocument interface. If the ICouchDocument does not have an Id, CouchDB will generate the id for you
    /// </summary>
    /// <typeparam name="TDocument">ICouchDocument Type to return</typeparam>
    /// <param name="aDoc">ICouchDocument to create</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<TDocument> CreateDocument<TDocument>(TDocument aDoc, Result<TDocument> aResult) where TDocument : class, ICouchDocument
    {
      if (aDoc == null)
        throw new ArgumentNullException("aDoc");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      ObjectSerializer<TDocument> serializer = new ObjectSerializer<TDocument>();

      IAuditableDocument auditableCouchDocument = aDoc as IAuditableDocument;
      if (auditableCouchDocument != null)
      {
        auditableCouchDocument.Creating();
      }

      CreateDocument(aDoc.Id, serializer.Serialize(aDoc), new Result<string>()).WhenDone(
        a =>
        {
          JObject value = JObject.Parse(a);
          aDoc.Id = value[Constants.ID].Value<string>();
          aDoc.Rev = value[Constants.REV].Value<string>();

          if (auditableCouchDocument != null)
          {
            auditableCouchDocument.Created();
          }

          aResult.Return(aDoc);
        },
        aResult.Throw
      );
      return aResult;
    }
    /// <summary>
    /// Update a document
    /// </summary>
    /// <typeparam name="TDocument">Type of document</typeparam>
    /// <param name="aDoc">Document to update</param>
    /// <param name="aResult"></param>
    /// <returns>Updated document</returns>
    public Result<TDocument> UpdateDocument<TDocument>(TDocument aDoc, Result<TDocument> aResult) where TDocument : class, ICouchDocument
    {
      if (aDoc == null)
        throw new ArgumentNullException("aDoc");
      if (aResult == null)
        throw new ArgumentNullException("aResult");
      if (String.IsNullOrEmpty(aDoc.Id))
        throw new ArgumentException("Document must have an id");
      if (String.IsNullOrEmpty(aDoc.Rev))
        throw new ArgumentException("Document must have a revision");

      ObjectSerializer<TDocument> objectSerializer = new ObjectSerializer<TDocument>();

      IAuditableDocument auditableCouchDocument = aDoc as IAuditableDocument;
      if (auditableCouchDocument != null)
      {
        auditableCouchDocument.Updating();
      }

      UpdateDocument(aDoc.Id, aDoc.Rev, objectSerializer.Serialize(aDoc), new Result<string>()).WhenDone(
        a =>
        {
          JObject value = JObject.Parse(a);
          aDoc.Id = value[Constants.ID].Value<string>();
          aDoc.Rev = value[Constants.REV].Value<string>();
          if (auditableCouchDocument != null)
          {
            auditableCouchDocument.Updated();
          }
          aResult.Return(aDoc);
        },
        aResult.Throw
      );
      return aResult;
    }
    /// <summary>
    /// Retrieve Document using with specified id and deserialize result
    /// </summary>
    /// <typeparam name="TDocument">Object created during deserialization, must inherit ICouchDocument</typeparam>
    /// <param name="anId">id of the document</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<TDocument> GetDocument<TDocument>(string anId, Result<TDocument> aResult) where TDocument : ICouchDocument
    {
      BasePlug.AtPath(XUri.EncodeFragment(anId)).Get(new Result<DreamMessage>()).WhenDone(
        a =>
        {
          switch (a.Status)
          {
            case DreamStatus.Ok:
              try
              {
                ObjectSerializer<TDocument> objectSerializer = new ObjectSerializer<TDocument>();
                TDocument res = objectSerializer.Deserialize(a.ToText());
                // If object inherit BaseDocument, id and rev are set during Deserialiation
                if (!(res is CouchDocument))
                {
                  // Load id and rev (TODO: try to optimise this)
                  JObject idrev = JObject.Parse(a.ToText());
                  res.Id = idrev[Constants._ID].Value<string>();
                  res.Rev = idrev[Constants._REV].Value<string>();
                }
                aResult.Return(res);
              }
              catch (Exception ex)
              {
                aResult.Throw(ex);
              }
              break;
            case DreamStatus.NotFound:
              aResult.Return(default(TDocument));
              break;
            default:
              aResult.Throw(new CouchException(a));
              break;
          }
        },
        aResult.Throw
        );

      return aResult;
    }
    /// <summary>
    /// Delete specified document
    /// </summary>
    /// <param name="aDoc">document to delete</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<JObject> DeleteDocument(ICouchDocument aDoc, Result<JObject> aResult)
    {
      if (aDoc == null)
        throw new ArgumentNullException("aDoc");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      IAuditableDocument auditableCouchDocument = aDoc as IAuditableDocument;
      if (auditableCouchDocument != null)
      {
        auditableCouchDocument.Deleting();
      }

      DeleteDocument(aDoc.Id, aDoc.Rev, new Result<string>()).WhenDone(
        a =>
        {
          if (auditableCouchDocument != null)
          {
            auditableCouchDocument.Deleted();
          }
          aResult.Return(JObject.Parse(a));
        },
        aResult.Throw
      );
      return aResult;
    }

    /// <summary>
    /// Create a document based on object based on ICouchDocument interface. If the ICouchDocument does not have an Id, CouchDB will generate the id for you
    /// This method is synchronous
    /// </summary>
    /// <typeparam name="TDocument">ICouchDocument Type to return</typeparam>
    /// <param name="aDoc">ICouchDocument to create</param>
    /// <returns></returns>
    public TDocument CreateDocument<TDocument>(TDocument aDoc) where TDocument : class, ICouchDocument
    {
      return CreateDocument(aDoc, new Result<TDocument>()).Wait();
    }
    /// <summary>
    /// Update a document
    /// This method is synchronous
    /// </summary>
    /// <typeparam name="TDocument">Type of document</typeparam>
    /// <param name="aDoc">Document to update</param>
    /// <returns>Updated document</returns>
    public TDocument UpdateDocument<TDocument>(TDocument aDoc) where TDocument : class, ICouchDocument
    {
      return UpdateDocument(aDoc, new Result<TDocument>()).Wait();
    }
    /// <summary>
    /// Retrieve Document using with specified id and deserialize result
    /// This method is synchronous
    /// </summary>
    /// <typeparam name="TDocument">Object created during deserialization, must inherit ICouchDocument</typeparam>
    /// <param name="anId">id of the document</param>
    /// <returns></returns>
    public TDocument GetDocument<TDocument>(string anId) where TDocument : class, ICouchDocument
    {
      return GetDocument(anId, new Result<TDocument>()).Wait();
    }
    /// <summary>
    /// Delete specified document
    /// This method is synchronous
    /// </summary>
    /// <param name="aDoc">document to delete</param>
    /// <returns></returns>
    public void DeleteDocument(ICouchDocument aDoc)
    {
      DeleteDocument(aDoc, new Result<JObject>()).Wait();
    }
    /// <summary>
    /// Check id a document exists in Database
    /// This method is synchronous
    /// </summary>
    /// <param name="anId"></param>
    /// <returns></returns>
    public bool DocumentExists(string anId)
    {
      return DocumentExists(anId, new Result<bool>()).Wait();
    }
    #endregion

    #region Attachment Management

    #region Primitives methods
    /// <summary>
    /// Adds an attachment to the documnet.  Rev must be specified on this signature.  If you want to attach no matter what then use the method without the rev param
    /// </summary>
    /// <param name="anId">id of the couch Document</param>
    /// <param name="aRev">revision _rev of the Couch Document</param>
    /// <param name="anAttachment">Stream of the attachment. Will be closed once request is sent</param>
    /// <param name="anAttachmentLength">Length of the Stream.</param>
    /// <param name="aFileName">filename of the attachment</param>
    /// <param name="aContentType">Content Type of the document</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<JObject> AddAttachment(string anId, string aRev, Stream anAttachment, long anAttachmentLength, string aFileName, MimeType aContentType, Result<JObject> aResult)
    {
      if (String.IsNullOrEmpty(anId))
        throw new ArgumentNullException("id");
      if (String.IsNullOrEmpty(aRev))
        throw new ArgumentNullException("aRev");
      if (anAttachment == null)
        throw new ArgumentNullException("attachment");
      if (anAttachmentLength < 0)
        throw new ArgumentOutOfRangeException("attachmentLength");
      if (String.IsNullOrEmpty(aFileName))
        throw new ArgumentNullException("aFileName");
      if (aContentType == null)
        throw new ArgumentNullException("aContentType");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      BasePlug.AtPath(XUri.EncodeFragment(anId)).At(XUri.EncodeFragment(aFileName)).With(Constants.REV, aRev).Put(DreamMessage.Ok(aContentType, anAttachmentLength, anAttachment), new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if (a.Status == DreamStatus.Created)
            aResult.Return(JObject.Parse(a.ToText()));
          else
            aResult.Throw(new CouchException(a));
        },
        aResult.Throw
      );
      return aResult;
    }
    /// <summary>
    /// Delete an attachment
    /// </summary>
    /// <param name="anId">Id of the document</param>
    /// <param name="aRev">Revision of the document</param>
    /// <param name="anAttachmentName">Attachment file name</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<JObject> DeleteAttachment(string anId, string aRev, string anAttachmentName, Result<JObject> aResult)
    {
      if (String.IsNullOrEmpty(anId))
        throw new ArgumentNullException("id");
      if (String.IsNullOrEmpty(aRev))
        throw new ArgumentNullException("aRev");
      if (String.IsNullOrEmpty(anAttachmentName))
        throw new ArgumentNullException("attachmentName");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      BasePlug.AtPath(XUri.EncodeFragment(anId)).At(XUri.EncodeFragment(anAttachmentName)).With(Constants.REV, aRev).Delete(new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if (a.Status == DreamStatus.Ok)
            aResult.Return(JObject.Parse(a.ToText()));
          else
            aResult.Throw(new CouchException(a));
        },
        aResult.Throw
      );
      return aResult;
    }
    /// <summary>
    /// Retrieve an attachment
    /// </summary>
    /// <param name="anId">Id of the document</param>
    /// <param name="aRev">Revision of the document</param>
    /// <param name="anAttachmentName">Attachment file name</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<Stream> GetAttachment(string anId, string aRev, string anAttachmentName, Result<Stream> aResult)
    {
      if (String.IsNullOrEmpty(anId))
        throw new ArgumentNullException("docId");
      if (String.IsNullOrEmpty(aRev))
        throw new ArgumentNullException("aRev");
      if (String.IsNullOrEmpty(anAttachmentName))
        throw new ArgumentNullException("attachmentName");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      BasePlug.AtPath(XUri.EncodeFragment(anId)).At(XUri.EncodeFragment(anAttachmentName)).Get(new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if (a.Status == DreamStatus.Ok)
          {
            aResult.Return(a.ToStream());
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
    #endregion

    /// <summary>
    /// Add Attachment to the document
    /// </summary>
    /// <param name="anId">id of the document</param>
    /// <param name="aRev">rev of the document</param>
    /// <param name="anAttachment">attachment stream (will be closed)</param>
    /// <param name="anAttachmentLength"></param>
    /// <param name="aFileName">Attachment file name</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<JObject> AddAttachment(string anId, string aRev, Stream anAttachment, long anAttachmentLength, string aFileName, Result<JObject> aResult)
    {
      if (String.IsNullOrEmpty(anId))
        throw new ArgumentNullException("anId");
      if (String.IsNullOrEmpty(aRev))
        throw new ArgumentNullException("aRev");
      if (anAttachment == null)
        throw new ArgumentNullException("anAttachment");
      if (String.IsNullOrEmpty(aFileName))
        throw new ArgumentNullException("aFileName");
      if (anAttachmentLength < 0)
        throw new ArgumentOutOfRangeException("anAttachmentLength");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      return AddAttachment(anId, aRev, anAttachment, anAttachmentLength, aFileName, MimeType.FromFileExtension(aFileName), aResult);
    }
    /// <summary>
    /// Add Attachment to the document
    /// </summary>
    /// <param name="anId">id of the document</param>
    /// <param name="aRev">rev of the document</param>
    /// <param name="anAttachment">attachment stream (must be seekable, will be closed)</param>
    /// <param name="aFileName">Attachment file name</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<JObject> AddAttachment(string anId, string aRev, Stream anAttachment, string aFileName, Result<JObject> aResult)
    {
      if (!anAttachment.CanSeek)
        throw new ArgumentException("Stream must be seekable");

      return AddAttachment(anId, aRev, anAttachment, anAttachment.Length, aFileName, aResult);
    }
    /// <summary>
    /// Adds an attachment to a document.  If revision is not specified then the most recent will be fetched and used.  
    /// Warning: if you need document update conflicts to occur please use the method that specifies the revision
    /// </summary>
    /// <param name="anId">id of the couch Document</param>
    /// <param name="anAttachment">Stream of the attachment. Will be closed once request is sent</param>
    /// <param name="anAttachmentLength">Size of attachment</param>
    /// <param name="aFilename">Filename must be specifed</param>
    /// <param name="aResult"></param>  
    public Result<JObject> AddAttachment(string anId, Stream anAttachment, long anAttachmentLength, string aFilename, Result<JObject> aResult)
    {
      if (String.IsNullOrEmpty(anId))
        throw new ArgumentNullException("id");
      if (anAttachment == null)
        throw new ArgumentNullException("anAttachment");
      if (String.IsNullOrEmpty(aFilename))
        throw new ArgumentNullException("aFileName");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      GetDocument(anId, new Result<CouchDocument>()).WhenDone(
        a => AddAttachment(anId, a.Rev, anAttachment, anAttachmentLength, aFilename, aResult),
        aResult.Throw
      );
      return aResult;
    }
    /// <summary>
    /// Adds an attachment to a document.  If revision is not specified then the most recent will be fetched and used.  
    /// Warning: if you need document update conflicts to occur please use the method that specifies the revision
    /// </summary>
    /// <param name="anId">id of the couch Document</param>
    /// <param name="anAttachment">Stream of the attachment. Must be seekable and will be closed once request is sent</param>
    /// <param name="aFilename">Filename must be specifed</param>
    /// <param name="aResult"></param>  
    public Result<JObject> AddAttachment(string anId, Stream anAttachment, string aFilename, Result<JObject> aResult)
    {
      return AddAttachment(anId, anAttachment, anAttachment.Length, aFilename, aResult);
    }

    /// <summary>
    /// Add Attachment to the document to the specified document
    /// </summary>
    /// <param name="aDoc">Document</param>
    /// <param name="aFilePath">File path</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<JObject> AddAttachment(ICouchDocument aDoc, string aFilePath, Result<JObject> aResult)
    {
      if (aDoc == null)
        throw new ArgumentNullException("aDoc");
      if (String.IsNullOrEmpty(aFilePath))
        throw new ArgumentNullException("aFilePath");
      if (aResult == null)
        throw new ArgumentNullException("aResult");
      if (!File.Exists(aFilePath))
        throw new FileNotFoundException("File not found", aFilePath);

      return AddAttachment(aDoc.Id, aDoc.Rev, File.Open(aFilePath, FileMode.Open), -1, Path.GetFileName(aFilePath), aResult);
    }
    /// <summary>
    /// GetAttachment Stream of document
    /// </summary>
    /// <param name="aDoc">document</param>
    /// <param name="anAttachmentName">Attachment file name</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<Stream> GetAttachment(ICouchDocument aDoc, string anAttachmentName, Result<Stream> aResult)
    {
      if (aDoc == null)
        throw new ArgumentNullException("aDoc");
      if (String.IsNullOrEmpty(anAttachmentName))
        throw new ArgumentNullException("anAttachmentName");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      return GetAttachment(aDoc.Id, aDoc.Rev, anAttachmentName, aResult);
    }
    /// <summary>
    /// GetAttachment Stream of document with specified id
    /// </summary>
    /// <param name="aDocId">Document id</param>
    /// <param name="anAttachmentName">Attachment file name</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<Stream> GetAttachment(string aDocId, string anAttachmentName, Result<Stream> aResult)
    {
      if (String.IsNullOrEmpty(aDocId))
        throw new ArgumentNullException("aDocId");
      if (String.IsNullOrEmpty(anAttachmentName))
        throw new ArgumentNullException("anAttachmentName");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      GetDocument(aDocId, new Result<CouchDocument>()).WhenDone(
        a => GetAttachment(aDocId, a.Rev, anAttachmentName, aResult),
        aResult.Throw
      );

      return aResult;
    }
    /// <summary>
    /// Delete Attachment
    /// </summary>
    /// <param name="anId">id of the document</param>
    /// <param name="anAttachmentName">Attachment file name</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<JObject> DeleteAttachment(string anId, string anAttachmentName, Result<JObject> aResult)
    {
      if (String.IsNullOrEmpty(anId))
        throw new ArgumentNullException("aId");
      if (String.IsNullOrEmpty(anAttachmentName))
        throw new ArgumentNullException("anAttachmentName");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      GetDocument(anId, new Result<CouchDocument>()).WhenDone(
        a => DeleteAttachment(a.Id, a.Rev, anAttachmentName, aResult),
        aResult.Throw
      );
      return aResult;
    }
    /// <summary>
    /// Delete Attachment
    /// </summary>
    /// <param name="aDoc">document</param>
    /// <param name="anAttachmentName">Attachment file name</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result<JObject> DeleteAttachment(ICouchDocument aDoc, string anAttachmentName, Result<JObject> aResult)
    {
      if (aDoc == null)
        throw new ArgumentNullException("aDoc");
      if (String.IsNullOrEmpty(anAttachmentName))
        throw new ArgumentNullException("anAttachmentName");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      DeleteAttachment(aDoc.Id, aDoc.Rev, anAttachmentName, aResult).WhenDone(
        aResult.Return,
        aResult.Throw
      );
      return aResult;
    }

    public JObject AddAttachment(string anId, string aRev, Stream anAttachment, string aFileName)
    {
      return AddAttachment(anId, aRev, anAttachment, anAttachment.Length, aFileName, new Result<JObject>()).Wait();
    }

    public JObject AddAttachment(string anId, string aRev, Stream anAttachment, long anAttachmentLength, string aFileName)
    {
      return AddAttachment(anId, aRev, anAttachment, anAttachmentLength, aFileName, new Result<JObject>()).Wait();
    }

    public JObject AddAttachment(string anId, Stream anAttachment, string aFileName)
    {
      return AddAttachment(anId, anAttachment, anAttachment.Length, aFileName, new Result<JObject>()).Wait();
    }

    public JObject AddAttachment(string anId, Stream anAttachment, long anAttachmentLength, string aFileName)
    {
      return AddAttachment(anId, anAttachment, anAttachmentLength, aFileName, new Result<JObject>()).Wait();
    }
    public JObject AddAttachment(ICouchDocument aDoc, string aFilePath)
    {
      return AddAttachment(aDoc, aFilePath, new Result<JObject>()).Wait();
    }
    public Stream GetAttachment(ICouchDocument aDoc, string anAttachmentName)
    {
      return GetAttachment(aDoc, anAttachmentName, new Result<Stream>()).Wait();
    }
    public Stream GetAttachment(string aDocId, string anAttachmentName)
    {
      return GetAttachment(aDocId, anAttachmentName, new Result<Stream>()).Wait();
    }
    public JObject DeleteAttachment(string anId, string anAttachmentName)
    {
      return DeleteAttachment(anId, anAttachmentName, new Result<JObject>()).Wait();
    }
    public JObject DeleteAttachment(ICouchDocument aDoc, string anAttachmentName)
    {
      return DeleteAttachment(aDoc, anAttachmentName, new Result<JObject>()).Wait();
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