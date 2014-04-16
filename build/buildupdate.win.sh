#!/bin/bash
# server=build.palaso.org
# project=Chorus
# build=chorus-win32-master Continuous
# root_dir=..
# $Id: 5c57bb709d99a3f7fdebc821d8c88f682a1cfb0a $

cd "$(dirname "$0")"

# *** Functions ***
force=0
clean=0

while getopts fc opt; do
case $opt in
f) force=1 ;;
c) clean=1 ;;
esac
done

shift $((OPTIND - 1))

copy_auto() {
if [ "$clean" == "1" ]
then
echo cleaning $2
rm -f ""$2""
else
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
fi
}

copy_curl() {
echo "curl: $2 <= $1"
if [ -e "$2" ] && [ "$force" != "1" ]
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
# build: chorus-win32-master Continuous (bt2)
# project: Chorus
# URL: http://build.palaso.org/viewType.html?buildTypeId=bt2
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
#     paths: {"L10NSharp.dll"=>"lib/Release"}
#     VCS: https://bitbucket.org/hatton/l10nsharp []
# [3] build: L10NSharp continuous (bt196)
#     project: L10NSharp
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt196
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"L10NSharp.dll"=>"lib/Debug"}
#     VCS: https://bitbucket.org/hatton/l10nsharp []
# [4] build: palaso-win32-master Continuous (bt223)
#     project: libpalaso
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt223
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"Palaso.dll"=>"lib/Release", "Palaso.TestUtilities.dll"=>"lib/Release", "PalasoUIWindowsForms.dll"=>"lib/Release", "Palaso.Lift.dll"=>"lib/Release", "debug/Palaso.dll"=>"lib/Debug", "debug/Palaso.pdb"=>"lib/Debug", "debug/Palaso.TestUtilities.dll"=>"lib/Debug", "debug/Palaso.TestUtilities.pdb"=>"lib/Debug", "debug/PalasoUIWindowsForms.dll"=>"lib/Debug", "debug/PalasoUIWindowsForms.pdb"=>"lib/Debug", "debug/Palaso.Lift.dll"=>"lib/Debug", "debug/Palaso.Lift.pdb"=>"lib/Debug"}
#     VCS: https://github.com/sillsdev/libpalaso.git []
# [5] build: icucil-win32-default Continuous (bt14)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt14
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"icu*.dll"=>"lib/Release"}
#     VCS: https://github.com/sillsdev/icu-dotnet [master]
# [6] build: icucil-win32-default Continuous (bt14)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt14
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"icu*.dll"=>"lib/Debug"}
#     VCS: https://github.com/sillsdev/icu-dotnet [master]

# make sure output directories exist
mkdir -p ../lib
mkdir -p ../lib/common
mkdir -p ../lib/Release
mkdir -p ../lib/Debug

# download artifact dependencies
copy_auto http://build.palaso.org/guestAuth/repository/download/bt216/latest.lastSuccessful/Chorus_Help.chm ../lib/Chorus_Help.chm
copy_auto http://build.palaso.org/guestAuth/repository/download/bt225/latest.lastSuccessful/Vulcan.Uczniowie.HelpProvider.dll ../lib/common/Vulcan.Uczniowie.HelpProvider.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt196/latest.lastSuccessful/L10NSharp.dll ../lib/Release/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt196/latest.lastSuccessful/L10NSharp.dll ../lib/Debug/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/latest.lastSuccessful/Palaso.dll ../lib/Release/Palaso.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/latest.lastSuccessful/Palaso.TestUtilities.dll ../lib/Release/Palaso.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/latest.lastSuccessful/PalasoUIWindowsForms.dll ../lib/Release/PalasoUIWindowsForms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/latest.lastSuccessful/Palaso.Lift.dll ../lib/Release/Palaso.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/latest.lastSuccessful/debug/Palaso.dll ../lib/Debug/Palaso.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/latest.lastSuccessful/debug/Palaso.pdb ../lib/Debug/Palaso.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/latest.lastSuccessful/debug/Palaso.TestUtilities.dll ../lib/Debug/Palaso.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/latest.lastSuccessful/debug/Palaso.TestUtilities.pdb ../lib/Debug/Palaso.TestUtilities.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/latest.lastSuccessful/debug/PalasoUIWindowsForms.dll ../lib/Debug/PalasoUIWindowsForms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/latest.lastSuccessful/debug/PalasoUIWindowsForms.pdb ../lib/Debug/PalasoUIWindowsForms.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/latest.lastSuccessful/debug/Palaso.Lift.dll ../lib/Debug/Palaso.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt223/latest.lastSuccessful/debug/Palaso.Lift.pdb ../lib/Debug/Palaso.Lift.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icu.net.dll ../lib/Release/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icudt40.dll ../lib/Release/icudt40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuin40.dll ../lib/Release/icuin40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuuc40.dll ../lib/Release/icuuc40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icu.net.dll ../lib/Debug/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icudt40.dll ../lib/Debug/icudt40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuin40.dll ../lib/Debug/icuin40.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt14/latest.lastSuccessful/icuuc40.dll ../lib/Debug/icuuc40.dll
# End of script
