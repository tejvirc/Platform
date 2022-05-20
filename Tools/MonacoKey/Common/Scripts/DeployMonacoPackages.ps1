<#
    .SYNOPSIS
        Deploys files to the Platform 'packages' sub-folder. on the
        EGM.

    .DESCRIPTION
        Script to deploy files to the Platform 'packages' sub-folder
        on the EGM.  It will OVERWRITE any files that are named identically to
        the files present on the USB thumb drive.  It will otherwise not remove
        or change any other existing Platform data.

        The script assumes the files to be deployed are located on the USB thumb
        drive in the root folder named 'Packages'.

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
    [String] $USBDriveLetter = "[USBDRIVE]"
)

##########################################################################################
#                                       Constants
##########################################################################################

Set-Variable -Name c_UIEnvVarName                       -Option Constant -Value "YOZZO"
Set-Variable -Name c_USBMonacoPackagesSourceFolder      -Option Constant -Value "$($USBDriveLetter)\Packages"
Set-Variable -Name c_EGMMonacoPackagesDestinationFolder -Option Constant -Value "$($MonacoPlatformRootFolder)\Platform\packages"

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

Function DeployMonacoPackages()
{

    $Step = 1
    $NumSteps = 2

    #
    # STEP 1: Ensure the USB source folder, containing new Platform package files, exists.
    #

    If (!(Test-Path -Path "$($c_USBMonacoPackagesSourceFolder)" -PathType Container))
    {
        SendTextToUI -Text "Step $($Step;$Step++) of $($NumSteps): The packages were not found on the USB thumb drive in the folder '$($c_USBMonacoPackagesSourceFolder)'.  Nothing to deploy."
    }
    else
    {
        #
        # Ensure the 'packages' destination folder exists.
        #
        If (!(Test-Path -Path "$($c_EGMMonacoPackagesDestinationFolder)" -PathType Container))
        {
            SendTextToUI -Text "Step $($Step;$Step++) of $($NumSteps): The Platform 'packages' folder does not exist on the EGM. Unable to deploy the package(s) from the USB thumb drive."
        }
        else
        {
            $NumPackagesToDeploy = Get-ChildItem -Path "$($c_USBMonacoPackagesSourceFolder)\*" -Recurse | Sort Length -Descending | Measure-Object | %{$_.Count}

            SendTextToUI -Text "Step $($Step;$Step++) of $($NumSteps): Deploying the packages to the Platfom. ($($NumPackagesToDeploy) file(s))"
            Copy-Item -Path "$($c_USBMonacoPackagesSourceFolder)\*" -Destination "$($c_EGMMonacoPackagesDestinationFolder)" -Recurse -Force
        }
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

    SendTextToUI -Text "Platform package deployment has completed."
}

##########################################################################################
#                                     Script Start
##########################################################################################

$ScriptStartTime = Get-Date
Write-Host "`nScript started at '$($ScriptStartTime)'.`n"

Try
{
    DeployMonacoPackages
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