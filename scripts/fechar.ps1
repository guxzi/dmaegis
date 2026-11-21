# Desliga o servidor do DMAegis (a API e o site que ela serve). O banco (MySQL) continua ligado.
# Normalmente não precisa: fechar a janela do app já desliga. Serve para quando ele ficou ligado.
param([switch]$Silencioso)

$processos = Get-CimInstance Win32_Process -Filter "Name='dotnet.exe' OR Name='DMAegis.WebApi.exe'" |
    Where-Object { $_.CommandLine -match 'DMAegis\.WebApi' }
foreach ($p in $processos) { Stop-Process -Id $p.ProcessId -Force -ErrorAction SilentlyContinue }

if (-not $Silencioso) {
    Add-Type -AssemblyName System.Windows.Forms
    $texto = if ($processos) { 'O DMAegis foi desligado.' } else { 'O DMAegis já estava desligado.' }
    [System.Windows.Forms.MessageBox]::Show($texto, 'DMAegis', 'OK', 'Information') | Out-Null
}
