# Abre o DMAegis como um aplicativo: sem terminal, numa janela própria do Edge (sem barra de endereço).
# Sobe a API escondida servindo o site já compilado (uma porta só, a 5080), mostra "Abrindo o DMAegis…"
# enquanto espera e, quando a janela do app é fechada, desliga a API que ele mesmo subiu.
# Uso: o atalho "DMAegis" da área de trabalho (ou dois cliques em "Abrir DMAegis.cmd").
# O que a API escreve fica em %LOCALAPPDATA%\DMAegis\api.log.

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms, System.Drawing

$raiz = Split-Path -Parent $PSScriptRoot
$pastaApp = Join-Path $env:LOCALAPPDATA 'DMAegis'
New-Item -ItemType Directory -Force -Path $pastaApp | Out-Null
$log = Join-Path $pastaApp 'api.log'
$endereco = 'http://localhost:5080'

function Mostrar-Erro([string]$texto) {
    [System.Windows.Forms.MessageBox]::Show($texto, 'DMAegis', 'OK', 'Warning') | Out-Null
}

function Testar-Url([string]$url) {
    try { return (Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 2).StatusCode -eq 200 } catch { return $false }
}

function Site-No-Ar { try { return (Invoke-WebRequest -Uri "$endereco/" -UseBasicParsing -TimeoutSec 2).Content -match 'id="root"' } catch { return $false } }

# Janelinha "Abrindo o DMAegis…" no tema do app, enquanto compila e sobe.
function Nova-Abertura {
    $janela = New-Object System.Windows.Forms.Form
    $janela.FormBorderStyle = 'None'
    $janela.StartPosition = 'CenterScreen'
    $janela.Size = New-Object System.Drawing.Size(380, 150)
    $janela.BackColor = [System.Drawing.Color]::FromArgb(15, 22, 54)
    $janela.TopMost = $true
    $janela.ShowInTaskbar = $true
    $janela.Text = 'DMAegis'
    $icone = Join-Path $PSScriptRoot 'dmaegis.ico'
    if (Test-Path $icone) { $janela.Icon = New-Object System.Drawing.Icon($icone) }
    $titulo = New-Object System.Windows.Forms.Label
    $titulo.Text = 'DMAegis'
    $titulo.ForeColor = [System.Drawing.Color]::FromArgb(196, 181, 253)
    $titulo.Font = New-Object System.Drawing.Font('Georgia', 20, [System.Drawing.FontStyle]::Bold)
    $titulo.AutoSize = $false
    $titulo.TextAlign = 'MiddleCenter'
    $titulo.SetBounds(0, 28, 380, 44)
    $estado = New-Object System.Windows.Forms.Label
    $estado.Text = 'Abrindo…'
    $estado.ForeColor = [System.Drawing.Color]::FromArgb(154, 163, 212)
    $estado.Font = New-Object System.Drawing.Font('Segoe UI', 10)
    $estado.AutoSize = $false
    $estado.TextAlign = 'MiddleCenter'
    $estado.SetBounds(0, 82, 380, 30)
    $janela.Controls.AddRange(@($titulo, $estado))
    $janela.Tag = $estado
    $janela.Show()
    [System.Windows.Forms.Application]::DoEvents()
    return $janela
}

function Avisar($janela, [string]$texto) {
    $janela.Tag.Text = $texto
    [System.Windows.Forms.Application]::DoEvents()
}

$subiuAqui = $null
$abertura = $null

if (-not (Site-No-Ar)) {
    if (Testar-Url "$endereco/api/saude") {
        # Há uma API de desenvolvimento (sem o site) na porta: abre o site do Vite, se estiver no ar.
        if (Testar-Url 'http://localhost:5173') { $endereco = 'http://localhost:5173' }
        else {
            Mostrar-Erro 'Já existe uma API do DMAegis aberta sem o site (provavelmente uma janela "DMAegis API" ou a mesa para os jogadores). Feche-a e abra o DMAegis de novo.'
            exit 1
        }
    }
    else {
        $abertura = Nova-Abertura

        # Compila o site se ainda não foi compilado ou se algum arquivo mudou desde a última vez.
        $indice = Join-Path $raiz 'dmaegis.web\dist\index.html'
        $fonte = Get-ChildItem (Join-Path $raiz 'dmaegis.web\src'), (Join-Path $raiz 'dmaegis.web\public'), (Join-Path $raiz 'dmaegis.web\index.html') -Recurse -File |
            Sort-Object LastWriteTime -Descending | Select-Object -First 1
        if (-not (Test-Path $indice) -or $fonte.LastWriteTime -gt (Get-Item $indice).LastWriteTime) {
            Avisar $abertura 'Preparando o site (só quando algo mudou)…'
            $npm = Start-Process -FilePath 'cmd.exe' -ArgumentList '/c', 'npm', '--prefix', "`"$(Join-Path $raiz 'dmaegis.web')`"", 'run', 'build' `
                -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $pastaApp 'site.log') -RedirectStandardError (Join-Path $pastaApp 'site-erros.log')
            while (-not $npm.HasExited) { Start-Sleep -Milliseconds 150; [System.Windows.Forms.Application]::DoEvents() }
            if ($npm.ExitCode -ne 0) {
                $abertura.Close()
                Mostrar-Erro "Não deu para preparar o site. Veja $(Join-Path $pastaApp 'site-erros.log')."
                exit 1
            }
        }

        # A API em modo app: ambiente de desenvolvimento (cookies funcionam em http) servindo o site compilado.
        Avisar $abertura 'Ligando o servidor…'
        # Pelo cmd, para a saída ir direto para o log (sem janela nenhuma).
        $info = New-Object System.Diagnostics.ProcessStartInfo
        $info.FileName = 'cmd.exe'
        $info.Arguments = "/c dotnet run --project `"$(Join-Path $raiz 'DMAegis.WebApi')`" --no-launch-profile --urls $endereco > `"$log`" 2>&1"
        $info.WorkingDirectory = Join-Path $raiz 'DMAegis.WebApi'
        $info.UseShellExecute = $false
        $info.CreateNoWindow = $true
        $info.EnvironmentVariables['ASPNETCORE_ENVIRONMENT'] = 'Development'
        $info.EnvironmentVariables['DMAEGIS_SERVIR_SITE'] = '1'
        $subiuAqui = [System.Diagnostics.Process]::Start($info)

        $limite = (Get-Date).AddSeconds(120)
        while (-not (Site-No-Ar)) {
            if ($subiuAqui.HasExited -or (Get-Date) -gt $limite) {
                $abertura.Close()
                Mostrar-Erro "O servidor do DMAegis não ligou. Confira se o MySQL está ligado e veja $log."
                exit 1
            }
            Start-Sleep -Milliseconds 300
            [System.Windows.Forms.Application]::DoEvents()
        }
    }
}

# A janela do app: um Edge só do DMAegis (perfil próprio), para fechar a janela desligar o servidor.
$edge = @(
    "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe",
    "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if ($abertura) { Avisar $abertura 'Pronto!'; Start-Sleep -Milliseconds 300; $abertura.Close() }

if (-not $edge) {
    # Sem o Edge: abre no navegador padrão (aí o servidor fica ligado até usar "Fechar DMAegis").
    Start-Process $endereco
    exit 0
}

$perfil = Join-Path $pastaApp 'janela'
$janelaApp = Start-Process -FilePath $edge -PassThru -ArgumentList @(
    "--app=$endereco", "--user-data-dir=`"$perfil`"", '--no-first-run', '--no-default-browser-check', '--window-size=1400,860'
)

# Espera a janela do app fechar e desliga o servidor que subiu aqui (se já estava ligado, deixa como estava).
if ($subiuAqui) {
    while ($true) {
        Start-Sleep -Seconds 2
        $abertos = Get-CimInstance Win32_Process -Filter "Name='msedge.exe'" | Where-Object { $_.CommandLine -like "*$perfil*" }
        if (-not $abertos) { break }
    }
    & (Join-Path $PSScriptRoot 'fechar.ps1') -Silencioso
}
