$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$ProjectPath = if ($args.Count -ge 1) { $args[0] } else { Join-Path $RootDir "My project" }
$OutputDir = if ($env:UNITY_OUTPUT_DIR) { $env:UNITY_OUTPUT_DIR } else { Join-Path $RootDir ".outputs/tuanjie" }
$VisualOutputDir = if ($env:VISUAL_OUTPUT_DIR) { $env:VISUAL_OUTPUT_DIR } else { Join-Path $RootDir ".outputs/visual" }
$CopyPath = if ($env:VISUAL_PROJECT_COPY) { $env:VISUAL_PROJECT_COPY } else { Join-Path $OutputDir "visual-project-copy" }
$PreviewCopyPath = if ($env:VISUAL_PREVIEW_COPY) { $env:VISUAL_PREVIEW_COPY } else { Join-Path $OutputDir "visual-preview-copy" }
$ResultsPath = if ($env:UNITY_VISUAL_TEST_RESULTS) { $env:UNITY_VISUAL_TEST_RESULTS } else { Join-Path $OutputDir "visual-smoke-graphics-copy-results.xml" }
$LogPath = if ($env:UNITY_VISUAL_LOG_PATH) { $env:UNITY_VISUAL_LOG_PATH } else { Join-Path $OutputDir "visual-smoke-graphics-copy.log" }
$MaxAttempts = if ($env:VISUAL_SMOKE_MAX_ATTEMPTS) { [int]$env:VISUAL_SMOKE_MAX_ATTEMPTS } else { 2 }

New-Item -ItemType Directory -Force -Path $OutputDir, $VisualOutputDir | Out-Null

function Resolve-Unity {
    if ($env:UNITY_BIN -and (Test-Path $env:UNITY_BIN)) { return $env:UNITY_BIN }

    $LocalTuanjie = Join-Path (Split-Path -Parent $RootDir) "Editor/Tuanjie.exe"
    if (Test-Path $LocalTuanjie) { return $LocalTuanjie }

    $Patterns = @(
        "E:/万朝归一/Editor/Tuanjie.exe",
        "C:/Program Files/Tuanjie/Editor/Tuanjie.exe",
        "C:/Program Files/Unity/Hub/Editor/*/Editor/Unity.exe",
        "C:/Program Files/Unity */Editor/Unity.exe",
        "C:/Program Files/Unity/Editor/Unity.exe",
        "C:/Program Files (x86)/Unity/Editor/Unity.exe"
    )

    foreach ($Pattern in $Patterns) {
        $Candidate = Get-ChildItem -Path $Pattern -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($Candidate) { return $Candidate.FullName }
    }

    $Command = Get-Command Tuanjie -ErrorAction SilentlyContinue
    if ($Command) { return $Command.Source }

    $Command = Get-Command Unity -ErrorAction SilentlyContinue
    if ($Command) { return $Command.Source }

    return $null
}

function Assert-SafeOutputPath {
    param([string]$Path, [string]$Parent)

    if (-not (Test-Path $Parent)) {
        New-Item -ItemType Directory -Force -Path $Parent | Out-Null
    }

    $ParentFullPath = (Resolve-Path $Parent).Path
    $CandidateFullPath = [System.IO.Path]::GetFullPath($Path)
    if (-not $CandidateFullPath.StartsWith($ParentFullPath, [System.StringComparison]::OrdinalIgnoreCase)) {
        Write-Error "Refusing to clean outside output directory: $CandidateFullPath"
    }
}

function Remove-VisualSmokeTempFiles {
    Get-ChildItem -LiteralPath $VisualOutputDir -Filter "unity-*.png" -ErrorAction SilentlyContinue | Remove-Item -Force

    if (Test-Path $CopyPath) {
        Assert-SafeOutputPath -Path $CopyPath -Parent $OutputDir
        Remove-Item -LiteralPath $CopyPath -Recurse -Force
    }
    if (Test-Path $PreviewCopyPath) {
        Assert-SafeOutputPath -Path $PreviewCopyPath -Parent $OutputDir
        Remove-Item -LiteralPath $PreviewCopyPath -Recurse -Force
    }
}

function New-VisualProjectCopy {
    Remove-VisualSmokeTempFiles
    New-Item -ItemType Directory -Force -Path $CopyPath | Out-Null

    $ExcludedDirs = @("Library", "Temp", "Obj", "Logs")
    Get-ChildItem -LiteralPath $ProjectPath -Force | Where-Object { $ExcludedDirs -notcontains $_.Name } | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $CopyPath -Recurse -Force
    }
}

function Invoke-VisualSmokeAttempt {
    param([string]$UnityExecutable)

    Remove-Item -LiteralPath $ResultsPath, $LogPath -Force -ErrorAction SilentlyContinue
    Get-ChildItem -LiteralPath $VisualOutputDir -Filter "unity-*.png" -ErrorAction SilentlyContinue | Remove-Item -Force

    $env:VISUAL_OUTPUT_DIR = $VisualOutputDir
    try {
        $Arguments = @(
            "-batchmode",
            "-projectPath", $CopyPath,
            "-runTests",
            "-testPlatform", "PlayMode",
            "-testFilter", "WanChaoGuiYi.Tests.VisualSmokeCaptureTests",
            "-testResults", $ResultsPath,
            "-logFile", $LogPath
        )
        $Process = Start-Process -FilePath $UnityExecutable -ArgumentList $Arguments -Wait -PassThru -WindowStyle Hidden
        return $Process.ExitCode
    }
    finally {
        Remove-Item Env:\VISUAL_OUTPUT_DIR -ErrorAction SilentlyContinue
    }
}

function Get-TestRunSummary {
    if (-not (Test-Path $ResultsPath)) {
        return $null
    }

    [xml]$ResultsXml = Get-Content -Raw $ResultsPath
    $Run = $ResultsXml.DocumentElement
    return [pscustomobject]@{
        Result = $Run.GetAttribute("result")
        Total = [int]$Run.GetAttribute("total")
        Passed = [int]$Run.GetAttribute("passed")
        Failed = [int]$Run.GetAttribute("failed")
        Skipped = [int]$Run.GetAttribute("skipped")
    }
}

function Test-IsKnownFirstImportFailure {
    if (-not (Test-Path $ResultsPath) -or -not (Test-Path $LogPath)) {
        return $false
    }

    $Needle = "Cannot create required material because shader is null"
    return (Select-String -Path $ResultsPath, $LogPath -SimpleMatch $Needle -Quiet)
}

function Get-VisualScreenshotStats {
    Add-Type -AssemblyName System.Drawing

    $ExpectedFiles = @(
        "unity-map-hud.png",
        "unity-region-building-panel.png",
        "unity-weather-panel.png",
        "unity-governance-default.png",
        "unity-governance-forecast.png",
        "unity-occupation-chain.png",
        "unity-war-route-risk.png",
        "unity-map-lens-risk.png",
        "unity-outliner.png",
        "unity-diplomacy-bridge.png",
        "unity-war-route.png",
        "unity-battle-report.png"
    )

    $Stats = @()
    foreach ($FileName in $ExpectedFiles) {
        $Path = Join-Path $VisualOutputDir $FileName
        if (-not (Test-Path $Path)) {
            Write-Error "Visual smoke screenshot was not created: $Path"
        }

        $Item = Get-Item $Path
        if ($Item.Length -le 0) {
            Write-Error "Visual smoke screenshot is empty: $Path"
        }

        $Bitmap = [System.Drawing.Bitmap]::new($Path)
        try {
            if ($Bitmap.Width -ne 1600 -or $Bitmap.Height -ne 900) {
                Write-Error "Visual smoke screenshot has unexpected size: $Path $($Bitmap.Width)x$($Bitmap.Height)"
            }

            $Colors = [System.Collections.Generic.HashSet[int]]::new()
            $StepX = [Math]::Max(1, [int]($Bitmap.Width / 160))
            $StepY = [Math]::Max(1, [int]($Bitmap.Height / 90))
            for ($Y = 0; $Y -lt $Bitmap.Height; $Y += $StepY) {
                for ($X = 0; $X -lt $Bitmap.Width; $X += $StepX) {
                    [void]$Colors.Add($Bitmap.GetPixel($X, $Y).ToArgb())
                }
            }

            if ($Colors.Count -lt 50) {
                Write-Error "Visual smoke screenshot appears blank or near-blank: $Path sampledColors=$($Colors.Count)"
            }

            $Stats += [pscustomobject]@{
                File = $FileName
                Width = $Bitmap.Width
                Height = $Bitmap.Height
                Bytes = $Item.Length
                SampledColors = $Colors.Count
            }
        }
        finally {
            $Bitmap.Dispose()
        }
    }

    return $Stats
}

$UnityExecutable = Resolve-Unity
if (-not $UnityExecutable) {
    Write-Error "Unity/Tuanjie editor executable not found. Set UNITY_BIN=/path/to/Unity-or-Tuanjie or install Tuanjie 2022.3.62t7."
}

New-VisualProjectCopy
$Succeeded = $false
$LastExitCode = 0
$FinalExitCode = 0
try {
    for ($Attempt = 1; $Attempt -le $MaxAttempts; $Attempt++) {
        Write-Host "VisualSmoke attempt $Attempt of $MaxAttempts"
        $LastExitCode = Invoke-VisualSmokeAttempt -UnityExecutable $UnityExecutable
        $Summary = Get-TestRunSummary

        if ($Summary -and $Summary.Failed -eq 0 -and $Summary.Total -eq 1 -and $Summary.Passed -eq 1) {
            $Succeeded = $true
            break
        }

        if ($Attempt -lt $MaxAttempts -and (Test-IsKnownFirstImportFailure)) {
            Write-Host "Retrying after known first-import URP material log."
            continue
        }

        break
    }

    if (-not $Succeeded) {
        Write-Host "VisualSmoke test results: $ResultsPath"
        Write-Host "Unity log: $LogPath"
        $FinalExitCode = if ($LastExitCode -ne 0) { $LastExitCode } else { 1 }
    }
    else {
        $ScreenshotStats = Get-VisualScreenshotStats
        Write-Host "VisualSmoke test results: $ResultsPath"
        Write-Host "Unity log: $LogPath"
        $ScreenshotStats | Format-Table -AutoSize
    }
}
finally {
    Remove-VisualSmokeTempFiles
}

$RemainingScreenshots = @(Get-ChildItem -LiteralPath $VisualOutputDir -Filter "unity-*.png" -ErrorAction SilentlyContinue).Count
if ($RemainingScreenshots -ne 0) {
    Write-Error "Temporary VisualSmoke screenshots were not cleaned up: $RemainingScreenshots"
}

if (Test-Path $CopyPath) {
    Write-Error "Temporary VisualSmoke project copy was not cleaned up: $CopyPath"
}

if (Test-Path $PreviewCopyPath) {
    Write-Error "Temporary VisualSmoke preview copy was not cleaned up: $PreviewCopyPath"
}

if ($FinalExitCode -ne 0) {
    exit $FinalExitCode
}

Write-Host "VisualSmoke graphics gate passed and temporary screenshots/project copy were removed."
