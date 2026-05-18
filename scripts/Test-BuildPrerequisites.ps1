param(
    [string]$VisualStudioPath = ""
)

$ErrorActionPreference = "Stop"

function Get-VisualStudioPath {
    if (-not [string]::IsNullOrWhiteSpace($VisualStudioPath)) {
        return $VisualStudioPath
    }

    $vswherePath = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswherePath) {
        $detectedPath = & $vswherePath `
            -products * `
            -version "[17.0,18.0)" `
            -requires Microsoft.Component.MSBuild `
            -property installationPath

        if (-not [string]::IsNullOrWhiteSpace($detectedPath)) {
            return ($detectedPath | Select-Object -First 1)
        }
    }

    return "C:\Program Files\Microsoft Visual Studio\2022\Community"
}

$resolvedVisualStudioPath = Get-VisualStudioPath
Write-Host "Using Visual Studio installation: $resolvedVisualStudioPath"
Write-Host ""

$checks = @(
    @{
        Name = "Visual Studio 2022 MSBuild"
        Path = Join-Path $resolvedVisualStudioPath "MSBuild\Current\Bin\MSBuild.exe"
    },
    @{
        Name = "MSVC toolchain"
        Path = Join-Path $resolvedVisualStudioPath "VC\Tools\MSVC"
    },
    @{
        Name = "Windows app packaging PRI tasks"
        Path = Join-Path $resolvedVisualStudioPath "MSBuild\Microsoft\VisualStudio\v17.0\AppxPackage\Microsoft.Build.Packaging.Pri.Tasks.dll"
    }
)

$failed = $false

foreach ($check in $checks) {
    if (Test-Path $check.Path) {
        Write-Host "[OK] $($check.Name): $($check.Path)"
        continue
    }

    Write-Host "[MISSING] $($check.Name): $($check.Path)"
    $failed = $true
}

if ($failed) {
    Write-Host ""
    Write-Host "Install the Visual Studio components declared in .vsconfig, then run this script again."
    exit 1
}

Write-Host ""
Write-Host "Build prerequisites are present."
