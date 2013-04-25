namespace Chesterfield.Interfaces
{
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
