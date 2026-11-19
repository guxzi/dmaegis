@echo off
rem Abre a mesa virtual para os jogadores: compila o site, sobe a API em modo mesa
rem (servindo o site, sem Swagger, cookies seguros) e cria um tunel https com o cloudflared.
rem Os jogadores entram pelos links que voce gera na pagina Mesa > Jogadores.
rem Para encerrar: feche esta janela (o tunel) e a janela "DMAegis Mesa (API)".
chcp 65001 >nul
cd /d "%~dp0"

echo.
echo   DMAegis - abrindo a mesa para os jogadores
echo   ------------------------------------------
echo.

where cloudflared >nul 2>nul
if errorlevel 1 (
  echo O cloudflared nao esta instalado. Ele cria o endereco https que os jogadores vao abrir.
  echo Instale uma vez com o comando abaixo, feche esta janela e abra de novo:
  echo.
  echo     winget install --id Cloudflare.cloudflared
  echo.
  pause
  exit /b 1
)

curl -s -o nul -m 2 http://localhost:5080/api/saude
if not errorlevel 1 (
  echo Ja existe uma API rodando na porta 5080 ^(provavelmente a janela "DMAegis API"^).
  echo Feche essa janela e abra este arquivo de novo: a mesa precisa da API em modo mesa.
  echo.
  pause
  exit /b 1
)

echo Compilando o site...
call npm --prefix dmaegis.web run build
if errorlevel 1 (
  echo Nao deu para compilar o site. Veja a mensagem acima.
  pause
  exit /b 1
)

echo Subindo a API em modo mesa...
start "DMAegis Mesa (API)" /min cmd /k "set "ASPNETCORE_ENVIRONMENT=Production" && set "DMAEGIS_SERVIR_SITE=1" && dotnet run --project DMAegis.WebApi --no-launch-profile --urls http://localhost:5080"

for /l %%i in (1,1,90) do (
  curl -s -o nul -m 2 http://localhost:5080/api/saude && goto tunel
  timeout /t 1 /nobreak >nul
)
echo A API nao respondeu. Veja a janela "DMAegis Mesa (API)".
pause
exit /b 1

:tunel
echo.
echo   Pronto! Agora o tunel vai mostrar um endereco como https://alguma-coisa.trycloudflare.com
echo.
echo   1. Abra esse endereco no SEU navegador e entre como mestre.
echo   2. Va em Mesa ^> Jogadores, cole o endereco no campo "Endereco da mesa" e gere os links.
echo   3. Mande cada link para o jogador do personagem.
echo.
echo   O endereco muda toda vez que voce abre a mesa: gere os links de novo a cada sessao.
echo   Deixe esta janela aberta enquanto jogam.
echo.
cloudflared tunnel --no-autoupdate --url http://localhost:5080
