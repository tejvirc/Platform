@cd /d "%~dp0"
@echo off
set EGMIP=%1

@echo Will shut down %EGMIP% if responding
:LOOP
timeout 60
%SystemRoot%\system32\ping.exe -n 1 %EGMIP% >nul
if errorlevel 1 goto NoResponse

echo %EGMIP% is availabe.
echo Rebooting %EGMIP% .
shutdown /r /m \\%EGMIP% /t 0 /c "SBC"
goto LOOP

:NoResponse
echo .

goto :LOOP