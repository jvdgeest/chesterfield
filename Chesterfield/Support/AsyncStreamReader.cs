using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Chesterfield.Support
{
  public class LineReceivedEventArgs : EventArgs
  {
    public string Line { get; private set; }
    public LineReceivedEventArgs(string aLine)
    {
      Line = aLine.Trim();
    }
  }

  public class AsyncStreamReader : IDisposable
  {
    private readonly byte[] theReadBuffer = new byte[1024];
    private readonly List<byte> theTempLineBytes = new List<byte>();
    private readonly EventHandler<LineReceivedEventArgs> theLineReaded;
    private readonly Stream theBaseStream;
    private readonly Encoding theEncoding;

    private bool isDisposed;
    private int theTempListIndex;

    public AsyncStreamReader(Stream aStream, EventHandler<LineReceivedEventArgs> aLineReceivedCallback) :
      this(aStream, Encoding.UTF8, aLineReceivedCallback)
    {
    }

    public AsyncStreamReader(Stream aStream, Encoding anEncoding, EventHandler<LineReceivedEventArgs> aLineReceivedCallback)
    {
      if (aStream == null)
        throw new ArgumentNullException("aStream");
      if (anEncoding == null)
        throw new ArgumentNullException("anEncoding");
      if (!aStream.CanRead)
        throw new ArgumentException("Stream does not support reading");

      theBaseStream = aStream;
      theEncoding = anEncoding;
      theLineReaded = aLineReceivedCallback;
      theBaseStream.BeginRead(theReadBuffer, 0, theReadBuffer.Length, AsyncCallback, null);
    }

    private void AsyncCallback(IAsyncResult ar)
    {
      try
      {
        int size = theBaseStream.EndRead(ar);
        if (size > 0)
        {
          for (int i = 0; i < size; i++)
          {
            theTempLineBytes.Add(theReadBuffer[i]);
          }

          ReadLinesAndCallback();

          if (!isDisposed)
          {
            theBaseStream.BeginRead(theReadBuffer, 0, theReadBuffer.Length, AsyncCallback, null);
          }
        }
      }
      catch
      {
        //TODO: ??
      }
    }

    private void ReadLinesAndCallback()
    {
      string line = ReadLine();
      while (line != null)
      {
        try
        {
          theLineReaded(this, new LineReceivedEventArgs(line));
        }
        catch
        {
          // TODO: Add Logging
        }
        line = ReadLine();
      }
    }

    private string ReadLine()
    {
      string line = null;
      int endLineIndex = 0;

      for (; theTempListIndex < theTempLineBytes.Count && endLineIndex == 0; theTempListIndex++)
      {
        if (theTempLineBytes[theTempListIndex] == '\n')
        {
          endLineIndex = theTempListIndex;
        }
      }

      if (endLineIndex > 0)
      {
        line = theEncoding.GetString(theTempLineBytes.ToArray(), 0, endLineIndex);
        theTempLineBytes.RemoveRange(0, theTempListIndex);
        theTempListIndex = 0;
      }

      return line;
    }

    public void Dispose()
    {
      isDisposed = true;
      theBaseStream.Dispose();
    }
  }
}