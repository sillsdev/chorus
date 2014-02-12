#!/bin/bash
cd "$(dirname "$0")"

xbuild "/target:Clean;Compile" /property:Configuration="${1:-Debug}Mono" /property:RootDir=.. /property:BUILD_NUMBER="1.5.1.abcd" build.mono.proj
