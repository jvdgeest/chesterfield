using System.Collections.Generic;
using MindTouch.Dream;
using Newtonsoft.Json;
using Chesterfield.Interfaces;

namespace Chesterfield
{
  /// <summary>
  /// View result that only contains the total number of rows, offset, ETag and
  /// HTTP status code. The rows are not included.
  /// </summary>
  public abstract class BaseViewResult : IBaseViewResult
  {
    /// <summary>
    /// Total number of rows that are returned by the view.
    /// </summary>
    [JsonProperty(Constants.TOTAL_ROWS)]
    public int TotalRows
    {
      get;
      internal set;
    }

    /// <summary>
    /// The offset of the first row returned as it exists in the entire view.
    /// For instance, if you have three documents in your view called a, b and
    /// c, and query with no startkey then the offset will be 0. If you query
    /// with startkey="b", then the offset will be 1.
    /// </summary>
    [JsonProperty(Constants.OFFSET)]
    public int OffSet
    {
      get;
      internal set;
    }

    /// <summary>
    /// Entity tag that can be used for web cache validation.
    /// </summary>
    [JsonIgnore]
    public string ETag { get; internal set; }

    /// <summary>
    /// The HTTP status code returned by CouchDB (such as 200 OK).
    /// </summary>
    [JsonIgnore]
    public DreamStatus Status { get; internal set; }
  }

  /// <summary>
  /// View result that contains the metadata (such as total number of rows and
  /// offset) and the row data. Each row contains the CouchDB document ID, a key
  /// (can be null) and a value (can be null).
  /// </summary>
  /// <typeparam name="TKey">Object type for the key.</typeparam>
  /// <typeparam name="TValue">Object type for the value.</typeparam>
  public class ViewResult<TKey, TValue> : BaseViewResult, 
    IViewResult<TKey, TValue>
  {
    /// <summary>
    /// List of ViewResultRow objects containing the keys and values.
    /// </summary>
    [JsonProperty(Constants.ROWS)]
    public IEnumerable<ViewResultRow<TKey, TValue>> Rows
    {
      get;
      internal set;
    }
  }

  /// <summary>
  /// View result that contains the metadata (such as total number of rows and
  /// offset) and the row data. Each row contains the CouchDB document ID, a key
  /// (can be null), a value (can be null) and the corresponding CouchDB 
  /// document. You must set the "include_docs" query option to true to make
  /// this work.
  /// </summary>
  /// <typeparam name="TKey">Object type for the key.</typeparam>
  /// <typeparam name="TValue">Object type for the value.</typeparam>
  /// <typeparam name="TDocument">Object type for the document (must inherit 
  /// from ICouchDocument).</typeparam>
  public class ViewResult<TKey, TValue, TDocument> : BaseViewResult, 
    IViewResult<TKey, TValue, TDocument> where TDocument : ICouchDocument
  {
    /// <summary>
    /// List of ViewResultRow objects containing the keys, values and documents.
    /// </summary>
    [JsonProperty(Constants.ROWS)]
    public IEnumerable<ViewResultRow<TKey, TValue, TDocument>> Rows
    {
      get;
      internal set;
    }
  }
}