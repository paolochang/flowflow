param(
    [string]$VisualStudioPath = "C:\Program Files\Microsoft Visual Studio\2022\Community"
)

$ErrorActionPreference = "Stop"

$checks = @(
    @{
        Name = "Visual Studio 2022 MSBuild"
        Path = Join-Path $VisualStudioPath "MSBuild\Current\Bin\MSBuild.exe"
    },
    @{
        Name = "MSVC toolchain"
        Path = Join-Path $VisualStudioPath "VC\Tools\MSVC"
    },
    @{
        Name = "Windows app packaging PRI tasks"
        Path = Join-Path $VisualStudioPath "MSBuild\Microsoft\VisualStudio\v17.0\AppxPackage\Microsoft.Build.Packaging.Pri.Tasks.dll"
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
