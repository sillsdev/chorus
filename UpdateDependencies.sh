#!/bin/bash

# Since Palaso libraries change frequently, you will likely need to
# get that project and be able to build it, then run this script.
# This script assumes that the libraries project are on the same level as this project.
# It copies the needed libraries both into the lib folder and the ${BUILD_CONFIG}Mono folder.

PALASO_DIR=../libpalaso

if [ ! -d ${PALASO_DIR} ]
then
	exit
fi

if [ "$1"=="" ]
then
	BUILD_CONFIG=Debug
else
	BUILD_CONFIG=$1
fi

pushd ${PALASO_DIR}
build/TestBuild.sh ${BUILD_CONFIG}Mono
popd

if [ ! -d output/${BUILD_CONFIG}Mono ]
then
	if [ ! -d output ]
	then
		mkdir output
	fi
	mkdir output/${BUILD_CONFIG}Mono
fi


cp ${PALASO_DIR}/output/${BUILD_CONFIG}Mono/Palaso*.*  lib/${BUILD_CONFIG}Mono
cp ${PALASO_DIR}/output/${BUILD_CONFIG}Mono/SIL.*.*  lib/${BUILD_CONFIG}Mono

cp ${PALASO_DIR}/output/${BUILD_CONFIG}Mono/Palaso*.*  output/${BUILD_CONFIG}Mono
cp ${PALASO_DIR}/output/${BUILD_CONFIG}Mono/SIL.*.*  output/${BUILD_CONFIG}Mono
