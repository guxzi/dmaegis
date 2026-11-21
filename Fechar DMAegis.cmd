@echo off
rem Desliga o servidor do DMAegis, se ele tiver ficado ligado. O MySQL continua ligado.
start "" "%WINDIR%\System32\conhost.exe" --headless powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\fechar.ps1"
