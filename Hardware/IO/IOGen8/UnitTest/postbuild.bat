:: This script should be executed as a setup script for the unit tests.

:: This script is responsible for copying files (assemblies, XML files, etc.) needed
:: by the unit tests, that would not otherwise be copied to the bin directory.  These
:: files are typically not copied (deployed) by the unit test framework, because they
:: are not referenced by the unit test project.

:: This script takes a configuration parameter to tell it which build configuration is
:: being built, and thus, which bin directory should be prepared.



@ECHO off

set SOLUTION_DIR=..\..\..

set LOBE_DIR=%SOLUTION_DIR%\..\..\..\..

set BIN_DIR=.

set COPY_CMD=XCOPY /D /I /Y /R

echo Copying the following files to bin\%1
%COPY_CMD% %LOBE_DIR%\Client12\Kernel\ServiceInterfaces\ServiceManager\bin\%1\ServiceManager.addin.xml %BIN_DIR%\
%COPY_CMD% %LOBE_DIR%\Client12\Testing\UnitTest\TestServiceManager\TestServiceManager\bin\%1\TestServiceManager.addin.xml %BIN_DIR%\
%COPY_CMD% %LOBE_DIR%\mono-addins\bin\Release\Mono.Addins.CecilReflector.* %BIN_DIR%\

echo.
