@cd /d "%~dp0"

if "%1"=="" goto FAILED
set DISKID=%1

disktool-ali.exe  -d %DISKID% -o dwrite -x 446 -l  64 -f production_partitiontables.bin
IF %errorlevel% NEQ 0 GOTO FAILED
disktool-ali.exe  -d %DISKID% -o boot -f vlt_production.signed.img
IF %errorlevel% NEQ 0 GOTO FAILED
disktool-ali.exe  -t 4 -o make -d %DISKID% -f production_system_partition.bin.part -f production_os_partition.bin.part
IF %errorlevel% NEQ 0 GOTO FAILED

goto FINAL
:FAILED

echo failed.
:FINAL

echo done.