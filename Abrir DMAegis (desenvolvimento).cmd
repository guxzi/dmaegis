@echo off
rem Modo de desenvolvimento: sobe a API (.NET) e o site (Vite, com recarga ao salvar) em duas janelas.
rem Para usar no dia a dia, prefira o atalho "DMAegis" (sem terminal).
rem O MySQL (servico MySQL80) ja inicia sozinho com o Windows.
rem Para fechar o app, feche as duas janelas "DMAegis API" e "DMAegis Site".
chcp 65001 >nul
cd /d "%~dp0"

echo Abrindo o DMAegis...

rem Se a API ja estiver no ar, nao abre outra.
curl -s -o nul -m 2 http://localhost:5080/api/saude && goto site
start "DMAegis API" /min cmd /k dotnet run --project DMAegis.WebApi

:site
curl -s -o nul -m 2 http://localhost:5173 && goto esperar
start "DMAegis Site" /min cmd /k "cd dmaegis.web && npm run dev"

:esperar
echo Aguardando a API e o site ficarem prontos...
for /l %%i in (1,1,60) do (
  curl -s -o nul -m 2 http://localhost:5080/api/saude && curl -s -o nul -m 2 http://localhost:5173 && goto abrir
  timeout /t 1 /nobreak >nul
)
echo Nao consegui confirmar que o app subiu. Veja as janelas "DMAegis API" e "DMAegis Site".
pause
exit /b 1

:abrir
start "" http://localhost:5173
exit /b 0
