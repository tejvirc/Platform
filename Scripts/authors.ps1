[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [ValidateScript({(Test-Path -Path $_ -PathType Leaf)})]
    [String] $AuthorsPath
)

$byUsername = @{}
$byName = @{}
$byEmail = @{}

function Populate {
    $file = Get-Content $AuthorsPath -Raw

    $pattern = "(?<username>\w+)\s+=\s+(?<name>[A-Za-z ,.'-]+)\s+<(?<email>[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,})>"
    $results = [Regex]::Matches($file, $pattern)

    foreach ($match in $results) {
        $user = [pscustomobject]@{
            Username = $match.Groups["username"].Value
            Name     = $match.Groups["name"].Value
            Email    = $match.Groups["email"].Value
        }

        $byUsername.Add($user.Username, @($user.Name, $user.Email))
        $byName.Add($user.Name, @($user.Username))
        $byEmail.Add($user.Email, @($user.Username))
    }
}

function Lookup ($author) {
    if ($byUsername.ContainsKey($author)) {
        return $byUsername[$author]
    }

    if ($byName.ContainsKey($author)) {
        return $byName[$author]
    }

    if ($byEmail.ContainsKey($author)) {
        return $byEmail[$author]
    }
}

Populate

while ($author = Read-Host) {
    if ($null -eq $output) {
        $output = Lookup $author
    }
}

if ($null -eq $output) {
    exit 1
}

$output
