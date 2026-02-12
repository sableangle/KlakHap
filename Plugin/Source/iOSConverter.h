//
// iOS-specific DXT to RGBA32 converter for KlakHap
// This enables HAP video playback on iOS devices without DXT support
//
#pragma once

#include <stdint.h>
#include <cstddef>

namespace KlakHap
{
    namespace iOS
    {
        // Convert DXT1 compressed data to RGBA32 format
        void ConvertDXT1ToRGBA32(
            const uint8_t* dxtData, 
            uint8_t* rgbaData, 
            int width, int height
        );
        
        // Convert DXT5 compressed data to RGBA32 format  
        void ConvertDXT5ToRGBA32(
            const uint8_t* dxtData,
            uint8_t* rgbaData,
            int width, int height
        );
        
        // Get the RGBA32 buffer size for given dimensions
        size_t GetRGBA32BufferSize(int width, int height);
        
        // Check if iOS format conversion should be used
        bool ShouldUseFormatConversion();
    }
}