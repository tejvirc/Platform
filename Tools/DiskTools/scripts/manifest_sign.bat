@cd /d "%~dp0"
@echo off
if "%1"=="" goto FAILED
set DriveLetter=%1
copy whitelist %DriveLetter%
manifest-ali.exe -k vlt_partition.prv -m hmac -d -i %DriveLetter%\whitelist %DriveLetter%
copy manifest %DriveLetter%
goto :EOF

:FAILED
echo missing argument
