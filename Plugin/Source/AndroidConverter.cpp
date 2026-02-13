#include "AndroidConverter.h"
#include <algorithm>

namespace KlakHap
{
    namespace Android
    {
        // Simple DXT1 decompressor
        struct Color565
        {
            uint16_t value;
            
            uint8_t GetR() const { return ((value >> 11) & 0x1F) << 3; }
            uint8_t GetG() const { return ((value >> 5) & 0x3F) << 2; }
            uint8_t GetB() const { return (value & 0x1F) << 3; }
        };
        
        void DecompressDXT1Block(const uint8_t* block, uint8_t* output, int stride)
        {
            Color565 color0 = {*reinterpret_cast<const uint16_t*>(block)};
            Color565 color1 = {*reinterpret_cast<const uint16_t*>(block + 2)};
            uint32_t indices = *reinterpret_cast<const uint32_t*>(block + 4);
            
            uint8_t colors[4][4];
            
            // Extract color0
            colors[0][0] = color0.GetR();
            colors[0][1] = color0.GetG();
            colors[0][2] = color0.GetB();
            colors[0][3] = 255;
            
            // Extract color1
            colors[1][0] = color1.GetR();
            colors[1][1] = color1.GetG();
            colors[1][2] = color1.GetB();
            colors[1][3] = 255;
            
            if (color0.value > color1.value)
            {
                // Color 2: 2/3 color0 + 1/3 color1
                colors[2][0] = (2 * colors[0][0] + colors[1][0]) / 3;
                colors[2][1] = (2 * colors[0][1] + colors[1][1]) / 3;
                colors[2][2] = (2 * colors[0][2] + colors[1][2]) / 3;
                colors[2][3] = 255;
                
                // Color 3: 1/3 color0 + 2/3 color1
                colors[3][0] = (colors[0][0] + 2 * colors[1][0]) / 3;
                colors[3][1] = (colors[0][1] + 2 * colors[1][1]) / 3;
                colors[3][2] = (colors[0][2] + 2 * colors[1][2]) / 3;
                colors[3][3] = 255;
            }
            else
            {
                // Color 2: average of color0 and color1
                colors[2][0] = (colors[0][0] + colors[1][0]) / 2;
                colors[2][1] = (colors[0][1] + colors[1][1]) / 2;
                colors[2][2] = (colors[0][2] + colors[1][2]) / 2;
                colors[2][3] = 255;
                
                // Color 3: transparent black
                colors[3][0] = 0;
                colors[3][1] = 0;
                colors[3][2] = 0;
                colors[3][3] = 0;
            }
            
            // Decode pixels
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    int index = (indices >> ((y * 4 + x) * 2)) & 0x3;
                    uint8_t* pixel = output + y * stride + x * 4;
                    pixel[0] = colors[index][0]; // R
                    pixel[1] = colors[index][1]; // G
                    pixel[2] = colors[index][2]; // B
                    pixel[3] = colors[index][3]; // A
                }
            }
        }
        
        void ConvertDXT1ToRGBA32(
            const uint8_t* dxtData, 
            uint8_t* rgbaData, 
            int width, int height)
        {
            int blocksX = (width + 3) / 4;
            int blocksY = (height + 3) / 4;
            
            for (int by = 0; by < blocksY; by++)
            {
                for (int bx = 0; bx < blocksX; bx++)
                {
                    const uint8_t* blockData = dxtData + (by * blocksX + bx) * 8;
                    uint8_t* blockOutput = rgbaData + (by * 4 * width + bx * 4) * 4;
                    
                    DecompressDXT1Block(blockData, blockOutput, width * 4);
                }
            }
        }
        
        void DecompressDXT5AlphaBlock(const uint8_t* block, uint8_t* alphaOutput, int stride)
        {
            uint8_t alpha0 = block[0];
            uint8_t alpha1 = block[1];
            uint64_t alphaIndices = *reinterpret_cast<const uint64_t*>(block) >> 16;
            
            uint8_t alphas[8];
            alphas[0] = alpha0;
            alphas[1] = alpha1;
            
            if (alpha0 > alpha1)
            {
                for (int i = 2; i < 8; i++)
                {
                    alphas[i] = static_cast<uint8_t>((alpha0 * (8 - i) + alpha1 * (i - 1)) / 7);
                }
            }
            else
            {
                for (int i = 2; i < 6; i++)
                {
                    alphas[i] = static_cast<uint8_t>((alpha0 * (6 - i) + alpha1 * (i - 1)) / 5);
                }
                alphas[6] = 0;
                alphas[7] = 255;
            }
            
            // Extract alpha values
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    int index = (alphaIndices >> ((y * 4 + x) * 3)) & 0x7;
                    uint8_t* pixel = alphaOutput + y * stride + x * 4;
                    pixel[3] = alphas[index]; // Set alpha channel
                }
            }
        }
        
        void ConvertDXT5ToRGBA32(
            const uint8_t* dxtData,
            uint8_t* rgbaData,
            int width, int height)
        {
            int blocksX = (width + 3) / 4;
            int blocksY = (height + 3) / 4;
            
            for (int by = 0; by < blocksY; by++)
            {
                for (int bx = 0; bx < blocksX; bx++)
                {
                    const uint8_t* blockData = dxtData + (by * blocksX + bx) * 16;
                    uint8_t* blockOutput = rgbaData + (by * 4 * width + bx * 4) * 4;
                    
                    // Decompress color block first (last 8 bytes) 
                    DecompressDXT1Block(blockData + 8, blockOutput, width * 4);
                    
                    // Then decompress alpha block (first 8 bytes) - this overwrites alpha only
                    DecompressDXT5AlphaBlock(blockData, blockOutput, width * 4);
                }
            }
        }
        
        size_t GetRGBA32BufferSize(int width, int height)
        {
            return static_cast<size_t>(width * height * 4);
        }
        
        bool ShouldUseFormatConversion()
        {
#ifdef __ANDROID__
            return true;  // Always use conversion on Android
#endif
            return false;
        }
    }
}