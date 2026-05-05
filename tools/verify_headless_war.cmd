@echo off
setlocal
set "SCRIPT_DIR=%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%verify_headless_war.ps1" %*
exit /b %ERRORLEVEL%
