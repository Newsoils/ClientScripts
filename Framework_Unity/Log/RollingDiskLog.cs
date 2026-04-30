using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace CLIP.Framework_Unity
{
    /// <summary>
    /// 单文件追加日志；超过 <see cref="MaxFileSizeBytes"/> 时丢弃最旧内容（从某行边界起保留尾部），再写入新行。
    /// 默认仅在真机写入（Editor 下 <see cref="Enable"/> 为 false），测试可在启动后设 <c>RollingDiskLog.Enable = true</c>。
    /// </summary>
    public static class RollingDiskLog
    {
        public static long MaxFileSizeBytes { get; set; } = 10L * 1024 * 1024;

        /// <summary>完整日志文件路径（<see cref="EnsureInitialized"/> 之后有效）。</summary>
        public static string FilePath { get; private set; }

        /// <summary>Editor 默认 false；Standalone/手机 true。</summary>
        public static bool Enable { get; set; } = !Application.isEditor;

        private static readonly object LockObj = new object();
        private static StreamWriter _writer;
        private static bool _inited;

        public static void EnsureInitialized()
        {
            if (!Enable || _inited) return;
            _inited = true;
            string logDir = Path.Combine(Application.persistentDataPath, "Logs");
            Directory.CreateDirectory(logDir);
            FilePath = Path.Combine(logDir, "rolling_trace.log");
            OpenWriterAppend();
        }

        public static void Shutdown()
        {
            lock (LockObj)
            {
                _writer?.Flush();
                _writer?.Dispose();
                _writer = null;
                _inited = false;
            }
        }

        /// <summary>写一行（可含换行）；自动加时间戳前缀。</summary>
        public static void AppendLine(string text)
        {
            if (!Enable) return;
            EnsureInitialized();
            if (_writer == null) return;

            string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {text}";
            byte[] utf8 = Encoding.UTF8.GetBytes(line);
            int incoming = utf8.Length + 1;

            lock (LockObj)
            {
                _writer.Flush();
                var fs = (FileStream)_writer.BaseStream;
                if (fs.Length + incoming > MaxFileSizeBytes)
                {
                    long keep = MaxFileSizeBytes - incoming - 4096;
                    if (keep < 65536) keep = 65536;
                    _writer.Dispose();
                    _writer = null;
                    TrimOldestContent(FilePath, keep);
                    OpenWriterAppend();
                }

                _writer.WriteLine(line);
                _writer.Flush();
            }
        }

        private static void OpenWriterAppend()
        {
            var stream = new FileStream(FilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            _writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
            {
                AutoFlush = false
            };
        }

        /// <summary>从文件开头删到「保留不超过 maxKeepBytes 的尾部」，尽量从换行处切开。</summary>
        private static void TrimOldestContent(string path, long maxKeepBytes)
        {
            if (!File.Exists(path)) return;

            long len = new FileInfo(path).Length;
            if (len <= maxKeepBytes) return;

            long cut = len - maxKeepBytes;
            long startCopyFrom = cut;
            const int scanCap = 65536;
            long scanEnd = Math.Min(cut + scanCap, len);

            using (var readFs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                readFs.Seek(cut, SeekOrigin.Begin);
                while (readFs.Position < scanEnd)
                {
                    int b = readFs.ReadByte();
                    if (b < 0) break;
                    if (b == (byte)'\n')
                    {
                        startCopyFrom = readFs.Position;
                        break;
                    }
                }
            }

            string tempPath = path + ".rolltmp";
            using (var readFs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var outFs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                readFs.Seek(startCopyFrom, SeekOrigin.Begin);
                readFs.CopyTo(outFs);
            }

            File.Delete(path);
            File.Move(tempPath, path);
        }
    }
}
