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
   * > CouchClient.Config.cs
   * > CouchClient.Databases.cs
   * > CouchClient.Users.cs
   */
  public partial class CouchClient : CouchBase
  {
    /// <summary>
    /// Constructs the CouchClient and gets an authentication cookie.
    /// </summary>
    /// <param name="host">The hostname of the CouchDB instance</param>
    /// <param name="port">The port of the CouchDB instance</param>
    /// <param name="username">The username of the CouchDB instance</param>
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

    /* =========================================================================
     * Asynchronous methods 
     * =======================================================================*/

    /// <summary>
    /// <para>Restarts the CouchDB instance. You must be authenticated as a user
    /// with administration privileges for this to work.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result RestartServer(Result result)
    {
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(Constants._RESTART)
        .Post(DreamMessage.Ok(MimeType.JSON, String.Empty), 
              new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Ok)
              result.Return();
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
    /// Restarts the CouchDB instance. You must be authenticated as a user with 
    /// administration privileges for this to work.
    /// </summary>
    public void RestartServer()
    {
      RestartServer(new Result()).Wait();
    }
  }
}
