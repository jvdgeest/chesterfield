using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chesterfield.Interfaces
{
  /// <summary>
  /// Response from an update handler.
  /// </summary>
  public interface IUpdateResponse
  {
    /// <summary>
    /// If the update handler contains the CouchDB document as first member of
    /// the return array, then this field will contain the revision number from
    /// the document after the change or create has been applied. Otherwise,
    /// the value will be null.
    /// </summary>
    string Rev { get; set;  }

    /// <summary>
    /// The raw HTTP response of the update handler. 
    /// </summary>
    string HttpResponse { get; set; }
  }
}
