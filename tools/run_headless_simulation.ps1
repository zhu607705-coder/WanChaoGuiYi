$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent $PSScriptRoot
$Project = Join-Path $RootDir "tools/headless_runner/WanChaoGuiYiHeadless/WanChaoGuiYiHeadless.csproj"
$DataDir = if ($args.Count -ge 1) { $args[0] } else { Join-Path $RootDir "My project/Assets/Data" }
$PlayerFactionId = if ($args.Count -ge 2) { $args[1] } else { "faction_qin_shi_huang" }

$DotnetBin = $env:DOTNET_BIN
if (-not $DotnetBin) {
    $dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($dotnetCommand) {
        $DotnetBin = $dotnetCommand.Source
    } elseif (Test-Path "C:/Program Files/dotnet/dotnet.exe") {
        $DotnetBin = "C:/Program Files/dotnet/dotnet.exe"
    } elseif (Test-Path "C:/Program Files (x86)/dotnet/dotnet.exe") {
        $DotnetBin = "C:/Program Files (x86)/dotnet/dotnet.exe"
    } else {
        Write-Error "dotnet SDK is required to run the headless simulation harness. Install .NET SDK 8+, then run: tools/run_headless_simulation.ps1 [data-dir] [player-faction-id]"
    }
}

$runtimes = & $DotnetBin --list-runtimes
if ($runtimes -match "Microsoft\.NETCore\.App 10\.") {
    $Framework = "net10.0"
} elseif ($runtimes -match "Microsoft\.NETCore\.App 8\.") {
    $Framework = "net8.0"
} else {
    Write-Error "A .NET 8 or .NET 10 runtime is required to run the headless simulation harness."
}

& $DotnetBin run --project $Project --framework $Framework -- $DataDir $PlayerFactionId
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
