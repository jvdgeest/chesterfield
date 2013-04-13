using System;
using MindTouch.Dream;
using Chesterfield.Interfaces;
using Chesterfield.Support;

namespace Chesterfield
{
  public delegate void CouchChangeDelegate(
    object sender, 
    CouchChangeResult result);

  public delegate void CouchChangeDelegate<T>(
      object sender, 
      CouchChangeResult<T> result) 
    where T : ICouchDocument;

  public class CouchContinuousChanges : IDisposable
  {
    private readonly AsyncStreamReader theReader;
    private readonly ObjectSerializer<CouchChangeResult> theSerializer = 
      new ObjectSerializer<CouchChangeResult>();

    internal CouchContinuousChanges(
      DreamMessage message, 
      CouchChangeDelegate callback)
    {
      if (message == null)
        throw new ArgumentNullException("message");
      if (callback == null)
        throw new ArgumentNullException("callback");

      theReader = new AsyncStreamReader(message.ToStream(), (x, y) =>
      {
        if (!String.IsNullOrEmpty(y.Line))
        {
          CouchChangeResult result = theSerializer.Deserialize(y.Line);
          callback(this, result);
        }
      });
    }

    public void Dispose()
    {
      theReader.Dispose();
    }
  }

  public class CouchContinuousChanges<T> : IDisposable where T : ICouchDocument
  {
    private readonly AsyncStreamReader theReader;
    private readonly ObjectSerializer<CouchChangeResult<T>> theSerializer = 
      new ObjectSerializer<CouchChangeResult<T>>();

    internal CouchContinuousChanges(
      DreamMessage message, 
      CouchChangeDelegate<T> callback)
    {
      if (message == null)
        throw new ArgumentNullException("message");
      if (callback == null)
        throw new ArgumentNullException("callback");

      theReader = new AsyncStreamReader(message.ToStream(), (x, y) =>
      {
        if (!String.IsNullOrEmpty(y.Line))
        {
          CouchChangeResult<T> result = theSerializer.Deserialize(y.Line);
          callback(this, result);
        }
      });
    }

    public void Dispose()
    {
      theReader.Dispose();
    }
  }
}