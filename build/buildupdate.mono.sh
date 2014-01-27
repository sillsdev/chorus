#!/bin/bash
# server=build.palaso.org
# project=Chorus
# build=chorus-precise64-develop-continuous
# root_dir=..
# $Id: e762fae63d184bd9834c06d6cd2e2c4b27de3b57 $

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
# build: chorus-precise64-develop-continuous (bt337)
# project: Chorus
# URL: http://build.palaso.org/viewType.html?buildTypeId=bt337
# VCS: https://github.com/sillsdev/chorus.git [develop]
# dependencies:
# [0] build: Helpprovider (bt225)
#     project: Helpprovider
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt225
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"Vulcan.Uczniowie.HelpProvider.dll"=>"lib/common"}
#     VCS: http://hg.palaso.org/helpprovider []
# [1] build: L10NSharp Mono continuous (bt271)
#     project: L10NSharp
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt271
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"L10NSharp.dll"=>"lib/ReleaseMono", "L10NSharp.dll.mdb"=>"lib/ReleaseMono"}
#     VCS: https://bitbucket.org/hatton/l10nsharp [default]
# [2] build: L10NSharp Mono continuous (bt271)
#     project: L10NSharp
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt271
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"L10NSharp.dll"=>"lib/DebugMono", "L10NSharp.dll.mdb"=>"lib/DebugMono"}
#     VCS: https://bitbucket.org/hatton/l10nsharp [default]
# [3] build: palaso-precise64-develop-continuous (bt334)
#     project: libpalaso
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt334
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"debug/Palaso.dll"=>"lib/DebugMono", "debug/Palaso.dll.mdb"=>"lib/DebugMono", "debug/Palaso.TestUtilities.dll"=>"lib/DebugMono", "debug/Palaso.TestUtilities.dll.mdb"=>"lib/DebugMono", "debug/Palaso.Lift.dll"=>"lib/DebugMono", "debug/Palaso.Lift.dll.mdb"=>"lib/DebugMono", "debug/PalasoUIWindowsForms.dll"=>"lib/DebugMono", "debug/PalasoUIWindowsForms.dll.mdb"=>"lib/DebugMono", "release/Palaso.dll"=>"lib/ReleaseMono", "release/Palaso.dll.mdb"=>"lib/ReleaseMono", "release/Palaso.TestUtilities.dll"=>"lib/ReleaseMono", "release/Palaso.TestUtilities.dll.mdb"=>"lib/ReleaseMono", "release/Palaso.Lift.dll"=>"lib/ReleaseMono", "release/Palaso.Lift.dll.mdb"=>"lib/ReleaseMono", "release/PalasoUIWindowsForms.dll"=>"lib/ReleaseMono", "release/PalasoUIWindowsForms.dll.mdb"=>"lib/ReleaseMono"}
#     VCS: https://github.com/sillsdev/libpalaso.git [develop]
# [4] build: icucil-precise64-Continuous (bt281)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt281
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"*.dll"=>"lib/ReleaseMono", "*.config"=>"lib/ReleaseMono"}
#     VCS: https://github.com/sillsdev/icu-dotnet [master]
# [5] build: icucil-precise64-Continuous (bt281)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt281
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"*.dll"=>"lib/DebugMono", "*.config"=>"lib/DebugMono"}
#     VCS: https://github.com/sillsdev/icu-dotnet [master]

# make sure output directories exist
mkdir -p ../lib/common
mkdir -p ../lib/ReleaseMono
mkdir -p ../lib/DebugMono

# download artifact dependencies
copy_auto http://build.palaso.org/guestAuth/repository/download/bt225/latest.lastSuccessful/Vulcan.Uczniowie.HelpProvider.dll ../lib/common/Vulcan.Uczniowie.HelpProvider.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt271/latest.lastSuccessful/L10NSharp.dll ../lib/ReleaseMono/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt271/latest.lastSuccessful/L10NSharp.dll.mdb ../lib/ReleaseMono/L10NSharp.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt271/latest.lastSuccessful/L10NSharp.dll ../lib/DebugMono/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt271/latest.lastSuccessful/L10NSharp.dll.mdb ../lib/DebugMono/L10NSharp.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt334/latest.lastSuccessful/debug/Palaso.dll ../lib/DebugMono/Palaso.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt334/latest.lastSuccessful/debug/Palaso.dll.mdb ../lib/DebugMono/Palaso.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt334/latest.lastSuccessful/debug/Palaso.TestUtilities.dll ../lib/DebugMono/Palaso.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt334/latest.lastSuccessful/debug/Palaso.TestUtilities.dll.mdb ../lib/DebugMono/Palaso.TestUtilities.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt334/latest.lastSuccessful/debug/Palaso.Lift.dll ../lib/DebugMono/Palaso.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt334/latest.lastSuccessful/debug/Palaso.Lift.dll.mdb ../lib/DebugMono/Palaso.Lift.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt334/latest.lastSuccessful/debug/PalasoUIWindowsForms.dll ../lib/DebugMono/PalasoUIWindowsForms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt334/latest.lastSuccessful/debug/PalasoUIWindowsForms.dll.mdb ../lib/DebugMono/PalasoUIWindowsForms.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt334/latest.lastSuccessful/release/Palaso.dll ../lib/ReleaseMono/Palaso.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt334/latest.lastSuccessful/release/Palaso.dll.mdb ../lib/ReleaseMono/Palaso.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt334/latest.lastSuccessful/release/Palaso.TestUtilities.dll ../lib/ReleaseMono/Palaso.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt334/latest.lastSuccessful/release/Palaso.TestUtilities.dll.mdb ../lib/ReleaseMono/Palaso.TestUtilities.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt334/latest.lastSuccessful/release/Palaso.Lift.dll ../lib/ReleaseMono/Palaso.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt334/latest.lastSuccessful/release/Palaso.Lift.dll.mdb ../lib/ReleaseMono/Palaso.Lift.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt334/latest.lastSuccessful/release/PalasoUIWindowsForms.dll ../lib/ReleaseMono/PalasoUIWindowsForms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt334/latest.lastSuccessful/release/PalasoUIWindowsForms.dll.mdb ../lib/ReleaseMono/PalasoUIWindowsForms.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/latest.lastSuccessful/icu.net.dll ../lib/ReleaseMono/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/latest.lastSuccessful/icu.net.dll.config ../lib/ReleaseMono/icu.net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/latest.lastSuccessful/icu.net.dll ../lib/DebugMono/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/latest.lastSuccessful/icu.net.dll.config ../lib/DebugMono/icu.net.dll.config
# End of script
