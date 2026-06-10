using System;
using System.Collections.Generic;
using CLIP.Framework_Unity.Asset;
using CLIP.Project_Mouse.Kernel;
using UnityEngine;
using PM_RM = CLIP.Framework_Unity.Asset.GameAssets;

namespace CLIP.Project_Mouse.Game_Play_System
{
    /// <summary>
    /// Reference-counted on-demand texture cache for locally saved photos.
    /// Call LoadTexture when a UI starts using a photo and ReleaseTexture when it stops.
    /// </summary>
    public sealed class PhotoTextureCache
    {
        private const int MaxCachedTextures = 20;

        private sealed class CacheEntry
        {
            public Texture2D texture;
            public int refCount;
            public float lastAccessTime;
        }

        private readonly Dictionary<string, CacheEntry> _textureCache = new Dictionary<string, CacheEntry>();

        public void LoadTexture(string path, Action<Texture2D> callback)
        {
            if (string.IsNullOrEmpty(path))
            {
                callback?.Invoke(null);
                return;
            }

            if (_textureCache.TryGetValue(path, out var entry))
            {
                entry.refCount++;
                entry.lastAccessTime = Time.time;
                callback?.Invoke(entry.texture);
                return;
            }

            EvictCacheIfNeeded();

            _ = PM_RM.LoadPngAsTextureAsync(path, tex =>
            {
                if (tex != null)
                {
                    tex.name = path;
                    _textureCache[path] = new CacheEntry
                    {
                        texture = tex,
                        refCount = 1,
                        lastAccessTime = Time.time
                    };
                }

                callback?.Invoke(tex);
            });
        }

        public void ReleaseTexture(string path)
        {
            if (!_textureCache.TryGetValue(path, out var entry))
                return;

            entry.refCount--;
            if (entry.refCount <= 0)
            {
                DestroyTex(entry.texture);
                _textureCache.Remove(path);
            }
        }

        public void ReleaseTextureByInfo(PhotoRecordInfo info)
        {
            if (info != null && !string.IsNullOrEmpty(info.localPath))
                ReleaseTexture(info.localPath);
        }

        public void UnloadUnusedTextures()
        {
            var toRemove = new List<string>();
            foreach (var kvp in _textureCache)
            {
                if (kvp.Value.refCount <= 0)
                {
                    DestroyTex(kvp.Value.texture);
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var path in toRemove)
                _textureCache.Remove(path);
        }

        public void UnloadAllTextures()
        {
            foreach (var kvp in _textureCache)
                DestroyTex(kvp.Value.texture);

            _textureCache.Clear();
        }

        public bool IsTextureLoaded(string path)
        {
            return !string.IsNullOrEmpty(path) && _textureCache.ContainsKey(path);
        }

        private void EvictCacheIfNeeded()
        {
            while (_textureCache.Count >= MaxCachedTextures)
            {
                string victim = FindEvictionCandidate();
                if (victim == null)
                    break;

                DestroyTex(_textureCache[victim].texture);
                _textureCache.Remove(victim);
            }
        }

        private string FindEvictionCandidate()
        {
            string best = null;
            float bestScore = float.MaxValue;

            foreach (var kvp in _textureCache)
            {
                if (kvp.Value.refCount <= 0)
                    return kvp.Key;

                float score = kvp.Value.lastAccessTime;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = kvp.Key;
                }
            }

            return best;
        }

        private static void DestroyTex(Texture2D tex)
        {
            if (tex == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(tex);
            else
                UnityEngine.Object.DestroyImmediate(tex);
        }
    }
}
