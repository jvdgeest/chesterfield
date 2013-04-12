using System;
using System.Collections.Generic;
using System.Web;
using MindTouch.Dream;
using MindTouch.Tasking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Chesterfield.Support;

namespace Chesterfield
{
  /* This class has the following partial classes:
   * > CouchClient.cs
   * > CouchClient.Users.cs
   */
  public partial class CouchClient : CouchBase
  {
    /// <summary>
    /// Constructs the CouchClient and gets an authentication cookie.
    /// </summary>
    /// <param name="host">The hostname of the CouchDB instance</param>
    /// <param name="port">The port of the CouchDB instance</param>
    /// <param name="aUserName">The username of the CouchDB instance</param>
    /// <param name="password">The password of the CouchDB instance</param>
    public CouchClient(
      string host = Constants.LOCALHOST,
      int port = Constants.DEFAULT_PORT,
      string username = null,
      string password = null)
      : base(new XUri(String.Format("http://{0}:{1}", host, port)), 
             username, password)
    {
    }

    /// <summary>
    /// Constructs the CouchClient and gets an authentication cookie.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string in
    /// the Web.Config or App.Config file.</param>
    /// <example>
    /// The connection string should be in the following format:
    /// "Host=localhost;Port=5984;Database=ExampleDB;Username=admin;
    /// Password=adminPass;SslEnabled=true"
    /// </example>
    public CouchClient(string connectionStringName) : base(connectionStringName)
    {
    }

    /// <summary>
    /// Triggers one way replication from the source to target.  If bidirection is needed call this method twice with the source and target args reversed.
    /// </summary>
    /// <param name="aReplicationOptions">Replication Options</param>
    /// <param name="aResult"></param>
    /// <returns></returns>
    [Obsolete("If using CouchDB >= 1.1 use CouchReplicationDocument")]
    public Result<JObject> TriggerReplication(ReplicationOptions aReplicationOptions, Result<JObject> aResult)
    {
      if (aReplicationOptions == null)
        throw new ArgumentNullException("aReplicationOptions");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      Plug p = BasePlug.At(Constants._REPLICATE);

      string json = aReplicationOptions.ToString();
      p.Post(DreamMessage.Ok(MimeType.JSON, json), new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if ((a.Status == DreamStatus.Accepted) ||
             (a.Status == DreamStatus.Ok))
          {
            aResult.Return(JObject.Parse(a.ToText()));
          }
          else
          {
            aResult.Throw(new CouchException(a));
          }
        },
        aResult.Throw
      );

      return aResult;
    }
    /// <summary>
    /// Restarts the CouchDB instance. You must be authenticated as a user with administration privileges for this to work.
    /// </summary>
    /// <param name="aResult"></param>
    /// <returns></returns>
    public Result RestartServer(Result aResult)
    {
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      BasePlug.At(Constants._RESTART).Post(DreamMessage.Ok(MimeType.JSON, String.Empty), new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if (a.Status == DreamStatus.Ok)
            aResult.Return();
          else
            aResult.Throw(new CouchException(a));
        },
        aResult.Throw
      );
      return aResult;
    }

    /// <summary>
    /// Triggers one way replication from the source to target.  If bidirection is needed call this method twice with the source and target args reversed.
    /// </summary>
    /// <param name="aReplicationOptions">Replication options</param>
    /// <returns></returns>
    public JObject TriggerReplication(ReplicationOptions aReplicationOptions)
    {
      return TriggerReplication(aReplicationOptions, new Result<JObject>()).Wait();
    }
    /// <summary>
    /// Restarts the CouchDB instance. You must be authenticated as a user with administration privileges for this to work.
    /// </summary>
    public void RestartServer()
    {
      RestartServer(new Result()).Wait();
    }


    #region Configuration Management
    #region Asynchronous Methods
    public Result<Dictionary<string, Dictionary<string, string>>> GetConfig(Result<Dictionary<string, Dictionary<string, string>>> aResult)
    {
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      BasePlug.At(Constants._CONFIG).Get(DreamMessage.Ok(), new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if (a.Status == DreamStatus.Ok)
            aResult.Return(JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(a.ToText()));
          else
            aResult.Throw(new CouchException(a));
        },
        aResult.Throw
      );
      return aResult;
    }
    public Result<Dictionary<string, string>> GetConfigSection(string aSection, Result<Dictionary<string, string>> aResult)
    {
      if (aSection == null)
        throw new ArgumentNullException("aSection");
      if (aResult == null)
        throw new ArgumentNullException("aResult");
      if (String.IsNullOrEmpty(aSection))
        throw new ArgumentException("Section cannot be empty");

      BasePlug.At(Constants._CONFIG, XUri.EncodeFragment(aSection)).Get(DreamMessage.Ok(), new Result<DreamMessage>()).WhenDone(
        a =>
        {
          switch (a.Status)
          {
            case DreamStatus.Ok:
              aResult.Return(JsonConvert.DeserializeObject<Dictionary<string, string>>(a.ToText()));
              break;
            case DreamStatus.NotFound:
              aResult.Return(new Dictionary<string, string>());
              break;
            default:
              aResult.Throw(new CouchException(a));
              break;
          }
        },
        aResult.Throw
      );
      return aResult;
    }
    public Result<string> GetConfigValue(string aSection, string aKeyName, Result<string> aResult)
    {
      if (String.IsNullOrEmpty(aSection))
        throw new ArgumentException("aSection cannot be null nor empty");
      if (String.IsNullOrEmpty(aKeyName))
        throw new ArgumentException("aKeyName cannot be null nor empty");
      if (aResult == null)
        throw new ArgumentNullException("aResult");


      BasePlug.At(Constants._CONFIG, XUri.EncodeFragment(aSection), XUri.EncodeFragment(aKeyName)).Get(DreamMessage.Ok(), new Result<DreamMessage>()).WhenDone(
        a =>
        {
          string value = a.ToText();
          switch (a.Status)
          {
            case DreamStatus.Ok:
              aResult.Return(value.Substring(1, value.Length - 3));// remove " and "\n
              break;
            case DreamStatus.NotFound:
              aResult.Return((string)null);
              break;
            default:
              aResult.Throw(new CouchException(a));
              break;
          }
        },
        aResult.Throw
      );
      return aResult;
    }
    public Result SetConfigValue(string aSection, string aKeyName, string aValue, Result aResult)
    {
      if (String.IsNullOrEmpty(aSection))
        throw new ArgumentException("aSection cannot be null nor empty");
      if (String.IsNullOrEmpty(aKeyName))
        throw new ArgumentException("aKeyName cannot be null nor empty");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      if (aValue == null)
        return DeleteConfigValue(aSection, aKeyName, aResult);

      BasePlug.At(Constants._CONFIG, XUri.EncodeFragment(aSection), XUri.EncodeFragment(aKeyName)).Put(DreamMessage.Ok(MimeType.TEXT, "\"" + aValue + "\""), new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if (a.Status == DreamStatus.Ok)
            aResult.Return();
          else
            aResult.Throw(new CouchException(a));
        },
        aResult.Throw
      );
      return aResult;
    }
    public Result DeleteConfigValue(string aSection, string aKeyName, Result aResult)
    {
      if (String.IsNullOrEmpty(aSection))
        throw new ArgumentException("aSection cannot be null nor empty");
      if (String.IsNullOrEmpty(aKeyName))
        throw new ArgumentException("aKeyName cannot be null nor empty");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      BasePlug.At(Constants._CONFIG, XUri.EncodeFragment(aSection), XUri.EncodeFragment(aKeyName)).Delete(DreamMessage.Ok(), new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if (a.Status == DreamStatus.Ok)
            aResult.Return();// remove " and "\n
          else
            aResult.Throw(new CouchException(a));
        },
        aResult.Throw
      );
      return aResult;
    }
    #endregion

    #region Synchronous Methods
    public Dictionary<string, Dictionary<string, string>> GetConfig()
    {
      return GetConfig(new Result<Dictionary<string, Dictionary<string, string>>>()).Wait();
    }
    public Dictionary<string, string> GetConfig(string aSection)
    {
      return GetConfigSection(aSection, new Result<Dictionary<string, string>>()).Wait();
    }
    public string GetConfigValue(string aSection, string aKeyName)
    {
      return GetConfigValue(aSection, aKeyName, new Result<string>()).Wait();
    }
    public void SetConfigValue(string aSection, string aKeyName, string aValue)
    {
      SetConfigValue(aSection, aKeyName, aValue, new Result()).Wait();
    }
    public void DeleteConfigValue(string aSection, string aKeyName)
    {
      DeleteConfigValue(aSection, aKeyName, new Result()).Wait();
    }
    #endregion
    #endregion
  }
}
