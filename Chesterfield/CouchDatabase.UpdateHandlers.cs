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
    /// <para>Invokes an update handler in CouchDB. This method will call a 
    /// handler function without sending a document ID and request object.
    /// You can use update handlers to invoke server-side logic that will create
    /// or update a CouchDB document.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <typeparam name="TResponse">Type of response to return, such as plain
    /// HTML, JSON or a deserialized ICouchDocument (must inherit from
    /// IUpdateResponse).</typeparam>
    /// <param name="designName">Name of the design document containing the 
    /// update handler.</param>
    /// <param name="functionName">Name of the update handler function to 
    /// invoke.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<TResponse> UpdateHandle<TResponse>(
        string designName,
        string functionName,
        Result<TResponse> result)
      where TResponse : class, IUpdateResponse
    {
      if (String.IsNullOrEmpty(designName))
        throw new ArgumentNullException("designName");
      if (String.IsNullOrEmpty(functionName))
        throw new ArgumentNullException("functionName");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(Constants._DESIGN)
        .At(XUri.EncodeFragment(designName))
        .At(Constants._UPDATE)
        .At(XUri.EncodeFragment(functionName))
        .Post(new Result<DreamMessage>())
        .WhenDone(
          a => {
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

    /* =========================================================================
     * Synchronous methods 
     * =======================================================================*/

    /// <summary>
    /// Invokes an update handler in CouchDB. This method will call a handler 
    /// function without sending a document ID and request object. You can use 
    /// update handlers to invoke server-side logic that will create or update a
    /// CouchDB document.
    /// </summary>
    /// <typeparam name="TResponse">Type of response to return, such as plain
    /// HTML, JSON or a deserialized ICouchDocument (must inherit from
    /// IUpdateResponse).</typeparam>
    /// <param name="designName">Name of the design document containing the 
    /// update handler.</param>
    /// <param name="functionName">Name of the update handler function to
    /// invoke.</param>
    /// <returns>Response of the update handler.</returns>
    public TResponse UpdateHandle<TResponse>(
        string designName, 
        string functionName)
      where TResponse : class, IUpdateResponse
    {
      return UpdateHandle<TResponse>(designName, functionName, 
        new Result<TResponse>()).Wait();
    }
  }
}
