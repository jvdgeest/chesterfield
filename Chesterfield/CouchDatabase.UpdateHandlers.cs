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
    /// <param name="designName">Name of the design document containing the 
    /// update handler.</param>
    /// <param name="functionName">Name of the function to invoke.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<JObject> UpdateHandle(
      string designName,
      string functionName,
      Result<JObject> result)
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
          a => result.Return(JObject.Parse(a.ToText())),
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
    /// <param name="designName">Name of the design document containing the 
    /// update handler.</param>
    /// <param name="functionName">Name of the function to invoke.</param>
    /// <returns></returns>
    public JObject UpdateHandle(string designName, string functionName)
    {
      return GetView(designName, functionName, new Result<JObject>()).Wait();
    }
  }
}
