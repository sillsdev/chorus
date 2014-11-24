REM Since Palaso libraries change frequently, you will likely need to
REM get that project and be able to build it, then run this script.
REM This script assumes that the libraries project are on the same level as this project.
REM It copies the needed libraries both into the lib folder and the debug/release folder.

IF "%1"=="" (
	set BUILD_CONFIG="Debug"
) ELSE (
	set BUILD_CONFIG=%1
)

pushd .
cd ..\libpalaso
call GetAndBuildThis.bat %BUILD_CONFIG% %2
popd

mkdir output\%BUILD_CONFIG%

REM Uncomment these lines if you are working on L10NSharp
REM copy /Y ..\l10nsharp\output\%BUILD_CONFIG%\L10NSharp.dll lib\common\
REM copy /Y ..\l10nsharp\output\%BUILD_CONFIG%\L10NSharp.* lib\%BUILD_CONFIG%\
REM copy /Y ..\l10nsharp\output\%BUILD_CONFIG%\L10NSharp.* output\%BUILD_CONFIG%\

copy /Y ..\libpalaso\output\%BUILD_CONFIG%\palaso.dll lib\%BUILD_CONFIG%
copy /Y ..\libpalaso\output\%BUILD_CONFIG%\palaso.pdb output\%BUILD_CONFIG%

copy /Y ..\libpalaso\output\%BUILD_CONFIG%\Palaso.Lift.dll lib\%BUILD_CONFIG%
copy /Y ..\libpalaso\output\%BUILD_CONFIG%\Palaso.Lift.pdb output\%BUILD_CONFIG%

copy /Y ..\libpalaso\output\%BUILD_CONFIG%\palasouiwindowsforms.dll  lib\%BUILD_CONFIG%
copy /Y ..\libpalaso\output\%BUILD_CONFIG%\palasouiwindowsforms.pdb  output\%BUILD_CONFIG%

copy /Y ..\libpalaso\output\%BUILD_CONFIG%\palaso.testutilities.dll lib\%BUILD_CONFIG%
copy /Y ..\libpalaso\output\%BUILD_CONFIG%\palaso.testutilities.pdb output\%BUILD_CONFIG%
