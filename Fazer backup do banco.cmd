@echo off
rem Faz um backup do banco do DMAegis (com as imagens) em Documentos\Backups DMAegis.
chcp 65001 >nul
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "scripts\backup.ps1"
if errorlevel 1 echo Nao deu para fazer o backup. Veja a mensagem acima.
pause
