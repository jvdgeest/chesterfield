using System;
using MindTouch.Dream;

namespace Chesterfield.Interfaces
{
  public interface IListResult : IEquatable<IListResult>
  {
    DreamStatus StatusCode { get; }
    string Etag { get; }
    string RawString { get; }
  }
}