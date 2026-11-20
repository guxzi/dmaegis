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

// O que a projeção mostra: { id, titulo } (uma imagem), { idMesa } (a mesa) ou null (tela de espera).
export function normalizarProjecao(valor) {
  if (!valor) return null;
  if (Number.isInteger(valor.idMesa)) return { idMesa: valor.idMesa };
  if (!Number.isInteger(valor.id)) return null;
  return { id: valor.id, titulo: typeof valor.titulo === 'string' ? valor.titulo : '' };
}

export function lerUltimaProjecao(armazenamento = armazenamentoPadrao()) {
  try {
    return normalizarProjecao(JSON.parse(armazenamento?.getItem(CHAVE) ?? 'null'));
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

function postar(mensagem) {
  if (typeof BroadcastChannel === 'undefined') return;
  const canal = new BroadcastChannel(NOME_CANAL);
  canal.postMessage(mensagem);
  canal.close();
}

function enviar(projecao) {
  gravarUltimaProjecao(projecao);
  postar(projecao);
}

// Mostra a imagem na tela dos jogadores. O título é opcional (um mapa do mestre não deve revelar nada).
export const mostrarNaProjecao = (imagem, comTitulo = false) =>
  enviar({ id: imagem.id, titulo: comTitulo ? imagem.titulo : '' });

// Mostra a mesa (mapa, fichas e iniciativa) na tela dos jogadores, já filtrada.
export const mostrarMesaNaProjecao = (idMesa) => enviar({ idMesa });

// Avisa a projeção que a mesa mudou (ficha movida, turno, PV), para ela buscar o estado de novo.
export const avisarMesaMudou = (idMesa) => postar({ mesaMudou: idMesa });

// Repassa à projeção um sinal passageiro (ping, reação, arrasto) que os jogadores podem ver.
export const avisarSinalNaProjecao = (sinal) => postar({ sinal });

// Volta a projeção para a tela de espera.
export const limparProjecao = () => enviar(null);

// Avisa a cada imagem ou mesa nova. Devolve a função que para de ouvir.
// `aoMudarMesa(idMesa)` recebe os avisos de mudança da mesa mostrada; `aoSinal`, os sinais passageiros.
export function ouvirProjecao(aoReceber, aoMudarMesa = () => {}, aoSinal = () => {}) {
  if (typeof BroadcastChannel === 'undefined') return () => {};
  const canal = new BroadcastChannel(NOME_CANAL);
  canal.onmessage = (evento) => {
    if (Number.isInteger(evento.data?.mesaMudou)) aoMudarMesa(evento.data.mesaMudou);
    else if (evento.data?.sinal) aoSinal(evento.data.sinal);
    else aoReceber(normalizarProjecao(evento.data));
  };
  return () => canal.close();
}

// Abre (ou traz para a frente) a janela de projeção, para compartilhar a tela ou levar ao segundo monitor.
export function abrirProjecao() {
  window.open('/projecao', 'dmaegis-projecao', 'popup,width=1280,height=720');
}
