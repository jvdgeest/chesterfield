namespace Chesterfield.Interfaces
{
  /// <summary>
  /// Interface for a single view row containg the CouchDB document ID and a key 
  /// (can be null).
  /// </summary>
  /// <typeparam name="TKey">The row key type.</typeparam>
  public interface IViewResultRow<TKey>
  {
    /// <summary>
    /// ID of the CouchDB document.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// The key that is assigned by the view's map function (can be null).
    /// </summary>
    TKey Key { get; }
  }

  /// <summary>
  /// Interface for a single view row containing the CouchDB document ID, a key 
  /// (can be null) and a value (can be null).
  /// </summary>
  /// <typeparam name="TKey">The row key type.</typeparam>
  /// <typeparam name="TValue">The row value type.</typeparam>
  public interface IViewResultRow<TKey, TValue> : IViewResultRow<TKey>
  {
    /// <summary>
    /// The value that is assigned by the view's map function (can be null).
    /// </summary>
    TValue Value { get; }
  }

  /// <summary>
  /// Interface for a single view row containing the CouchDB document ID, a key 
  /// (can be null), a value (can be null) and the corresponding CouchDB 
  /// document. You must set the "include_docs" query option to true to make 
  /// this work.
  /// </summary>
  /// <typeparam name="TKey">The row key type.</typeparam>
  /// <typeparam name="TValue">The row value type.</typeparam>
  /// <typeparam name="TDocument">The row document type (must inherit from 
  /// ICouchDocument).</typeparam>
  public interface IViewResultRow<TKey, TValue, TDocument> : 
    IViewResultRow<TKey, TValue> where TDocument : ICouchDocument
  {
    /// <summary>
    /// The CouchDB document that belongs to the view row.
    /// </summary>
    TDocument Doc { get; }
  }
}