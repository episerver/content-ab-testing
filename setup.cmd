@echo off
setlocal

SET AlloyMVC=samples\EPiServer.Templates.Alloy.Mvc

IF EXIST %AlloyMVC%\App_Data (
    ECHO Remove all files from the app data folder
    DEL %AlloyMVC%\App_Data\*.* /F /Q || Exit /B 1
) ELSE (
    MKDIR %AlloyMVC%\App_Data || Exit /B 1
)

COPY /y build\resources\database\DefaultSiteContent.episerverdata %AlloyMVC%\App_Data\DefaultSiteContent.episerverdata || Exit /B 1
REM copy the database with ACL
XCOPY /y/i/k build\resources\database\Alloy.mdf %AlloyMVC%\App_Data\ || Exit /B 1
XCOPY /y/i/k build\resources\database\AlloyCommerce.mdf %AlloyMVC%\App_Data\ || Exit /B 1
