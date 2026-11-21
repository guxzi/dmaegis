@echo off
rem Abre o DMAegis como app, sem terminal: o servidor sobe escondido e o site abre numa janela propria.
rem Fechar a janela do app desliga o servidor. O atalho "DMAegis" da area de trabalho faz o mesmo.
cd /d "%~dp0"
start "" "%WINDIR%\System32\conhost.exe" --headless powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\abrir.ps1"
