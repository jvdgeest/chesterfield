using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using MindTouch.Tasking;
using MindTouch.Dream;
using Chesterfield.Support;

namespace Chesterfield
{
  public partial class CouchClient : CouchBase
  {
    /* =========================================================================
     * Asynchronous methods 
     * =======================================================================*/

    /// <summary>
    /// <para>Returns a bool indicating whether or not a database exists.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <param name="databaseName">Name of the database to be checked.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<bool> HasDatabase(string databaseName, Result<bool> result)
    {
      if (String.IsNullOrEmpty(databaseName))
        throw new ArgumentException("databaseName cannot be null nor empty");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(XUri.EncodeFragment(databaseName))
        .Head(new Result<DreamMessage>())
        .WhenDone(
          a => result.Return(a.Status == DreamStatus.Ok),
          result.Throw
        );
      return result;
    }

    /// <summary>
    /// <para>Creates a database.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <param name="databaseName">Name of the new database.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<JObject> CreateDatabase(
      string databaseName, 
      Result<JObject> result)
    {
      if (String.IsNullOrEmpty(databaseName))
        throw new ArgumentException("databaseName cannot be null nor empty");
      if (result == null)
        throw new ArgumentNullException("aResult");

      BasePlug
        .At(XUri.EncodeFragment(databaseName))
        .Put(DreamMessage.Ok(), new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Created)
              result.Return(JObject.Parse(a.ToText()));
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      return result;
    }

    /// <summary>
    /// <para>Deletes the specified database.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <param name="databaseName">Name of the database to delete.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<JObject> DeleteDatabase(
      string databaseName, 
      Result<JObject> result)
    {
      if (String.IsNullOrEmpty(databaseName))
        throw new ArgumentException("databaseName cannot be null nor empty");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(XUri.EncodeFragment(databaseName))
        .Delete(new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Ok)
              result.Return(JObject.Parse(a.ToText()));
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      return result;
    }

    /// <summary>
    /// <para>Retrieves a database.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <param name="databaseName">Name of the database.</param>
    /// <param name="createIfNotExists">Flag specifying whether the database 
    /// must be created in case it doesn't exist.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<CouchDatabase> GetDatabase(
      string databaseName, 
      bool createIfNotExists, 
      Result<CouchDatabase> result)
    {
      if (String.IsNullOrEmpty(databaseName))
        throw new ArgumentException("databaseName cannot be null nor empty");
      if (result == null)
        throw new ArgumentNullException("result");

      HasDatabase(databaseName, new Result<bool>()).WhenDone(
        exists =>
        {
          if (exists)
            result.Return(new CouchDatabase(BasePlug.At(
              XUri.EncodeFragment(databaseName))));
          else
          {
            if (createIfNotExists)
            {
              CreateDatabase(databaseName, new Result<JObject>()).WhenDone(
                a => result.Return(new CouchDatabase(BasePlug.At(
                  XUri.EncodeFragment(databaseName)))),
                result.Throw
              );
            }
            else
              result.Return((CouchDatabase)null);
          }
        },
        result.Throw
      );
      return result;
    }

    /// <summary>
    /// <para>Retrieves a database. In case the database doesn't exist, it will
    /// be created for you.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <param name="databaseName">Name of the database.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<CouchDatabase> GetDatabase(
      string databaseName, 
      Result<CouchDatabase> result)
    {
      return GetDatabase(databaseName, true, result);
    }

    /// <summary>
    /// <para>Retrieves a list of available databases on the server.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<IEnumerable<string>> GetAllDatabases(
      Result<IEnumerable<string>> result)
    {
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(Constants._ALL_DBS)
        .Get(new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Ok)
            {
              var d = JArray.Parse(a.ToText());
              result.Return(d.Values<string>());
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
    /// Returns a bool indicating whether or not a database exists.
    /// </summary>
    /// <param name="databaseName">Name of the database to be checked.</param>
    /// <returns></returns>
    public bool HasDatabase(string databaseName)
    {
      return HasDatabase(databaseName, new Result<bool>()).Wait();
    }

    /// <summary>
    /// Creates a database.
    /// </summary>
    /// <param name="databaseName">Name of the new database.</param>
    /// <returns></returns>
    public JObject CreateDatabase(string databaseName)
    {
      return CreateDatabase(databaseName, new Result<JObject>()).Wait();
    }

    /// <summary>
    /// Deletes the specified database.
    /// </summary>
    /// <param name="databaseName">Name of the database to delete.</param>
    /// <returns></returns>
    public JObject DeleteDatabase(string databaseName)
    {
      return DeleteDatabase(databaseName, new Result<JObject>()).Wait();
    }

    /// <summary>
    /// Retrieves a database.
    /// </summary>
    /// <param name="databaseName">Name of the database.</param>
    /// <param name="createIfNotExists">Flag specifying whether the database 
    /// must be created in case it doesn't exist.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public CouchDatabase GetDatabase(
      string databaseName, 
      bool createIfNotExists)
    {
      return GetDatabase(databaseName, createIfNotExists, 
        new Result<CouchDatabase>()).Wait();
    }

    /// <summary>
    /// Retrieves a database. In case the database doesn't exist, it will be 
    /// created for you.</para>
    /// </summary>
    /// <param name="databaseName">Name of the database.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public CouchDatabase GetDatabase(string databaseName)
    {
      return GetDatabase(databaseName, new Result<CouchDatabase>()).Wait();
    }

    /// <summary>
    /// Retrieves a list of available databases on the server.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetAllDatabases()
    {
      return GetAllDatabases(new Result<IEnumerable<string>>()).Wait();
    }
  }
}