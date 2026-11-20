// Contas da névoa de guerra: o pincel do mestre trabalha com um conjunto de células reveladas ("coluna,linha").

export const chaveCelula = (coluna, linha) => `${coluna},${linha}`;

export function celulaDaChave(chave) {
  const [coluna, linha] = chave.split(',').map(Number);
  return { coluna, linha };
}

export const conjuntoDe = (celulas = []) => new Set(celulas.map((c) => chaveCelula(c.coluna, c.linha)));

// Quantas colunas e linhas a grade tem sobre o mapa (largura e altura em pixels da imagem).
export function limitesDaGrade(largura, altura, { tamanhoCelula, deslocamentoX = 0, deslocamentoY = 0 }) {
  return {
    colunas: Math.max(0, Math.ceil((largura - deslocamentoX) / tamanhoCelula)),
    linhas: Math.max(0, Math.ceil((altura - deslocamentoY) / tamanhoCelula)),
  };
}

const dentro = (c, l, limites) => c >= 0 && l >= 0 && (!limites || (c < limites.colunas && l < limites.linhas));

// Células de um pincel quadrado de `tamanho` (1, 3, 5…) centrado na célula.
export function celulasDoPincel(coluna, linha, tamanho = 1, limites = null) {
  const raio = Math.floor(tamanho / 2);
  const celulas = [];
  for (let dl = -raio; dl <= raio; dl++) {
    for (let dc = -raio; dc <= raio; dc++) {
      if (dentro(coluna + dc, linha + dl, limites)) celulas.push({ coluna: coluna + dc, linha: linha + dl });
    }
  }
  return celulas;
}

// Todas as células entre dois cantos (Shift + arrastar).
export function celulasDoRetangulo(a, b, limites = null) {
  const celulas = [];
  for (let l = Math.min(a.linha, b.linha); l <= Math.max(a.linha, b.linha); l++) {
    for (let c = Math.min(a.coluna, b.coluna); c <= Math.max(a.coluna, b.coluna); c++) {
      if (dentro(c, l, limites)) celulas.push({ coluna: c, linha: l });
    }
  }
  return celulas;
}

// Células no caminho entre duas células (para um arrasto rápido não pular buracos).
export function caminhoEntre(a, b) {
  const passos = Math.max(Math.abs(b.coluna - a.coluna), Math.abs(b.linha - a.linha));
  if (passos === 0) return [{ ...b }];
  return Array.from({ length: passos + 1 }, (_, i) => ({
    coluna: Math.round(a.coluna + ((b.coluna - a.coluna) * i) / passos),
    linha: Math.round(a.linha + ((b.linha - a.linha) * i) / passos),
  }));
}

// Aplica a pintura: "revelar" põe as células no conjunto; "esconder" tira. Devolve um conjunto novo.
export function pintar(reveladas, celulas, modo) {
  const novo = new Set(reveladas);
  for (const { coluna, linha } of celulas) {
    if (modo === 'esconder') novo.delete(chaveCelula(coluna, linha));
    else novo.add(chaveCelula(coluna, linha));
  }
  return novo;
}

// O que mudou entre antes e depois, no formato da API.
export function diferenca(antes, depois) {
  const revelar = [...depois].filter((k) => !antes.has(k)).map(celulaDaChave);
  const esconder = [...antes].filter((k) => !depois.has(k)).map(celulaDaChave);
  return { revelar, esconder };
}

// Todas as células da grade (para "Revelar tudo").
export function todasAsCelulas(limites) {
  return celulasDoRetangulo({ coluna: 0, linha: 0 }, { coluna: limites.colunas - 1, linha: limites.linhas - 1 });
}

// Divide uma lista em lotes (a API aceita até 5000 células por vez).
export function emLotes(lista, tamanho = 5000) {
  const lotes = [];
  for (let i = 0; i < lista.length; i += tamanho) lotes.push(lista.slice(i, i + tamanho));
  return lotes;
}

// Desfaz uma pintura: tira o que foi revelado e devolve o que foi escondido.
export function desfazerPintura(final, { revelar, esconder }) {
  const anterior = new Set(final);
  for (const { coluna, linha } of revelar) anterior.delete(chaveCelula(coluna, linha));
  for (const { coluna, linha } of esconder) anterior.add(chaveCelula(coluna, linha));
  return anterior;
}
