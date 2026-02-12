#pragma once

#include <stdint.h>
#include <mutex>
#include <vector>
#include "ReadBuffer.h"
#include "hap.h"
#include "PlatformConverter.h"

namespace KlakHap
{
    class Decoder
    {
    public:

        #pragma region Constructor/destructor

        Decoder(int width, int height, int typeID)
            : width_(width), height_(height), typeID_(typeID)
        {
            if (Platform::ShouldUseFormatConversion())
            {
                // For mobile platforms, allocate RGBA32 buffer
                buffer_.resize(Platform::GetRGBA32BufferSize(width, height));
                // Allocate temporary DXT buffer for HAP decoding
                dxtBuffer_.resize(width * height * GetBppFromTypeID(typeID) / 8);
            }
            else
            {
                // Standard DXT buffer
                buffer_.resize(width * height * GetBppFromTypeID(typeID) / 8);
            }
        }

        #pragma endregion

        #pragma region Public accessors

        const void* LockBuffer()
        {
            bufferLock_.lock();
            return buffer_.data();
        }

        void UnlockBuffer()
        {
            bufferLock_.unlock();
        }

        size_t GetBufferSize() const
        {
            return buffer_.size();
        }

        #pragma endregion

        #pragma region Decoding operations

        void DecodeFrame(const ReadBuffer& input)
        {
            std::lock_guard<std::mutex> lock(bufferLock_);

            unsigned int format;

            if (Platform::ShouldUseFormatConversion())
            {
                // Decode HAP to DXT format first
                HapDecode(
                    input.storage.data(),
                    static_cast<unsigned long>(input.storage.size()),
                    0, hap_callback, nullptr,
                    dxtBuffer_.data(),
                    static_cast<unsigned long>(dxtBuffer_.size()),
                    nullptr, &format
                );
                
                // Convert DXT to RGBA32 for mobile platforms
                int formatType = typeID_ & 0xf;
                if (formatType == 0xb)  // DXT1
                {
                    Platform::ConvertDXT1ToRGBA32(
                        dxtBuffer_.data(), 
                        buffer_.data(), 
                        width_, height_
                    );
                }
                else if (formatType == 0xe || formatType == 0xf)  // DXT5/YCoCg
                {
                    Platform::ConvertDXT5ToRGBA32(
                        dxtBuffer_.data(), 
                        buffer_.data(), 
                        width_, height_
                    );
                }
            }
            else
            {
                // Standard HAP decoding
                HapDecode(
                    input.storage.data(),
                    static_cast<unsigned long>(input.storage.size()),
                    0, hap_callback, nullptr,
                    buffer_.data(),
                    static_cast<unsigned long>(buffer_.size()),
                    nullptr, &format
                );
            }
        }

        #pragma endregion

    private:

        #pragma region Internal-use members

        std::vector<uint8_t> buffer_;
        std::vector<uint8_t> dxtBuffer_;  // Temporary DXT buffer for iOS conversion
        std::mutex bufferLock_;
        int width_, height_, typeID_;

        static size_t GetBppFromTypeID(int typeID)
        {
            switch (typeID & 0xf)
            {
            case 0xb: return 4; // DXT1
            case 0xe: return 8; // DXT5
            case 0xf: return 8; // DXT5
            case 0xc: return 8; // BC7
            case 0x1: return 4; // BC4
            }
            return 0;
        }

        #pragma endregion

        #pragma region HAP callback implementation

        static void hap_callback(
            HapDecodeWorkFunction work, void* p,
            unsigned int count, void* info
        )
        {
            // FIXME: This should be threaded.
            for (auto i = 0u; i < count; i++) work(p, i);
        }

        #pragma endregion
    };
}
