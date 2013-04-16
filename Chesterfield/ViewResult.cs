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
  /// <typeparam name="TKey">The row key type.</typeparam>
  /// <typeparam name="TValue">The row value type.</typeparam>
  public class ViewResult<TKey, TValue> : BaseViewResult, 
    IViewResult<TKey, TValue>
  {
    /// <summary>
    /// List of ViewResultRow objects containing the CouchDB document ID's, keys 
    /// and values.
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
  /// <typeparam name="TKey">The row key type.</typeparam>
  /// <typeparam name="TValue">The row value type.</typeparam>
  /// <typeparam name="TDocument">The row document type (must inherit from 
  /// ICouchDocument).</typeparam>
  public class ViewResult<TKey, TValue, TDocument> : BaseViewResult, 
    IViewResult<TKey, TValue, TDocument> where TDocument : ICouchDocument
  {
    /// <summary>
    /// List of ViewResultRow objects containing the CouchDB document ID's, 
    /// keys, values and CouchDB documents.
    /// </summary>
    [JsonProperty(Constants.ROWS)]
    public IEnumerable<ViewResultRow<TKey, TValue, TDocument>> Rows
    {
      get;
      internal set;
    }
  }
}