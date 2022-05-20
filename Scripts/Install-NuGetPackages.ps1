param ( 
    [Parameter (Mandatory = $false)]
    [ValidateScript({ (Test-Path -Path "$_\Monaco.sln" -PathType Leaf) })]
    [String] 
    $SourceDirectory = "$PSScriptRoot\..",

    [Parameter (Mandatory = $false)]
    [String]
    $OutputDirectory = "$PSScriptRoot\..\NuGetPackages"
)

if (-not (Test-Path $OutputDirectory)) {
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
}

$sourcePath = Resolve-Path $SourceDirectory
$outputPath = Resolve-Path $OutputDirectory

Push-Location $sourcePath
try {
    Get-Content "$sourcePath\Monaco.sln" | `
    Where-Object { $_ -match "Project.+, ""(.+)""," } | `
    ForEach-Object { $matches[1] } | `
    Where-Object { $_ -match '\.csproj$' } | `
    ForEach-Object { Get-Content $_ | Find "<PackageReference Include" } | `
    Where-Object { $_ -match "Include=""(.+?)""" } | `
    ForEach-Object { $matches[1] } | `
    Sort-Object -Unique | `
    ForEach-Object { nuget install $_ -OutputDirectory $outputPath -Verbosity detailed }
}
catch {
    Write-Error $_
    return 1
}
finally {
    Pop-Location
}
