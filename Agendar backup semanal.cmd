@echo off
rem Agenda o backup semanal do banco do DMAegis no Agendador de Tarefas do Windows.
chcp 65001 >nul
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "scripts\agendar-backup.ps1"
if errorlevel 1 echo Nao deu para agendar o backup. Veja a mensagem acima.
pause
