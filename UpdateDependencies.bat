REM Since Palaso libraries change frequently, you will likely need to
REM get that project and be able to build it, then run this script.
REM This script assumes that the libraries project are on the same level as this project.
REM It copies the needed libraries both into the lib folder and the debug/release folder.

set PALASO_DIR="..\libpalaso"

IF NOT EXIST %PALASO_DIR% GOTO :EOF

IF "%1"=="" (
	set BUILD_CONFIG="Debug"
) ELSE (
	set BUILD_CONFIG=%1
)

REM pushd %PALASO_DIR%
REM REM Presence of a second argument indicates that the caller has already run vsvars32.bat
REM call GetAndBuildThis.bat %BUILD_CONFIG% %2
REM popd

mkdir output\%BUILD_CONFIG%

REM Uncomment these lines if you are working on L10NSharp
REM copy /Y ..\l10nsharp\output\%BUILD_CONFIG%\L10NSharp.dll lib\common\
REM copy /Y ..\l10nsharp\output\%BUILD_CONFIG%\L10NSharp.* lib\%BUILD_CONFIG%\
REM copy /Y ..\l10nsharp\output\%BUILD_CONFIG%\L10NSharp.* output\%BUILD_CONFIG%\

copy /Y %PALASO_DIR%\output\%BUILD_CONFIG%\Palaso*.dll lib\%BUILD_CONFIG%\
copy /Y %PALASO_DIR%\output\%BUILD_CONFIG%\Palaso*.dll output\%BUILD_CONFIG%\
copy /Y %PALASO_DIR%\output\%BUILD_CONFIG%\Palaso*.pdb output\%BUILD_CONFIG%\

copy /Y %PALASO_DIR%\output\%BUILD_CONFIG%\SIL.*.dll lib\%BUILD_CONFIG%\
copy /Y %PALASO_DIR%\output\%BUILD_CONFIG%\SIL.*.dll output\%BUILD_CONFIG%\
copy /Y %PALASO_DIR%\output\%BUILD_CONFIG%\SIL.*.pdb output\%BUILD_CONFIG%\
