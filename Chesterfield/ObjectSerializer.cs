using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Chesterfield
{
  internal class JsonDocumentConverter : JsonConverter
  {
    public override bool CanConvert(Type type)
    {
      if (type == null)
        throw new ArgumentNullException("type");

      return (type == typeof(JDocument)) || 
             (type.IsSubclassOf(typeof(JDocument)));
    }

    public override object ReadJson(
      JsonReader reader, 
      Type type, 
      object existingValue, 
      JsonSerializer serializer)
    {
      if (reader == null)
        throw new ArgumentNullException("reader");
      if (type == null)
        throw new ArgumentNullException("type");

      return type == typeof(JDocument) 
        ? new JDocument(JObject.Load(reader)) 
        : Activator.CreateInstance(type, JObject.Load(reader));
    }

    public override void WriteJson(
      JsonWriter writer, 
      object value, 
      JsonSerializer serializer)
    {
      if (writer == null)
        throw new ArgumentNullException("writer");
      if (value == null)
        throw new ArgumentNullException("value");

      ((JDocument)value).WriteTo(writer);
    }
  }

  internal class JObjectConverter : JsonConverter
  {
    public override bool CanConvert(Type type)
    {
      return type == typeof(JObject);
    }

    public override object ReadJson(
      JsonReader reader, 
      Type type, 
      object existingValue, 
      JsonSerializer serializer)
    {
      if (reader == null)
        throw new ArgumentNullException("reader");
      if (type == null)
        throw new ArgumentNullException("type");
      return JObject.Load(reader);
    }

    public override void WriteJson(
      JsonWriter writer, 
      object value, 
      JsonSerializer serializer)
    {
      if (writer == null)
        throw new ArgumentNullException("writer");
      if (value == null)
        throw new ArgumentNullException("value");
      ((JObject)value).WriteTo(writer);
    }
  }

  internal interface IObjectSerializer<T>
  {
    T Deserialize(string json);
    string Serialize(T obj);
  }

  internal class ObjectSerializer<T> : IObjectSerializer<T>
  {
    private readonly JsonSerializerSettings settings;

    public ObjectSerializer()
    {
      settings = new JsonSerializerSettings
      {
        Converters = new List<JsonConverter> { 
          new IsoDateTimeConverter(), 
          new JsonDocumentConverter(), 
          new JObjectConverter() 
        },
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
      };
    }

    public virtual T Deserialize(string json)
    {
      return JsonConvert.DeserializeObject<T>(json, settings);
    }

    public virtual string Serialize(T obj)
    {
      return JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
    }
  }
}