@echo off
del *.nupkg 2> nul

echo building CMS11
msbuild BVNetwork.404Handler.sln /t:Rebuild /p:Configuration=Release /v:m
rem echo %ErrorLevel%
if "%ErrorLevel%" neq "0" goto error
echo CMS11 built

call ..\BVNetwork.404Handler.Tests\bin\Release\BVNetwork.404Handler.Tests.exe
rem echo %ErrorLevel%
if "%ErrorLevel%" neq "0" goto error


:end        
	echo Build successfully completed
	dir /b *.nupkg
	goto fin

:error      
	del *.nupkg 2>nul
	echo "Build FAILED, see output for details. Build results (if any) have been removed. "
	goto fin

:fin