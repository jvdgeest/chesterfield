using System;
using System.Configuration;
using System.Data.SqlClient;
using MindTouch.Dream;
using MindTouch.Tasking;
using Newtonsoft.Json.Linq;

namespace Chesterfield.Support
{
  public abstract class CouchBase
  {
    protected Plug BasePlug;

    protected CouchBase(Plug plug)
    {
      BasePlug = plug;
    }

    protected CouchBase(XUri baseUri, string username = null, 
      string password = null)
    {
      if (baseUri == null)
        throw new ArgumentNullException("baseUri");

      BasePlug = Plug.New(baseUri).WithCredentials(username, password);
    }

    protected CouchBase(string connectionStringName)
    {
      ConnectionStringSettings connectionString = 
        ConfigurationManager.ConnectionStrings[connectionStringName];
      if (connectionString == null)
        throw new ArgumentException("Invalid connection string name");

      CouchDbConnectionStringBuilder cs = new CouchDbConnectionStringBuilder(
        connectionString.ConnectionString);

      BasePlug = Plug.New(String.Format("{0}://{1}:{2}", 
        cs.SslEnabled ? "https" : "http", cs.Host, cs.Port)).WithCredentials(
        cs.Username, cs.Password);
    }

    /// <summary>
    /// Perform Cookie base authentication with given username and password
    /// Resulting cookie will be automatically used for all subsequent requests
    /// </summary>
    /// <param name="username">User Name</param>
    /// <param name="password">Password</param>
    /// <param name="result"></param>
    /// <returns>true if authentication succeed</returns>
    public Result<bool> Logon(string username, string password, 
      Result<bool> result)
    {
      if (String.IsNullOrEmpty(username))
        throw new ArgumentException("username cannot be null nor empty");
      if (String.IsNullOrEmpty(password))
        throw new ArgumentException("password cannot be null nor empty");
      if (result == null)
        throw new ArgumentNullException("result");

      string content = String.Format("name={0}&password={1}", 
        username, password);

      BasePlug
        .At("_session")
        .Post(DreamMessage.Ok(MimeType.FORM_URLENCODED, content),
          new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            switch (a.Status)
            {
              case DreamStatus.Ok:
                BasePlug.CookieJar.Update(a.Cookies, 
                  new XUri(BasePlug.Uri.SchemeHostPort));
                BasePlug = BasePlug.WithHeader("X-CouchDB-WWW-Authenticate", 
                  "Cookie");
                result.Return(true);
                break;
              default:
                result.Throw(new CouchException(a));
                break;
            }
          },
          result.Throw
        );
      return result;
    }

    public Result<bool> Logoff(Result<bool> result)
    {
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At("_session")
        .Delete(new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Ok)
              result.Return(true);
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      return result;
    }

    public Result<bool> IsLogged(Result<bool> result)
    {
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At("_session")
        .Get(new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Ok)
            {
              JObject user = JObject.Parse(a.ToText());
              result.Return(user["info"]["authenticated"] != null);
            }
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      return result;
    }
  }
}