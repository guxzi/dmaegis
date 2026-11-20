# Agenda (ou tira) o backup semanal do banco do DMAegis no Agendador de Tarefas do Windows.
# A tarefa roda com o seu usuário, sem janela, todo domingo ao meio-dia. Se o PC estiver desligado
# nessa hora, ela roda assim que der. Os 8 backups mais recentes ficam em Documentos\Backups DMAegis.
# Uso: dois cliques em "Agendar backup semanal.cmd". Para tirar: powershell -File scripts/agendar-backup.ps1 -Remover

param([switch]$Remover)

$ErrorActionPreference = 'Stop'
$nome = 'DMAegis - backup semanal'
$script = Join-Path $PSScriptRoot 'backup.ps1'

if ($Remover) {
    if (Get-ScheduledTask -TaskName $nome -ErrorAction SilentlyContinue) {
        Unregister-ScheduledTask -TaskName $nome -Confirm:$false
        Write-Host ''
        Write-Host '  O backup semanal foi tirado do Agendador de Tarefas.'
    } else {
        Write-Host ''
        Write-Host '  Não havia backup semanal agendado.'
    }
    Write-Host ''
    return
}

$acao = New-ScheduledTaskAction -Execute 'powershell.exe' `
    -Argument "-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File `"$script`" -Silencioso"
$quando = New-ScheduledTaskTrigger -Weekly -DaysOfWeek Sunday -At '12:00'
$opcoes = New-ScheduledTaskSettingsSet -StartWhenAvailable -DontStopIfGoingOnBatteries -AllowStartIfOnBatteries `
    -ExecutionTimeLimit (New-TimeSpan -Hours 1)
Register-ScheduledTask -TaskName $nome -Action $acao -Trigger $quando -Settings $opcoes `
    -Description 'Backup do banco do DMAegis (campanhas, caderno, sessões, mesas e imagens).' -Force | Out-Null

Write-Host ''
Write-Host '  Pronto: o backup do DMAegis roda todo domingo ao meio-dia (ou assim que o PC ligar, se estiver desligado).'
Write-Host '  Os 8 mais recentes ficam em Documentos\Backups DMAegis; o resultado do último fica em ultimo-backup.txt.'
Write-Host '  Para tirar o agendamento: powershell -File scripts\agendar-backup.ps1 -Remover'
Write-Host ''
