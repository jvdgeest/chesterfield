using System;
using System.IO;
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
    /// Adds an attachment to a document. The document revision must be 
    /// specified when using this method.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="rev">Revision of the CouchDB document.</param>
    /// <param name="attachment">Stream of the attachment. Will be closed once
    /// the request is sent.</param>
    /// <param name="attachmentLength">Length of the attachment stream.</param>
    /// <param name="fileName">Filename of the attachment.</param>
    /// <param name="contentType">Content type of the document.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<JObject> AddAttachment(
      string id, 
      string rev, 
      Stream attachment, 
      long attachmentLength, 
      string fileName, 
      MimeType contentType, 
      Result<JObject> result)
    {
      if (String.IsNullOrEmpty(id))
        throw new ArgumentNullException("id");
      if (String.IsNullOrEmpty(rev))
        throw new ArgumentNullException("rev");
      if (attachment == null)
        throw new ArgumentNullException("attachment");
      if (attachmentLength < 0)
        throw new ArgumentOutOfRangeException("attachmentLength");
      if (String.IsNullOrEmpty(fileName))
        throw new ArgumentNullException("fileName");
      if (contentType == null)
        throw new ArgumentNullException("contentType");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .AtPath(XUri.EncodeFragment(id))
        .At(XUri.EncodeFragment(fileName))
        .With(Constants.REV, rev)
        .Put(DreamMessage.Ok(contentType, attachmentLength, attachment), 
             new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Created)
              result.Return(JObject.Parse(a.ToText()));
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );

      return result;
    }

    /// <summary>
    /// Deletes an attachment from a document. The document revision must be 
    /// specified when using this method.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="rev">Revision of the CouchDB document.</param>
    /// <param name="fileName">Filename of the attachment.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<JObject> DeleteAttachment(
      string id, 
      string rev, 
      string fileName, 
      Result<JObject> result)
    {
      if (String.IsNullOrEmpty(id))
        throw new ArgumentNullException("id");
      if (String.IsNullOrEmpty(rev))
        throw new ArgumentNullException("rev");
      if (String.IsNullOrEmpty(fileName))
        throw new ArgumentNullException("fileName");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .AtPath(XUri.EncodeFragment(id))
        .At(XUri.EncodeFragment(fileName))
        .With(Constants.REV, rev)
        .Delete(new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Ok)
              result.Return(JObject.Parse(a.ToText()));
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      
      return result;
    }

    /// <summary>
    /// Retrieves an attachment. The document revision must be specified when 
    /// using this method.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="rev">Revision of the CouchDB document.</param>
    /// <param name="fileName">Filename of the attachment.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<Stream> GetAttachment(
      string id, 
      string rev, 
      string fileName, 
      Result<Stream> result)
    {
      if (String.IsNullOrEmpty(id))
        throw new ArgumentNullException("id");
      if (String.IsNullOrEmpty(rev))
        throw new ArgumentNullException("rev");
      if (String.IsNullOrEmpty(fileName))
        throw new ArgumentNullException("fileName");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .AtPath(XUri.EncodeFragment(id))
        .At(XUri.EncodeFragment(fileName))
        .Get(new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Ok)
              result.Return(a.ToStream());
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );

      return result;
    }

    /// <summary>
    /// Adds an attachment to a document. The document revision must be 
    /// specified when using this method.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="rev">Revision of the CouchDB document.</param>
    /// <param name="attachment">Stream of the attachment. Will be closed once 
    /// the request is sent.</param>
    /// <param name="attachmentLength">Length of the attachment stream.</param>
    /// <param name="fileName">Filename of the attachment.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<JObject> AddAttachment(
      string id, 
      string rev, 
      Stream attachment, 
      long attachmentLength, 
      string fileName,
      Result<JObject> result)
    {
      if (String.IsNullOrEmpty(fileName))
        throw new ArgumentNullException("fileName");

      return AddAttachment(id, rev, attachment, attachmentLength, fileName, 
        MimeType.FromFileExtension(fileName), result);
    }

    /// <summary>
    /// Adds an attachment to a document. The document revision must be 
    /// specified when using this method.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="rev">Revision of the CouchDB document.</param>
    /// <param name="attachment">Stream of the attachment. Must be seekable and 
    /// will be closed once the request is sent.</param>
    /// <param name="fileName">Filename of the attachment.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<JObject> AddAttachment(string id, 
      string rev, 
      Stream attachment, 
      string fileName, 
      Result<JObject> result)
    {
      if (!attachment.CanSeek)
        throw new ArgumentException("Stream must be seekable");

      return AddAttachment(id, rev, attachment, attachment.Length, fileName, 
        result);
    }

    /// <summary>
    /// Adds an attachment to a document. The most recent revision will be 
    /// used. Warning: if you need to prevent document update conflicts, then 
    /// please use the method that specifies the revision.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="attachment">Stream of the attachment. Will be closed once 
    /// the request is sent.</param>
    /// <param name="attachmentLength">Length of the attachment stream.</param>
    /// <param name="fileName">Filename of the attachment.</param>
    /// <param name="result"></param>  
    public Result<JObject> AddAttachment(
      string id, 
      Stream attachment, 
      long attachmentLength, 
      string filename, 
      Result<JObject> result)
    {
      if (String.IsNullOrEmpty(id))
        throw new ArgumentNullException("id");
      if (attachment == null)
        throw new ArgumentNullException("attachment");
      if (String.IsNullOrEmpty(filename))
        throw new ArgumentNullException("fileName");
      if (result == null)
        throw new ArgumentNullException("result");

      GetDocument(id, new Result<CouchDocument>()).WhenDone(
        a => AddAttachment(id, a.Rev, attachment, attachmentLength, filename, 
                           result),
        result.Throw
      );

      return result;
    }

    /// <summary>
    /// Adds an attachment to a document. The most recent revision will be 
    /// used. Warning: if you need to prevent document update conflicts, then 
    /// please use the method that specifies the revision.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="attachment">Stream of the attachment. Must be seekable and 
    /// will be closed once the request is sent.</param>
    /// <param name="fileName">Filename of the attachment.</param>
    /// <param name="result"></param>  
    public Result<JObject> AddAttachment(
      string id, 
      Stream attachment, 
      string filename, 
      Result<JObject> result)
    {
      if (!attachment.CanSeek)
        throw new ArgumentException("Stream must be seekable");

      return AddAttachment(id, attachment, attachment.Length, filename, result);
    }

    /// <summary>
    /// Adds an attachment from a specified path. The most recent revision will 
    /// be used. Warning: if you need to prevent document update conflicts, then 
    /// please use the method that specifies the revision.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="doc">ICouchDocument object.</param>
    /// <param name="filePath">Path of the attachment to be added.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<JObject> AddAttachment(
      ICouchDocument doc, 
      string filePath, 
      Result<JObject> result)
    {
      if (doc == null)
        throw new ArgumentNullException("doc");
      if (String.IsNullOrEmpty(filePath))
        throw new ArgumentNullException("filePath");
      if (!File.Exists(filePath))
        throw new FileNotFoundException("File not found", filePath);

      return AddAttachment(doc.Id, doc.Rev, File.Open(filePath, FileMode.Open), 
        -1, Path.GetFileName(filePath), result);
    }

    /// <summary>
    /// Retrieves an attachment. The most recent revision will be used.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="doc">ICouchDocument object.</param>
    /// <param name="fileName">Filename of the attachment.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<Stream> GetAttachment(
      ICouchDocument doc, 
      string fileName, 
      Result<Stream> result)
    {
      if (doc == null)
        throw new ArgumentNullException("doc");

      return GetAttachment(doc.Id, doc.Rev, fileName, result);
    }

    /// <summary>
    /// Retrieves an attachment. The most recent revision will be used.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="fileName">Filename of the attachment.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<Stream> GetAttachment(
      string id, 
      string fileName, 
      Result<Stream> result)
    {
      GetDocument(id, new Result<CouchDocument>()).WhenDone(
        a => GetAttachment(id, a.Rev, fileName, result),
        result.Throw
      );
      return result;
    }

    /// <summary>
    /// Deletes an attachment. The most recent revision will be used.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="fileName">Filename of the attachment.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<JObject> DeleteAttachment(
      string id, 
      string fileName, 
      Result<JObject> result)
    {
      GetDocument(id, new Result<CouchDocument>()).WhenDone(
        a => DeleteAttachment(a.Id, a.Rev, fileName, result),
        result.Throw
      );
      return result;
    }

    /// <summary>
    /// Deletes an attachment. The most recent revision will be used.
    /// <para>&#160;</para>
    /// This method is asynchronous.
    /// </summary>
    /// <param name="doc">ICouchDocument object.</param>
    /// <param name="fileName">Filename of the attachment.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<JObject> DeleteAttachment(
      ICouchDocument doc, 
      string fileName, 
      Result<JObject> result)
    {
      if (doc == null)
        throw new ArgumentNullException("doc");

      DeleteAttachment(doc.Id, doc.Rev, fileName, result).WhenDone(
        result.Return,
        result.Throw
      );
      return result;
    }

    /* =========================================================================
     * Synchronous methods 
     * =======================================================================*/

    /// <summary>
    /// Adds an attachment to a document. The document revision must be 
    /// specified when using this method.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="rev">Revision of the CouchDB document.</param>
    /// <param name="attachment">Stream of the attachment. Must be seekable and 
    /// will be closed once the request is sent.</param>
    /// <param name="attachmentLength">Length of the attachment stream.</param>
    /// <param name="fileName">Filename of the attachment.</param>
    /// <returns></returns>
    public JObject AddAttachment(
      string id, 
      string rev, 
      Stream attachment,
      long attachmentLength, 
      string fileName)
    {
      return AddAttachment(id, rev, attachment, attachmentLength, fileName,
        new Result<JObject>()).Wait();
    }

    /// <summary>
    /// Adds an attachment to a document. The document revision must be 
    /// specified when using this method.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="rev">Revision of the CouchDB document.</param>
    /// <param name="attachment">Stream of the attachment. Will be closed once 
    /// the request is sent.</param>
    /// <param name="fileName">Filename of the attachment.</param>
    /// <returns></returns>
    public JObject AddAttachment(
      string id, 
      string rev, 
      Stream attachment, 
      string fileName)
    {
      return AddAttachment(id, rev, attachment, attachment.Length, fileName, 
        new Result<JObject>()).Wait();
    }

    /// <summary>
    /// Adds an attachment to a document. The most recent revision will be 
    /// used. Warning: if you need to prevent document update conflicts, then 
    /// please use the method that specifies the revision.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="attachment">Stream of the attachment. Will be closed once 
    /// the request is sent.</param>
    /// <param name="attachmentLength">Length of the attachment stream.</param>
    /// <param name="fileName">Filename of the attachment.</param>
    /// <returns></returns>
    public JObject AddAttachment(
      string id, 
      Stream attachment,
      long attachmentLength,
      string fileName)
    {
      return AddAttachment(id, attachment, attachmentLength, fileName, 
        new Result<JObject>()).Wait();
    }

    /// <summary>
    /// Adds an attachment to a document. The most recent revision will be 
    /// used. Warning: if you need to prevent document update conflicts, then 
    /// please use the method that specifies the revision.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="attachment">Stream of the attachment. Must be seekable and 
    /// will be closed once the request is sent.</param>
    /// <param name="fileName">Filename of the attachment.</param>
    /// <returns></returns>
    public JObject AddAttachment(
      string id, 
      Stream attachment, 
      string fileName)
    {
      return AddAttachment(id, attachment, fileName,
        new Result<JObject>()).Wait();
    }

    /// <summary>
    /// Adds an attachment from a specified path. The most recent revision will 
    /// be used. Warning: if you need to prevent document update conflicts, then 
    /// please use the method that specifies the revision.
    /// </summary>
    /// <param name="doc">ICouchDocument object.</param>
    /// <param name="filePath">Path of the attachment to be added.</param>
    /// <returns></returns>
    public JObject AddAttachment(ICouchDocument doc, string filePath)
    {
      return AddAttachment(doc, filePath, new Result<JObject>()).Wait();
    }

    /// <summary>
    /// Retrieves an attachment. The most recent revision will be used.
    /// </summary>
    /// <param name="doc">ICouchDocument object.</param>
    /// <param name="fileName">Filename of the attachment.</param>
    /// <returns></returns>
    public Stream GetAttachment(ICouchDocument doc, string fileName)
    {
      return GetAttachment(doc, fileName, new Result<Stream>()).Wait();
    }

    /// <summary>
    /// Retrieves an attachment. The most recent revision will be used.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="fileName">Filename of the attachment.</param>
    /// <returns></returns>
    public Stream GetAttachment(string id, string fileName)
    {
      return GetAttachment(id, fileName, new Result<Stream>()).Wait();
    }

    /// <summary>
    /// Deletes an attachment. The most recent revision will be used.
    /// </summary>
    /// <param name="id">ID of the CouchDB document.</param>
    /// <param name="fileName">Filename of the attachment.</param>
    /// <returns></returns>
    public JObject DeleteAttachment(string id, string fileName)
    {
      return DeleteAttachment(id, fileName, new Result<JObject>()).Wait();
    }

    /// <summary>
    /// Deletes an attachment. The most recent revision will be used.
    /// </summary>
    /// <param name="doc">ICouchDocument object.</param>
    /// <param name="fileName">Filename of the attachment.</param>
    /// <returns></returns>
    public JObject DeleteAttachment(ICouchDocument doc, string fileName)
    {
      return DeleteAttachment(doc, fileName, new Result<JObject>()).Wait();
    }
  }
}
