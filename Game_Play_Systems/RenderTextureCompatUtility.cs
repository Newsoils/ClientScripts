using UnityEngine;

namespace CLIP.Project_Mouse.Game_Play_System
{
    public static class RenderTextureCompatUtility
    {
        public static bool ShouldUseConservativeSettings()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return false;
            }

            string vendor = SystemInfo.graphicsDeviceVendor ?? string.Empty;
            string deviceName = SystemInfo.graphicsDeviceName ?? string.Empty;
            string deviceModel = SystemInfo.deviceModel ?? string.Empty;

            bool isMaliGpu =
                vendor.ToLowerInvariant().Contains("arm") ||
                deviceName.ToLowerInvariant().Contains("mali");

            bool isHuaweiLikeDevice =
                deviceModel.ToLowerInvariant().Contains("huawei") ||
                deviceModel.ToLowerInvariant().Contains("honor");

            return isMaliGpu || isHuaweiLikeDevice;
        }

        public static RenderTexture EnsureCompatible(RenderTexture source, string debugName = null)
        {
            if (source == null)
            {
                return null;
            }

            if (!ShouldUseConservativeSettings())
            {
                EnsureCreated(source);
                return source;
            }

            RenderTextureFormat colorFormat = ChooseCompatibleColorFormat(source.format);
            int depth = source.depth > 0 ? source.depth : 24;
            RenderTextureReadWrite readWrite = source.sRGB ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear;

            RenderTexture rt = new RenderTexture(source.width, source.height, depth, colorFormat, readWrite)
            {
                name = string.IsNullOrEmpty(debugName) ? source.name + "_Compat" : debugName,
                antiAliasing = 1,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = source.filterMode,
                wrapMode = source.wrapMode,
                anisoLevel = 0
            };

            EnsureCreated(rt);
            return rt;
        }

        public static RenderTexture CreateCompatible(int width, int height, int depth, bool useSrgb, string debugName)
        {
            RenderTextureFormat colorFormat = ChooseCompatibleColorFormat(RenderTextureFormat.ARGB32);
            RenderTextureReadWrite readWrite = useSrgb ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear;

            RenderTexture rt = new RenderTexture(width, height, depth, colorFormat, readWrite)
            {
                name = debugName,
                antiAliasing = 1,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                anisoLevel = 0
            };

            EnsureCreated(rt);
            return rt;
        }

        private static RenderTextureFormat ChooseCompatibleColorFormat(RenderTextureFormat preferred)
        {
            if (ShouldUseConservativeSettings())
            {
                if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32))
                {
                    return RenderTextureFormat.ARGB32;
                }

                return RenderTextureFormat.Default;
            }

            if (SystemInfo.SupportsRenderTextureFormat(preferred))
            {
                return preferred;
            }

            if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32))
            {
                return RenderTextureFormat.ARGB32;
            }

            return RenderTextureFormat.Default;
        }

        private static void EnsureCreated(RenderTexture rt)
        {
            if (rt != null && !rt.IsCreated())
            {
                rt.Create();
            }
        }
    }
}
