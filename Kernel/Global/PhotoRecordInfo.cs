using System;
using System.IO;

namespace CLIP.Project_Mouse.Kernel
{
    [System.Serializable]
    public class PhotoRecordInfo
    {
        public long photoUId;
        public int photoConfigId;
        public int mapConfigId;
        public string photoName;
        public DateTime captureTime;
        public PhotoType photoType;//normal,present 
        public string photoConfigName = "";
        public string localPath = "";

        public string FileName => string.IsNullOrEmpty(localPath) ? string.Empty : Path.GetFileName(localPath);
    }

    public enum PhotoType
    {
        Default,
        Room,
        Dispatch,
        Firend
    }
}
