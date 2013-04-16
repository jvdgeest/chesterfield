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

    /// <summary>
    /// <para>Queries a view using custom view options.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <typeparam name="TKey">The row key type.</typeparam>
    /// <typeparam name="TValue">The row value type.</typeparam>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="viewName">Name of the view.</param>
    /// <param name="viewOptions">View options to apply.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<ViewResult<TKey, TValue>> GetView<TKey, TValue>(
      string designName,
      string viewName,
      ViewOptions viewOptions,
      Result<ViewResult<TKey, TValue>> result)
    {
      if (String.IsNullOrEmpty(designName))
        throw new ArgumentNullException("designName");
      if (String.IsNullOrEmpty(viewName))
        throw new ArgumentNullException("viewName");
      if (result == null)
        throw new ArgumentNullException("result");

      GetView(designName, viewName, viewOptions, new Result<DreamMessage>())
        .WhenDone(
          a => result.Return(GetViewResult<TKey, TValue>(a)),
          result.Throw
        );
      return result;
    }

    /// <summary>
    /// <para>Queries a view using the default view options.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <typeparam name="TKey">The row key type.</typeparam>
    /// <typeparam name="TValue">The row value type.</typeparam>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="viewName">Name of the view.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<ViewResult<TKey, TValue>> GetView<TKey, TValue>(
      string designName, 
      string viewName, 
      Result<ViewResult<TKey, TValue>> result)
    {
      return GetView(designName, viewName, new ViewOptions(), result);
    }

    /// <summary>
    /// <para>Queries a view using custom view options. This method will also 
    /// add the corresponding CouchDB document to each row in the view, by
    /// setting the "include_docs" view option to true. This will cause a single
    /// document lookup per returned view result row. This adds significant 
    /// strain on the storage system if you are under high load or return a lot 
    /// of rows per request. If you are concerned about this, you can emit the 
    /// full doc in each row (and not use this method); this will increase view 
    /// index time and space requirements, but will make view reads optimally 
    /// fast.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <typeparam name="TKey">The row key type.</typeparam>
    /// <typeparam name="TValue">The row value type.</typeparam>
    /// <typeparam name="TDocument">The row document type (must inherit from
    /// ICouchDocument).</typeparam>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="viewName">Name of the view.</param>
    /// <param name="viewOptions">View options to apply.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<ViewResult<TKey, TValue, TDocument>>
          GetView<TKey, TValue, TDocument>(
            string designName,
            string viewName,
            ViewOptions viewOptions,
            Result<ViewResult<TKey, TValue, TDocument>> result)
          where TDocument : ICouchDocument
    {
      if (String.IsNullOrEmpty(designName))
        throw new ArgumentNullException("designName");
      if (String.IsNullOrEmpty(viewName))
        throw new ArgumentNullException("viewName");
      if (result == null)
        throw new ArgumentNullException("result");

      viewOptions.IncludeDocs = true; // Ensure documents are requested.

      GetView(designName, viewName, viewOptions, new Result<DreamMessage>())
        .WhenDone(
          a => result.Return(GetViewResult<TKey, TValue, TDocument>(a)),
          result.Throw
        );
      return result;
    }

    /// <summary>
    /// <para>Queries a view using the default view options. This method will
    /// also add the corresponding CouchDB document to each row in the view, by
    /// setting the "include_docs" view option to true. This will cause a single
    /// document lookup per returned view result row. This adds significant 
    /// strain on the storage system if you are under high load or return a lot 
    /// of rows per request. If you are concerned about this, you can emit the 
    /// full doc in each row (and not use this method); this will increase view 
    /// index time and space requirements, but will make view reads optimally 
    /// fast.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <typeparam name="TKey">The row key type.</typeparam>
    /// <typeparam name="TValue">The row value type.</typeparam>
    /// <typeparam name="TDocument">The row document type (must inherit from
    /// ICouchDocument).</typeparam>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="viewName">Name of the view.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<ViewResult<TKey, TValue, TDocument>> 
      GetView<TKey, TValue, TDocument>(
        string designName, 
        string viewName, 
        Result<ViewResult<TKey, TValue, TDocument>> result) 
      where TDocument : ICouchDocument
    {
      return GetView(designName, viewName, new ViewOptions(), result);
    }

    /// <summary>
    /// <para>Creates and queries a temporary view using custom view options.
    /// Temporary views are only good during development. Final code should not 
    /// rely on them as they are very expensive to compute each time they get 
    /// called.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <typeparam name="TKey">The row key type.</typeparam>
    /// <typeparam name="TValue">The row value type.</typeparam>
    /// <param name="view">Temporary view containing the MapReduce code.</param>
    /// <param name="viewOptions">View options to apply.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<ViewResult<TKey, TValue>> GetTempView<TKey, TValue>(
      CouchView view,
      ViewOptions viewOptions,
      Result<ViewResult<TKey, TValue>> result)
    {
      if (view == null)
        throw new ArgumentNullException("view");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(Constants._TEMP_VIEW)
        .With(viewOptions)
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

    /// <summary>
    /// <para>Creates and queries a temporary view using the default view 
    /// options. Temporary views are only good during development. Final code 
    /// should not rely on them as they are very expensive to compute each time
    /// they get called.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <typeparam name="TKey">The row key type.</typeparam>
    /// <typeparam name="TValue">The row value type.</typeparam>
    /// <param name="view">Temporary view containing the MapReduce code.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<ViewResult<TKey, TValue>> GetTempView<TKey, TValue>(
      CouchView view, 
      Result<ViewResult<TKey, TValue>> result)
    {
      return GetTempView(view, null, result);
    }

    /// <summary>
    /// <para>Creates and queries a temporary view using custom view options.
    /// Temporary views are only good during development. Final code should not 
    /// rely on them as they are very expensive to compute each time they get 
    /// called. This method will also add the corresponding CouchDB document to 
    /// each row in the view, by setting the "include_docs" view option to true. 
    /// This will cause a single document lookup per returned view result row. 
    /// This adds significant strain on the storage system if you are under high 
    /// load or return a lot of rows per request. If you are concerned about 
    /// this, you can emit the full doc in each row (and not use this method); 
    /// this will increase view index time and space requirements, but will make
    /// view reads optimally fast.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <typeparam name="TKey">The row key type.</typeparam>
    /// <typeparam name="TValue">The row value type.</typeparam>
    /// <typeparam name="TDocument">The row document type (must inherit from
    /// ICouchDocument).</typeparam>
    /// <param name="view">Temporary view containing the MapReduce code.</param>
    /// <param name="viewOptions">View options to apply.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<ViewResult<TKey, TValue, TDocument>> 
      GetTempView<TKey, TValue, TDocument>(
        CouchView view, 
        ViewOptions viewOptions, 
        Result<ViewResult<TKey, TValue, TDocument>> result) 
      where TDocument : ICouchDocument
    {
      if (view == null)
        throw new ArgumentNullException("view");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(Constants._TEMP_VIEW)
        .With(viewOptions)
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

    /// <summary>
    /// <para>Creates and queries a temporary view using the default view 
    /// options. Temporary views are only good during development. Final code 
    /// should not rely on them as they are very expensive to compute each time 
    /// they get called. This method will also add the corresponding CouchDB 
    /// document to each row in the view, by setting the "include_docs" view 
    /// option to true. This will cause a single document lookup per returned 
    /// view result row. This adds significant strain on the storage system if 
    /// you are under high load or return a lot of rows per request. If you are 
    /// concerned about this, you can emit the full doc in each row (and not use
    /// this method); this will increase view index time and space requirements, 
    /// but will make view reads optimally fast.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <typeparam name="TKey">The row key type.</typeparam>
    /// <typeparam name="TValue">The row value type.</typeparam>
    /// <typeparam name="TDocument">The row document type (must inherit from
    /// ICouchDocument).</typeparam>
    /// <param name="view">Temporary view containing the MapReduce code.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<ViewResult<TKey, TValue, TDocument>>
      GetTempView<TKey, TValue, TDocument>(
        CouchView view,
        Result<ViewResult<TKey, TValue, TDocument>> result)
      where TDocument : ICouchDocument
    {
      return GetTempView(view, null, result);
    }

    /// <summary>
    /// <para>Queries a view using custom view options. This method returns the
    /// raw JSON response.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="viewName">Name of the view.</param>
    /// <param name="viewOptions">View options to apply.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<JObject> GetView(
      string designName,
      string viewName,
      ViewOptions viewOptions,
      Result<JObject> result)
    {
      if (String.IsNullOrEmpty(designName))
        throw new ArgumentNullException("designName");
      if (String.IsNullOrEmpty(viewName))
        throw new ArgumentNullException("viewName");
      if (result == null)
        throw new ArgumentNullException("result");

      GetView(designName, viewName, viewOptions, new Result<DreamMessage>())
        .WhenDone(
          a => result.Return(JObject.Parse(a.ToText())),
          result.Throw
        );
      return result;
    }

    /// <summary>
    /// <para>Queries a view using the default view options. This method returns 
    /// the raw JSON response.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="viewName">Name of the view.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<JObject> GetView(
      string designName, 
      string viewName, 
      Result<JObject> result)
    {
      return GetView(designName, viewName, new ViewOptions(), result);
    }


    /* =========================================================================
     * Synchronous methods 
     * =======================================================================*/

    /// <summary>
    /// Queries a view using custom view options.
    /// </summary>
    /// <typeparam name="TKey">The row key type.</typeparam>
    /// <typeparam name="TValue">The row value type.</typeparam>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="viewName">Name of the view.</param>
    /// <param name="viewOptions">View options to apply.</param>
    /// <returns></returns>
    public ViewResult<TKey, TValue> GetView<TKey, TValue>(
      string designName,
      string viewName,
      ViewOptions viewOptions)
    {
      return GetView(designName, viewName, viewOptions,
        new Result<ViewResult<TKey, TValue>>()).Wait();
    }

    /// <summary>
    /// Queries a view using the default view options.
    /// </summary>
    /// <typeparam name="TKey">The row key type.</typeparam>
    /// <typeparam name="TValue">The row value type.</typeparam>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="viewName">Name of the view.</param>
    /// <returns></returns>
    public ViewResult<TKey, TValue> GetView<TKey, TValue>(
      string designName, 
      string viewName)
    {
      return GetView(designName, viewName, 
        new Result<ViewResult<TKey, TValue>>()).Wait();
    }

    /// <summary>
    /// Queries a view using custom view options. This method will also add the 
    /// corresponding CouchDB document to each row in the view, by setting the 
    /// "include_docs" view option to true. This will cause a single document 
    /// lookup per returned view result row. This adds significant strain on the 
    /// storage system if you are under high load or return a lot of rows per 
    /// request. If you are concerned about this, you can emit the full doc in 
    /// each row (and not use this method); this will increase view index time 
    /// and space requirements, but will make view reads optimally fast.
    /// </summary>
    /// <typeparam name="TKey">The row key type.</typeparam>
    /// <typeparam name="TValue">The row value type.</typeparam>
    /// <typeparam name="TDocument">The row document type (must inherit from
    /// ICouchDocument).</typeparam>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="viewName">Name of the view.</param>
    /// <param name="viewOptions">View options to apply.</param>
    /// <returns></returns>
    public ViewResult<TKey, TValue, TDocument>
      GetView<TKey, TValue, TDocument>(
        string designName,
        string viewName,
        ViewOptions viewOptions)
      where TDocument : ICouchDocument
    {
      return GetView(designName, viewName, viewOptions,
        new Result<ViewResult<TKey, TValue, TDocument>>()).Wait();
    }

    /// <summary>
    /// Queries a view using the default view options. This method will also add 
    /// the corresponding CouchDB document to each row in the view, by setting 
    /// the "include_docs" view option to true. This will cause a single
    /// document lookup per returned view result row. This adds significant 
    /// strain on the storage system if you are under high load or return a lot 
    /// of rows per request. If you are concerned about this, you can emit the 
    /// full doc in each row (and not use this method); this will increase view 
    /// index time and space requirements, but will make view reads optimally 
    /// fast.
    /// </summary>
    /// <typeparam name="TKey">The row key type.</typeparam>
    /// <typeparam name="TValue">The row value type.</typeparam>
    /// <typeparam name="TDocument">The row document type (must inherit from
    /// ICouchDocument).</typeparam>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="viewName">Name of the view.</param>
    /// <returns></returns>
    public ViewResult<TKey, TValue, TDocument> 
      GetView<TKey, TValue, TDocument>(
        string designName, 
        string viewName) 
      where TDocument : ICouchDocument
    {
      return GetView(designName, viewName, 
        new Result<ViewResult<TKey, TValue, TDocument>>()).Wait();
    }

    /// <summary>
    /// Creates and queries a temporary view using custom view options.
    /// Temporary views are only good during development. Final code should not 
    /// rely on them as they are very expensive to compute each time they get 
    /// called.
    /// </summary>
    /// <typeparam name="TKey">The row key type.</typeparam>
    /// <typeparam name="TValue">The row value type.</typeparam>
    /// <param name="view">Temporary view containing the MapReduce code.</param>
    /// <param name="viewOptions">View options to apply.</param>
    /// <returns></returns>
    public ViewResult<TKey, TValue> GetTempView<TKey, TValue>(
      CouchView view, 
      ViewOptions viewOptions)
    {
      return GetTempView(view, viewOptions, 
        new Result<ViewResult<TKey, TValue>>()).Wait();
    }

    /// <summary>
    /// Creates and queries a temporary view using the default view 
    /// options. Temporary views are only good during development. Final code 
    /// should not rely on them as they are very expensive to compute each time
    /// they get called.
    /// </summary>
    /// <typeparam name="TKey">The row key type.</typeparam>
    /// <typeparam name="TValue">The row value type.</typeparam>
    /// <param name="view">Temporary view containing the MapReduce code.</param>
    /// <returns></returns>
    public ViewResult<TKey, TValue> GetTempView<TKey, TValue>(CouchView view)
    {
      return GetTempView(view, null,
        new Result<ViewResult<TKey, TValue>>()).Wait();
    }

    /// <summary>
    /// Creates and queries a temporary view using the default view options.
    /// Temporary views are only good during development. Final code should not 
    /// rely on them as they are very expensive to compute each time they get 
    /// called. This method will also add the corresponding CouchDB document to 
    /// each row in the view, by setting the "include_docs" view option to true. 
    /// This will cause a single document lookup per returned view result row. 
    /// This adds significant strain on the storage system if you are under high 
    /// load or return a lot of rows per request. If you are concerned about 
    /// this, you can emit the full doc in each row (and not use this method); 
    /// this will increase view index time and space requirements, but will make
    /// view reads optimally fast.
    /// </summary>
    /// <typeparam name="TKey">The row key type.</typeparam>
    /// <typeparam name="TValue">The row value type.</typeparam>
    /// <typeparam name="TDocument">The row document type (must inherit from
    /// ICouchDocument).</typeparam>
    /// <param name="view">Temporary view containing the MapReduce code.</param>
    /// <param name="viewOptions">View options to apply.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public ViewResult<TKey, TValue, TDocument> 
      GetTempView<TKey, TValue, TDocument>(
        CouchView view, 
        ViewOptions viewOptions) 
      where TDocument : ICouchDocument
    {
      return GetTempView(view, viewOptions, 
        new Result<ViewResult<TKey, TValue, TDocument>>()).Wait();
    }

    /// <summary>
    /// Creates and queries a temporary view using the default view options. 
    /// Temporary views are only good during development. Final code should not 
    /// rely on them as they are very expensive to compute each time they get 
    /// called. This method will also add the corresponding CouchDB document to 
    /// each row in the view, by setting the "include_docs" view option to true. 
    /// This will cause a single document lookup per returned view result row. 
    /// This adds significant strain on the storage system if you are under high 
    /// load or return a lot of rows per request. If you are concerned about 
    /// this, you can emit the full doc in each row (and not use this method);
    /// this will increase view index time and space requirements, but will make 
    /// view reads optimally fast.
    /// </summary>
    /// <typeparam name="TKey">The row key type.</typeparam>
    /// <typeparam name="TValue">The row value type.</typeparam>
    /// <typeparam name="TDocument">The row document type (must inherit from
    /// ICouchDocument).</typeparam>
    /// <param name="view">Temporary view containing the MapReduce code.</param>
    /// <returns></returns>
    public ViewResult<TKey, TValue, TDocument>
      GetTempView<TKey, TValue, TDocument>(
        CouchView view)
      where TDocument : ICouchDocument
    {
      return GetTempView(view, null,
        new Result<ViewResult<TKey, TValue, TDocument>>()).Wait();
    }

    /// <summary>
    /// Queries a view using custom view options. This method returns the raw 
    /// JSON response.
    /// </summary>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="viewName">Name of the view.</param>
    /// <param name="viewOptions">View options to apply.</param>
    /// <returns></returns>
    public JObject GetView(
      string designName,
      string viewName,
      ViewOptions viewOptions)
    {
      return GetView(designName, viewName, viewOptions,
        new Result<JObject>()).Wait();
    }

    /// <summary>
    /// Queries a view using the default view options. This method returns the 
    /// raw JSON response.
    /// </summary>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="viewName">Name of the view.</param>
    /// <returns></returns>
    public JObject GetView(string designName, string viewName)
    {
      return GetView(designName, viewName, new Result<JObject>()).Wait();
    }

    /* =========================================================================
     * Private methods 
     * =======================================================================*/

    private Result<DreamMessage> GetView(
      string designName, 
      string viewName, 
      ViewOptions viewOptions, 
      Result<DreamMessage> result)
    {
      if (String.IsNullOrEmpty(designName))
        throw new ArgumentNullException("designName");
      if (String.IsNullOrEmpty(viewName))
        throw new ArgumentNullException("viewName");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(Constants._DESIGN, XUri.EncodeFragment(designName), Constants._VIEW,
            XUri.EncodeFragment(viewName))
        .With(viewOptions)
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

    private static ViewResult<TKey, TValue> GetViewResult<TKey, TValue>(
      DreamMessage dreamMessage)
    {
      ViewResult<TKey, TValue> val;
      switch (dreamMessage.Status)
      {
        case DreamStatus.Ok:
          ObjectSerializer<ViewResult<TKey, TValue>> objectSerializer =
            new ObjectSerializer<ViewResult<TKey, TValue>>();
          val = objectSerializer.Deserialize(dreamMessage.ToText());
          val.Status = DreamStatus.Ok;
          val.ETag = dreamMessage.Headers.ETag;
          break;
        default:
          val = new ViewResult<TKey, TValue> { Status = dreamMessage.Status };
          break;
      }
      return val;
    }

    private static ViewResult<TKey, TValue, TDocument> 
      GetViewResult<TKey, TValue, TDocument>(
        DreamMessage dreamMessage) 
      where TDocument : ICouchDocument
    {
      ViewResult<TKey, TValue, TDocument> val;
      switch (dreamMessage.Status)
      {
        case DreamStatus.Ok:
          ObjectSerializer<ViewResult<TKey, TValue, TDocument>> 
            objectSerializer = new ObjectSerializer<ViewResult<TKey, TValue, 
              TDocument>>();
          val = objectSerializer.Deserialize(dreamMessage.ToText());
          val.Status = DreamStatus.Ok;
          val.ETag = dreamMessage.Headers.ETag;
          break;
        default:
          val = new ViewResult<TKey, TValue, TDocument> { Status = 
            dreamMessage.Status };
          break;
      }
      return val;
    }
  }
}