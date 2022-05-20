@ECHO off

:: Enable extensions
setlocal enableextensions enabledelayedexpansion

:CHECK_USAGE
if "%~1" == "" goto INVALID_USAGE 
set VID=%1

set DEVCON="C:\Program Files (x86)\Windows Kits\10\Tools\x64\devcon.exe"
set ERRORCODE=0

:CHECK_DEVCON
if not exist %DEVCON% goto DEVCON_NOT_FOUND

if "%~2" == "-r" goto RESTART 
goto DISABLE

:RESTART
call %DEVCON% restart *%VID%*
set ERRORCODE=%ERRORLEVEL% 
if %ERRORCODE%==0 goto SLEEP_AFTER_RESTART
goto QUIT

:DISABLE
call %DEVCON% disable *%VID%*
set ERRORCODE=%ERRORLEVEL% 
if %ERRORCODE%==0 goto ENABLE_1
goto QUIT

:ENABLE_1
call %DEVCON% enable *%VID%*
set ERRORCODE=%ERRORLEVEL% 
if %ERRORCODE%==0 goto SLEEP_AFTER_ENABLE
goto QUIT

:SLEEP_AFTER_RESTART
REM SLEEP A BIT TO ALLOW WINDOWS SOME TIME TO FINISH THE RESTART.
timeout /t 10 /nobreak
goto QUIT

:SLEEP_AFTER_ENABLE
REM SLEEP A BIT TO ALLOW WINDOWS SOME TIME TO FINALIZE THE FIRST ENABLE BEFORE PROCEDING TO 
REM ENABLE THE REMAINING DRIVERS.  WE NEED TO ENABLE MULTIPLE TIMES TO ENSURE ALL DRIVERS
REM THAT WERE DISABLED ARE RE-ENABLED.
timeout /t 10 /nobreak
goto ENABLE_2

:ENABLE_2
call %DEVCON% enable *%VID%*
set ERRORCODE=%ERRORLEVEL% 
if %ERRORCODE%==0 goto ENABLE_3
goto QUIT

:ENABLE_3
call %DEVCON% enable *%VID%*
set ERRORCODE=%ERRORLEVEL% 
goto QUIT

:INVALID_USAGE
set ERRORCODE=1
call:ColorText 0c "ERROR" 
echo  INVALID USAGE
goto QUIT

:DEVCON_NOT_FOUND
set ERRORCODE=1 
call:ColorText 0c "ERROR" 
echo  %DEVCON% NOT FOUND
goto QUIT

:QUIT
exit /b %ERRORCODE%

:ColorText
::
:: Prints String in color specified by Color.
::
::   Color should be 2 hex digits
::     The 1st digit specifies the background
::     The 2nd digit specifies the foreground
::     See COLOR /? for more help
::
::   String is the text to print. All quotes will be stripped.
::     The string cannot contain any of the following: * ? < > | : \ /
::     Also, any trailing . or <space> will be stripped.
::
::   The string is printed to the screen without issuing a <newline>,
::   so multiple colors can appear on one line. To terminate the line
::   without printing anything, use the ECHO( command.
::
setlocal
pushd %temp%
for /F "tokens=1 delims=#" %%a in ('"prompt #$H#$E# & echo on & for %%b in (1) do rem"') do (
  <nul set/p"=%%a" >"%~2"
)
findstr /v /a:%1 /R "^$" "%~2" nul
del "%~2" > nul 2>&1
popd
endlocal
goto:eof