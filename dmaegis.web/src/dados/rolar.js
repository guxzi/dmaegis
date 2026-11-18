import { interpretar, normalizar } from './notacao.js';

// Um dado honesto de 1 a `faces`: crypto.getRandomValues com rejeição, para não ter viés de módulo.
export function dadoSeguro(faces) {
  const limite = Math.floor(2 ** 32 / faces) * faces;
  const buffer = new Uint32Array(1);
  for (;;) {
    globalThis.crypto.getRandomValues(buffer);
    if (buffer[0] < limite) return (buffer[0] % faces) + 1;
  }
}

// Rola a expressão. `sortear(faces)` é a fonte de aleatoriedade (nos testes, uma sequência fixa).
export function rolar(texto, sortear = dadoSeguro) {
  const termos = interpretar(texto);

  const rolados = termos.map((termo) => {
    if (termo.tipo === 'constante') return { ...termo, subtotal: termo.valor };

    const valores = Array.from({ length: termo.quantidade }, () => sortear(termo.faces));
    let mantidos = null;
    if (termo.manter) {
      // Ordena os índices pelo valor (kh: maiores primeiro; kl: menores). Em empate, vale a ordem da rolagem.
      const ordem = valores
        .map((valor, indice) => ({ valor, indice }))
        .sort((a, b) => (termo.manter.modo === 'kh' ? b.valor - a.valor : a.valor - b.valor) || a.indice - b.indice);
      mantidos = new Set(ordem.slice(0, termo.manter.quantidade).map((x) => x.indice));
    }
    const dados = valores.map((valor, indice) => ({ valor, mantido: !mantidos || mantidos.has(indice) }));
    const subtotal = dados.filter((d) => d.mantido).reduce((soma, d) => soma + d.valor, 0);
    return { ...termo, dados, subtotal };
  });

  const total = rolados.reduce((soma, t) => soma + t.sinal * t.subtotal, 0);
  return { expressao: normalizar(termos), termos: rolados, total };
}

// "2d20kh1+5 → ([4], 17) + 5 = 22": descartados entre colchetes.
export function formatarRolagem(resultado) {
  const partes = resultado.termos.map((t, i) => {
    const operador = i === 0 ? (t.sinal < 0 ? '-' : '') : t.sinal < 0 ? ' - ' : ' + ';
    const corpo = t.tipo === 'dados'
      ? `(${t.dados.map((d) => (d.mantido ? `${d.valor}` : `[${d.valor}]`)).join(', ')})`
      : `${t.valor}`;
    return operador + corpo;
  });
  return `${resultado.expressao} → ${partes.join('')} = ${resultado.total}`;
}

// Expressão dos botões rápidos do escudo.
const BASE_RAPIDA = { vantagem: '2d20kh1', desvantagem: '2d20kl1' };

export function expressaoRapida(botao, modificador) {
  const mod = Number(modificador) || 0;
  const base = BASE_RAPIDA[botao] ?? `1${botao}`;
  if (mod > 0) return `${base}+${mod}`;
  if (mod < 0) return `${base}${mod}`;
  return base;
}

// Histórico na memória: o mais recente primeiro, no máximo `limite` itens.
export function adicionarAoHistorico(historico, item, limite = 20) {
  return [item, ...historico].slice(0, limite);
}
