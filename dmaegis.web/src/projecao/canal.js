// Tela de projeção para os jogadores. O escudo manda a imagem pela BroadcastChannel (entre abas do
// mesmo navegador, sem servidor) e a última imagem fica no localStorage, para a janela voltar a mostrá-la.
const NOME_CANAL = 'dmaegis-projecao';
const CHAVE = 'dmaegis:projecao';

function armazenamentoPadrao() {
  try {
    return globalThis.localStorage ?? null;
  } catch {
    return null;
  }
}

// { id, titulo } da última imagem mostrada, ou null (tela de espera).
export function lerUltimaProjecao(armazenamento = armazenamentoPadrao()) {
  try {
    const valor = JSON.parse(armazenamento?.getItem(CHAVE) ?? 'null');
    if (!valor || !Number.isInteger(valor.id)) return null;
    return { id: valor.id, titulo: typeof valor.titulo === 'string' ? valor.titulo : '' };
  } catch {
    return null;
  }
}

export function gravarUltimaProjecao(projecao, armazenamento = armazenamentoPadrao()) {
  try {
    armazenamento?.setItem(CHAVE, JSON.stringify(projecao));
  } catch {
    // Armazenamento bloqueado: a projeção funciona, só não é lembrada ao reabrir.
  }
}

function enviar(projecao) {
  gravarUltimaProjecao(projecao);
  if (typeof BroadcastChannel === 'undefined') return;
  const canal = new BroadcastChannel(NOME_CANAL);
  canal.postMessage(projecao);
  canal.close();
}

// Mostra a imagem na tela dos jogadores. O título é opcional (um mapa do mestre não deve revelar nada).
export const mostrarNaProjecao = (imagem, comTitulo = false) =>
  enviar({ id: imagem.id, titulo: comTitulo ? imagem.titulo : '' });

// Volta a projeção para a tela de espera.
export const limparProjecao = () => enviar(null);

// Avisa a cada imagem nova. Devolve a função que para de ouvir.
export function ouvirProjecao(aoReceber) {
  if (typeof BroadcastChannel === 'undefined') return () => {};
  const canal = new BroadcastChannel(NOME_CANAL);
  canal.onmessage = (evento) => aoReceber(evento.data && Number.isInteger(evento.data.id) ? evento.data : null);
  return () => canal.close();
}

// Abre (ou traz para a frente) a janela de projeção, para compartilhar a tela ou levar ao segundo monitor.
export function abrirProjecao() {
  window.open('/projecao', 'dmaegis-projecao', 'popup,width=1280,height=720');
}
