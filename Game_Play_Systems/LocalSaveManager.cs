using System;
using System.Collections.Generic;
using System.IO;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>
    /// Owns local photo file paths and photo record persistence.
    /// Network synchronization can consume dispatch records from Global_Photo_Manager without knowing disk details.
    /// </summary>
    public static class LocalSaveManager
    {
        private const string PhotoRecordSaveJson = "local_image_list.json";
        private const string DispatchPhotoRecordSaveJson = "dispatch_photo_path_list.json";
        private const string PhotoFolderName = "SavedPhotos";
        private const string DispatchPhotoFolderName = "DispatchPhotos";

        public static void LoadPhotoRecords(out List<PhotoRecordInfo> allPhotos, out List<PhotoRecordInfo> dispatchPhotos)
        {
            allPhotos = Save_Load_Tools.Load<List<PhotoRecordInfo>>(PhotoRecordSaveJson) ?? new List<PhotoRecordInfo>();
            dispatchPhotos = Save_Load_Tools.Load<List<PhotoRecordInfo>>(DispatchPhotoRecordSaveJson);

            if (dispatchPhotos == null)
            {
                dispatchPhotos = new List<PhotoRecordInfo>();
                if (allPhotos != null)
                {
                    foreach (var info in allPhotos)
                    {
                        if (info != null && info.photoType == PhotoType.Dispatch)
                            dispatchPhotos.Add(info);
                    }
                }
            }
        }

        public static void SavePhotoRecords(List<PhotoRecordInfo> allPhotos, List<PhotoRecordInfo> dispatchPhotos)
        {
            Save_Load_Tools.Save(PhotoRecordSaveJson, allPhotos ?? new List<PhotoRecordInfo>());
            Save_Load_Tools.Save(DispatchPhotoRecordSaveJson, dispatchPhotos ?? new List<PhotoRecordInfo>());
        }


        public static string GetPhotoPath(PhotoType type, string fileName, string folderOverride = null)
        {
            return Path.Combine(GetPhotoFolder(type, folderOverride), fileName);
        }

        public static string GetPhotoFolder(PhotoType type, string folderOverride = null)
        {
            if (!string.IsNullOrEmpty(folderOverride))
                return folderOverride;

            string root = Path.Combine(Application.persistentDataPath, PhotoFolderName);
            return type == PhotoType.Dispatch
                ? Path.Combine(root, DispatchPhotoFolderName)
                : root;
        }

        public static void DeletePhotoFile(PhotoRecordInfo info)
        {
            if (info == null || string.IsNullOrEmpty(info.localPath))
                return;

            if (File.Exists(info.localPath))
                File.Delete(info.localPath);
        }

        public static bool TryFindLocalPhotoFile(PhotoRecordInfo info, out string path)
        {
            path = string.Empty;
            if (info == null)
                return false;

            if (!string.IsNullOrEmpty(info.localPath) && File.Exists(info.localPath))
            {
                path = info.localPath;
                return true;
            }

            string folder = GetPhotoFolder(info.photoType);
            if (!Directory.Exists(folder))
                return false;

            string uidPattern = info.photoUId != 0 ? $"*{info.photoUId}*.png" : null;
            if (!string.IsNullOrEmpty(uidPattern) && TryFindNewestFile(folder, uidPattern, out path))
                return true;

            if (!string.IsNullOrEmpty(info.photoName))
            {
                string safeName = SanitizeFileName(info.photoName);
                if (TryFindNewestFile(folder, $"{safeName}*.png", out path))
                    return true;
            }

            return false;
        }

        public static long GenerateLocalPhotoUid()
        {
            long value = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
            if (value == long.MinValue)
                value = long.MaxValue;

            value = Math.Abs(value);
            return value == 0 ? DateTime.UtcNow.Ticks : -value;
        }

        public static string BuildDispatchPhotoFileName(long photoUid, string photoName, int cameraId)
        {
            string safeName = SanitizeFileName(photoName);
            string uidPart = photoUid != 0 ? $"{photoUid}_" : string.Empty;
            return $"{uidPart}{safeName}_{cameraId}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        }

        public static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Photo";

            foreach (char invalid in Path.GetInvalidFileNameChars())
                value = value.Replace(invalid, '_');

            return value.Trim();
        }

        private static bool TryFindNewestFile(string folder, string searchPattern, out string path)
        {
            path = string.Empty;
            var files = Directory.GetFiles(folder, searchPattern, SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
                return false;

            Array.Sort(files, (a, b) => File.GetLastWriteTimeUtc(b).CompareTo(File.GetLastWriteTimeUtc(a)));
            path = files[0];
            return true;
        }
      
    }
}
