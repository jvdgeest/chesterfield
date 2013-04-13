using Newtonsoft.Json;

namespace Chesterfield
{
  public class CouchView
  {
    public CouchView(string map = null, string reduce = null)
    {
      Map = map;
      Reduce = reduce;
    }

    /// <summary>
    /// The map function takes a series of key/value pairs, processes each, and 
    /// generates zero or more output key/value pairs. The input and output 
    /// types of the map can be (and often are) different from each other.
    /// </summary>
    [JsonProperty("map")]
    public string Map { get; set; }

    /// <summary>
    /// The reduce function is called once for each unique key. The reduce can
    /// iterate through the values that are associated with that key and produce
    /// zero or more outputs.
    /// </summary>
    [JsonProperty("reduce")]
    public string Reduce { get; set; }
  }
}