namespace Chesterfield.Interfaces
{
  public interface ICouchDocument
  {
    string Id { get; set; }
    string Rev { get; set; }
  }
}