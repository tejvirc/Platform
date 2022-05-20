<#
    .SYNOPSIS
        Resets the Platform, and all installed games, back to their initial
        pre-launch state.  (Known as an NVRam clear)

    .DESCRIPTION
        This script will reset the Platform, and all installed games, back to
        their initial pre-launch state.
        
        This script will not remove any games from the system, including those that
        were installed after initial platform configuration.  This script only removes
        data.

    .NOTES
        Authors  : James Helm (James.Helm@aristocrat.com)
        Date     : 01-27-2020
        Modified : 10-19-2021
        Version  : 1.5

    Copyright © 2021 Aristocrat Technologies, Inc. All Rights Reserved
#>

##########################################################################################
#                                       Constants
##########################################################################################

Set-Variable -Name c_UIEnvVarName                      -Option Constant -Value "YOZZO"
Set-Variable -Name c_MonacoPlatformFolderName          -Option Constant -Value "Aristocrat-VLT"
Set-Variable -Name c_EGM1stMonacoPlatformFolder        -Option Constant -Value "D:\$($c_MonacoPlatformFolderName)\Platform"
Set-Variable -Name c_EGM1stMonacoPlatformRuntimeFolder -Option Constant -Value "D:\$($c_MonacoPlatformFolderName)\Runtime"
Set-Variable -Name c_EGM1stMonacoPlatformGamesFolder   -Option Constant -Value "D:\$($c_MonacoPlatformFolderName)\Games"
Set-Variable -Name c_EGM2ndMonacoPlatformFolder        -Option Constant -Value "E:\$($c_MonacoPlatformFolderName)\Platform"
Set-Variable -Name c_EGM2ndMonacoPlatformRuntimeFolder -Option Constant -Value "E:\$($c_MonacoPlatformFolderName)\Runtime"
Set-Variable -Name c_EGM2ndMonacoPlatformGamesFolder   -Option Constant -Value "E:\$($c_MonacoPlatformFolderName)\Games"

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

Function ResetPlatformFolder([String] $PlatformFolderPath)
{
    #
    # Remove everything inside of the specified folder, if it exists, except for
    # the sub-folder 'packages'
    #

    If (Test-Path -Path "$($PlatformFolderPath)" -PathType Container)
    {
        $PlatformItemsToRemove = Get-ChildItem -Path "$($PlatformFolderPath)" | Select -ExpandProperty FullName | Sort Length -Descending
        $NumItemsRemoved = 0
        ForEach ($item in $PlatformItemsToRemove)
        {
            If (($item.StartsWith("$($PlatformFolderPath)\packages")) -and (Test-Path -Path $item -PathType Container))
            {
                Continue
            }

            $NumItemsRemoved++
            Write-Host "Removing '$($item)'."
            Remove-Item -Path "$($item)" -Recurse -Force
        }

        SendTextToUI -Text "Removed '$($NumItemsRemoved)' Platform item(s), out of '$($PlatformItemsToRemove.Count)'."
    }
}

Function RemoveFolder([String] $FolderPath)
{
    #
    # Removes the specified folder and all sub-folders
    #

    If (Test-Path -Path "$($FolderPath)" -PathType Container)
    {
        SendTextToUI -Text "Removing '$($FolderPath)'."
        Remove-Item -Path "$($FolderPath)" -Recurse -Force
    }
}

Function ResetMonacoPlatform()
{

    $Step = 1
    $NumSteps = 2

    #
    # STEP 1: Ensure the Platform is installed, and if so, reset it back to its initial
    # pre-launch state.
    #

    If ((!(Test-Path -Path "D:\$($c_MonacoPlatformFolderName)" -PathType Container)) -and (!(Test-Path -Path "E:\$($c_MonacoPlatformFolderName)" -PathType Container)))
    {
        SendTextToUI -Text "Step $($Step;$Step++) of $($NumSteps): The Platform folder was not found.  Nothing to reset."
    }
    else
    {
        
        #
        # Because Platform exists, reset it back to its initial pre-launch state.
        #

        SendTextToUI -Text "Step $($Step;$Step++) of $($NumSteps): Resetting the Platform to its initial pre-launch state."

        #
        # Remove everything inside of the 'Platform' folder, if it exists, except for
        # the sub-folder 'packages'
        #

        ResetPlatformFolder -PlatformFolderPath $c_EGM1stMonacoPlatformFolder
        ResetPlatformFolder -PlatformFolderPath $c_EGM2ndMonacoPlatformFolder

        #
        # Remove the 'Runtime' folder, if it exists
        #

        RemoveFolder -FolderPath $c_EGM1stMonacoPlatformRuntimeFolder
        RemoveFolder -FolderPath $c_EGM2ndMonacoPlatformRuntimeFolder

        #
        # Remove the 'Games' folder, if it exists
        #

        RemoveFolder -FolderPath $c_EGM1stMonacoPlatformGamesFolder
        RemoveFolder -FolderPath $c_EGM2ndMonacoPlatformGamesFolder
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

    SendTextToUI -Text "The Platform reset has been completed."
}

##########################################################################################
#                                     Script Start
##########################################################################################

$ScriptStartTime = Get-Date
Write-Host "`nScript started at '$($ScriptStartTime)'.`n"

Try
{
    ResetMonacoPlatform
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