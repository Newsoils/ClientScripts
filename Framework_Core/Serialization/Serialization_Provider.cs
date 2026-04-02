using System;
using Newtonsoft.Json;

namespace CLIP.Framework_Core.Serialization
{
    /// <summary>
    /// 通用序列化工具类。
    /// 默认使用 Newtonsoft.Json，可根据需要扩展为 MessagePack / Protobuf 等。
    /// </summary>
    public static class Serialization_Provider
    {
        private static ISerializer _serializer = new Newtonsoft_Serializer();

        public static void SetSerializer(ISerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeObject(object obj)
        {
            return _serializer.Serialize(obj);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T DeserializeObject<T>(string json)
        {
            return _serializer.Deserialize<T>(json);
        }

        public static object DeserializeObject(string json, Type type)
        {
            return _serializer.Deserialize(json, type);
        }

        /// <summary>
        /// 切回 Newtonsoft.Json
        /// </summary>
        public static void InitNewtonsoftJson()
        {
            _serializer = new Newtonsoft_Serializer();
        }


        // 未来如果有新的序列化方案可以继续添加，例如：
        // public static void InitMessagePack() => _serializer = new MessagePackSerializerProvider();
    }
}
