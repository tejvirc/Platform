@echo off
setLocal enableExtensions
set DISKID=%1
set SCRIPTPATH=%~dp0
pushd %~dp0

IF "%DISKID%" == "" goto FAILED
IF "%DISKID%" == "0" goto FAILED

echo working dir: %SCRIPTPATH%
echo target drive: %DISKID%

rem Configuration options
set InstallPackage="%SCRIPTPATH%\platform.zip"

rem defaults
set PowerScript="%SCRIPTPATH%\buildimage.ps1"
set TargetDrive=%DISKID%

@echo on
rem PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& {Start-Process -FilePath PowerShell.exe -ArgumentList '-NoExit  -NoProfile -ExecutionPolicy Bypass -File """%PowerScript%""" -PlatformPackage """%InstallPackage%"""  -TargetDrive %TargetDrive%' -Verb RunAs}"
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& {Start-Process -FilePath PowerShell.exe -ArgumentList '-NoExit -NoProfile -ExecutionPolicy Bypass -File """%PowerScript%""" -PlatformPackage """%InstallPackage%"""  -TargetDrive %TargetDrive%' -Verb RunAs}"


@echo off
goto :EOF

:FAILED
echo No disk or wrong id provided

popd

endlocal