using System.Collections.Generic;
using MindTouch.Dream;
using Newtonsoft.Json;
using Chesterfield.Interfaces;

namespace Chesterfield
{
  public abstract class BaseViewResult : IBaseViewResult
  {
    [JsonProperty(Constants.TOTAL_ROWS)]
    public int TotalRows
    {
      get;
      internal set;
    }
    [JsonProperty(Constants.OFFSET)]
    public int OffSet
    {
      get;
      internal set;
    }
    [JsonIgnore]
    public string ETag { get; internal set; }

    [JsonIgnore]
    public DreamStatus Status { get; internal set; }
  }

  public class ViewResult<TKey, TValue> : BaseViewResult, IViewResult<TKey, TValue>
  {
    [JsonProperty(Constants.ROWS)]
    public IEnumerable<ViewResultRow<TKey, TValue>> Rows
    {
      get;
      internal set;
    }
  }

  public class ViewResult<TKey, TValue, TDocument> : BaseViewResult, IViewResult<TKey, TValue, TDocument> where TDocument : ICouchDocument
  {
    [JsonProperty(Constants.ROWS)]
    public IEnumerable<ViewResultRow<TKey, TValue, TDocument>> Rows
    {
      get;
      internal set;
    }
  }
}