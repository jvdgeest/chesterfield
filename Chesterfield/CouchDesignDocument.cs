using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chesterfield
{
  public class CouchDesignDocument : CouchDocument
  {
    /// <summary>
    /// Creates an empty CouchDesignDocument with JavaScript as language and 
    /// empty views, shows, lists and update handlers.
    /// </summary>
    public CouchDesignDocument()
    {
      Language = Constants.JAVASCRIPT;
      Views = new Dictionary<string, CouchView>();
      Shows = new Dictionary<string, string>();
      Lists = new Dictionary<string, string>();
      Updates = new Dictionary<string, string>();
    }

    /// <summary>
    /// Initializes a CouchDesignDocument with the proper document ID. If your
    /// design document is located at "_design/test", then pass "test" as 
    /// argument (without the quotes).
    /// </summary>
    /// <param name="designName">Name of the design document (what comes after
    /// "_design/").</param>
    public CouchDesignDocument(string designName)
      : this()
    {
      Id = string.Format("{0}/{1}", Constants._DESIGN, designName);
    }

    /// <summary>
    /// This property tells CouchDB the language of the functions inside the
    /// design document (such as map, reduce, validate, show, list, etc.). Based
    /// on this it selects the appropriate ViewServer (as specified in your 
    /// couch.ini file). The default is to assume Javascript.
    /// </summary>
    [JsonProperty(Constants.LANGUAGE)]
    public string Language { get; set; }

    /// <summary>
    /// Views are the primary tool used for querying and reporting on CouchDB 
    /// documents. The dictionary contains the names of the functions as keys 
    /// and the code to execute as values.
    /// </summary>
    [JsonProperty(Constants.VIEWS)]
    public Dictionary<string, CouchView> Views { get; internal set; }

    /// <summary>
    /// Show functions can transform documents into any format. The dictionary 
    /// contains the names of the functions as keys and the code to execute as 
    /// values.
    /// </summary>
    [JsonProperty(Constants.SHOWS)]
    public Dictionary<string, string> Shows { get; private set; }

    /// <summary>
    /// List functions can be used to render documents as a group. The
    /// dictionary contains the names of the functions as keys and the code
    /// to execute as values.
    /// </summary>
    [JsonProperty(Constants.LISTS)]
    public Dictionary<string, string> Lists { get; private set; }

    /// <summary>
    /// Update handlers are functions that clients can request to invoke 
    /// server-side logic that will create or update a document. The dictionary 
    /// contains the names of the functions as keys and the code to execute as 
    /// values.
    /// </summary>
    [JsonProperty(Constants.UPDATES)]
    public Dictionary<string, string> Updates { get; private set; }

    /// <summary>
    /// This function can be used to prevent invalid or unauthorized document 
    /// updates from proceeding.
    /// </summary>
    [JsonProperty(Constants.VALIDATE_DOC_UPDATE)]
    public string ValidateDocUpdate { get; set; }
  }
}