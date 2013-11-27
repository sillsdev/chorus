#!/bin/bash

# Since Palaso libraries change frequently, you will likely need to
# get that project and be able to build it, then run this script.
# This script assumes that the libraries project are on the same level as this project.
# It copies the needed libraries both into the lib folder and the ${BUILD_CONFIG}Mono folder.

# pushd Palaso
# call GetAndBuildThis.bat
# popd

if [ "$1" == "" ]
then
	BUILD_CONFIG=Debug
else
	BUILD_CONFIG=$1
fi


cp ../palaso/output/${BUILD_CONFIG}Mono/Palaso.*  lib/${BUILD_CONFIG}Mono
cp ../palaso/output/${BUILD_CONFIG}Mono/PalasoUIWindowsForms.*  lib/${BUILD_CONFIG}Mono

cp ../palaso/output/${BUILD_CONFIG}Mono/Palaso.*  output/${BUILD_CONFIG}Mono
cp ../palaso/output/${BUILD_CONFIG}Mono/PalasoUIWindowsForms.*  output/${BUILD_CONFIG}Mono
