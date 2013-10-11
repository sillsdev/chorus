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
cd ..\palaso
call GetAndBuildThis.bat %BUILD_CONFIG% %2
popd

copy /Y ..\palaso\output\%BUILD_CONFIG%\palaso.dll lib\%BUILD_CONFIG%
copy /Y ..\palaso\output\%BUILD_CONFIG%\palaso.pdb lib\%BUILD_CONFIG%

copy /Y ..\palaso\output\%BUILD_CONFIG%\Palaso.Lift.dll lib\%BUILD_CONFIG%
copy /Y ..\palaso\output\%BUILD_CONFIG%\Palaso.Lift.pdb lib\%BUILD_CONFIG%

copy /Y ..\palaso\output\%BUILD_CONFIG%\palasouiwindowsforms.dll  lib\%BUILD_CONFIG%
copy /Y ..\palaso\output\%BUILD_CONFIG%\palasouiwindowsforms.pdb  lib\%BUILD_CONFIG%


copy /Y ..\palaso\output\%BUILD_CONFIG%\palaso.testutilities.dll lib\%BUILD_CONFIG%
copy /Y ..\palaso\output\%BUILD_CONFIG%\palaso.testutilities.pdb lib\%BUILD_CONFIG%

pause