using System.Collections;

//加载请求信息-
public struct RMAssetRequire
{
    public int                      ItorCount;              //请求加载次数-
    public int                      loadState;              //0:未加载，1:已加载完成 -1:未加载到对象-
    public string                   BundleName;             //所属的包名-
    public string                   Name;                   //资源名-
    public string                   AssetPath;              //资源全路径-
    public UnityEngine.Object       Module;                 //载入的镜像对象-
}
