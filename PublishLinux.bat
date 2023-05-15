@echo off
setlocal enabledelayedexpansion

set CONFIG=Release
set OUTPUT_DIR=bin\%CONFIG%\Platform\bin\linux-x64\publish
set PUBLISH_DIR=_PublishLinux
set ARCHIVE_NAME=PublishLinux.tar

SET DEL_DIR_CMD=rmdir /S /Q

echo.

if exist %OUTPUT_DIR% (
    echo Deleting output dir: %OUTPUT_DIR%
    %DEL_DIR_CMD% %OUTPUT_DIR%
)

if exist %PUBLISH_DIR% (
    echo Deleting publish dir: %PUBLISH_DIR%
     %DEL_DIR_CMD% %PUBLISH_DIR%
)

echo Building...
echo.
dotnet publish .\Linux.sln -c Release --no-self-contained --framework net6.0 --os linux
::dotnet publish .\Linux.sln -c Release --framework net6.0 --os linux

echo.
echo Done building

echo.
echo Moving output to %PUBLISH_DIR%
move /Y %OUTPUT_DIR% %PUBLISH_DIR%

echo.
echo Creating %ARCHIVE_NAME% ...
tar -c -f %ARCHIVE_NAME% %PUBLISH_DIR%

echo.
echo Done
echo.
