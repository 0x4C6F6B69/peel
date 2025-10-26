param(
    [Parameter(Mandatory)] [string] $CsprojPath
)

# Check if the csproj file exists
if (-not (Test-Path $CsprojPath)) {
    Write-Error "Project file not found at: $CsprojPath"
    exit 1
}

# Read the current file content
$content = Get-Content $CsprojPath -Raw

# Regex pattern to match the Version tag
$pattern = '<Version>([^<]+)</Version>'

if ($content -match $pattern) {
    $version = $matches[1]

    # Split by dot and increment the last numeric part
    $parts = $version.Split('.')
    $lastPart = $parts[-1]

    if ($lastPart -match '^\d+$') {
        $incremented = [int]$lastPart + 1
        $parts[-1] = $incremented.ToString()
        $newVersion = ($parts -join '.')

        $newValue = "<Version>$newVersion</Version>"
        $newContent = $content -replace $pattern, $newValue

        # Write the updated content back to the file
        [System.IO.File]::WriteAllText($CsprojPath, $newContent, [System.Text.Encoding]::UTF8)

        Write-Host "Updated version from $version to $newVersion"
    } else {
        Write-Error "Last part of version ($lastPart) is not numeric."
        exit 1
    }
} else {
    Write-Error "Version tag not found in $CsprojPath"
    exit 1
}
