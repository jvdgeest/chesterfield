﻿using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MindTouch.Dream;
using MindTouch.Tasking;
using Chesterfield.Interfaces;
using Chesterfield.Support;

namespace Chesterfield
{
  public partial class CouchDatabase : CouchBase
  {
    /* =========================================================================
     * Asynchronous methods 
     * =======================================================================*/

    /// <summary>
    /// Creates a document using the provided JSON. This method lets CouchDB 
    /// generate the document ID for you.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="json">JSON data for creating the document.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<string> CreateDocument(string json, Result<string> result)
    {
      return CreateDocument(null, json, result);
    }

    /// <summary>
    /// Creates a document using the provided JSON.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="id">ID of the CouchDB document. When null or empty, an ID
    /// will be generated by the CouchDB server.</param>
    /// <param name="json">JSON data for creating the document.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<string> CreateDocument(string id, string json, 
      Result<string> result)
    {
      if (String.IsNullOrEmpty(json))
        throw new ArgumentNullException("json");
      if (result == null)
        throw new ArgumentNullException("result");

      JObject jobj = JObject.Parse(json);
      if (jobj.Value<object>(Constants._REV) != null)
        jobj.Remove(Constants._REV);

      Plug plug = BasePlug;
      string verb = Verb.POST;
      if (!String.IsNullOrEmpty(id))
      {
        plug = plug.AtPath(XUri.EncodeFragment(id));
        verb = Verb.PUT;
      }

      plug
        .Invoke(verb, DreamMessage.Ok(MimeType.JSON, 
          jobj.ToString(Formatting.None)), new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Created)
              result.Return(a.ToText());
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      return result;
    }

    /// <summary>
    /// Updates a document using the provided JSON.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="rev">Revision of the CouchDB document.</param>
    /// <param name="json">JSON data for updating the document.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<string> UpdateDocument(string id, string rev, string json, 
      Result<string> result)
    {
      if (String.IsNullOrEmpty(id))
        throw new ArgumentNullException("id");
      if (String.IsNullOrEmpty(rev))
        throw new ArgumentNullException("rev");
      if (String.IsNullOrEmpty(json))
        throw new ArgumentNullException("json");
      if (result == null)
        throw new ArgumentNullException("result");

      JObject jobj = JObject.Parse(json);
      BasePlug
        .AtPath(XUri.EncodeFragment(id)).With(Constants.REV, rev)
        .Put(DreamMessage.Ok(MimeType.JSON, jobj.ToString(Formatting.None)), 
          new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Created)
              result.Return(a.ToText());
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      return result;
    }

    /// <summary>
    /// Deletes a document. 
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="rev">Revision of the CouchDB document.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<string> DeleteDocument(string id, string rev, 
      Result<string> result)
    {
      if (String.IsNullOrEmpty(id))
        throw new ArgumentNullException("id");
      if (String.IsNullOrEmpty(rev))
        throw new ArgumentNullException("rev");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .AtPath(XUri.EncodeFragment(id))
        .With(Constants.REV, rev)
        .Delete(new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Ok)
              result.Return(a.ToText());
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      return result;
    }

    /// <summary>
    /// Retrieves a document in a JSON string format.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<string> GetDocument(string id, Result<string> aResult)
    {
      if (String.IsNullOrEmpty(id))
        throw new ArgumentNullException("id");
      if (aResult == null)
        throw new ArgumentNullException("result");

      BasePlug
        .AtPath(XUri.EncodeFragment(id))
        .Get(new Result<DreamMessage>())
        .WhenDone(
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
    /// Checks if a document exists.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="result"></param>
    /// <returns>Boolean indicating whether the document exists.</returns>
    public Result<bool> DocumentExists(string id, Result<bool> result)
    {
      if (String.IsNullOrEmpty(id))
        throw new ArgumentNullException("id");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .AtPath(XUri.EncodeFragment(id))
        .Head(new Result<DreamMessage>())
        .WhenDone(
          a => result.Return(a.IsSuccessful),
          result.Throw
        );
      return result;
    }

    /// <summary>
    /// Creates a document based on a type that inherits from the ICouchDocument
    /// interface. If the ICouchDocument does not have an ID, then CouchDB will 
    /// generate it for you.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <typeparam name="TDocument">Type of object that must be created (must 
    /// inherit from ICouchDocument).</typeparam>
    /// <param name="doc">Document object that contains the data.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<TDocument> CreateDocument<TDocument>(TDocument doc, 
      Result<TDocument> result) where TDocument : class, ICouchDocument
    {
      if (doc == null)
        throw new ArgumentNullException("doc");
      if (result == null)
        throw new ArgumentNullException("result");

      ObjectSerializer<TDocument> serializer = 
        new ObjectSerializer<TDocument>();

      IAuditableDocument auditableCouchDocument = doc as IAuditableDocument;
      if (auditableCouchDocument != null)
        auditableCouchDocument.Creating();

      CreateDocument(doc.Id, serializer.Serialize(doc), new Result<string>())
        .WhenDone(
          a =>
          {
            JObject value = JObject.Parse(a);
            doc.Id = value[Constants.ID].Value<string>();
            doc.Rev = value[Constants.REV].Value<string>();

            if (auditableCouchDocument != null)
              auditableCouchDocument.Created();

            result.Return(doc);
          },
          result.Throw
        );
      return result;
    }

    /// <summary>
    /// Updates a document using an ICouchDocument object.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <typeparam name="TDocument">Type of object that must be updated (must 
    /// inherit from ICouchDocument).</typeparam>
    /// <param name="doc">Document object that contains the data.</param>
    /// <param name="result"></param>
    /// <returns>Updated document.</returns>
    public Result<TDocument> UpdateDocument<TDocument>(TDocument doc, 
      Result<TDocument> result) where TDocument : class, ICouchDocument
    {
      if (doc == null)
        throw new ArgumentNullException("doc");
      if (result == null)
        throw new ArgumentNullException("result");
      if (String.IsNullOrEmpty(doc.Id))
        throw new ArgumentException("Document must have an ID");
      if (String.IsNullOrEmpty(doc.Rev))
        throw new ArgumentException("Document must have a revision");

      ObjectSerializer<TDocument> objectSerializer = 
        new ObjectSerializer<TDocument>();

      IAuditableDocument auditableCouchDocument = doc as IAuditableDocument;
      if (auditableCouchDocument != null)
        auditableCouchDocument.Updating();

      UpdateDocument(doc.Id, doc.Rev, objectSerializer.Serialize(doc), 
        new Result<string>())
        .WhenDone(
          a =>
          {
            JObject value = JObject.Parse(a);
            doc.Id = value[Constants.ID].Value<string>();
            doc.Rev = value[Constants.REV].Value<string>();
            if (auditableCouchDocument != null)
              auditableCouchDocument.Updated();

            result.Return(doc);
          },
          result.Throw
        );
      return result;
    }

    /// <summary>
    /// Retrieves a document with a given ID and object type which inherits
    /// from ICouchDocument. This method will perform the JSON deserialization.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <typeparam name="TDocument">Type of object that will be created during 
    /// deserialization (must inherit from ICouchDocument).</typeparam>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<TDocument> GetDocument<TDocument>(string id, 
      Result<TDocument> result) where TDocument : ICouchDocument
    {
      BasePlug
        .AtPath(XUri.EncodeFragment(id))
        .Get(new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            switch (a.Status)
            {
              case DreamStatus.Ok:
                try
                {
                  ObjectSerializer<TDocument> objectSerializer = 
                    new ObjectSerializer<TDocument>();
                  TDocument obj = objectSerializer.Deserialize(a.ToText());

                  // If the object inherits BaseDocument, then Id and Rev are 
                  // set during Deserialiation.
                  if (!(obj is CouchDocument))
                  {
                    JObject idrev = JObject.Parse(a.ToText());
                    obj.Id = idrev[Constants._ID].Value<string>();
                    obj.Rev = idrev[Constants._REV].Value<string>();
                  }
                  result.Return(obj);
                }
                catch (Exception ex)
                {
                  result.Throw(ex);
                }
                break;
              case DreamStatus.NotFound:
                result.Return(default(TDocument));
                break;
              default:
                result.Throw(new CouchException(a));
                break;
            }
          },
          result.Throw
          );
      return result;
    }

    /// <summary>
    /// Deletes a document.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="doc">Document object that must be deleted.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<JObject> DeleteDocument(ICouchDocument doc, 
      Result<JObject> result)
    {
      if (doc == null)
        throw new ArgumentNullException("doc");
      if (result == null)
        throw new ArgumentNullException("result");

      IAuditableDocument auditableCouchDocument = doc as IAuditableDocument;
      if (auditableCouchDocument != null)
        auditableCouchDocument.Deleting();

      DeleteDocument(doc.Id, doc.Rev, new Result<string>())
        .WhenDone(
          a =>
          {
            if (auditableCouchDocument != null)
              auditableCouchDocument.Deleted();

            result.Return(JObject.Parse(a));
          },
          result.Throw
        );
      return result;
    }

    /* =========================================================================
     * Synchronous methods 
     * =======================================================================*/

    /// <summary>
    /// Creates a document based on a type that inherits from the ICouchDocument
    /// interface. If the ICouchDocument does not have an ID, then CouchDB will 
    /// generate it for you.
    /// </summary>
    /// <typeparam name="TDocument">Type of object that must be created (must 
    /// inherit from ICouchDocument).</typeparam>
    /// <param name="doc">Document object that contains the data.</param>
    /// <returns>Created document.</returns>
    public TDocument CreateDocument<TDocument>(TDocument doc) 
      where TDocument : class, ICouchDocument
    {
      return CreateDocument(doc, new Result<TDocument>()).Wait();
    }

    /// <summary>
    /// Updates a document using an ICouchDocument object.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <typeparam name="TDocument">Type of object that must be updated (must 
    /// inherit from ICouchDocument).</typeparam>
    /// <param name="doc">Document object that contains the data.</param>
    /// <returns>Updated document.</returns>
    public TDocument UpdateDocument<TDocument>(TDocument doc) 
      where TDocument : class, ICouchDocument
    {
      return UpdateDocument(doc, new Result<TDocument>()).Wait();
    }

    /// <summary>
    /// Retrieves a document with a given ID and object type which inherits
    /// from ICouchDocument. This method will perform the JSON deserialization.
    /// </summary>
    /// <typeparam name="TDocument">Type of object that will be created during 
    /// deserialization (must inherit from ICouchDocument).</typeparam>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <returns>Retrieved document.</returns>
    public TDocument GetDocument<TDocument>(string id) 
      where TDocument : class, ICouchDocument
    {
      return GetDocument(id, new Result<TDocument>()).Wait();
    }

    /// <summary>
    /// Deletes a document.
    /// </summary>
    /// <param name="doc">Document object that must be deleted.</param>
    public void DeleteDocument(ICouchDocument doc)
    {
      DeleteDocument(doc, new Result<JObject>()).Wait();
    }

    /// <summary>
    /// Checks if a document exists.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <returns>Boolean indicating whether the document exists.</returns>
    public bool DocumentExists(string id)
    {
      return DocumentExists(id, new Result<bool>()).Wait();
    }
  }
}