@setlocal enableextensions

@echo off 

for /f "usebackq tokens=*" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -property installationPath`) do (
    set InstallDir=%%i
)

if exist "%InstallDir%\Common7\Tools\vsdevcmd.bat" (
    "%InstallDir%\Common7\Tools\vsdevcmd.bat" 

    @cd /d "%~dp0"

    set Monaco_DisableCodeAnalysis=true
    start /b "Launching..." "%InstallDir%\Common7\IDE\devenv.exe" Monaco.sln
)

@endlocal