using System;
using MindTouch.Dream;

namespace Chesterfield.Support
{
  public static class PlugExtensions
  {
    public static Plug With(this Plug plug, ViewOptions viewOptions)
    {
      if (viewOptions == null)
        return plug;

      if ((viewOptions.Key != null) && (viewOptions.Key.Count > 0))
        plug = plug.With(Constants.KEY, viewOptions.Key.ToString());
      if ((viewOptions.StartKey != null) && (viewOptions.StartKey.HasValues))
        plug = plug.With(Constants.STARTKEY, viewOptions.StartKey.ToString());
      if ((viewOptions.EndKey != null) && (viewOptions.EndKey.Count > 0))
        plug = plug.With(Constants.ENDKEY, viewOptions.EndKey.ToString());
      if (viewOptions.Limit.HasValue)
        plug = plug.With(Constants.LIMIT, viewOptions.Limit.Value);
      if (viewOptions.Skip.HasValue)
        plug = plug.With(Constants.SKIP, viewOptions.Skip.ToString());
      if (viewOptions.Reduce.HasValue)
        plug = plug.With(Constants.REDUCE, 
          viewOptions.Reduce.Value ? "true" : "false");
      if (viewOptions.Group.HasValue)
        plug = plug.With(Constants.GROUP, 
          viewOptions.Group.Value ? "true" : "false");
      if (viewOptions.InclusiveEnd.HasValue)
        plug = plug.With(Constants.INCLUSIVE_END, 
          viewOptions.InclusiveEnd.Value ? "true" : "false");
      if (viewOptions.IncludeDocs.HasValue)
        plug = plug.With(Constants.INCLUDE_DOCS, 
          viewOptions.IncludeDocs.Value ? "true" : "false");
      if (viewOptions.GroupLevel.HasValue)
        plug = plug.With(Constants.GROUP_LEVEL, viewOptions.GroupLevel.Value);
      if (viewOptions.Descending.HasValue)
        plug = plug.With(Constants.DESCENDING, 
          viewOptions.Descending.Value ? "true" : "false");
      if (viewOptions.Stale.HasValue)
      {
        switch (viewOptions.Stale.Value)
        {
          case Stale.Normal:
            plug = plug.With(Constants.STALE, Constants.OK);
            break;
          case Stale.UpdateAfter:
            plug = plug.With(Constants.STALE, Constants.UPDATE_AFTER);
            break;
          default:
            throw new ArgumentException("Invalid Stale Option");
        }
      }
      if (!string.IsNullOrEmpty(viewOptions.StartKeyDocId))
        plug = plug.With(Constants.STARTKEY_DOCID, viewOptions.StartKeyDocId);
      if (!string.IsNullOrEmpty(viewOptions.EndKeyDocId))
        plug = plug.With(Constants.ENDKEY_DOCID, viewOptions.EndKeyDocId);
      if (!string.IsNullOrEmpty(viewOptions.Etag))
        plug = plug.WithHeader(DreamHeaders.IF_NONE_MATCH, viewOptions.Etag);

      return plug;
    }

    public static Plug With(this Plug plug, ChangeOptions changeOptions)
    {
      switch (changeOptions.Feed)
      {
        case ChangeFeed.LongPoll:
        case ChangeFeed.Normal:
          plug = plug.With(Constants.FEED, Constants.FEED_NORMAL);
          break;
        case ChangeFeed.Continuous:
          plug = plug.With(Constants.FEED, Constants.FEED_CONTINUOUS);
          break;
        default:
          //Never get here
          break;
      }
      if (!String.IsNullOrEmpty(changeOptions.Filter))
        plug = plug.With(Constants.FILTER, XUri.Encode(changeOptions.Filter));
      if (changeOptions.Heartbeat.HasValue)
        plug = plug.With(Constants.HEARTBEAT, changeOptions.Heartbeat.Value);
      if (changeOptions.IncludeDocs.HasValue)
        plug = plug.With(Constants.INCLUDE_DOCS, 
          changeOptions.IncludeDocs.Value ? "true" : "false");
      if (changeOptions.Limit.HasValue)
        plug = plug.With(Constants.LIMIT, changeOptions.Limit.Value);
      if (changeOptions.Since.HasValue)
        plug = plug.With(Constants.SINCE, changeOptions.Since.Value);
      if (changeOptions.Timeout.HasValue)
        plug = plug.With(Constants.TIMEOUT, changeOptions.Timeout.Value);

      return plug;
    }
  }
}