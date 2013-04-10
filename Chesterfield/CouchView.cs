using Newtonsoft.Json;

namespace Chesterfield
{
  public class CouchView
  {
    public CouchView(string aMap = null, string aReduce = null)
    {
      Map = aMap;
      Reduce = aReduce;
    }

    [JsonProperty("map")]
    public string Map { get; set; }

    [JsonProperty("reduce")]
    public string Reduce { get; set; }
  }
}