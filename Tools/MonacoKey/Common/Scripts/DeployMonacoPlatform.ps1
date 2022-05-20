<#
    .SYNOPSIS
        Deploys a full copy of the Platform and Games on to the EGM.

    .DESCRIPTION
        Script to deploy a full copy of the Platform and Games on to
        the EGM.  It will REMOVE any existing platform and game files found on
        the EGM and replace them with the files found on the USB thumb drive.

        The script assumes the new Platform and Game files are located
        on the USB thumb drive in the root folder 'Aristocrat-VLT'.

    .NOTES
        Authors  : James Helm (James.Helm@aristocrat.com)
        Date     : 01-27-2020
        Modified : 01-04-2021
        Version  : 1.1

    Copyright © 2021 Aristocrat Technologies, Inc. All Rights Reserved
#>

[CmdLetBinding ()]
Param
(
    [Parameter (Mandatory=$false)]
    [String] $MonacoPlatformRootFolder = "D:\Aristocrat-VLT",

    [Parameter (Mandatory=$false)]
    [String] $USBDriveLetter = "[USBDRIVE]"
)

##########################################################################################
#                                       Constants
##########################################################################################

Set-Variable -Name c_UIEnvVarName                  -Option Constant -Value "YOZZO"
Set-Variable -Name c_USBMonacoPlatformSourceFolder -Option Constant -Value "$($USBDriveLetter)\Aristocrat-VLT"

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

Function DeployMonacoPlatform()
{
    #
    # Ensure the USB source folder, containing the new Platform, exists.
    # If it does not we shouldn't remove any platform files that already exist
    # on the EGM.
    #

    If (!(Test-Path -Path "$($c_USBMonacoPlatformSourceFolder)" -PathType Container))
    {
        SendTextToUI -Text "The Platform was not found on the USB thumb drive in the folder '$($c_USBMonacoPlatformSourceFolder)'.  Nothing to deploy."
    }
    else
    {
        If (Test-Path -Path "$($MonacoPlatformRootFolder)" -PathType Container)
        {
            SendTextToUI -Text "Step 1 of 2: Removing the existing Platform from '$($MonacoPlatformRootFolder)'."
            Remove-Item -Path "$($MonacoPlatformRootFolder)" -Recurse -Force

            $Verifying = $true
            $MaxAttempts = 5
            $CurrentAttempt = 1
            Do
            {
                If (Test-Path -Path "$($MonacoPlatformRootFolder)" -PathType Container)
                {
                    If ($CurrentAttempt -ge $MaxAttempts)
                    {
                        Write-Host "Unable to fully remove the folder.  Continuing the process."
                        Break
                    }

                    SendTextToUI -Text "Step 1 of 2: Verifying the '$($MonacoPlatformRootFolder)' folder was removed. (Attempt $($CurrentAttempt;$CurrentAttempt++) of $($MaxAttempts))"
                    Start-Sleep -Seconds 1
                }
                Else
                {
                    $Verifying = $false
                }
            } While ($Verifying)

            SendTextToUI -Text "Step 2 of 2: Deploying the Platform from '$($c_USBMonacoPlatformSourceFolder)' to '$($MonacoPlatformRootFolder)'."
            robocopy "$($c_USBMonacoPlatformSourceFolder)" "$($MonacoPlatformRootFolder)" /mir /r:0 /nfl /ndl /np /njh
        }
    }

    SendTextToUI -Text "Platform deployment has completed."
}

##########################################################################################
#                                     Script Start
##########################################################################################

$ScriptStartTime = Get-Date
Write-Host "`nScript started at '$($ScriptStartTime)'.`n"

Try
{
    DeployMonacoPlatform
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