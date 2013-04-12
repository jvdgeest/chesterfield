using Newtonsoft.Json;

namespace Chesterfield
{
  public class CouchAttachment
  {
    /// <summary>
    /// When set to true, this entry will only contain the metadata.
    /// </summary>
    [JsonProperty(Constants.STUB)]
    public bool Stub { get; internal set; }

    /// <summary>
    /// Content type of this attachment (for instance "text/plain").
    /// </summary>
    [JsonProperty(Constants.CONTENT_TYPE)]
    public string ContentType { get; internal set; }

    /// <summary>
    /// Length of the attachment data in bytes.
    /// </summary>
    [JsonProperty(Constants.LENGTH)]
    public long Length { get; internal set; }

    /// <summary>
    /// Base64 data string that must be on a single line of characters.
    /// </summary>
    [JsonProperty(Constants.DATA)]
    public string Data { get; internal set; }

    /// <summary>
    /// MD5 hash of the attachment data. Please note that CouchDB compresses 
    /// some attachments (determined by the content type). "text/plain" is one  
    /// of them (the full list is in your server's default.ini file), and so the 
    /// MD5 returned is that of the compressed form. If you supply a digest when
    /// uploading a standalone attachment, CouchDB will verify it matches and 
    /// return an error if it doesn't.
    /// </summary>
    [JsonProperty(Constants.DIGEST)]
    public string Digest { get; internal set; }
  }
}