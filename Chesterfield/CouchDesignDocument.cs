﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Chesterfield
{
  public class CouchDesignDocument : CouchDocument
  {
    public CouchDesignDocument()
    {
      Language = Constants.JAVASCRIPT;
      Views = new Dictionary<string, CouchView>();
      Shows = new Dictionary<string, string>();
      Lists = new Dictionary<string, string>();
      Updates = new Dictionary<string, string>();
    }

    public CouchDesignDocument(string aDesignDocId)
      : this()
    {
      Id = string.Format("{0}/{1}", Constants.DESIGN, aDesignDocId);
    }

    [JsonProperty(Constants.LANGUAGE)]
    public string Language { get; set; }

    [JsonProperty(Constants.VIEWS)]
    public Dictionary<string, CouchView> Views { get; internal set; }

    [JsonProperty(Constants.SHOWS)]
    public Dictionary<string, string> Shows { get; private set; }

    [JsonProperty(Constants.LISTS)]
    public Dictionary<string, string> Lists { get; private set; }

    [JsonProperty(Constants.UPDATES)]
    public Dictionary<string, string> Updates { get; private set; }

    [JsonProperty(Constants.VALIDATE_DOC_UPDATE)]
    public string ValidateDocUpdate { get; set; }
  }
}