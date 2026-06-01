using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Dictionary[int, string] 转换器，用于临时兼容旧数据格式。
/// </summary>
public class IntStringDictConverter : JsonConverter<Dictionary<int, string>>
{
    public override Dictionary<int, string> ReadJson(JsonReader reader, Type objectType, Dictionary<int, string> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return new Dictionary<int, string>();

        if (reader.TokenType == JsonToken.StartArray)
        {
            try
            {
                var array = JArray.Load(reader);
                var dict = new Dictionary<int, string>();
                foreach (JArray pair in array)
                {
                    if (pair.Count != 2)
                        throw new JsonException("Each element must be [int, string]");
                    dict[pair[0].Value<int>()] = pair[1].Value<string>();
                }
                return dict;
            }
            catch (JsonReaderException)
            {
                return new Dictionary<int, string>();
            }
        }
        return new Dictionary<int, string>();
    }

    public override void WriteJson(JsonWriter w, Dictionary<int, string> d, JsonSerializer s)
    {
        if (d == null || d.Count == 0)
        {
            w.WriteStartArray();
            w.WriteEndArray();
            return;
        }

        w.WriteStartArray();
        foreach (var kv in d)
        {
            w.WriteStartArray();
            w.WriteValue(kv.Key);
            w.WriteValue(kv.Value);
            w.WriteEndArray();
        }
        w.WriteEndArray();
    }
}

/// <summary>
/// Dictionary[int, int] 转换器，用于临时兼容旧数据格式。
/// </summary>
public class IntIntDictConverter : JsonConverter<Dictionary<int, int>>
{
    public override Dictionary<int, int> ReadJson(JsonReader reader, Type objectType, Dictionary<int, int> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return new Dictionary<int, int>();

        if (reader.TokenType == JsonToken.StartArray)
        {
            try
            {
                var array = JArray.Load(reader);
                var dict = new Dictionary<int, int>();
                foreach (JArray pair in array)
                {
                    if (pair.Count != 2)
                        throw new JsonException("Each element must be [int, int]");
                    dict[pair[0].Value<int>()] = pair[1].Value<int>();
                }
                return dict;
            }
            catch (JsonReaderException)
            {
                return new Dictionary<int, int>();
            }
        }
        return new Dictionary<int, int>();
    }

    public override void WriteJson(JsonWriter writer, Dictionary<int, int> dict, JsonSerializer serializer)
    {
        if (dict == null || dict.Count == 0)
        {
            writer.WriteStartArray();
            writer.WriteEndArray();
            return;
        }

        writer.WriteStartArray();
        foreach (var kv in dict)
        {
            writer.WriteStartArray();
            writer.WriteValue(kv.Key);
            writer.WriteValue(kv.Value);
            writer.WriteEndArray();
        }
        writer.WriteEndArray();
    }
}
