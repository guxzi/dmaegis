# Backup do banco do DMAegis (campanhas, caderno, sessões, mesas e imagens) num arquivo .sql.
# Lê a connection string do .env da raiz; a senha vai para o mysqldump pela variável MYSQL_PWD e nunca aparece na tela.
# Uso: dois cliques em "Fazer backup do banco.cmd", ou: powershell -File scripts/backup.ps1
# Ficam os backups mais recentes (8, ou o número em -Manter); os mais velhos são apagados.
# -Silencioso é para o Agendador de Tarefas: sem mensagens na tela, e o resultado vai para "ultimo-backup.txt".

param(
    [int]$Manter = 8,
    [switch]$Silencioso
)

$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
$arquivoEnv = Join-Path $raiz '.env'
if (-not (Test-Path $arquivoEnv)) { throw "Não achei o .env em $raiz. Copie o .env.example e preencha a senha." }

$linha = Get-Content $arquivoEnv | Where-Object { $_ -match '^\s*ConnectionStrings__Default\s*=' } | Select-Object -First 1
if (-not $linha) { throw 'O .env não tem a linha ConnectionStrings__Default.' }
$conexao = ($linha -split '=', 2)[1]

$partes = @{}
foreach ($par in $conexao.Split(';')) {
    if ($par -match '^\s*([^=]+?)\s*=\s*(.*)$') { $partes[$Matches[1].ToLowerInvariant()] = $Matches[2] }
}
$servidor = if ($partes['server']) { $partes['server'] } else { 'localhost' }
$porta = if ($partes['port']) { $partes['port'] } else { '3306' }
$usuario = if ($partes['user id']) { $partes['user id'] } elseif ($partes['uid']) { $partes['uid'] } else { 'root' }
$senha = if ($partes['password']) { $partes['password'] } else { $partes['pwd'] }
$banco = if ($partes['database']) { $partes['database'] } else { 'dmaegis' }

$mysqldump = Get-Command mysqldump -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
if (-not $mysqldump) {
    $mysqldump = Get-ChildItem 'C:\Program Files\MySQL\*\bin\mysqldump.exe' -ErrorAction SilentlyContinue |
        Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
}
if (-not $mysqldump) { throw 'Não achei o mysqldump. Ele vem com o MySQL Server (pasta bin).' }

$pasta = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Backups DMAegis'
New-Item -ItemType Directory -Force -Path $pasta | Out-Null
$destino = Join-Path $pasta ("dmaegis-{0:yyyy-MM-dd_HHmm}.sql" -f (Get-Date))

$env:MYSQL_PWD = $senha
try {
    & $mysqldump --host=$servidor --port=$porta --user=$usuario --single-transaction --hex-blob `
        --default-character-set=utf8mb4 --databases $banco --result-file=$destino
    if ($LASTEXITCODE -ne 0) { throw "O mysqldump terminou com erro ($LASTEXITCODE)." }
}
finally {
    Remove-Item Env:MYSQL_PWD -ErrorAction SilentlyContinue
}

$tamanho = [Math]::Round((Get-Item $destino).Length / 1MB, 1)

# Apaga os backups mais velhos, mantendo os $Manter mais recentes.
$apagados = 0
if ($Manter -gt 0) {
    $velhos = Get-ChildItem $pasta -Filter 'dmaegis-*.sql' | Sort-Object LastWriteTime -Descending | Select-Object -Skip $Manter
    foreach ($velho in $velhos) { Remove-Item $velho.FullName; $apagados++ }
}

$resumo = "Backup salvo em: $destino ($tamanho MB)."
if ($apagados -gt 0) { $resumo += " $apagados backup(s) antigo(s) apagado(s)." }
Set-Content -Path (Join-Path $pasta 'ultimo-backup.txt') -Value "$(Get-Date -Format 'dd/MM/yyyy HH:mm') - $resumo" -Encoding utf8

if (-not $Silencioso) {
    Write-Host ''
    Write-Host "  $resumo"
    Write-Host '  Para restaurar: mysql -u root -p < "caminho do arquivo .sql"'
    Write-Host ''
}
