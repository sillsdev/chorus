#!/bin/bash
# server=build.palaso.org
# project=Chorus
# build=chorus-win32-develop-continuous
# root_dir=..

# *** Functions ***
copy_auto() {
  where_curl=$(type -P curl)
  where_wget=$(type -P wget)
  if [ "$where_curl" != "" ]
  then
    copy_curl $1 $2
  elif [ "$where_wget" != "" ]
  then
    copy_wget $1 $2
  else
    echo "Missing curl or wget"
    exit 1
  fi
}

copy_curl() {
  echo "curl: $2 <= $1"
  if [ -e "$2" ]
  then
    curl -# -L -z $2 -o $2 $1
  else
    curl -# -L -o $2 $1
  fi
}

copy_wget() {
  echo "wget: $2 <= $1"
  f=$(basename $2)
  d=$(dirname $2)
  cd $d
  wget -q -L -N $1
  cd -
}

# *** Results ***
# build: chorus-win32-develop-continuous (bt331)
# project: Chorus
# URL: http://build.palaso.org/viewType.html?buildTypeId=bt331
# VCS: https://github.com/sillsdev/chorus.git [master]
# dependencies:
# [0] build: Chorus-Documentation (bt216)
#     project: Chorus
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt216
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"*.chm"=>"lib"}
#     VCS: https://github.com/sillsdev/chorushelp.git [master]
# [1] build: Helpprovider (bt225)
#     project: Helpprovider
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt225
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"Vulcan.Uczniowie.HelpProvider.dll"=>"lib/common"}
#     VCS: http://hg.palaso.org/helpprovider []
# [2] build: L10NSharp continuous (bt196)
#     project: L10NSharp
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt196
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"L10NSharp.dll"=>"lib/Release", "L10NSharp.pdb"=>"lib/Release"}
#     VCS: https://bitbucket.org/hatton/l10nsharp []
# [3] build: L10NSharp continuous (bt196)
#     project: L10NSharp
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt196
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"L10NSharp.dll "=>" lib/Debug", "L10NSharp.pdb "=>" lib/Debug"}
#     VCS: https://bitbucket.org/hatton/l10nsharp []
# [4] build: palaso-win32-develop-continuous (bt330)
#     project: libpalaso
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt330
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"debug/Palaso.dll"=>"lib/debug", "debug/Palaso.pdb"=>"lib/debug", "debug/Palaso.TestUtilities.dll"=>"lib/debug", "debug/Palaso.TestUtilities.pdb"=>"lib/debug", "debug/PalasoUIWindowsForms.dll"=>"lib/debug", "debug/PalasoUIWindowsForms.pdb"=>"lib/debug", "debug/Palaso.Lift.dll"=>"lib/debug", "debug/Palaso.Lift.pdb"=>"lib/debug", "release/Palaso.dll"=>"lib/Release", "release/Palaso.pdb"=>"lib/Release", "release/Palaso.TestUtilities.dll"=>"lib/Release", "release/Palaso.TestUtilities.pdb"=>"lib/Release", "release/PalasoUIWindowsForms.dll"=>"lib/Release", "release/PalasoUIWindowsForms.pdb"=>"lib/Release", "release/Palaso.Lift.dll"=>"lib/Release", "release/Palaso.Lift.pdb"=>"lib/Release"}
#     VCS: https://github.com/sillsdev/libpalaso.git []
# [5] build: icucil-win32-default Continuous (bt14)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt14
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"*.dll"=>"lib/Release", "*.config"=>"lib/Release"}
#     VCS: https://github.com/sillsdev/icu-dotnet [master]
# [6] build: icucil-win32-default Continuous (bt14)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt14
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"*.dll"=>"lib\\Debug", "*.config"=>"lib\\Debug"}
#     VCS: https://github.com/sillsdev/icu-dotnet [master]

# make sure output directories exist
mkdir -p ../lib
mkdir -p ../lib/common
mkdir -p ../lib/Release
mkdir -p "../ lib/Debug"
mkdir -p ../lib/debug
mkdir -p ../lib/Debug

# download artifact dependencies
copy_auto http://build.palaso.org/guestAuth/repository/download/bt216/latest.lastSuccessful/Chorus_Help.chm ../lib/Chorus_Help.chm
copy_auto http://build.palaso.org/guestAuth/repository/download/bt225/latest.lastSuccessful/Vulcan.Uczniowie.HelpProvider.dll ../lib/common/Vulcan.Uczniowie.HelpProvider.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt196/latest.lastSuccessful/L10NSharp.dll ../lib/Release/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt196/latest.lastSuccessful/L10NSharp.pdb ../lib/Release/L10NSharp.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt196/latest.lastSuccessful/L10NSharp.dll  "../ lib/Debug/L10NSharp.dll "
copy_auto http://build.palaso.org/guestAuth/repository/download/bt196/latest.lastSuccessful/L10NSharp.pdb  "../ lib/Debug/L10NSharp.pdb "
copy_auto http://build.palaso.org/guestAuth/repository/download/bt330/latest.lastSuccessful/debug/Palaso.dll ../lib/debug/Palaso.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt330/latest.lastSuccessful/debug/Palaso.pdb ../lib/debug/Palaso.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt330/latest.lastSuccessful/debug/Palaso.TestUtilities.dll ../lib/debug/Palaso.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt330/latest.lastSuccessful/debug/Palaso.TestUtilities.pdb ../lib/debug/Palaso.TestUtilities.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt330/latest.lastSuccessful/debug/PalasoUIWindowsForms.dll ../lib/debug/PalasoUIWindowsForms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt330/latest.lastSuccessful/debug/PalasoUIWindowsForms.pdb ../lib/debug/PalasoUIWindowsForms.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt330/latest.lastSuccessful/debug/Palaso.Lift.dll ../lib/debug/Palaso.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt330/latest.lastSuccessful/debug/Palaso.Lift.pdb ../lib/debug/Palaso.Lift.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt330/latest.lastSuccessful/release/Palaso.dll ../lib/Release/Palaso.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt330/latest.lastSuccessful/release/Palaso.pdb ../lib/Release/Palaso.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt330/latest.lastSuccessful/release/Palaso.TestUtilities.dll ../lib/Release/Palaso.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt330/latest.lastSuccessful/release/Palaso.TestUtilities.pdb ../lib/Release/Palaso.TestUtilities.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt330/latest.lastSuccessful/release/PalasoUIWindowsForms.dll ../lib/Release/PalasoUIWindowsForms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt330/latest.lastSuccessful/release/PalasoUIWindowsForms.pdb ../lib/Release/PalasoUIWindowsForms.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt330/latest.lastSuccessful/release/Palaso.Lift.dll ../lib/Release/Palaso.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt330/latest.lastSuccessful/release/Palaso.Lift.pdb ../lib/Release/Palaso.Lift.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icu.net.dll ../lib/Release/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icudt40.dll ../lib/Release/icudt40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuin40.dll ../lib/Release/icuin40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuuc40.dll ../lib/Release/icuuc40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icu.net.dll.config ../lib/Release/icu.net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icu.net.dll ../lib/Debug/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icudt40.dll ../lib/Debug/icudt40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuin40.dll ../lib/Debug/icuin40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuuc40.dll ../lib/Debug/icuuc40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icu.net.dll.config ../lib/Debug/icu.net.dll.config

