using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Chesterfield.Interfaces;

namespace Chesterfield
{
  public class CouchDocument : ICouchDocument
  {
    public CouchDocument()
    {
      Attachments = new Dictionary<string, CouchAttachment>();
    }

    /// <summary>
    /// Document ID that is unique per CouchDB database. You are free to choose 
    /// any string to be the ID, but for best results CouchDB recommends a UUID 
    /// (or GUID), i.e., a Universally (or Globally) Unique IDentifier.
    /// </summary>
    [JsonProperty(Constants._ID)]
    public string Id { get; set; }

    /// <summary>
    /// Revision ID of the document. Document revisions are used for optimistic 
    /// concurrency control. If you try to update a document using an old 
    /// revision the update will be in conflict. These conflicts should be 
    /// resolved by your client, usually by requesting the newest version of the
    /// document, modifying and trying the update again. 
    /// </summary>
    [JsonProperty(Constants._REV)]
    public string Rev { get; set; }

    /// <summary>
    /// A dictionary contaiing the filenames as keys and CouchAttachment objects
    /// as values. The attachment's actual data is not included by default, only
    /// the metadata.
    /// </summary>
    [JsonProperty(Constants._ATTACHMENTS)]
    internal Dictionary<string, CouchAttachment> Attachments;

    /// <summary>
    /// Checks whether this document has any attachments.
    /// </summary>
    [JsonIgnore]
    public bool HasAttachment { get { return Attachments.Count > 0; } }

    /// <summary>
    /// Returns a list with the names of the attachments.
    /// </summary>
    /// <returns>List with attachment names.</returns>
    public IEnumerable<string> GetAttachmentNames()
    {
      foreach (string key in Attachments.Keys)
        yield return key;
      yield break;
    }

    /// <summary>
    /// Checks for equality by comparing the ID and revision of two 
    /// CouchDocument objects.
    /// </summary>
    /// <param name="obj">Other object to compare.</param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      ICouchDocument o = obj as ICouchDocument;
      if (o == null)
        return false;

      return o.Id == Id && o.Rev == Rev;
    }

    /// <summary>
    /// Returns a hash code by using the ID and revision of this document.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      return (Id + Rev).GetHashCode();
    }
  }
}