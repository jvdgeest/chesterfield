using System;
using Newtonsoft.Json.Linq;
using Chesterfield.Interfaces;

namespace Chesterfield
{
  /// <summary>
  /// HTTP formatted response from an update handler.
  /// </summary>
  public class UpdateHttpResponse : IUpdateResponse
  {
    /// <summary>
    /// If the update handler contains the CouchDB document as first member of
    /// the return array, then this field will contain the revision number from
    /// the document after the change or create has been applied. Otherwise,
    /// the value will be null.
    /// </summary>
    public string Rev { get; set; }

    /// <summary>
    /// The raw HTTP response of the update handler.
    /// </summary>
    public string HttpResponse { get; set; }
  }

  /// <summary>
  /// JSON formatted response from an update handler.
  /// </summary>
  public class UpdateJsonResponse : UpdateHttpResponse
  {
    /// <summary>
    /// A JSON formatted response of the update handler.
    /// </summary>
    public JObject Response
    {
      get { return JObject.Parse(HttpResponse); }
    }
  }

  /// <summary>
  /// ICouchDocument deserialized response from an update handler.
  /// </summary>
  public class UpdateDocResponse<TDocument> : UpdateHttpResponse 
    where TDocument : ICouchDocument
  {
    /// <summary>
    /// A ICouchDocument deserialized response from an update handler.
    /// </summary>
    public TDocument Response
    {
      get
      {
        ObjectSerializer<TDocument> serializer = 
          new ObjectSerializer<TDocument>();
        return serializer.Deserialize(HttpResponse);
      }
    }
  }
}