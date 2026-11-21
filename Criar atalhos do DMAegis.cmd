@echo off
rem Cria o atalho "DMAegis" (com o icone do escudo) na area de trabalho e no menu Iniciar.
chcp 65001 >nul
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "scripts\criar-atalhos.ps1"
if errorlevel 1 echo Nao deu para criar os atalhos. Veja a mensagem acima.
pause
