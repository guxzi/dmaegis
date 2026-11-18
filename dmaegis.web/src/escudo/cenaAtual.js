// Cena em que a mesa está, por sessão. Fica só neste navegador (conveniência de quem conduz).
const chave = (idSessao) => `dmaegis:cena:${idSessao}`;

function armazenamentoPadrao() {
  try {
    return globalThis.localStorage ?? null;
  } catch {
    return null;
  }
}

// Índice da cena atual, sempre dentro de [0, total − 1]; sem nada gravado (ou com erro), 0.
export function lerCenaAtual(idSessao, total, armazenamento = armazenamentoPadrao()) {
  if (total <= 0) return 0;
  let valor = NaN;
  try {
    valor = Number.parseInt(armazenamento?.getItem(chave(idSessao)) ?? '', 10);
  } catch {
    return 0;
  }
  if (Number.isNaN(valor) || valor < 0) return 0;
  return Math.min(valor, total - 1);
}

export function gravarCenaAtual(idSessao, indice, armazenamento = armazenamentoPadrao()) {
  try {
    armazenamento?.setItem(chave(idSessao), String(indice));
  } catch {
    // Armazenamento bloqueado (janela privada, por exemplo): a cena só não fica lembrada.
  }
}
