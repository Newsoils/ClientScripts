using System;
using System.Collections.Generic;

[Serializable]
public class ResourceIndexData
{
    public List<ResourceItem> Items = new();
}

[Serializable]
public class ResourceItem
{
    /// <summary>
    /// 唯一资源 Key（逻辑名）
    /// </summary>
    public string key;          
    public string path;         // Resources / AB 相对路径
    public string subName;
    public string type;         
    public string group;        // 可选：Character / UI / Scene
    public string bundleName;
}
