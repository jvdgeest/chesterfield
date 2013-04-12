namespace Chesterfield.Interfaces
{
  public interface IAuditableDocument
  {
    void Creating();
    void Created();
    void Updating();
    void Updated();
    void Deleting();
    void Deleted();
  }
}
