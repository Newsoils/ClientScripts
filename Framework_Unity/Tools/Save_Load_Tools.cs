using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using CLIP.Framework_Core.Serialization;
using UnityEngine;



/// <summary>
/// 存取文件工具类
/// </summary>
public static class Save_Load_Tools
{

    public static void Save<T>(string fileName, T data)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);

        string dir = Path.GetDirectoryName(path);

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string json = Serialization_Provider.SerializeObject(data);

        File.WriteAllText(path, json);

        Debug.Log($"Save Json: {path}");
    }

    /// <summary>
    /// 从持久化目录读取 JSON 并反序列化为 T。
    /// 兼容历史写法：若曾对「已是 JSON 文本的 string」再 Save 一次，
    /// 则文件整段是一个 JSON 字符串字面量，内层才是真正的对象 JSON；会先按 T 直接解析，失败则先解外层 string 再解析 T。
    /// 会去掉 UTF-8 BOM，减少真机/工具写入后的解析问题。
    /// </summary>
    public static T Load<T>(string fileName)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (!File.Exists(path))
        {
            Debug.LogError($"Json file not found: {path}");
            return default;
        }

        try
        {
            string text = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(text))
            {
                Debug.LogError($"Json file empty: {path}");
                return default;
            }

            text = text.TrimStart('\uFEFF').Trim();

            try
            {
                return Serialization_Provider.DeserializeObject<T>(text);
            }
            catch (Exception first)
            {
                try
                {
                    var inner = Serialization_Provider.DeserializeObject<string>(text);
                    if (string.IsNullOrEmpty(inner))
                    {
                        Debug.LogError($"Json unwrap empty ({path}): {first.Message}");
                        return default;
                    }

                    return Serialization_Provider.DeserializeObject<T>(inner);
                }
                catch (Exception second)
                {
                    Debug.LogError($"Json parse error ({path}): {first.Message} | after unwrap: {second.Message}");
                    return default;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Json read error ({path}): {e}");
            return default;
        }
    }

    public static void WriteIOFile(string filePath, string content)
    {
        FileStream writeStream = File.Open(filePath, FileMode.Create, FileAccess.Write);
        if (writeStream != null)
        {
            System.Text.UTF8Encoding encoding_utf8_withOutBOM = new System.Text.UTF8Encoding(false);
            byte[] fileBytes = encoding_utf8_withOutBOM.GetBytes(content);
            writeStream.Write(fileBytes, 0, fileBytes.Length);

            writeStream.Flush();
            writeStream.Close();
            writeStream.Dispose();
        }
    }

    //io方式的文件加载-
    public static byte[] LoadIOFile(string fullPath)
    {
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"读取文件错误: {fullPath}");
            return null;
        }

        return File.ReadAllBytes(fullPath);
    }

    public static async Task<byte[]> LoadIOFileAnsy(string fullPath)
    {
        try
        {
            if (File.Exists(fullPath))
            {
                //打开这个文件-
                FileStream fs = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                byte[] content = new byte[fs.Length];

                //这里注意: Result使用方法会造成当前线程阻塞,等待直到任务完成，所以在主线程里使用是危险的！-
                var res = await fs.ReadAsync(content, 0, (int)fs.Length);

                fs.Flush();
                fs.Close();

                return content;
            }
        }
        catch (System.Exception) { Debug.LogError(string.Format("读取文件{0}错误", fullPath)); }

        return default;
    }


}