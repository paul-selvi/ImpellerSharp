param(
    [ValidateSet("macos", "linux", "windows")]
    [string]$Platform = $env:PLATFORM,
    [ValidateSet("x64", "arm64", "AMD64")]
    [string]$Arch = $env:ARCH,
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = $env:CONFIGURATION
)

if (-not $Platform) {
    $Platform = "windows"
}

if (-not $Arch) {
    $Arch = $env:PROCESSOR_ARCHITECTURE
}

switch ($Arch.ToLower()) {
    "amd64" { $Arch = "x64" }
    "x86_64" { $Arch = "x64" }
    "aarch64" { $Arch = "arm64" }
}

if (-not $Configuration) {
    $Configuration = "Release"
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$repoRoot = Resolve-Path "$scriptDir\..\.."

$py = Get-Command python -ErrorAction SilentlyContinue
if (-not $py) {
    Write-Error "Python not found on PATH. Install Python 3 before running this script."
    exit 1
}

$arguments = @(
    "--platform", $Platform,
    "--arch", $Arch,
    "--configuration", $Configuration
)
$arguments += $args

& $py.Path "$scriptDir\build_rive.py" @arguments
