REM Since Palaso libraries change frequently, you will likely need to
REM get that project and be able to build it, then run this script.
REM This script assumes that the libraries project are on the same level as this project.
REM It copies the needed libraries both into the lib folder and the debug folder.

copy /Y ..\palaso\output\debug\palaso.dll lib\debug\
copy /Y ..\palaso\output\debug\palaso.xml lib\debug\
copy /Y ..\palaso\output\debug\palaso.pdb lib\debug\

copy /Y ..\palaso\output\debug\palasouiwindowsforms.dll  lib\debug
copy /Y ..\palaso\output\debug\palasouiwindowsforms.xml  lib\debug
copy /Y ..\palaso\output\debug\palasouiwindowsforms.pdb  lib\debug


copy /Y ..\palaso\output\debug\palaso.testutilities.dll lib\debug
copy /Y ..\palaso\output\debug\palaso.testutilities.xml lib\debug
copy /Y ..\palaso\output\debug\palaso.testutilities.pdb debug

copy /Y ..\palaso\output\debug\palaso.*  output\debug
copy /Y ..\palaso\output\debug\palaso.testutilities.*  output\debug
copy /Y ..\palaso\output\debug\palasouiwindowsforms.*  output\debug

pause