using System;
using System.Web;
using MindTouch.Dream;
using MindTouch.Tasking;
using Chesterfield.Support;

namespace Chesterfield
{
  public partial class CouchClient : CouchBase
  {
    /// <summary>
    /// Creates an administrator.
    /// </summary>
    /// <param name="username">The username of the administrator.</param>
    /// <param name="password">A plain text password.</param>
    public void CreateAdminUser(string username, string password)
    {
      if (String.IsNullOrEmpty(username))
        throw new ArgumentException("username cannot be null nor empty");
      if (String.IsNullOrEmpty(password))
        throw new ArgumentException("password cannot be null nor empty");

      SetConfigValue("admins", username, password, new Result()).Wait();
      BasePlug.WithCredentials(username, password); 
      CouchUser user = new CouchUser { Name = username };

      ObjectSerializer<CouchUser> serializer = 
        new ObjectSerializer<CouchUser>();
      BasePlug
        .At(Constants._USERS, 
            HttpUtility.UrlEncode("org.couchdb.user:" + username))
        .Put(DreamMessage.Ok(MimeType.JSON, serializer.Serialize(user)), 
             new Result<DreamMessage>())
        .Wait();
    }

    /// <summary>
    /// Deletes an administrator.
    /// </summary>
    /// <param name="username">The username of the administrator.</param>
    public void DeleteAdminUser(string username)
    {
      if (String.IsNullOrEmpty(username))
        throw new ArgumentException("username cannot be null nor empty");

      DeleteConfigValue("admins", username, new Result()).Wait();

      var userDb = GetDatabase(Constants._USERS);
      var userId = "org.couchdb.user:" + username;
      var userDoc = userDb.GetDocument(userId, new Result<JDocument>()).Wait();
      if (userDoc != null)
        userDb.DeleteDocument(userDoc.Id, userDoc.Rev, 
          new Result<string>()).Wait();
    }

    /// <summary>
    /// Retrieves a user.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <returns></returns>
    public JDocument GetUser(string username)
    {
      if (String.IsNullOrEmpty(username))
        throw new ArgumentException("username cannot be null nor empty");

      var db = new CouchDatabase(BasePlug.At(Constants._USERS));
      return db.GetDocument(UserId(username), new Result<JDocument>()).Wait();
    }

    /// <summary>
    /// Determines if a user exists.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <returns>Boolean indicating whether the user exists.</returns>
    public bool HasUser(string username)
    {
      if (String.IsNullOrEmpty(username))
        throw new ArgumentException("username cannot be null nor empty");

      return GetUser(username) != null;
    }

    /// <summary>
    /// Generates a CouchDB document ID for a given username.
    /// </summary>
    /// <param name="username">The Username.</param>
    /// <returns>CouchDB document ID.</returns>
    private string UserId(string username)
    {
      return "org.couchdb.user:" + HttpUtility.UrlEncode(username);
    }
  }
}