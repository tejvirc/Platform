<#
    .SYNOPSIS
        This script is used to build the Jurisdiction localization resources into Satellite assemblies.
    
    .PARAMETER Configuration
        The build configuration to target (Debug/Release/Retail).
    
    .EXAMPLE
        BuildResources.ps1 -Configuration Debug

    .NOTES
        You need to install the Invoke-MsBuild module to use this script.
        https://www.powershellgallery.com/packages/Invoke-MsBuild/2.6.4
#>
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet('Debug', 'Release', 'Retail')]
    [String]$Configuration = "Debug"
)

Set-StrictMode -Version Latest

Push-Location "$PSScriptRoot\..\Jurisdiction"

try {
    $outputPath = [IO.Path]::GetFullPath("$PSScriptRoot\..\bin\$Configuration\Platform\bin\jurisdiction\")

    $path = "$PSScriptRoot\..\Tasks\Resources.msbuild"
    $parameters = "/v:d /p:Configuration=$Configuration /p:OutputPath=$outputPath"
    
    "Build Path: " + $path 
    "Build Parameters: " + $parameters
    
    Invoke-MsBuild -Path $path -MsBuildParameters $parameters -ShowBuildOutputInCurrentWindow        
}
finally {
    Pop-Location
}
