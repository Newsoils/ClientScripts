using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace CLIP.Framework_Core.Tools
{
    public class Core_Tools
    {

        /// <summary>
        /// 把源路径的文件或文件夹内容复制到目标路径
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        public static void CopyFolder(string sourcePath, string destPath)
        {
            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(destPath)) return;
            if (!Directory.Exists(sourcePath)) return;

            // 如果目标目录不存在则创建
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            // 复制文件
            string[] files = Directory.GetFiles(sourcePath);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destPath, fileName);
                File.Copy(file, destFile, true); // true表示覆盖同名文件
            }

            // 递归复制子目录
            string[] directories = Directory.GetDirectories(sourcePath);
            foreach (string dir in directories)
            {
                string dirName = Path.GetFileName(dir);
                string destDir = Path.Combine(destPath, dirName);
                CopyFolder(dir, destDir);
            }
        }


        public static byte[] LoadIOFile(string fullPath)
        {
            try
            {
                if (File.Exists(fullPath))
                {
                    //打开这个文件-
                    FileStream fs = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    byte[] content = new byte[fs.Length];
                    fs.Read(content, 0, (int)fs.Length);

                    fs.Flush();
                    fs.Close();

                    return content;
                }
                else
                {
                    throw (new System.Exception());
                }
            }
            catch (System.Exception)
            {
                //Debug.LogError(string.Format("读取文件{0}错误", fullPath));
            }

            return new byte[0];
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
            catch (System.Exception) { 
                //Debug.LogError(string.Format("读取文件{0}错误", fullPath));
            }

            return default;
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
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

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="createCode"></param>
        public static void CodeScript(string pathName, string createCode = "")
        {
            byte[] data = LoadIOFile(pathName);
            System.Text.UTF8Encoding encoding_utf8 = new System.Text.UTF8Encoding(true);
            string readContent = encoding_utf8.GetString(data);

            //string res = StringEncodeMgr.Encrypt(readContent, code, iv);
            //string res = CodelizationSprite.CodeScript(readContent);
            WriteIOFile(pathName, readContent);
        }

  
    }
}
