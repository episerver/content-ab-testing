@echo off
set /a counter=1
:ARGLBEG
if "%1"=="" goto ARGLEND
set arg%counter%=%1
goto ARGNEXT 

:ARGNEXT
shift
set /a counter=%counter%+1
goto ARGLBEG

:ARGLEND

powershell .\build\pack.ps1 -maVersion %arg1% -configuration %arg12%
EXIT /B %errorlevel%