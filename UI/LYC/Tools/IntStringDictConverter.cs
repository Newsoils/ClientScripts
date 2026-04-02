using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Dictionary<int, string>
public class IntStringDictConverter : JsonConverter<Dictionary<int, string>>
{
    public override Dictionary<int, string> ReadJson(JsonReader reader, Type objectType, Dictionary<int, string> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        // 处理空值
        if (reader.TokenType == JsonToken.Null)
        {
            return new Dictionary<int, string>();
        }

        // 处理空数组
        if (reader.TokenType == JsonToken.StartArray)
        {
            try
            {
                var array = JArray.Load(reader);
                if (array.Count == 0)
                {
                    return new Dictionary<int, string>();
                }

                var dict = new Dictionary<int, string>();
                foreach (JArray pair in array)
                {
                    if (pair.Count != 2)
                        throw new JsonException("Each option must be [int, string]");
                    dict[pair[0].Value<int>()] = pair[1].Value<string>();
                }
                return dict;
            }
            catch (JsonReaderException)
            {
                // 如果解析失败，返回空字典
                return new Dictionary<int, string>();
            }
        }

        // 如果既不是null也不是数组，返回空字典
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

// Dictionary<string, int>
public class StringIntDictConverter : JsonConverter<Dictionary<string, int>>
{
    public override Dictionary<string, int> ReadJson(JsonReader reader, Type objectType, Dictionary<string, int> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        // 处理空值
        if (reader.TokenType == JsonToken.Null)
        {
            return new Dictionary<string, int>();
        }

        // 处理空数组
        if (reader.TokenType == JsonToken.StartArray)
        {
            try
            {
                var array = JArray.Load(reader);
                if (array.Count == 0)
                {
                    return new Dictionary<string, int>();
                }

                var dict = new Dictionary<string, int>();
                foreach (JArray pair in array)
                {
                    if (pair.Count != 2)
                        throw new JsonException("Each option must be [string, int]");
                    dict[pair[0].Value<string>()] = pair[1].Value<int>();
                }
                return dict;
            }
            catch (JsonReaderException)
            {
                // 如果解析失败，返回空字典
                return new Dictionary<string, int>();
            }
        }

        // 如果既不是null也不是数组，返回空字典
        return new Dictionary<string, int>();
    }

    public override void WriteJson(JsonWriter w, Dictionary<string, int> d, JsonSerializer s)
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

// Dictionary<int, int>
public class IntIntDictConverter : JsonConverter<Dictionary<int, int>>
{
    public override Dictionary<int, int> ReadJson(
        JsonReader reader,
        Type objectType,
        Dictionary<int, int> existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        // 处理空值
        if (reader.TokenType == JsonToken.Null)
        {
            return new Dictionary<int, int>();
        }

        // 处理空数组
        if (reader.TokenType == JsonToken.StartArray)
        {
            try
            {
                var array = JArray.Load(reader);
                if (array.Count == 0)
                {
                    return new Dictionary<int, int>();
                }

                var dict = new Dictionary<int, int>();
                foreach (JArray pair in array)
                {
                    if (pair.Count != 2)
                        throw new JsonException("每个元素必须是 [int, int] 数组");
                    dict[pair[0].Value<int>()] = pair[1].Value<int>();
                }
                return dict;
            }
            catch (JsonReaderException)
            {
                // 如果解析失败，返回空字典
                return new Dictionary<int, int>();
            }
        }

        // 如果既不是null也不是数组，返回空字典
        return new Dictionary<int, int>();
    }

    public override void WriteJson(
        JsonWriter writer,
        Dictionary<int, int> dict,
        JsonSerializer serializer)
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