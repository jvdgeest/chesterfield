using System;
using MindTouch.Tasking;
using MindTouch.Dream;
using Chesterfield.Interfaces;
using Chesterfield.Support;

namespace Chesterfield
{
  public partial class CouchDatabase : CouchBase
  {
    /* =========================================================================
     * Asynchronous methods 
     * =======================================================================*/

    public Result<ViewResult<TKey, TValue>> GetAllDocuments<TKey, TValue>(
      Result<ViewResult<TKey, TValue>> aResult)
    {
      return GetAllDocuments(new ViewOptions(), aResult);
    }

    public Result<ViewResult<TKey, TValue>> GetAllDocuments<TKey, TValue>(
      ViewOptions aViewOptions, Result<ViewResult<TKey, TValue>> aResult)
    {
      if (aViewOptions == null)
        throw new ArgumentNullException("aViewOptions");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      BasePlug.At(Constants.ALL_DOCS).With(aViewOptions).Get(
        new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if (a.Status == DreamStatus.Ok || a.Status == DreamStatus.NotModified)
          {
            aResult.Return(GetViewResult<TKey, TValue>(a));
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

    public Result<ViewResult<TKey, TValue, TDocument>> GetAllDocuments<TKey, 
      TValue, TDocument>(Result<ViewResult<TKey, TValue, TDocument>> aResult) 
      where TDocument : ICouchDocument
    {
      return GetAllDocuments(new ViewOptions(), aResult);
    }

    public Result<ViewResult<TKey, TValue, TDocument>> GetAllDocuments<TKey, 
      TValue, TDocument>(ViewOptions aViewOptions, Result<ViewResult<TKey, 
      TValue, TDocument>> aResult) 
      where TDocument : ICouchDocument
    {
      if (aViewOptions == null)
        throw new ArgumentNullException("aViewOptions");
      if (aResult == null)
        throw new ArgumentNullException("aResult");

      BasePlug.At(Constants.ALL_DOCS).With(Constants.INCLUDE_DOCS, true).
        With(aViewOptions).Get(new Result<DreamMessage>()).WhenDone(
        a =>
        {
          if (a.Status == DreamStatus.Ok || a.Status == DreamStatus.NotModified)
          {
            aResult.Return(GetViewResult<TKey, TValue, TDocument>(a));
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

    /* =========================================================================
     * Synchronous methods 
     * =======================================================================*/

    public ViewResult<TKey, TValue> GetAllDocuments<TKey, TValue>()
    {
      return GetAllDocuments<TKey, TValue>(new ViewOptions());
    }

    public ViewResult<TKey, TValue> GetAllDocuments<TKey, TValue>(
      ViewOptions aViewOptions)
    {
      return GetAllDocuments(aViewOptions, 
        new Result<ViewResult<TKey, TValue>>()).Wait();
    }

    public ViewResult<TKey, TValue, TDocument> GetAllDocuments<TKey, TValue, 
      TDocument>() where TDocument : ICouchDocument
    {
      return GetAllDocuments<TKey, TValue, TDocument>(new ViewOptions());
    }

    public ViewResult<TKey, TValue, TDocument> GetAllDocuments<TKey, TValue, 
      TDocument>(ViewOptions aViewOptions) where TDocument : ICouchDocument
    {
      return GetAllDocuments(aViewOptions, 
        new Result<ViewResult<TKey, TValue, TDocument>>()).Wait();
    }
  }
}
