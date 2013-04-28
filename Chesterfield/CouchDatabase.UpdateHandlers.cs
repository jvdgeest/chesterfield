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
    /// <para>Invokes an update handler.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <typeparam name="TResponse">Type of response to return, such as plain
    /// HTML, JSON or a deserialized ICouchDocument (must inherit from
    /// IUpdateResponse).</typeparam>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="functionName">Name of the update handler function.</param>
    /// <param name="id">Document ID to send to the update handler.</param>
    /// <param name="request">Request data to send (can be null).</param>
    /// <param name="contentType">Content type of the request data (can be null
    /// if the request parameter is also null).</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<TResponse> UpdateHandle<TResponse>(
        string designName,
        string functionName,
        string id,
        string request,
        MimeType contentType,
        Result<TResponse> result)
      where TResponse : class, IUpdateResponse
    {
      if (String.IsNullOrEmpty(designName))
        throw new ArgumentNullException("designName");
      if (String.IsNullOrEmpty(functionName))
        throw new ArgumentNullException("functionName");
      if (request != null && contentType == null)
        throw new ArgumentNullException("contentType");
      if (result == null)
        throw new ArgumentNullException("result");

      Plug plug = BasePlug
        .At(Constants._DESIGN)
        .At(XUri.EncodeFragment(designName))
        .At(Constants._UPDATE)
        .At(XUri.EncodeFragment(functionName));

      if (id != null)
        plug = plug.At(id);

      plug.Invoke(
          id == null ? Verb.POST : Verb.PUT,
          request == null
            ? DreamMessage.Ok()
            : DreamMessage.Ok(contentType, request),
          new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Ok || a.Status == DreamStatus.Created)
            {
              TResponse response = Activator.CreateInstance<TResponse>();
              response.Rev = a.Headers["X-Couch-Update-NewRev"];
              response.HttpResponse = a.ToText();
              result.Return(response);
            }
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      return result;
    }

    /// <summary>
    /// <para>Invokes an update handler with an ICouchDocument as request data.
    /// This method will serialize the document to JSON format.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <typeparam name="TDocument">Type of document to serialize and send as
    /// request data (must inherit from ICouchDocument).</typeparam>
    /// <typeparam name="TResponse">Type of response to return, such as plain
    /// HTML, JSON or a deserialized ICouchDocument (must inherit from
    /// IUpdateResponse).</typeparam>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="functionName">Name of the update handler function.</param>
    /// <param name="id">Document ID to send to the update handler.</param>
    /// <param name="request">ICouchDocument to send to the handler.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<TResponse> UpdateHandle<TDocument, TResponse>(
        string designName,
        string functionName,
        string id,
        TDocument request,
        Result<TResponse> result)
      where TResponse : class, IUpdateResponse
      where TDocument : class, ICouchDocument
    {
      ObjectSerializer<TDocument> serializer =
        new ObjectSerializer<TDocument>();
      return UpdateHandle<TResponse>(designName, functionName, id,
        serializer.Serialize(request), MimeType.JSON, result);
    }

    /// <summary>
    /// <para>Invokes an update handler with a JSON object as request data. 
    /// </para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <typeparam name="TResponse">Type of response to return, such as plain
    /// HTML, JSON or a deserialized ICouchDocument (must inherit from
    /// IUpdateResponse).</typeparam>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="functionName">Name of the update handler function.</param>
    /// <param name="id">Document ID to send to the update handler.</param>
    /// <param name="request">JSON object to send to the handler.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<TResponse> UpdateHandle<TResponse>(
        string designName,
        string functionName,
        string id,
        JObject request,
        Result<TResponse> result)
      where TResponse : class, IUpdateResponse
    {
      return UpdateHandle<TResponse>(designName, functionName, id,
        request.ToString(Formatting.None), MimeType.JSON, result);
    }

    /// <summary>
    /// <para>Invokes an update handler without sending any request data.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <typeparam name="TResponse">Type of response to return, such as plain
    /// HTML, JSON or a deserialized ICouchDocument (must inherit from
    /// IUpdateResponse).</typeparam>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="functionName">Name of the update handler function.</param>
    /// <param name="id">Document ID to send to the update handler.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<TResponse> UpdateHandle<TResponse>(
        string designName,
        string functionName,
        string id,
        Result<TResponse> result)
      where TResponse : class, IUpdateResponse
    {
      return UpdateHandle<TResponse>(designName, functionName, id, null, null, 
        result);
    }

    /* =========================================================================
     * Synchronous methods 
     * =======================================================================*/

    /// <summary>
    /// Invokes an update handler with an ICouchDocument as request data. This 
    /// method will serialize the document to JSON format.
    /// </summary>
    /// <typeparam name="TDocument">Type of document to serialize and send as
    /// request data (must inherit from ICouchDocument).</typeparam>
    /// <typeparam name="TResponse">Type of response to return, such as plain
    /// HTML, JSON or a deserialized ICouchDocument (must inherit from
    /// IUpdateResponse).</typeparam>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="functionName">Name of the update handler function.</param>
    /// <param name="id">Document ID to send to the update handler.</param>
    /// <param name="request">ICouchDocument to send to the handler.</param>
    /// <returns>Response of the update handler.</returns>
    public TResponse UpdateHandle<TDocument, TResponse>(
        string designName,
        string functionName,
        string id,
        TDocument request)
      where TResponse : class, IUpdateResponse
      where TDocument : class, ICouchDocument
    {
      return UpdateHandle<TDocument, TResponse>(designName, functionName, id, 
        request, new Result<TResponse>()).Wait();
    }

    /// <summary>
    /// Invokes an update handler with an ICouchDocument as request data. This 
    /// method will serialize the document to JSON format.
    /// </summary>
    /// <typeparam name="TDocument">Type of document to serialize and send as
    /// request data (must inherit from ICouchDocument).</typeparam>
    /// <typeparam name="TResponse">Type of response to return, such as plain
    /// HTML, JSON or a deserialized ICouchDocument (must inherit from
    /// IUpdateResponse).</typeparam>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="functionName">Name of the update handler function.</param>
    /// <param name="request">ICouchDocument to send to the handler.</param>
    /// <returns>Response of the update handler.</returns>
    public TResponse UpdateHandle<TDocument, TResponse>(
        string designName,
        string functionName,
        TDocument request)
      where TResponse : class, IUpdateResponse
      where TDocument : class, ICouchDocument
    {
      return UpdateHandle<TDocument, TResponse>(designName, functionName, null,
        request, new Result<TResponse>()).Wait();
    }

    /// <summary>
    /// Invokes an update handler with a JSON object as request data. 
    /// </summary>
    /// <typeparam name="TResponse">Type of response to return, such as plain
    /// HTML, JSON or a deserialized ICouchDocument (must inherit from
    /// IUpdateResponse).</typeparam>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="functionName">Name of the update handler function.</param>
    /// <param name="id">Document ID to send to the update handler.</param>
    /// <param name="request">JSON object to send to the handler.</param>
    /// <returns>Response of the update handler</returns>
    public TResponse UpdateHandle<TResponse>(
        string designName,
        string functionName,
        string id,
        JObject request)
      where TResponse : class, IUpdateResponse
    {
      return UpdateHandle<TResponse>(designName, functionName, id, request,
        new Result<TResponse>()).Wait();
    }

    /// <summary>
    /// Invokes an update handler with a JSON object as request data. 
    /// </summary>
    /// <typeparam name="TResponse">Type of response to return, such as plain
    /// HTML, JSON or a deserialized ICouchDocument (must inherit from
    /// IUpdateResponse).</typeparam>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="functionName">Name of the update handler function.</param>
    /// <param name="request">JSON object to send to the handler.</param>
    /// <returns>Response of the update handler,</returns>
    public TResponse UpdateHandle<TResponse>(
        string designName,
        string functionName,
        JObject request)
      where TResponse : class, IUpdateResponse
    {
      return UpdateHandle<TResponse>(designName, functionName, null, request,
        new Result<TResponse>()).Wait();
    }

    /// <summary>
    /// Invokes an update handler without sending any request data.
    /// </summary>
    /// <typeparam name="TResponse">Type of response to return, such as plain
    /// HTML, JSON or a deserialized ICouchDocument (must inherit from
    /// IUpdateResponse).</typeparam>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="functionName">Name of the update handler function.</param>
    /// <param name="id">Document ID to send to the update handler.</param>
    /// <returns>Response of the update handler.</returns>
    public TResponse UpdateHandle<TResponse>(
        string designName,
        string functionName,
        string id)
      where TResponse : class, IUpdateResponse
    {
      return UpdateHandle<TResponse>(designName, functionName, id,
        new Result<TResponse>()).Wait();
    }

    /// <summary>
    /// Invokes an update handler without sending any request data.
    /// </summary>
    /// <typeparam name="TResponse">Type of response to return, such as plain
    /// HTML, JSON or a deserialized ICouchDocument (must inherit from
    /// IUpdateResponse).</typeparam>
    /// <param name="designName">Name of the design document.</param>
    /// <param name="functionName">Name of the update handler function.</param>
    /// <returns>Response of the update handler.</returns>
    public TResponse UpdateHandle<TResponse>(
        string designName,
        string functionName)
      where TResponse : class, IUpdateResponse
    {
      return UpdateHandle<TResponse>(designName, functionName, null,
        new Result<TResponse>()).Wait();
    }
  }
}
