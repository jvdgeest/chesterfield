namespace Chesterfield.Interfaces
{
  /// <summary>
  /// Use this interface in combination with ICouchDocument to be able to
  /// perform actions when a document is being created, updated or deleted, or
  /// after a document has been created, updated or deleted. The CouchDatabase
  /// class will check if a given ICouchDocument implements this interface and
  /// then invoke the corresponding method(s) of this interface.
  /// </summary>
  public interface IAuditableDocument
  {
    /// <summary>
    /// Gets invoked when a document is being created.
    /// </summary>
    void Creating();

    /// <summary>
    /// Gets invoked after a document is created.
    /// </summary>
    void Created();

    /// <summary>
    /// Gets invoked when a document is being updated.
    /// </summary>
    void Updating();

    /// <summary>
    /// Gets invoked when a document is updated.
    /// </summary>
    void Updated();

    /// <summary>
    /// Gets invoked when a document is being deleted.
    /// </summary>
    void Deleting();

    /// <summary>
    /// Gets invoked when a document is deleted.
    /// </summary>
    void Deleted();
  }
}
