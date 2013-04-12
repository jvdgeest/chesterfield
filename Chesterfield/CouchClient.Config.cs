using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using MindTouch.Dream;
using MindTouch.Tasking;
using Chesterfield.Support;

namespace Chesterfield
{
  public partial class CouchClient : CouchBase
  {
    /* =========================================================================
     * Asynchronous methods 
     * =======================================================================*/

    /// <summary>
    /// <para>Retrieves the entire CouchDB server configuration.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<Dictionary<string, Dictionary<string, string>>> 
      GetConfig(Result<Dictionary<string, Dictionary<string, string>>> result)
    {
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(Constants._CONFIG)
        .Get(DreamMessage.Ok(), new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Ok)
              result.Return(JsonConvert.DeserializeObject<Dictionary<
                string, Dictionary<string, string>>>(a.ToText()));
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      return result;
    }

    /// <summary>
    /// <para>Retrieves a single section from the CouchDB server configuration.
    /// </para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <param name="section">The configuration section.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<Dictionary<string, string>> GetConfigSection(
      string section, 
      Result<Dictionary<string, string>> result)
    {
      if (String.IsNullOrEmpty(section))
        throw new ArgumentException("section cannot be null nor empty");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(Constants._CONFIG, XUri.EncodeFragment(section))
        .Get(DreamMessage.Ok(), new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            switch (a.Status)
            {
              case DreamStatus.Ok:
                result.Return(JsonConvert.DeserializeObject<
                  Dictionary<string, string>>(a.ToText()));
                break;
              case DreamStatus.NotFound:
                result.Return(new Dictionary<string, string>());
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

    /// <summary>
    /// <para>Retrieves a single configuration value from a given section in the
    /// CouchDB server configuration.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <param name="section">The configuration section.</param>
    /// <param name="keyName">The name of the configuration key.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<string> GetConfigValue(
      string section, 
      string keyName,
      Result<string> result)
    {
      if (String.IsNullOrEmpty(section))
        throw new ArgumentException("section cannot be null nor empty");
      if (String.IsNullOrEmpty(keyName))
        throw new ArgumentException("keyName cannot be null nor empty");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(Constants._CONFIG, XUri.EncodeFragment(section), 
            XUri.EncodeFragment(keyName))
        .Get(DreamMessage.Ok(), new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            string value = a.ToText();
            switch (a.Status)
            {
              case DreamStatus.Ok:
                // remove " and "\n
                result.Return(value.Substring(1, value.Length - 3));
                break;
              case DreamStatus.NotFound:
                result.Return((string)null);
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

    /// <summary>
    /// <para>Sets a single configuration value for a given section in the
    /// CouchDB server configuration.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <param name="section">The configuration section.</param>
    /// <param name="keyName">The name of the configuration key.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result SetConfigValue(
      string section, 
      string keyName, 
      string value, 
      Result result)
    {
      if (String.IsNullOrEmpty(section))
        throw new ArgumentException("section cannot be null nor empty");
      if (String.IsNullOrEmpty(keyName))
        throw new ArgumentException("keyName cannot be null nor empty");
      if (result == null)
        throw new ArgumentNullException("result");

      if (value == null)
        return DeleteConfigValue(section, keyName, result);

      BasePlug
        .At(Constants._CONFIG, XUri.EncodeFragment(section), 
            XUri.EncodeFragment(keyName))
        .Put(DreamMessage.Ok(MimeType.TEXT, "\"" + value + "\""), 
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

    /// <summary>
    /// <para>Deletes a single configuration value from a given section in the
    /// CouchDB server configuration.</para>
    /// <para>(This method is asynchronous.)</para>
    /// </summary>
    /// <param name="section">The configuration section.</param>
    /// <param name="keyName">The name of the configuration key.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result DeleteConfigValue(
      string section, 
      string keyName, 
      Result result)
    {
      if (String.IsNullOrEmpty(section))
        throw new ArgumentException("section cannot be null nor empty");
      if (String.IsNullOrEmpty(keyName))
        throw new ArgumentException("keyName cannot be null nor empty");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(Constants._CONFIG, XUri.EncodeFragment(section), 
            XUri.EncodeFragment(keyName))
        .Delete(DreamMessage.Ok(), new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Ok)
              result.Return(); // remove " and "\n
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
    /// Retrieves the entire CouchDB server configuration.
    /// </summary>
    /// <returns>A dictionary containing the configuration sections. Each 
    /// section has its keys and values stored in a dictionary.</key>
    /// </returns>
    public Dictionary<string, Dictionary<string, string>> GetConfig()
    {
      return GetConfig(new Result<Dictionary<string, 
        Dictionary<string, string>>>()).Wait();
    }

    /// <summary>
    /// Retrieves a single section from the CouchDB server configuration.
    /// </summary>
    /// <param name="section">The configuration section.</param>
    /// <returns>A dictionary containing the keys and values.</returns>
    public Dictionary<string, string> GetConfig(string section)
    {
      return GetConfigSection(section, 
        new Result<Dictionary<string, string>>()).Wait();
    }

    /// <summary>
    /// Retrieves a single configuration value from a given section in the
    /// CouchDB server configuration.
    /// </summary>
    /// <param name="section">The configuration section.</param>
    /// <param name="keyName">The name of the configuration key.</param>
    /// <returns></returns>
    public string GetConfigValue(string section, string keyName)
    {
      return GetConfigValue(section, keyName, new Result<string>()).Wait();
    }

    /// <summary>
    /// Sets a single configuration value for a given section in the CouchDB 
    /// server configuration.
    /// </summary>
    /// <param name="section">The configuration section.</param>
    /// <param name="keyName">The name of the configuration key.</param>
    /// <param name="value">The value to set.</param>
    public void SetConfigValue(string section, string keyName, string value)
    {
      SetConfigValue(section, keyName, value, new Result()).Wait();
    }

    /// <summary>
    /// Deletes a single configuration value from a given section in the CouchDB 
    /// server configuration.
    /// </summary>
    /// <param name="section">The configuration section.</param>
    /// <param name="keyName">The name of the configuration key.</param>
    public void DeleteConfigValue(string section, string keyName)
    {
      DeleteConfigValue(section, keyName, new Result()).Wait();
    }
  }
}
