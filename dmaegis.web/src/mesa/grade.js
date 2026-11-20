// Contas da grade e do palco da mesa. Tudo em pixels da imagem do mapa (antes do zoom).

export const ZOOM_MINIMO = 0.2;
export const ZOOM_MAXIMO = 4;

// Célula que contém o ponto (x, y) da imagem, para uma ficha de `tamanho` células.
// O ponto é o centro da ficha; a célula devolvida é o canto de cima à esquerda dela.
export function celulaDoPonto(x, y, { tamanhoCelula, deslocamentoX = 0, deslocamentoY = 0 }, tamanho = 1) {
  const meio = (tamanho - 1) / 2;
  const coluna = Math.round((x - deslocamentoX) / tamanhoCelula - 0.5 - meio);
  const linha = Math.round((y - deslocamentoY) / tamanhoCelula - 0.5 - meio);
  return { coluna: Math.max(0, coluna), linha: Math.max(0, linha) };
}

// Canto de cima à esquerda da célula, em pixels da imagem.
export function pontoDaCelula(coluna, linha, { tamanhoCelula, deslocamentoX = 0, deslocamentoY = 0 }) {
  return { x: deslocamentoX + coluna * tamanhoCelula, y: deslocamentoY + linha * tamanhoCelula };
}

// Zoom em direção a um ponto da tela: o que está sob o cursor continua sob o cursor.
export function zoomNoPonto({ x, y, escala }, novaEscala, pontoTela) {
  const limitada = Math.min(ZOOM_MAXIMO, Math.max(ZOOM_MINIMO, novaEscala));
  const fator = limitada / escala;
  return {
    escala: limitada,
    x: pontoTela.x - (pontoTela.x - x) * fator,
    y: pontoTela.y - (pontoTela.y - y) * fator,
  };
}

// Enquadra o mapa inteiro na área visível, centralizado, com uma pequena margem.
export function enquadrar(larguraMapa, alturaMapa, larguraTela, alturaTela, margem = 24) {
  if (!larguraMapa || !alturaMapa || !larguraTela || !alturaTela) return { x: 0, y: 0, escala: 1 };
  const escala = Math.min(ZOOM_MAXIMO, Math.max(ZOOM_MINIMO,
    Math.min((larguraTela - margem * 2) / larguraMapa, (alturaTela - margem * 2) / alturaMapa)));
  return { escala, x: (larguraTela - larguraMapa * escala) / 2, y: (alturaTela - alturaMapa * escala) / 2 };
}

// Primeira célula livre perto de (coluna, linha), andando em espiral. `ocupadas` é um Set de "c,l".
export function celulaLivrePerto(coluna, linha, ocupadas, limite = 12) {
  if (!ocupadas.has(`${coluna},${linha}`)) return { coluna, linha };
  for (let raio = 1; raio <= limite; raio++) {
    for (let dl = -raio; dl <= raio; dl++) {
      for (let dc = -raio; dc <= raio; dc++) {
        if (Math.max(Math.abs(dc), Math.abs(dl)) !== raio) continue;
        const c = coluna + dc;
        const l = linha + dl;
        if (c < 0 || l < 0) continue;
        if (!ocupadas.has(`${c},${l}`)) return { coluna: c, linha: l };
      }
    }
  }
  return { coluna, linha };
}

// Distância entre duas células para a régua. Nas regras de 2024, cada quadrado conta 1,5 m, inclusive na diagonal,
// então a distância é o maior dos dois deslocamentos (colunas ou linhas).
export const METROS_POR_QUADRADO = 1.5;

export function distanciaNaGrade(a, b) {
  const quadrados = Math.max(Math.abs(b.coluna - a.coluna), Math.abs(b.linha - a.linha));
  return { quadrados, metros: quadrados * METROS_POR_QUADRADO };
}

// Texto da régua: "4 quadrados · 6 m" (com vírgula decimal).
export function textoDaDistancia({ quadrados, metros }) {
  const m = Number.isInteger(metros) ? String(metros) : metros.toFixed(1).replace('.', ',');
  return `${quadrados} ${quadrados === 1 ? 'quadrado' : 'quadrados'} · ${m} m`;
}

// Vista centrada numa célula, com a célula ocupando cerca de `ladoNaTela` pixels (para telas pequenas).
export function vistaCentradaNaCelula(coluna, linha, mesa, larguraTela, alturaTela, ladoNaTela = 52) {
  const escala = Math.min(ZOOM_MAXIMO, Math.max(ZOOM_MINIMO, ladoNaTela / mesa.tamanhoCelula));
  const centro = pontoDaCelula(coluna, linha, mesa);
  const meio = mesa.tamanhoCelula / 2;
  return { escala, x: larguraTela / 2 - (centro.x + meio) * escala, y: alturaTela / 2 - (centro.y + meio) * escala };
}

// Zoom com dois dedos: a distância entre os dedos muda a escala e o ponto do meio arrasta a vista.
export function vistaDaPinca(inicial, distInicial, meioInicial, dist, meio) {
  const base = { ...inicial, x: inicial.x + meio.x - meioInicial.x, y: inicial.y + meio.y - meioInicial.y };
  return zoomNoPonto(base, inicial.escala * (dist / distInicial), meio);
}
