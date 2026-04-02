using System;

namespace CLIP.Framework_Core.Serialization
{
    public interface ISerializer
    {
        string Serialize(object obj);
        T Deserialize<T>(string json);
        object Deserialize(string json, Type type);
    }
}