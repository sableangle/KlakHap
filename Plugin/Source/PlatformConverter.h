//
// Platform-specific converter wrapper for KlakHap
// Provides a unified interface for DXT to RGBA32 conversion across platforms
//
#pragma once

#include <stdint.h>
#include <cstddef>

#ifdef __APPLE__
    #include "TargetConditionals.h"
    #if TARGET_OS_IPHONE
        #include "iOSConverter.h"
        #define PLATFORM_IOS
    #endif
#elif defined(__ANDROID__)
    #include "AndroidConverter.h"
    #define PLATFORM_ANDROID
#endif

namespace KlakHap
{
    namespace Platform
    {
        // Unified interface for platform-specific conversion
        inline bool ShouldUseFormatConversion()
        {
#ifdef PLATFORM_IOS
            return iOS::ShouldUseFormatConversion();
#elif defined(PLATFORM_ANDROID)
            return Android::ShouldUseFormatConversion();
#else
            return false;
#endif
        }
        
        inline size_t GetRGBA32BufferSize(int width, int height)
        {
#ifdef PLATFORM_IOS
            return iOS::GetRGBA32BufferSize(width, height);
#elif defined(PLATFORM_ANDROID)
            return Android::GetRGBA32BufferSize(width, height);
#else
            return 0;
#endif
        }
        
        inline void ConvertDXT1ToRGBA32(
            const uint8_t* dxtData, 
            uint8_t* rgbaData, 
            int width, int height)
        {
#ifdef PLATFORM_IOS
            iOS::ConvertDXT1ToRGBA32(dxtData, rgbaData, width, height);
#elif defined(PLATFORM_ANDROID)
            Android::ConvertDXT1ToRGBA32(dxtData, rgbaData, width, height);
#endif
        }
        
        inline void ConvertDXT5ToRGBA32(
            const uint8_t* dxtData,
            uint8_t* rgbaData,
            int width, int height)
        {
#ifdef PLATFORM_IOS
            iOS::ConvertDXT5ToRGBA32(dxtData, rgbaData, width, height);
#elif defined(PLATFORM_ANDROID)
            Android::ConvertDXT5ToRGBA32(dxtData, rgbaData, width, height);
#endif
        }
    }
}