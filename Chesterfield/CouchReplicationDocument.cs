using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chesterfield
{
  /// <summary>
  /// Represents the user context under which a replication runs. Can be part of
  /// a CouchDB replication document. 
  /// </summary>
  public class UserContext
  {
    /// <summary>
    /// The username under which a replication will run.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// A (sub)set of roles of the given user.
    /// </summary>
    [JsonProperty("roles")]
    public string[] Roles { get; set; }
  }

  /// <summary>
  /// Represents a CouchDB replication document. These documents have to be 
  /// stored in the "_replicate" database of your CouchDB server.
  /// </summary>
  public class CouchReplicationDocument : CouchDocument
  {
    /// <summary>
    /// Creates a CouchDB replication document. These documents have to be 
    /// stored in the "_replicate" database of your CouchDB server.
    /// </summary>
    public CouchReplicationDocument()
    {
      QueryParams = new Dictionary<string, string>();
    }

    /// <summary>
    /// Identifies the database to copy revisions from. Can be a string 
    /// containing a local database name or a remote database URL, or an object 
    /// whose url property contains the database name or URL.
    /// </summary>
    [JsonProperty(Constants.SOURCE)]
    public string Source { get; set; }

    /// <summary>
    /// Identifies the database to copy revisions to. Same format and 
    /// interpretation as source. 
    /// </summary>
    [JsonProperty(Constants.TARGET)]
    public string Target { get; set; }

    /// <summary>
    /// A value of true makes the replication continuous. CouchDB can persist 
    /// continuous replications over a server restart. 
    /// </summary>
    [JsonProperty(Constants.CONTINUOUS)]
    public bool Continuous { get; set; }

    /// <summary>
    /// Object containing properties that are passed to the filter function. 
    /// </summary>
    [JsonProperty(Constants.QUERY_PARAMS)]
    public Dictionary<string, string> QueryParams { get; set; }

    /// <summary>
    /// A value of true tells the replicator to create the target database if it 
    /// doesn't exist yet. 
    /// </summary>
    [JsonProperty(Constants.CREATE_TARGET)]
    public bool? CreateTarget { get; set; }

    /// <summary>
    /// Name of a filter function that can choose which revisions get 
    /// replicated. Sometimes you don't want to transfer all documents from 
    /// source to target. You can include one or more filter functions in a 
    /// design document (under the top-level "filters" key) on the source and 
    /// then tell the replicator to use them. A filter function takes two 
    /// arguments (the document to be replicated and the the replication 
    /// request) and returns true or false. If the result is true, then the 
    /// document is replicated. 
    /// </summary>
    [JsonProperty(Constants.FILTER)]
    public string Filter { get; set; }

    /// <summary>
    /// Defines the user context under which a replication runs. For non admin 
    /// users, a user_ctx property, containing the user's name and a subset of 
    /// his/her roles, must be defined in the replication document. This is 
    /// ensured by the document update validation function present in the 
    /// default design document of the replicator database. This validation 
    /// function also ensure that a non admin user can set a user name property 
    /// in the user_ctx property that doesn't match his/her own name (same 
    /// principle applies for the roles). When the roles property of user_ctx is 
    /// missing, it defaults to the empty list [].
    /// </summary>
    [JsonProperty(Constants.USER_CONTEXT)]
    public UserContext UserContext { get; set; }

    /// <summary>
    /// Array of document IDs; if given, only these documents will be 
    /// replicated. 
    /// </summary>
    [JsonProperty(Constants.DOC_IDS)]
    public string[] DocIds { get; set; }

    /// <summary>
    /// The ID internally assigned to the replication. This is the ID exposed by
    /// the output from /_active_tasks/.
    /// </summary>
    [JsonProperty(Constants._REPLICATION_ID)]
    public string ReplicationId { get; internal set; }

    /// <summary>
    /// The current state of the replication.
    /// </summary>
    [JsonProperty(Constants._REPLICATION_STATE)]
    public string ReplicationState { get; internal set; }

    /// <summary>
    /// An RFC3339 compliant timestamp that tells us when the current 
    /// replication state (defined in _replication_state) was set.
    /// </summary>
    [JsonProperty(Constants._REPLICATION_STATE_TIME)]
    public DateTimeOffset? ReplicationStateTime { get; internal set; }
  }
}