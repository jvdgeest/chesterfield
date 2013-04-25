namespace Chesterfield.Interfaces
{
  public interface ICouchDocument
  {
    /// <summary>
    /// Document ID that is unique per CouchDB database. You are free to choose 
    /// any string to be the ID, but for best results CouchDB recommends a UUID 
    /// (or GUID), i.e., a Universally (or Globally) Unique IDentifier.
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// Revision ID of the document. Document revisions are used for optimistic 
    /// concurrency control. If you try to update a document using an old 
    /// revision the update will be in conflict. These conflicts should be 
    /// resolved by your client, usually by requesting the newest version of the
    /// document, modifying and trying the update again. 
    /// </summary>
    string Rev { get; set; }
  }
}