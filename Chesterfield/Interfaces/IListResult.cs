using MindTouch.Dream;

namespace Chesterfield.Interfaces
{
  public interface IListResult : System.IEquatable<IListResult>
  {
    DreamStatus StatusCode { get; }
    string Etag { get; }
    string RawString { get; }
  }
}