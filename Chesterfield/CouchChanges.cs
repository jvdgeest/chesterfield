using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Chesterfield.Interfaces;

namespace Chesterfield
{
  public class CouchChanges
  {
    /// <summary>
    /// A list of changes in sequential order.
    /// </summary>
    [JsonProperty(Constants.RESULTS)]
    public CouchChangeResult[] Results { get; internal set; }

    /// <summary>
    /// The sequence number of the last update returned. (In the current CouchDB 
    /// it will always be the same as the sequence of the last item in results.) 
    /// </summary>
    [JsonProperty(Constants.LAST_SEQUENCE)]
    public int LastSeq { get; internal set; }
  }

  public class CouchChangeResult
  {
    /// <summary>
    /// The document ID.
    /// </summary>
    [JsonProperty(Constants.ID)]
    public string Id { get; protected set; }

    /// <summary>
    /// The update_seq of the database that was created when this document got 
    /// created or changed.
    /// </summary>
    [JsonProperty(Constants.SEQUENCE)]
    public int Sequence { get; protected set; }

    /// <summary>
    /// An array of fields, which by default includes the document's revision 
    /// ID, but can also include information about document conflicts and other 
    /// things.
    /// </summary>
    [JsonProperty(Constants.CHANGES)]
    public JObject[] Changes { get; protected set; }

    /// <summary>
    /// Deleted documents have this attribute set to true. The other revisions 
    /// listed might be deleted even if there is no deleted property; you have 
    /// to check them individually to make sure.
    /// </summary>
    [JsonProperty(Constants.DELETED)]
    public bool Deleted { get; protected set; }
  }

  public class CouchChanges<T> where T : ICouchDocument
  {
    /// <summary>
    /// A list of changes in sequential order.
    /// </summary>
    [JsonProperty(Constants.RESULTS)]
    public CouchChangeResult<T>[] Results { get; internal set; }

    /// <summary>
    /// The sequence number of the last update returned. (In the current CouchDB 
    /// it will always be the same as the sequence of the last item in results.) 
    /// </summary>
    [JsonProperty(Constants.LAST_SEQUENCE)]
    public int LastSeq { get; internal set; }
  }

  public class CouchChangeResult<T> : CouchChangeResult where T : ICouchDocument
  {
    /// <summary>
    /// An ICouchDocument that reflects a CouchDB change record. 
    /// </summary>
    [JsonProperty(Constants.DOC)]
    public T Doc { get; private set; }
  }
}