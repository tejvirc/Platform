<#
    .SYNOPSIS
        This script is used to build the Jurisdiction localization resources into Satellite assemblies.
    
    .PARAMETER Configuration
        The build configuration to target (Debug/Release/Retail).
    
    .PARAMETER CopyConfigs
        This flag determines whether the config files should be copied to the target directory.
        
    .EXAMPLE
        BuildResources.ps1 -Configuration Debug

    .NOTES
        You need to install the Invoke-MsBuild module to use this script.
        https://www.powershellgallery.com/packages/Invoke-MsBuild/2.6.4
#>
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet('Debug', 'Release', 'Retail')]
    [String]$Configuration = "Debug",
    [switch]$CopyConfigs = $false
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$targetDir = [IO.Path]::GetFullPath("$PSScriptRoot\..\..\bin\$Configuration\Platform\bin\")

$path = "$PSScriptRoot\Resources.msbuild"
$parameters = "/v:d /property:TargetDir=$targetDir"

"Build Path: " + $path 
"Build Parameters: " + $parameters 
"Copy Configs: " + $CopyConfigs

if ($CopyConfigs) {
    Copy-Item $PSScriptRoot\..\..\Jurisdiction -Destination $targetDir -Recurse -Force -Verbose
}

Invoke-MsBuild -Path $path -MsBuildParameters $parameters -ShowBuildOutputInCurrentWindow
