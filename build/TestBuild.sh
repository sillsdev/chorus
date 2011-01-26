#!/bin/bash
xbuild "/target:Clean;Compile" /property:Configuration=DebugMono /property:RootDir=.. /property:BUILD_NUMBER="1.5.1.abcd" build.mono.proj
