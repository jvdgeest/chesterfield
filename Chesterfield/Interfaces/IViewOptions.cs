using Chesterfield.Support;

namespace Chesterfield.Interfaces
{
  public interface IViewOptions
  {
    KeyOptions Key { get; set; }
    KeyOptions StartKey { get; set; }
    KeyOptions EndKey { get; set; }
    int? Limit { get; set; }
    int? Skip { get; set; }
    bool? Reduce { get; set; }
    bool? Group { get; set; }
    bool? InclusiveEnd { get; set; }
    int? GroupLevel { get; set; }
    bool? Descending { get; set; }
    Stale? Stale { get; set; }
    string Etag { get; set; }
    string StartKeyDocId { get; set; }
    string EndKeyDocId { get; set; }
    string ToString();
  }
}