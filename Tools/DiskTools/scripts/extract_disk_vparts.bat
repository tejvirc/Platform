@cd /d "%~dp0"
@echo off
if "%1"=="" goto FAILED
set DriveLetter=%1

disktool-ali.exe  -d %DriveLetter% -o dread -x 446 -l  64 -f production_partitiontables.bin
disktool-ali.exe  -d %DriveLetter% -o extract -p 1 -f production_system_partition.bin -z System -k vlt_partition.prv
disktool-ali.exe  -d %DriveLetter% -o extract -p 2 -f production_os_partition.bin -z OS -k vlt_partition.prv

goto :EOF

:FAILED
echo missing argument
