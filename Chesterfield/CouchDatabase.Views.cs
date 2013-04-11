using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MindTouch.Tasking;
using MindTouch.Dream;
using Chesterfield.Support;
using Chesterfield.Interfaces;

namespace Chesterfield
{
  public partial class CouchDatabase : CouchBase
  {
    /* =========================================================================
     * Asynchronous methods 
     * =======================================================================*/

    public Result<ViewResult<TKey, TValue>> GetView<TKey, TValue>(string viewId, 
      string viewName, Result<ViewResult<TKey, TValue>> result)
    {
      return GetView(viewId, viewName, new ViewOptions(), result);
    }

    public Result<ViewResult<TKey, TValue>> GetView<TKey, TValue>(string viewId, 
      string viewName, ViewOptions options, Result<ViewResult<TKey, TValue>> 
      result)
    {
      if (String.IsNullOrEmpty(viewId))
        throw new ArgumentNullException("viewId");
      if (String.IsNullOrEmpty(viewName))
        throw new ArgumentNullException("viewName");
      if (result == null)
        throw new ArgumentNullException("result");

      GetView(viewId, viewName, options, new Result<DreamMessage>()).WhenDone(
        a => result.Return(GetViewResult<TKey, TValue>(a)),
        result.Throw
      );
      return result;
    }

    public Result<ViewResult<TKey, TValue, TDocument>> GetView<TKey, TValue, 
      TDocument>(string viewId, string viewName, Result<ViewResult<TKey, TValue, 
      TDocument>> result) where TDocument : ICouchDocument
    {
      return GetView(viewId, viewName, new ViewOptions(), result);
    }

    public Result<ViewResult<TKey, TValue, TDocument>> GetView<TKey, TValue, 
      TDocument>(string viewId, string viewName, ViewOptions options, 
      Result<ViewResult<TKey, TValue, TDocument>> result) 
      where TDocument : ICouchDocument
    {
      if (String.IsNullOrEmpty(viewId))
        throw new ArgumentNullException("viewId");
      if (String.IsNullOrEmpty(viewName))
        throw new ArgumentNullException("viewName");
      if (result == null)
        throw new ArgumentNullException("result");

      // Ensure documents are requested.
      options.IncludeDocs = true;

      GetView(viewId, viewName, options, new Result<DreamMessage>()).WhenDone(
        a => result.Return(GetViewResult<TKey, TValue, TDocument>(a)),
        result.Throw
      );
      return result;
    }

    public Result<ViewResult<TKey, TValue>> GetTempView<TKey, TValue>(
      CouchView view, Result<ViewResult<TKey, TValue>> result)
    {
      return GetTempView(view, null, result);
    }

    public Result<ViewResult<TKey, TValue>> GetTempView<TKey, TValue>(
      CouchView view, ViewOptions options, 
      Result<ViewResult<TKey, TValue>> result)
    {
      if (view == null)
        throw new ArgumentNullException("view");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(Constants.TEMP_VIEW)
        .With(options)
        .Post(DreamMessage.Ok(MimeType.JSON, JsonConvert.SerializeObject(view)), 
          new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Ok || 
              a.Status == DreamStatus.NotModified)
              result.Return(GetViewResult<TKey, TValue>(a));
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      return result;
    }

    public Result<ViewResult<TKey, TValue, TDocument>> GetTempView<TKey, TValue, 
      TDocument>(CouchView view, Result<ViewResult<TKey, TValue, TDocument>> 
      result) where TDocument : ICouchDocument
    {
      return GetTempView(view, null, result);
    }

    public Result<ViewResult<TKey, TValue, TDocument>> GetTempView<TKey, TValue, 
      TDocument>(CouchView view, ViewOptions options, Result<ViewResult<TKey, 
      TValue, TDocument>> result) where TDocument : ICouchDocument
    {
      if (view == null)
        throw new ArgumentNullException("view");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(Constants.TEMP_VIEW)
        .With(options)
        .Post(DreamMessage.Ok(MimeType.JSON, JsonConvert.SerializeObject(view)), 
          new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Ok || 
              a.Status == DreamStatus.NotModified)
              result.Return(GetViewResult<TKey, TValue, TDocument>(a));
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      return result;
    }

    public Result<JObject> GetView(string viewId, string viewName, 
      Result<JObject> result)
    {
      return GetView(viewId, viewName, new ViewOptions(), result);
    }

    public Result<JObject> GetView(string viewId, string viewName, 
      ViewOptions options, Result<JObject> result)
    {
      if (String.IsNullOrEmpty(viewId))
        throw new ArgumentNullException("viewId");
      if (String.IsNullOrEmpty(viewName))
        throw new ArgumentNullException("viewName");
      if (result == null)
        throw new ArgumentNullException("result");

      GetView(viewId, viewName, options, new Result<DreamMessage>()).WhenDone(
        a => result.Return(JObject.Parse(a.ToText())),
        result.Throw
      );
      return result;
    }

    /* =========================================================================
     * Synchronous methods 
     * =======================================================================*/

    public ViewResult<TKey, TValue> GetView<TKey, TValue>(string viewId, 
      string viewName)
    {
      return GetView(viewId, viewName, 
        new Result<ViewResult<TKey, TValue>>()).Wait();
    }

    public ViewResult<TKey, TValue> GetView<TKey, TValue>(string viewId, 
      string viewName, ViewOptions options)
    {
      return GetView(viewId, viewName, options, 
        new Result<ViewResult<TKey, TValue>>()).Wait();
    }

    public ViewResult<TKey, TValue, TDocument> GetView<TKey, TValue, TDocument>
      (string viewId, string viewName) where TDocument : ICouchDocument
    {
      return GetView(viewId, viewName, 
        new Result<ViewResult<TKey, TValue, TDocument>>()).Wait();
    }

    public ViewResult<TKey, TValue, TDocument> GetView<TKey, TValue, TDocument>
      (string viewId, string viewName, ViewOptions options) 
      where TDocument : ICouchDocument
    {
      return GetView(viewId, viewName, options, 
        new Result<ViewResult<TKey, TValue, TDocument>>()).Wait();
    }

    public ViewResult<TKey, TValue> GetTempView<TKey, TValue>(CouchView view)
    {
      return GetTempView(view, null, 
        new Result<ViewResult<TKey, TValue>>()).Wait();
    }

    public ViewResult<TKey, TValue> GetTempView<TKey, TValue>(CouchView view, 
      ViewOptions options)
    {
      return GetTempView(view, options, 
        new Result<ViewResult<TKey, TValue>>()).Wait();
    }

    public ViewResult<TKey, TValue, TDocument> GetTempView<TKey, TValue, 
      TDocument>(CouchView view) where TDocument : ICouchDocument
    {
      return GetTempView(view, null, 
        new Result<ViewResult<TKey, TValue, TDocument>>()).Wait();
    }

    public ViewResult<TKey, TValue, TDocument> GetTempView<TKey, TValue, 
      TDocument>(CouchView view, ViewOptions options) 
      where TDocument : ICouchDocument
    {
      return GetTempView(view, options, 
        new Result<ViewResult<TKey, TValue, TDocument>>()).Wait();
    }

    public JObject GetView(string viewId, string viewName)
    {
      return GetView(viewId, viewName, new Result<JObject>()).Wait();
    }

    public JObject GetView(string viewId, string viewName, ViewOptions options)
    {
      return GetView(viewId, viewName, options, new Result<JObject>()).Wait();
    }

    /* =========================================================================
     * Private methods 
     * =======================================================================*/

    private Result<DreamMessage> GetView(string viewId, string viewName, 
      ViewOptions options, Result<DreamMessage> result)
    {
      if (String.IsNullOrEmpty(viewId))
        throw new ArgumentNullException("viewId");
      if (String.IsNullOrEmpty(viewName))
        throw new ArgumentNullException("viewName");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(Constants.DESIGN, XUri.EncodeFragment(viewId), Constants.VIEW,
          XUri.EncodeFragment(viewName))
        .With(options)
        .Get(new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Ok || 
              a.Status == DreamStatus.NotModified)
              result.Return(a);
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      return result;
    }

    private static ViewResult<TKey, TValue> GetViewResult<TKey, TValue>
      (DreamMessage aDreamMessage)
    {
      ViewResult<TKey, TValue> val;
      switch (aDreamMessage.Status)
      {
        case DreamStatus.Ok:
          ObjectSerializer<ViewResult<TKey, TValue>> objectSerializer =
            new ObjectSerializer<ViewResult<TKey, TValue>>();
          val = objectSerializer.Deserialize(aDreamMessage.ToText());
          val.Status = DreamStatus.Ok;
          val.ETag = aDreamMessage.Headers.ETag;
          break;
        default:
          val = new ViewResult<TKey, TValue> { Status = aDreamMessage.Status };
          break;
      }
      return val;
    }

    private static ViewResult<TKey, TValue, TDocument> GetViewResult<TKey, 
      TValue, TDocument>(DreamMessage aDreamMessage) 
      where TDocument : ICouchDocument
    {
      ViewResult<TKey, TValue, TDocument> val;
      switch (aDreamMessage.Status)
      {
        case DreamStatus.Ok:
          ObjectSerializer<ViewResult<TKey, TValue, TDocument>> 
            objectSerializer = new ObjectSerializer<ViewResult<TKey, TValue, 
              TDocument>>();
          val = objectSerializer.Deserialize(aDreamMessage.ToText());
          val.Status = DreamStatus.Ok;
          val.ETag = aDreamMessage.Headers.ETag;
          break;
        default:
          val = new ViewResult<TKey, TValue, TDocument> { Status = 
            aDreamMessage.Status };
          break;
      }
      return val;
    }
  }
}