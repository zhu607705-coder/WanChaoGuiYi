$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$ProjectPath = if ($args.Count -ge 1) { $args[0] } else { Join-Path $RootDir "My project" }
if (-not [System.IO.Path]::IsPathRooted($ProjectPath)) {
    $ProjectPath = [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $ProjectPath))
}
else {
    $ProjectPath = [System.IO.Path]::GetFullPath($ProjectPath)
}
$OutputDir = if ($env:UNITY_OUTPUT_DIR) { $env:UNITY_OUTPUT_DIR } else { Join-Path $RootDir ".outputs/tuanjie" }
$ResultsPath = if ($env:UNITY_TEST_RESULTS) { $env:UNITY_TEST_RESULTS } else { Join-Path $OutputDir "wanchao-playmode-results.xml" }
$LogPath = if ($env:UNITY_LOG_PATH) { $env:UNITY_LOG_PATH } else { Join-Path $OutputDir "wanchao-playmode.log" }
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

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

function Join-ProcessArguments {
    param([string[]]$Items)

    return ($Items | ForEach-Object { '"' + ($_ -replace '"', '\"') + '"' }) -join " "
}

function Get-TestRunIntAttribute {
    param([System.Xml.XmlElement]$Element, [string]$Name)

    $Value = $Element.GetAttribute($Name)
    if ([string]::IsNullOrEmpty($Value)) {
        Write-Error "PlayMode test result is missing '$Name' attribute."
    }

    return [int]$Value
}

function Assert-PlayModeResults {
    param([string]$Path)

    [xml]$ResultsXml = Get-Content -Raw $Path
    $Run = $ResultsXml.DocumentElement
    if ($Run -eq $null -or $Run.Name -ne "test-run") {
        Write-Error "PlayMode test result root should be <test-run>: $Path"
    }

    $Total = Get-TestRunIntAttribute -Element $Run -Name "total"
    $Passed = Get-TestRunIntAttribute -Element $Run -Name "passed"
    $Failed = Get-TestRunIntAttribute -Element $Run -Name "failed"
    $Skipped = Get-TestRunIntAttribute -Element $Run -Name "skipped"

    if ($Total -le 0) { Write-Error "PlayMode test result has no tests: $Path" }
    if ($Passed -le 0) { Write-Error "PlayMode test result has no passing tests: $Path" }
    if ($Failed -ne 0) { Write-Error "PlayMode test result has failures: failed=$Failed" }
    if (($Passed + $Failed + $Skipped) -gt $Total) {
        Write-Error "PlayMode test result counts are inconsistent: total=$Total passed=$Passed failed=$Failed skipped=$Skipped"
    }

    $SkippedCases = @($ResultsXml.SelectNodes("//test-case[@result='Skipped' or @result='Ignored' or @label='Ignored']"))
    if ($Skipped -eq 0) {
        if ($SkippedCases.Count -ne 0) {
            Write-Error "PlayMode XML contains skipped test-cases but root skipped=0."
        }
    }
    elseif ($Skipped -eq 1 -and $SkippedCases.Count -eq 1) {
        $SkippedName = $SkippedCases[0].GetAttribute("fullname")
        if ([string]::IsNullOrEmpty($SkippedName)) { $SkippedName = $SkippedCases[0].GetAttribute("name") }
        if ($SkippedName -notlike "*VisualSmokeCaptureTests*") {
            Write-Error "Unexpected skipped PlayMode test: $SkippedName"
        }
    }
    else {
        Write-Error "Unexpected PlayMode skipped count: root=$Skipped cases=$($SkippedCases.Count)"
    }

    Write-Host "PlayMode test summary: total=$Total passed=$Passed failed=$Failed skipped=$Skipped"
}

$UnityExecutable = Resolve-Unity
if (-not $UnityExecutable) {
    Write-Error "Unity/Tuanjie editor executable not found. Set UNITY_BIN=/path/to/Unity-or-Tuanjie or install Tuanjie 2022.3.62t7."
}

Remove-Item -LiteralPath $ResultsPath, $LogPath -Force -ErrorAction SilentlyContinue

$Arguments = @(
    "-batchmode",
    "-nographics",
    "-projectPath", $ProjectPath,
    "-runTests",
    "-testPlatform", "PlayMode",
    "-testResults", $ResultsPath,
    "-logFile", $LogPath
)
$Process = Start-Process -FilePath $UnityExecutable -ArgumentList (Join-ProcessArguments $Arguments) -Wait -PassThru -WindowStyle Hidden
if ($Process.ExitCode -ne 0) { exit $Process.ExitCode }

if (-not (Test-Path $ResultsPath)) {
    Write-Error "PlayMode test results were not created: $ResultsPath"
}

Assert-PlayModeResults -Path $ResultsPath

Write-Host "PlayMode test results: $ResultsPath"
Write-Host "Unity log: $LogPath"
