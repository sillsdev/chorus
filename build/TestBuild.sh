#!/bin/bash
cd "$(dirname "$0")/.."
build/buildupdate.mono.sh
. environ
root=$PWD
cd build
xbuild "/target:${2:-Clean;Compile}" /property:Configuration="${1:-Debug}Mono" /property:RootDir=$root /property:BUILD_NUMBER="1.5.1.abcd" Chorus.proj
