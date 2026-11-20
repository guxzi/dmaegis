import { defineConfig, devices } from '@playwright/test';

// Testes de ponta a ponta: o navegador de verdade abre o site, entra como mestre e como jogador e
// confere o tempo real entre os dois. Precisa da API rodando (dotnet run --project DMAegis.WebApi)
// e do banco local com os dados de exemplo; o site sobe sozinho se não estiver no ar.
// Rodar: npm run e2e
export default defineConfig({
  testDir: './e2e',
  testMatch: '**/*.e2e.js',
  timeout: 60_000,
  fullyParallel: false,
  workers: 1,
  reporter: [['list']],
  use: {
    baseURL: 'http://localhost:5173',
    viewport: { width: 1366, height: 768 },
    trace: 'retain-on-failure',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'], viewport: { width: 1366, height: 768 } } }],
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: true,
    timeout: 60_000,
  },
});
