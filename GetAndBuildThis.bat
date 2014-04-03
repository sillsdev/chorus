setlocal

IF "%1"=="" (
	set BUILD_CONFIG="Debug"
) ELSE (
	set BUILD_CONFIG=%1
)

IF "%2"=="" (
	call "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"
	SET HaveCalledvcvarsall=True
)

git pull --rebase
call UpdateDependencies.bat %BUILD_CONFIG% %HaveCalledvcvarsall%
msbuild "Chorus VS2010.sln" /verbosity:quiet /maxcpucount /p:Configuration=%BUILD_CONFIG%
