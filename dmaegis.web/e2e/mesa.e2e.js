import { expect, test } from '@playwright/test';

// A mesa em tempo real, de ponta a ponta: o mestre monta uma mesa na campanha de exemplo,
// a jogadora entra pelo link de convite e cada ação de um aparece na tela do outro.
// Tudo o que o teste cria é apagado no fim.

// Usuário de exemplo do banco local (database/04_senha_usuario_teste.sql). Dá para trocar pelas variáveis.
const EMAIL = process.env.DMAEGIS_EMAIL ?? 'mestre@dmaegis.test';
const SENHA = process.env.DMAEGIS_SENHA ?? 'dmaegis-teste';
const CAMPANHA = Number(process.env.DMAEGIS_CAMPANHA ?? 1);

// Os testes dependem um do outro (mesma mesa) e o login tem limite de tentativas: rodam em sequência, sem repetir.
test.describe.configure({ mode: 'serial' });

let mestre;
let jogadora;
let criado = {};

// Chamada à API pela página do mestre (com o cookie da sessão e o token CSRF).
async function api(pagina, metodo, url, corpo) {
  return pagina.evaluate(async ({ metodo, url, corpo }) => {
    const token = decodeURIComponent(document.cookie.split('; ').find((c) => c.startsWith('XSRF-TOKEN='))?.split('=')[1] ?? '');
    const resposta = await fetch(url, {
      method: metodo,
      headers: { 'Content-Type': 'application/json', 'X-XSRF-TOKEN': token },
      body: corpo ? JSON.stringify(corpo) : undefined,
    });
    const texto = await resposta.text();
    return texto ? JSON.parse(texto).data : null;
  }, { metodo, url, corpo });
}

test.beforeAll(async ({ browser }) => {
  mestre = await (await browser.newContext()).newPage();
  await mestre.goto('/entrar');
  await mestre.getByLabel('E-mail').fill(EMAIL);
  await mestre.getByLabel('Senha').fill(SENHA);
  await mestre.getByRole('button', { name: /entrar/i }).click();
  await expect(mestre).toHaveURL(/\/$/);

  // Sobras de uma execução que parou no meio.
  for (const m of (await api(mestre, 'GET', `/api/campanhas/${CAMPANHA}/mesas`)).filter((x) => x.nome === 'Teste automático')) {
    await api(mestre, 'DELETE', `/api/mesas/${m.id}`);
  }

  // Mesa nova com os PJs e um goblin no mapa, e o link de convite do primeiro PJ.
  const mesa = await api(mestre, 'POST', `/api/campanhas/${CAMPANHA}/mesas`, {
    nome: 'Teste automático', tamanhoCelula: 70, deslocamentoX: 0, deslocamentoY: 0, gradeVisivel: true, movimentoLivre: true, nevoa: false,
  });
  criado.idMesa = mesa.id;
  await api(mestre, 'PUT', `/api/mesas/${mesa.id}/ativa`);
  await api(mestre, 'POST', `/api/mesas/${mesa.id}/participantes`, { todosJogadores: true });
  const registros = await api(mestre, 'GET', `/api/campanhas/${CAMPANHA}/registros`);
  const monstro = registros.find((r) => r.idTipoRegistro === 3);
  const comMonstro = await api(mestre, 'POST', `/api/mesas/${mesa.id}/participantes`, { idRegistro: monstro.id, quantidade: 1 });
  let coluna = 3;
  for (const p of comMonstro.participantes) {
    await api(mestre, 'PUT', `/api/participantes/${p.id}/posicao`, { coluna, linha: 3 });
    coluna += 2;
  }
  criado.pj = comMonstro.participantes.find((p) => p.jogador);
  criado.monstro = comMonstro.participantes.find((p) => !p.jogador);
  const { codigo } = await api(mestre, 'POST', `/api/registros/${criado.pj.idRegistro}/convite`);

  await mestre.goto(`/campanhas/${CAMPANHA}/mesa?mesa=${mesa.id}`);
  await expect(mestre.locator('.ficha')).toHaveCount(comMonstro.participantes.length);

  jogadora = await (await browser.newContext()).newPage();
  await jogadora.goto(`/convite/${codigo}`);
  await expect(jogadora).toHaveURL(/\/jogar$/);
  await expect(jogadora.locator('.ficha-minha')).toBeVisible();
});

test.afterAll(async () => {
  if (!mestre) return;
  const entregas = (await api(mestre, 'GET', `/api/campanhas/${CAMPANHA}/entregas`)) ?? [];
  for (const e of entregas.filter((x) => x.titulo.startsWith('[teste]'))) await api(mestre, 'DELETE', `/api/entregas/${e.id}`);
  if (criado.pj) await api(mestre, 'DELETE', `/api/registros/${criado.pj.idRegistro}/convite`);
  if (criado.idMesa) await api(mestre, 'DELETE', `/api/mesas/${criado.idMesa}`);
});

test('o ping do mestre aparece na tela da jogadora', async () => {
  const palco = mestre.locator('.palco-mapa');
  const caixa = await palco.boundingBox();
  await mestre.keyboard.down('Alt');
  // Um ponto vazio do mapa (no meio pode haver uma ficha; Alt + clique na ficha não solta ping).
  await mestre.mouse.click(caixa.x + caixa.width * 0.75, caixa.y + caixa.height * 0.3);
  await mestre.keyboard.up('Alt');
  // A âncora do efeito não tem tamanho (o desenho fica nos filhos): basta estar na página.
  await expect(mestre.locator('.ping-mapa')).toBeAttached();
  await expect(jogadora.locator('.ping-mapa')).toContainText('Mestre');
});

test('dano no monstro: o mestre vê o número, a jogadora só o golpe', async () => {
  const m = criado.monstro;
  await api(mestre, 'PUT', `/api/participantes/${m.id}`, {
    nome: m.nome, iniciativa: null, bonusIniciativa: m.bonusIniciativa, ca: m.ca, pvAtual: Math.max(1, (m.pvMaximo ?? 4) - 3),
    pvMaximo: m.pvMaximo ?? 4, tamanho: 1, visivel: true, ordem: m.ordem, condicoes: [],
  });
  await expect(mestre.locator('.numero-dano')).toHaveText('−3');
  await expect(jogadora.locator('.numero-dano')).toHaveText('golpe!');
});

test('a reação da jogadora sobe na tela do mestre', async () => {
  await jogadora.getByRole('button', { name: 'Reações' }).click();
  await jogadora.getByRole('button', { name: 'Reagir com 🔥' }).click();
  await expect(mestre.locator('.reacao-flutuante')).toHaveText('🔥');
});

test('a entrega do mestre abre sozinha na tela da jogadora', async () => {
  await mestre.getByRole('button', { name: 'Entregar', exact: true }).click();
  await mestre.getByLabel('Título').fill('[teste] Carta lacrada');
  await mestre.getByLabel('Texto').fill('Venham à prefeitura ao anoitecer.');
  await mestre.locator('.form-entrega').getByRole('button', { name: 'Entregar' }).click();
  await expect(mestre.locator('.lista-entregas')).toContainText('[teste] Carta lacrada');
  await mestre.getByRole('button', { name: 'Fechar' }).click();

  const cartao = jogadora.locator('.revelar-entrega');
  await expect(cartao).toContainText('Venham à prefeitura ao anoitecer.');
  await cartao.getByRole('button', { name: 'Guardar' }).click();
  await expect(cartao).toHaveCount(0);
});

test('o mestre pede iniciativa e o valor rolado entra na ficha', async () => {
  await mestre.getByRole('button', { name: 'Pedir iniciativa' }).click();
  const pedido = jogadora.locator('.pedido-iniciativa');
  await expect(pedido).toContainText('Rolem iniciativa!');
  await pedido.getByRole('button', { name: /^Rolar/ }).click();
  await expect(jogadora.locator('.minha-ficha')).toContainText(/Iniciativa \d+/);
  await expect(mestre.locator('.lista-rolagens')).toContainText('Iniciativa:');
});

test('combate: a abertura e a faixa da vez aparecem para os dois', async () => {
  await mestre.getByRole('button', { name: /Iniciar combate/ }).click();
  await expect(mestre.locator('.abertura-combate')).toContainText('Combate!');
  await expect(jogadora.locator('.abertura-combate')).toContainText('Combate!');
  await expect(jogadora.locator('.anuncio-vez')).toBeAttached();
  await mestre.getByRole('button', { name: 'Encerrar' }).click();
});
