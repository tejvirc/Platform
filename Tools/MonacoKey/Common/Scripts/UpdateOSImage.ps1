<#
    .SYNOPSIS
        Updates the OS Image on the EGM to the OS Image present on the USB thumb
        drive.

    .DESCRIPTION
        Script to update the OS Image component, of the Platform, to the
        image present on the USB thumb drive.  The update process is safe in that
        powering down the EGM, or interrupting the command during execution should
        not result in an EGM that will not boot.

        The script assumes the 4 files needed to deploy an OS Image are present at
        the root of the USB thumb drive.
        
        If the file 'disktool-ali.exe' is present at the root of the USB thumb
        drive the script will use it instead of the one present on the EGM to
        update the OS Image.

    .NOTES
        Authors  : James Helm (James.Helm@aristocrat.com)
        Date     : 01-28-2020
        Modified : 10-19-2021
        Version  : 1.2

    Copyright © 2021 Aristocrat Technologies, Inc. All Rights Reserved
#>

[CmdLetBinding ()]
Param
(
    [Parameter (Mandatory=$false)]
    [String] $MonacoPlatformRootFolder = "D:\Aristocrat-VLT",

    [Parameter (Mandatory=$false)]
    [String] $USBDriveLetter = "[USBDRIVE]",

    [Parameter (Mandatory=$false)]
    [String] $DestinationDriveNum = 0
)

##########################################################################################
#                                       Constants
##########################################################################################

Set-Variable -Name c_UIEnvVarName               -Option Constant -Value "YOZZO"
Set-Variable -Name c_USBOSImageFileSourceFolder -Option Constant -Value "$($USBDriveLetter)\"
Set-Variable -Name c_USBDiskToolExecutable      -Option Constant -Value "$($USBDriveLetter)\disktool-ali.exe"
Set-Variable -Name c_EGMOSUpdateCommand         -Option Constant -Value "C:\Aristocrat-VLT-Tools\Upgrade_OS.cmd"

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

Function UpdateOSImage()
{

    $Step = 1
    $NumSteps = 2

    #
    # STEP 1: Check if the 'disktool-ali.exe' exists on the USB thumb drive and use this OS update process,
    # else update the OS image using the Platform process.
    #

    If (Test-Path -Path "$($c_USBDiskToolExecutable)" -PathType Leaf)
    {
        SendTextToUI -Text "Step $($Step;$Step++) of $($NumSteps): Updating the OS Image using the Disktool-ali.exe executable on the USB thumb drive."
        Set-Location -Path $c_USBOSImageFileSourceFolder

        & $c_USBDiskToolExecutable -t 4 -o make -d $DestinationDriveNum -f
    }
    else
    {
        SendTextToUI -Text "Step $($Step;$Step++) of $($NumSteps): Updating the OS Image using the Platform process."
        & $c_EGMOSUpdateCommand $USBDriveLetter
    }

    #
    # STEP 2: Remove the extraneous 'Aristocrat-VLT' folder on the USB key if it is present.
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

    SendTextToUI -Text "The OS Image update has completed."
}

##########################################################################################
#                                     Script Start
##########################################################################################

$ScriptStartTime = Get-Date
Write-Host "`nScript started at '$($ScriptStartTime)'.`n"

Try
{
    UpdateOSImage
}
Catch
{
    Write-Host  -ForegroundColor Red "`n`nERROR                : USB thumb drive script has failed!"
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