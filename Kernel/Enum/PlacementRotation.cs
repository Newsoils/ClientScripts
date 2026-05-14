using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CLIP.Project_Mouse.ENUM
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Placement_Rotation
    {
        Deg0 = 0,
        Deg90 = 1,
        Deg180 = 2,
        Deg270 = 3
    }

}