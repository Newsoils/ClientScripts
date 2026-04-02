using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class PlacementGeometryData
{
    public string modelName;
    public Vector3 size;       // 长宽高
    public string prefabPath;  // 生成的Prefab路径

   
}

[CreateAssetMenu(fileName = "PlacementGeometry_SO",menuName = "Project_Mouse/PlacementGeometry_SO")]
public class PlacementGeometry_SO : ScriptableObject
{
    // 以模型名称作为 Key 方便查询
    public List<PlacementGeometryData> geometryList = new List<PlacementGeometryData>();

    public void Clear()
    {
        geometryList.Clear();
    }

    public void AddOrUpdateGeometryData(PlacementGeometryData data)
    {
        var existingData = geometryList.Find(g => g.modelName == data.modelName);
        if(existingData != null)
        {
            existingData.size = data.size;
            existingData.prefabPath = data.prefabPath;
        }
        else
        {
            geometryList.Add(data);
        }
    }

    public void RemoveGeometryData(PlacementGeometryData data)
    {
        geometryList.Remove(data);
    }

    public string ToJson()
    {
        var settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new Vector3Converter() }
        };

        return JsonConvert.SerializeObject(geometryList, Formatting.Indented, settings);
    }

}