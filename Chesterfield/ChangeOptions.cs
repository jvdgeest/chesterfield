namespace Chesterfield
{
  internal enum ChangeFeed
  {
    Normal,
    LongPoll, // unsupported
    Continuous
  }

  public class ChangeOptions
  {
    /// <summary>
    /// Include the associated document with each result. If there are 
    /// conflicts, only the winning revision is returned.
    /// </summary>
    public bool? IncludeDocs { get; set; }

    /// <summary>
    /// The type of feed.
    /// </summary>
    internal ChangeFeed Feed { get; set; }

    /// <summary>
    /// Reference a filter function from a design document to selectively get 
    /// updates. Must have "designdoc/filtername" as format.
    /// </summary>
    public string Filter { get; set; }
    
    /// <summary>
    /// Period (in milliseconds) after which an empty line is sent during 
    /// longpoll or continuous feeds.
    /// </summary>
    public int? Heartbeat { get; set; }

    /// <summary>
    /// Limit the number of result rows (note that using 0 here has the same 
    /// effect as 1: get a single result row).
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// Start the results from the change immediately after this sequence 
    /// number.
    /// </summary>
    public int? Since { get; set; }

    /// <summary>
    /// Maximum period in milliseconds to wait for a change before the response 
    /// is sent, even if there are no results. Only applicable for longpoll or 
    /// continuous feeds. Note that 60000 is also the default maximum timeout to 
    /// prevent undetected dead connections.
    /// </summary>
    public int? Timeout { get; set; }
  }
}
