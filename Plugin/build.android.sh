#!/bin/bash

# Build script for Android
# This script builds for multiple Android architectures

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

# Check if Android NDK is available
if [ -z "$ANDROID_NDK_ROOT" ]; then
  if [ -n "$ANDROID_HOME" ] && [ -d "$ANDROID_HOME/ndk-bundle" ]; then
    export ANDROID_NDK_ROOT="$ANDROID_HOME/ndk-bundle"
  elif [ -n "$ANDROID_HOME" ]; then
    # Try to find latest NDK version
    NDK_DIR=$(find "$ANDROID_HOME" -maxdepth 2 -name "ndk" -type d | head -1)
    if [ -n "$NDK_DIR" ]; then
      export ANDROID_NDK_ROOT="$NDK_DIR"
    else
      echo "Error: Android NDK not found. Please set ANDROID_NDK_ROOT environment variable."
      exit 1
    fi
  else
    echo "Error: ANDROID_HOME or ANDROID_NDK_ROOT must be set."
    exit 1
  fi
fi

echo "Using Android NDK: $ANDROID_NDK_ROOT"

# Architecture list
ARCHITECTURES=(
  "arm64-v8a"
  "armeabi-v7a"
  "x86_64"
  "x86"
)

echo "Building KlakHap for Android..."

# Clean previous builds
for arch in "${ARCHITECTURES[@]}"; do
  echo "Cleaning build-Android-$arch..."
  make -f Makefile.android ARCH="$arch" clean
done

# Build for each architecture
for arch in "${ARCHITECTURES[@]}"; do
  echo "Building for $arch..."
  make -f Makefile.android ARCH="$arch"
  
  # Copy to plugin directory structure
  PLUGIN_DIR="../Packages/jp.keijiro.klak.hap/Plugin/Android"
  mkdir -p "$PLUGIN_DIR/$arch"
  cp "build-Android-$arch/libKlakHap.so" "$PLUGIN_DIR/$arch/"
  echo "Copied libKlakHap.so to $PLUGIN_DIR/$arch/"
done

echo "Android build finished successfully!"
echo "Built libraries for architectures: ${ARCHITECTURES[*]}"