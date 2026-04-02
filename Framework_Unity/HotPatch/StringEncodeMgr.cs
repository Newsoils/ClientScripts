using System.Security.Cryptography;
using System.Text;

public static class StringEncodeMgr
{
    /// <summary>
    /// 计算字节数组的 MD5 值
    /// </summary>
    public static string GetMd5(byte[] data)
    {
        if (data == null || data.Length == 0)
            return string.Empty;

        using (MD5 md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(data);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
                sb.Append(hashBytes[i].ToString("x2"));
            return sb.ToString();
        }
    }

    /// <summary>
    /// 计算字符串的 MD5（如果以后你要对文本算hash）
    /// </summary>
    public static string GetMd5(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        byte[] bytes = Encoding.UTF8.GetBytes(text);
        return GetMd5(bytes);
    }

    /// <summary>
    /// 计算文件的 MD5（备用）
    /// </summary>
    public static string GetFileMd5(string filePath)
    {
        if (!System.IO.File.Exists(filePath))
            return string.Empty;

        using (var stream = System.IO.File.OpenRead(filePath))
        using (var md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(stream);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
                sb.Append(hashBytes[i].ToString("x2"));
            return sb.ToString();
        }
    }
}
