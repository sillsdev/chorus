#!/bin/bash
cd "$(dirname "$0")/.."
. environ
cd build
msbuild "/target:${2:-Clean;Compile}" /property:Configuration="${1:-Debug}" Chorus.proj
