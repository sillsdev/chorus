#!/bin/bash

if [ "$1"=="" ]
then
		BUILD_CONFIG=Debug
else
		BUILD_CONFIG=$1
fi


xbuild "/target:Clean;Compile" /property:Configuration=${BUILD_CONFIG}Mono /property:RootDir=.. /property:BUILD_NUMBER="1.5.1.abcd" build.mono.proj
