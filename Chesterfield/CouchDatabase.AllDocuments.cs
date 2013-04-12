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
      ViewOptions viewOptions, 
      Result<ViewResult<TKey, TValue>> result)
    {
      if (viewOptions == null)
        throw new ArgumentNullException("viewOptions");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(Constants._ALL_DOCS)
        .With(viewOptions)
        .Get(new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Ok || 
              a.Status == DreamStatus.NotModified)
              result.Return(GetViewResult<TKey, TValue>(a));
            else
              result.Throw(new CouchException(a));
          },
          result.Throw
        );
      return result;
    }

    public Result<ViewResult<TKey, TValue, TDocument>> 
      GetAllDocuments<TKey, TValue, TDocument>(
        Result<ViewResult<TKey, TValue, TDocument>> result) 
      where TDocument : ICouchDocument
    {
      return GetAllDocuments(new ViewOptions(), result);
    }

    public Result<ViewResult<TKey, TValue, TDocument>> 
      GetAllDocuments<TKey, TValue, TDocument>(
        ViewOptions viewOptions, 
        Result<ViewResult<TKey, TValue, TDocument>> result) 
      where TDocument : ICouchDocument
    {
      if (viewOptions == null)
        throw new ArgumentNullException("viewOptions");
      if (result == null)
        throw new ArgumentNullException("result");

      BasePlug
        .At(Constants._ALL_DOCS)
        .With(Constants.INCLUDE_DOCS, true)
        .With(viewOptions).Get(new Result<DreamMessage>())
        .WhenDone(
          a =>
          {
            if (a.Status == DreamStatus.Ok || 
              a.Status == DreamStatus.NotModified)
              result.Return(GetViewResult<TKey, TValue, TDocument>(a));
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

    public ViewResult<TKey, TValue, TDocument> 
      GetAllDocuments<TKey, TValue, TDocument>() 
      where TDocument : ICouchDocument
    {
      return GetAllDocuments<TKey, TValue, TDocument>(new ViewOptions());
    }

    public ViewResult<TKey, TValue, TDocument> 
      GetAllDocuments<TKey, TValue, TDocument>(
        ViewOptions viewOptions) 
      where TDocument : ICouchDocument
    {
      return GetAllDocuments(viewOptions, 
        new Result<ViewResult<TKey, TValue, TDocument>>()).Wait();
    }
  }
}