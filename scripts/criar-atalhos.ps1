# Cria os atalhos do DMAegis na área de trabalho e no menu Iniciar, com o ícone do escudo.
# "DMAegis" abre o app sem terminal; "Fechar DMAegis" desliga o servidor se ele ficar ligado.
# Os atalhos chamam o PowerShell pelo conhost sem janela (--headless), então nada pisca na tela.
# Uso: dois cliques em "Criar atalhos do DMAegis.cmd". Pode rodar de novo se mudar a pasta do projeto.

$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
$icone = Join-Path $PSScriptRoot 'dmaegis.ico'
$conhost = Join-Path $env:WINDIR 'System32\conhost.exe'
$shell = New-Object -ComObject WScript.Shell

function Novo-Atalho([string]$pasta, [string]$nome, [string]$script, [string]$descricao) {
    $atalho = $shell.CreateShortcut((Join-Path $pasta "$nome.lnk"))
    $atalho.TargetPath = $conhost
    $atalho.Arguments = "--headless powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$(Join-Path $PSScriptRoot $script)`""
    $atalho.WorkingDirectory = $raiz
    $atalho.IconLocation = "$icone,0"
    $atalho.Description = $descricao
    $atalho.Save()
}

$areaDeTrabalho = [Environment]::GetFolderPath('Desktop')
$menuIniciar = Join-Path ([Environment]::GetFolderPath('Programs')) 'DMAegis'
New-Item -ItemType Directory -Force -Path $menuIniciar | Out-Null

Novo-Atalho $areaDeTrabalho 'DMAegis' 'abrir.ps1' 'Abre o DMAegis, o escudo do mestre'
Novo-Atalho $menuIniciar 'DMAegis' 'abrir.ps1' 'Abre o DMAegis, o escudo do mestre'
Novo-Atalho $menuIniciar 'Fechar DMAegis' 'fechar.ps1' 'Desliga o servidor do DMAegis'

Write-Host ''
Write-Host '  Pronto: o atalho "DMAegis" está na área de trabalho e no menu Iniciar.'
Write-Host '  Dois cliques nele abrem o app; fechar a janela do app desliga o servidor.'
Write-Host ''
