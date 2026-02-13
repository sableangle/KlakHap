using UnityEngine;

namespace Klak.Hap
{
    internal static class Utility
    {
        public static void Destroy(Object o)
        {
            if (o == null) return;
            if (Application.isPlaying)
                Object.Destroy(o);
            else
                Object.DestroyImmediate(o);
        }

        public static CodecType DetermineCodecType(int videoType)
        {
            switch (videoType & 0xf)
            {
                case 0xb: return CodecType.Hap;
                case 0xe: return CodecType.HapAlpha;
                case 0xf: return CodecType.HapQ;
            }
            return CodecType.Unsupported;
        }

        // Helper function to extract device generation number
        static int GetDeviceGeneration(string deviceModel, string prefix)
        {
            if (!deviceModel.StartsWith(prefix)) return int.MaxValue;
            
            string numberPart = deviceModel.Substring(prefix.Length);
            int commaIndex = numberPart.IndexOf(',');
            if (commaIndex > 0)
                numberPart = numberPart.Substring(0, commaIndex);
            
            if (int.TryParse(numberPart, out int generation))
                return generation;
                
            return int.MaxValue;
        }

        public static TextureFormat DetermineTextureFormat(int videoType, int width = 0, int height = 0)
        {
            // For mobile platforms, always use RGBA32 when native conversion is available
            #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                Debug.Log($"[KlakHap] {Application.platform}: Using RGBA32 format with native DXT conversion for video type 0x{(videoType & 0xf):x}");
                return TextureFormat.RGBA32;
            #else
            
            TextureFormat preferredFormat;
            
            // Determine preferred format based on video type
            switch (videoType & 0xf)
            {
                case 0xb: preferredFormat = TextureFormat.DXT1; break;
                case 0xe: preferredFormat = TextureFormat.DXT5; break;
                case 0xf: preferredFormat = TextureFormat.DXT5; break;
                case 0xc: preferredFormat = TextureFormat.BC7; break;
                case 0x1: preferredFormat = TextureFormat.BC4; break;
                default: preferredFormat = TextureFormat.DXT1; break;
            }

            // Check if the preferred format is supported on this platform
            if (SystemInfo.SupportsTextureFormat(preferredFormat))
            {
                return preferredFormat;
            }

            // Log a warning about format fallback
            Debug.LogWarning($"KlakHap: Preferred texture format '{preferredFormat}' is not supported on this platform. Searching for fallback format...");

            // Determine fallback format based on platform and alpha channel requirement
            bool needsAlpha = (videoType & 0xf) == 0xe || (videoType & 0xf) == 0xf;
            
            // Helper function to check if dimensions are suitable for PVRTC
            bool IsPVRTCCompatible(int w, int h)
            {
                return w == h && (w & (w - 1)) == 0 && w >= 8; // Square and power of 2, minimum 8x8
            }
            
            TextureFormat selectedFormat = TextureFormat.RGBA32; // Default fallback
            
            // Check if we should use conservative formats for older iOS devices
            bool useConservativeFormats = false;
            
            // iOS/mobile optimized formats
            #if UNITY_IOS || UNITY_ANDROID
            
            // For older iOS devices (A12 and earlier), use more conservative formats to avoid corrupted rendering
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                string deviceModel = SystemInfo.deviceModel;
                int ipadGeneration = GetDeviceGeneration(deviceModel, "iPad");
                int iphoneGeneration = GetDeviceGeneration(deviceModel, "iPhone");
                
                // iPad <= 11 and iPhone <= 12 generally have A12 chip or earlier
                useConservativeFormats = (ipadGeneration <= 11) || (iphoneGeneration <= 12);
            }
            
            if (needsAlpha)
            {
                // For devices that might have compatibility issues, use specific strategies
                if (useConservativeFormats)
                {
                    // Strategy 1: Try ETC2_RGB (not ETC2_RGB4) which might have better compatibility
                    if (SystemInfo.SupportsTextureFormat(TextureFormat.ETC2_RGB))
                    {
                        selectedFormat = TextureFormat.ETC2_RGB;
                        Debug.LogWarning("KlakHap: Using ETC2_RGB for alpha content - alpha channel will be lost. Consider using non-alpha HAP format.");
                    }
                    // Strategy 2: Accept potential visual issues but use supported DXT5 with warning
                    else if (SystemInfo.SupportsTextureFormat(TextureFormat.DXT5))
                    {
                        selectedFormat = TextureFormat.DXT5;
                        Debug.LogWarning("KlakHap: Using DXT5 on legacy iOS device - visual artifacts may occur. This is a known limitation.");
                    }
                    // Strategy 3: Use ETC2_RGBA8 but with explicit data size validation
                    else if (SystemInfo.SupportsTextureFormat(TextureFormat.ETC2_RGBA8))
                    {
                        selectedFormat = TextureFormat.ETC2_RGBA8;
                        Debug.LogWarning("KlakHap: Using ETC2_RGBA8 - may cause data size mismatch errors on some content.");
                    }
                    else
                    {
                        // Last resort: disable HAP on this device
                        selectedFormat = TextureFormat.RGBA32;
                        Debug.LogError("KlakHap: No compatible compressed format found. HAP playback may fail on this device.");
                    }
                }
                else
                {
                    // Standard iOS fallback sequence for newer devices
                    if (SystemInfo.SupportsTextureFormat(TextureFormat.ETC2_RGBA8))
                    {
                        selectedFormat = TextureFormat.ETC2_RGBA8;
                    }
                    else if (SystemInfo.SupportsTextureFormat(TextureFormat.ASTC_4x4))
                    {
                        selectedFormat = TextureFormat.ASTC_4x4;
                    }
                    else if (SystemInfo.SupportsTextureFormat(TextureFormat.RGBA32))
                    {
                        selectedFormat = TextureFormat.RGBA32;
                    }
                }
            }
            else
            {
                // For devices that might have compatibility issues, use specific strategies
                if (useConservativeFormats)
                {
                    // Strategy 1: Try ETC2_RGB which should have better compatibility than ETC_RGB4
                    if (SystemInfo.SupportsTextureFormat(TextureFormat.ETC2_RGB))
                    {
                        selectedFormat = TextureFormat.ETC2_RGB;
                    }
                    // Strategy 2: Accept potential visual issues but use supported DXT1 with warning
                    else if (SystemInfo.SupportsTextureFormat(TextureFormat.DXT1))
                    {
                        selectedFormat = TextureFormat.DXT1;
                        Debug.LogWarning("KlakHap: Using DXT1 on legacy iOS device - visual artifacts may occur. This is a known limitation.");
                    }
                    // Strategy 3: Use ETC_RGB4 but with explicit warning about artifacts
                    else if (SystemInfo.SupportsTextureFormat(TextureFormat.ETC_RGB4))
                    {
                        selectedFormat = TextureFormat.ETC_RGB4;
                        Debug.LogWarning("KlakHap: Using ETC_RGB4 - visual artifacts may occur on legacy devices.");
                    }
                    else
                    {
                        // Last resort: disable HAP on this device
                        selectedFormat = TextureFormat.RGB24;
                        Debug.LogError("KlakHap: No compatible compressed format found. HAP playback may fail on this device.");
                    }
                }
                else
                {
                    // Standard iOS fallback sequence for newer devices
                    if (SystemInfo.SupportsTextureFormat(TextureFormat.ETC_RGB4))
                    {
                        selectedFormat = TextureFormat.ETC_RGB4;
                    }
                    else if (SystemInfo.SupportsTextureFormat(TextureFormat.ETC2_RGB))
                    {
                        selectedFormat = TextureFormat.ETC2_RGB;
                    }
                    else if (SystemInfo.SupportsTextureFormat(TextureFormat.RGB24))
                    {
                        selectedFormat = TextureFormat.RGB24;
                    }
                }
            }
            #else
            // General fallback for non-mobile platforms
            if (needsAlpha)
            {
                if (SystemInfo.SupportsTextureFormat(TextureFormat.RGBA32))
                    selectedFormat = TextureFormat.RGBA32;
                else if (SystemInfo.SupportsTextureFormat(TextureFormat.ARGB32))
                    selectedFormat = TextureFormat.ARGB32;
            }
            else
            {
                if (SystemInfo.SupportsTextureFormat(TextureFormat.RGB24))
                    selectedFormat = TextureFormat.RGB24;
                else if (SystemInfo.SupportsTextureFormat(TextureFormat.RGBA32))
                    selectedFormat = TextureFormat.RGBA32;
            }
            #endif

            // Log the actual format being used
            if (selectedFormat != preferredFormat)
            {
                string compatibilityInfo = "";
                if (useConservativeFormats)
                {
                    string deviceModel = SystemInfo.deviceModel;
                    if (deviceModel.StartsWith("iPad"))
                    {
                        int gen = GetDeviceGeneration(deviceModel, "iPad");
                        compatibilityInfo = $" (using conservative format for iPad generation {gen} ≤ 11)";
                    }
                    else if (deviceModel.StartsWith("iPhone"))
                    {
                        int gen = GetDeviceGeneration(deviceModel, "iPhone");
                        compatibilityInfo = $" (using conservative format for iPhone generation {gen} ≤ 12)";
                    }
                    else
                    {
                        compatibilityInfo = " (using conservative format for device compatibility)";
                    }
                }
                
                Debug.Log($"KlakHap: Using fallback texture format '{selectedFormat}' instead of '{preferredFormat}'{compatibilityInfo}. " +
                         $"Device: {SystemInfo.deviceModel}, Platform: {Application.platform}");
            }

            return selectedFormat;
            #endif
        }

        public static Shader DetermineBlitShader(int videoType)
        {
            if ((videoType & 0xf) == 0xf)
                return Shader.Find("Klak/HAP Q");
            else
                return Shader.Find("Klak/HAP");
        }
    }
}
