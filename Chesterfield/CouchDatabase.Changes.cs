using System;
using MindTouch.Dream;
using MindTouch.Tasking;
using Chesterfield.Interfaces;
using Chesterfield.Support;

namespace Chesterfield
{
  public partial class CouchDatabase : CouchBase
  {
    /* =========================================================================
     * Asynchronous methods 
     * =======================================================================*/

    /// <summary>
    /// Request database changes
    /// </summary>
    /// <param name="changeOptions">Change options</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<CouchChanges> GetChanges(
      ChangeOptions changeOptions, 
      Result<CouchChanges> result)
    {
      if (changeOptions == null)
        throw new ArgumentNullException("changeOptions");
      if (result == null)
        throw new ArgumentNullException("result");

      changeOptions.Feed = ChangeFeed.Normal;
      BasePlug
        .At(Constants._CHANGES)
        .With(changeOptions)
        .Get(new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Ok)
            {
              ObjectSerializer<CouchChanges> serializer = 
                new ObjectSerializer<CouchChanges>();
              result.Return(serializer.Deserialize(a.ToText()));
            }
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      return result;
    }

    /// <summary>
    /// Request database changes including documents
    /// </summary>
    /// <typeparam name="T">Type of document used while returning changes</typeparam>
    /// <param name="changeOptions">Change options</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<CouchChanges<T>> 
      GetChanges<T>(
        ChangeOptions changeOptions, 
        Result<CouchChanges<T>> result) 
      where T : ICouchDocument
    {
      if (changeOptions == null)
        throw new ArgumentNullException("changeOptions");
      if (result == null)
        throw new ArgumentNullException("result");

      changeOptions.Feed = ChangeFeed.Normal;
      changeOptions.IncludeDocs = true;
      BasePlug
        .At(Constants._CHANGES)
        .With(changeOptions)
        .Get(new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Ok)
            {
              ObjectSerializer<CouchChanges<T>> serializer = 
                new ObjectSerializer<CouchChanges<T>>();
              result.Return(serializer.Deserialize(a.ToText()));
            }
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      return result;
    }

    /// <summary>
    /// Request continuous changes from database
    /// </summary>
    /// <param name="changeOptions">Change options</param>
    /// <param name="callback">Callback used for each change notification</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<CouchContinuousChanges> GetCoutinuousChanges(
      ChangeOptions changeOptions, 
      CouchChangeDelegate callback,
      Result<CouchContinuousChanges> result)
    {
      if (changeOptions == null)
        throw new ArgumentNullException("changeOptions");
      if (callback == null)
        throw new ArgumentNullException("callback");
      if (result == null)
        throw new ArgumentNullException("result");

      changeOptions.Feed = ChangeFeed.Continuous;
      BasePlug
        .At(Constants._CHANGES)
        .With(changeOptions)
        .InvokeEx(Verb.GET, DreamMessage.Ok(), new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.IsSuccessful)
              result.Return(new CouchContinuousChanges(a, callback));
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      return result;
    }

    /// <summary>
    /// Request continuous changes from database including Documents
    /// </summary>
    /// <typeparam name="T">>Type of document used while returning changes</typeparam>
    /// <param name="changeOptions">Change options</param>
    /// <param name="callback">Callback used for each change notification</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public Result<CouchContinuousChanges<T>> 
      GetCoutinuousChanges<T>(
        ChangeOptions changeOptions, 
        CouchChangeDelegate<T> callback,
        Result<CouchContinuousChanges<T>> result) 
      where T : ICouchDocument
    {
      if (changeOptions == null)
        throw new ArgumentNullException("changeOptions");
      if (callback == null)
        throw new ArgumentNullException("callback");
      if (result == null)
        throw new ArgumentNullException("result");

      changeOptions.Feed = ChangeFeed.Continuous;
      changeOptions.IncludeDocs = true;
      BasePlug
        .At(Constants._CHANGES)
        .With(changeOptions)
        .InvokeEx(Verb.GET, DreamMessage.Ok(), new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.IsSuccessful)
              result.Return(new CouchContinuousChanges<T>(a, callback));
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
    /// Request database changes
    /// </summary>
    /// <param name="options">Change options</param>
    /// <returns></returns>
    public CouchChanges GetChanges(ChangeOptions changeOptions)
    {
      return GetChanges(changeOptions, new Result<CouchChanges>()).Wait();
    }

    /// <summary>
    /// Request database changes including documents
    /// </summary>
    /// <typeparam name="T">Type of document used while returning changes</typeparam>
    /// <param name="changeOptions">Change options</param>
    /// <returns></returns>
    public CouchChanges<T> GetChanges<T>(ChangeOptions changeOptions) 
      where T : ICouchDocument
    {
      return GetChanges(changeOptions, new Result<CouchChanges<T>>()).Wait();
    }

    /// <summary>
    /// Request continuous changes from database
    /// </summary>
    /// <param name="changeOptions">Change options</param>
    /// <param name="callback">Callback used for each change notification</param>
    /// <returns></returns>
    public CouchContinuousChanges GetCoutinuousChanges(
      ChangeOptions changeOptions, 
      CouchChangeDelegate callback)
    {
      return GetCoutinuousChanges(changeOptions, callback, 
        new Result<CouchContinuousChanges>()).Wait();
    }

    /// <summary>
    /// Request continuous changes from database including Documents
    /// </summary>
    /// <typeparam name="T">>Type of document used while returning changes</typeparam>
    /// <param name="changeOptions">Change options</param>
    /// <param name="callback">Callback used for each change notification</param>
    /// <returns></returns>
    public CouchContinuousChanges<T> 
      GetCoutinuousChanges<T>(
        ChangeOptions changeOptions, 
        CouchChangeDelegate<T> callback) 
      where T : ICouchDocument
    {
      return GetCoutinuousChanges(changeOptions, callback, 
        new Result<CouchContinuousChanges<T>>()).Wait();
    }
  }
}