using System;
using Newtonsoft.Json;

namespace Chesterfield
{
  /// <summary>
  /// Represents a CouchDB database information response. Use the GetInfo method
  /// of a CouchDatabase object to retrieve information of a database.
  /// </summary>
  public class CouchDatabaseInfo
  {
    private static readonly DateTime Epoch = 
      new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Indicates if a compaction is running.
    /// </summary>
    [JsonProperty(Constants.COMPACT_RUNNING)]
    public bool CompactRunning { get; private set; }

    /// <summary>
    /// Name of the database.
    /// </summary>
    [JsonProperty(Constants.DB_NAME)]
    public string Name { get; private set; }

    /// <summary>
    /// Current version of the internal database format on disk.
    /// </summary>
    [JsonProperty(Constants.DISK_FORMAT_VERSION)]
    public int DiskFormatVersion { get; private set; }

    /// <summary>
    /// Current size in bytes of the database (size of views indexes on disk are
    /// not included).
    /// </summary>
    [JsonProperty(Constants.DISK_SIZE)]
    public int DiskSize { get; private set; }

    /// <summary>
    /// Number of documents (including design documents) in the database.
    /// </summary>
    [JsonProperty(Constants.DOC_COUNT)]
    public int DocCount { get; private set; }

    /// <summary>
    /// Number of documents that have been deleted.
    /// </summary>
    [JsonProperty(Constants.DOC_DEL_COUNT)]
    public int DocDeletedCount { get; private set; }

    /// <summary>
    /// Number of purge operations.
    /// </summary>
    [JsonProperty(Constants.PURGE_SEQUENCE)]
    public int PurgeSequence { get; private set; }

    /// <summary>
    /// Current number of updates to the database.
    /// </summary>
    [JsonProperty(Constants.UPDATE_SEQUENCE)]
    public int UpdateSequence { get; private set; }

    /// <summary>
    /// Timestamp of CouchDB's start time (in milliseconds).
    /// </summary>
    [JsonProperty(Constants.INSTANCE_STARTTIME)]
    public double InstanceStartTimeMs { get; private set; }

    /// <summary>
    /// Timestamp of CouchDB's start time.
    /// </summary>
    public DateTime InstanceStartTime {
      get { return Epoch.AddMilliseconds(InstanceStartTimeMs / 1000); }
    }
  }
}