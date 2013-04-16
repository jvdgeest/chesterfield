using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Chesterfield.Interfaces;

namespace Chesterfield
{
  /// <summary>
  /// A single view row containg the CouchDB document ID and a key (can be 
  /// null).
  /// </summary>
  /// <typeparam name="TKey">The row key type.</typeparam>
  public class ViewResultRow<TKey> : IViewResultRow<TKey>
  {
    /// <summary>
    /// ID of the CouchDB document.
    /// </summary>
    [JsonProperty(Constants.ID)]
    public string Id
    {
      get;
      internal set;
    }

    /// <summary>
    /// The key that is assigned by the view's map function (can be null).
    /// </summary>
    [JsonProperty(Constants.KEY)]
    public TKey Key
    {
      get;
      internal set;
    }
  }

  /// <summary>
  /// A single view row containing the CouchDB document ID, a key (can be null)
  /// and a value (can be null).
  /// </summary>
  /// <typeparam name="TKey">The row key type.</typeparam>
  /// <typeparam name="TValue">The row value type.</typeparam>
  public class ViewResultRow<TKey, TValue> :
    ViewResultRow<TKey>,
    IViewResultRow<TKey, TValue>
  {
    /// <summary>
    /// The value that is assigned by the view's map function (can be null).
    /// </summary>
    [JsonProperty(Constants.VALUE)]
    public TValue Value
    {
      get;
      internal set;
    }
  }

  /// <summary>
  /// A single view row containing the CouchDB document ID, a key (can be null),
  /// a value (can be null) and the corresponding CouchDB document. You must set
  /// the "include_docs" query option to true to make this work.
  /// </summary>
  /// <typeparam name="TKey">The row key type.</typeparam>
  /// <typeparam name="TValue">The row value type.</typeparam>
  /// <typeparam name="TDocument">The row document type (must inherit from 
  /// ICouchDocument).</typeparam>
  public class ViewResultRow<TKey, TValue, TDocument> :
    ViewResultRow<TKey, TValue>,
    IViewResultRow<TKey, TValue, TDocument>
    where TDocument : ICouchDocument
  {
    /// <summary>
    /// The CouchDB document that belongs to the view row.
    /// </summary>
    [JsonProperty(Constants.DOC)]
    public TDocument Doc
    {
      get;
      internal set;
    }
  }
}