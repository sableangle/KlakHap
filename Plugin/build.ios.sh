#!/bin/bash

# Build script for iOS
# This script builds both arm64 (device) and x86_64 (simulator) versions
# and creates a universal library

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

echo "Building KlakHap for iOS..."
echo "Building universal library (arm64 + x86_64)..."

# Clean previous builds
make -f Makefile.ios clean

# Build universal library
make -f Makefile.ios

echo "Build complete!"
echo "Universal library created at: build-iOS-universal/libKlakHap.a"

# Copy to plugin directory
echo "Copying to plugin directory..."
make -f Makefile.ios copy

echo "iOS build finished successfully!"