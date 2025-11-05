param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = $env:CONFIGURATION,
    [string]$Output = $env:OUTPUT,
    [string]$PrereleaseSuffix = $env:PRERELEASE_SUFFIX,
    [string]$Rids = $env:RIDS,
    [switch]$SkipNativeCheck
)

if (-not $Configuration) {
    $Configuration = "Release"
}

if (-not $Output) {
    $Output = "artifacts/nuget"
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$repoRoot = Resolve-Path "$scriptDir\..\.."
$python = Get-Command python -ErrorAction SilentlyContinue
if (-not $python) {
    Write-Error "Python not found on PATH. Install Python 3 before running this script."
    exit 1
}

$argsList = @("--configuration", $Configuration, "--output", $Output)

if ($PrereleaseSuffix) {
    $argsList += @("--prerelease-suffix", $PrereleaseSuffix)
}

if ($SkipNativeCheck -or $env:SKIP_NATIVE_CHECK -eq "1") {
    $argsList += "--skip-native-check"
}

if (-not [string]::IsNullOrWhiteSpace($Rids)) {
    $normalized = $Rids.Replace(',', ' ')
    foreach ($rid in $normalized.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries)) {
        $argsList += @("--rid", $rid)
    }
}

$argsList += $args

& $python.Path "$scriptDir\package_all.py" @argsList
