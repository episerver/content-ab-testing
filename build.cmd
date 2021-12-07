@echo off
setlocal
SET PATH=.\.ci\tools\;%PATH%

IF "%1"=="Debug" (set Configuration=Debug) ELSE (set Configuration=Release)
ECHO Building in %Configuration%

REM Install KPI.commerce dependencies
CALL yarn --cwd src\Episerver.Marketing.KPI.Commerce\clientResources install
IF %errorlevel% NEQ 0 EXIT /B %errorlevel%

IF "%1"=="Release" (CALL yarn --cwd src\Episerver.Marketing.KPI.Commerce\clientResources build) ELSE (CALL yarn --cwd src\Episerver.Marketing.KPI.Commerce\clientResources dev)
IF %errorlevel% NEQ 0 EXIT /B %errorlevel%

powershell -File "build\build.ps1" -configuration %Configuration%