#!/bin/bash

# Since Palaso libraries change frequently, you will likely need to
# get that project and be able to build it, then run this script.
# This script assumes that the libraries project are on the same level as this project.
# It copies the needed libraries both into the lib folder and the DebugMono folder.

# pushd Palaso
# call GetAndBuildThis.bat
# popd

cp ../palaso/output/Debug/Palaso*.XML  lib/DebugMono
cp ../palaso/output/DebugMono/Palaso.*  lib/DebugMono
cp ../palaso/output/DebugMono/PalasoUIWindowsForms.*  lib/DebugMono

cp ../palaso/output/Debug/Palaso*.XML  output/DebugMono
cp ../palaso/output/DebugMono/Palaso.*  output/DebugMono
cp ../palaso/output/DebugMono/PalasoUIWindowsForms.*  output/DebugMono
