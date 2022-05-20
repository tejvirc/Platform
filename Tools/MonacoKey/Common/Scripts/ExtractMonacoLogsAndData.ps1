<#
    .SYNOPSIS
        Extract log and data files, related to the Platform, on to a USB
        thumb drive.

    .DESCRIPTION
        Script to extract log and data files from an Aristocrat EGM, related to
        the Platform, and place those files on to a USB Thumb drive for
        analysis.

        This script will copy the Platform 'Logs' and 'Data' as well as
        the 'Windows Event Logs'.

    .NOTES
        Authors  : James Helm (James.Helm@aristocrat.com)
        Date     : 01-27-2020
        Modified : 10-05-2021
        Version  : 1.4

    Copyright © 2021 Aristocrat Technologies, Inc. All Rights Reserved
#>

[CmdLetBinding ()]
Param
(
    [Parameter (Mandatory=$false)]
    [String] $MonacoPlatformRootFolder = "D:\Aristocrat-VLT",

    [Parameter (Mandatory=$false)]
    [String] $WindowsEventLogSourceFolder = "D:\WindowsEventLogs",

    [Parameter (Mandatory=$false)]
    [String] $USBDriveLetter = "[USBDRIVE]"
)

##########################################################################################
#                                       Constants
##########################################################################################

Set-Variable -Name c_UIEnvVarName                         -Option Constant -Value "YOZZO"
Set-Variable -Name c_EGMMonacoPlatformSourceFolder        -Option Constant -Value "$($MonacoPlatformRootFolder)\Platform"
Set-Variable -Name c_EGMMonacoLogsSourceFolder            -Option Constant -Value "$($c_EGMMonacoPlatformSourceFolder)\logs"
Set-Variable -Name c_EGMMonacoDataSourceFolder            -Option Constant -Value "$($c_EGMMonacoPlatformSourceFolder)\data"
Set-Variable -Name c_USBDestinationRootFolder             -Option Constant -Value "$($USBDriveLetter)\Platform_Info"
Set-Variable -Name c_USBMonacoLogsDestinationFolder       -Option Constant -Value "$($c_USBDestinationRootFolder)\logs"
Set-Variable -Name c_USBMonacoDataDestinationFolder       -Option Constant -Value "$($c_USBDestinationRootFolder)\data"
Set-Variable -Name c_USBWindowsEventLogsDestinationFolder -Option Constant -Value "$($c_USBDestinationRootFolder)\WindowsEventLogs"

##########################################################################################
#                                       Functions
##########################################################################################

Function SendTextToUI()
{
    [CmdLetBinding ()]
    Param
    (
        [Parameter (Mandatory=$true)]
        [String] $Text,

        [Parameter (Mandatory=$false)]
        [Bool] $OutputToConsole = $true
    )

    If ($OutputToConsole)
    {
        Write-Host $Text
    }

    [Environment]::SetEnvironmentVariable($c_UIEnvVarName, $Text, [EnvironmentVariableTarget]::Machine)
}

Function ExtractLogFiles()
{
    $Step = 1
    $NumSteps = 5

    #
    # STEP 1: Ensure the destination folder is cleaned out, if one is present, to
    # prevent mixing files from other EGMs in with the current data.
    #

    If (Test-Path -Path "$($c_USBDestinationRootFolder)" -PathType Container)
    {
        SendTextToUI -Text "Step $($Step;$Step++) of $($NumSteps): Removing existing log and data information from the USB Thumb Drive."
        Remove-Item -Path "$($c_USBDestinationRootFolder)" -Recurse -Force
    }

    #
    # STEP 2: Platform Logs: Ensure the folder exists and extract the files if present.
    #

    If (!(Test-Path -Path "$($c_EGMMonacoLogsSourceFolder)" -PathType Container))
    {
        SendTextToUI -Text "Step $($Step;$Step++) of $($NumSteps): No Platform logs were found."
    }
    else
    {
        $NumLogFiles = Get-ChildItem -Path "$($c_EGMMonacoLogsSourceFolder)" -File -Recurse | Measure-Object | %{$_.Count}
        SendTextToUI -Text "Step $($Step;$Step++) of $($NumSteps): Extracting the Platform logs ($($NumLogFiles) file(s)) to '$($c_USBMonacoLogsDestinationFolder)'."
        robocopy "$($c_EGMMonacoLogsSourceFolder)" "$($c_USBMonacoLogsDestinationFolder)" /mir /r:0 /nfl /ndl /np /njh
    }

    #
    # STEP 3: Platform Data: Ensure the folder exists and extract the files if present.
    #

    If (!(Test-Path -Path "$($c_EGMMonacoDataSourceFolder)" -PathType Container))
    {
        SendTextToUI -Text "Step $($Step;$Step++) of $($NumSteps): No Platform data was found."
    }
    else
    {
        $NumDataFiles = Get-ChildItem -Path "$($c_EGMMonacoDataSourceFolder)" -File -Recurse | Measure-Object | %{$_.Count}
        SendTextToUI -Text "Step $($Step;$Step++) of $($NumSteps): Extracting the Platform data ($($NumDataFiles) file(s)) to '$($c_USBMonacoDataDestinationFolder)'."
        robocopy "$($c_EGMMonacoDataSourceFolder)" "$($c_USBMonacoDataDestinationFolder)" /mir /r:0 /nfl /ndl /np /njh
    }

    #
    # STEP 4: Windows Event Logs: Ensure the folder exists and extract the files if present.
    #

    If (!(Test-Path -Path "$($WindowsEventLogSourceFolder)" -PathType Container))
    {
        SendTextToUI -Text "Step $($Step;$Step++) of $($NumSteps): No Windows event logs were found."
    }
    else
    {
        $NumWinEventLogFiles = Get-ChildItem -Path "$($WindowsEventLogSourceFolder)" -File -Recurse | Measure-Object | %{$_.Count}
        SendTextToUI -Text "Step $($Step;$Step++) of $($NumSteps): Extracting the Windows event logs ($($NumWinEventLogFiles) file(s)) to '$($c_USBWindowsEventLogsDestinationFolder)'."
        robocopy "$($WindowsEventLogSourceFolder)" "$($c_USBWindowsEventLogsDestinationFolder)" /mir /r:0 /nfl /ndl /np /njh
    }

    #
    # STEP 5: Remove the extraneous 'Aristocrat-VLT' folder on the USB key if it is present.
    #

    If (!(Test-Path -Path "$($USBDriveLetter)\Aristocrat-VLT" -PathType Container))
    {
        SendTextToUI -Text "Step $($Step;$Step++) of $($NumSteps): No extraneous Aristocrat-VLT folder was found on the USB Key."
    }
    else
    {
        SendTextToUI -Text "Step $($Step;$Step++) of $($NumSteps): Removing the extraneous Aristocrat-VLT folder from the USB Key."
        Remove-Item -Path "$($USBDriveLetter)\Aristocrat-VLT" -Recurse -Force
    }

    SendTextToUI -Text "Log and data extraction completed."
}

##########################################################################################
#                                     Script Start
##########################################################################################

$ScriptStartTime = Get-Date
Write-Host "`nScript started at '$($ScriptStartTime)'.`n"

Try
{
    ExtractLogFiles
}
Catch
{
    Write-Host  -ForegroundColor Red "`n`nERROR                : USB Thumb Drive script has failed!"
    Write-Host  -ForegroundColor Red "Exception message    : $($_.Exception.Message)"

    If ($_.Exception.ItemName)
    {
        Write-Host  -ForegroundColor Red "Exception item name : $($_.Exception.ItemName)"
    }
}
Finally
{
    #
    # Display the amount of time it took to run the script
    #

    $ScriptEndTime = Get-Date
    $NumMinutes = "{0:N2}" -f ($ScriptEndTime - $ScriptStartTime).TotalMinutes
    Write-Host "`nScript ended at '$($ScriptEndTime)'.  Elapsed time '$($NumMinutes)' minutes."
}