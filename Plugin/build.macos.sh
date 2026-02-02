#!/bin/sh
set -exuo pipefail

RPATH_FLAGS="-install_name @rpath/KlakHap.bundle"
VER_FLAGS="-current_version 1.0.0 -compatibility_version 1.0.0"

make ARCH=arm64  SO_ARGS="$RPATH_FLAGS $VER_FLAGS" -f Makefile.macos
make ARCH=x86_64 SO_ARGS="$RPATH_FLAGS $VER_FLAGS" -f Makefile.macos

lipo -create -output KlakHap.bundle \
  build-macOS-arm64/libKlakHap.so \
  build-macOS-x86_64/libKlakHap.so

cp KlakHap.bundle ../Packages/jp.keijiro.klak.hap/Plugin/MacOS/
