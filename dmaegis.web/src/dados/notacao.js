// Leitor da notação de dados: "2d6+3", "2d20kh1+5", "1d8-1d4", "-1+d4".
// Devolve a lista de termos ou lança ErroNotacao com uma mensagem em português.

export const LIMITE_DADOS = 100;
export const LIMITE_FACES = 1000;
export const LIMITE_CONSTANTE = 100000;
export const LIMITE_TERMOS = 20;
export const LIMITE_CARACTERES = 200;

export class ErroNotacao extends Error {
  constructor(mensagem) {
    super(mensagem);
    this.name = 'ErroNotacao';
  }
}

// [sinal]( [N]dF[kh|kl[M]] | constante )
const TERMO = /([+-]?)(?:(\d*)d(\d+)(?:(kh|kl)(\d*))?|(\d+))/y;

const naoEntendi = (trecho) => new ErroNotacao(`Não entendi “${trecho.slice(0, 20)}”. Use algo como 2d6+3.`);

export function interpretar(texto) {
  if (!texto || !texto.trim()) throw new ErroNotacao('Digite uma rolagem, como 1d20+5.');
  if (texto.length > LIMITE_CARACTERES) throw new ErroNotacao('Rolagem longa demais.');

  const s = texto.replace(/\s+/g, '').toLowerCase();
  const termos = [];
  let posicao = 0;

  while (posicao < s.length) {
    TERMO.lastIndex = posicao;
    const m = TERMO.exec(s);
    const fim = m ? posicao + m[0].length : posicao;
    // O termo precisa casar e terminar no fim do texto ou logo antes do próximo sinal.
    if (!m || (fim < s.length && s[fim] !== '+' && s[fim] !== '-')) throw naoEntendi(s.slice(posicao));

    const sinal = m[1] === '-' ? -1 : 1;
    if (m[3] !== undefined) {
      const quantidade = m[2] === '' ? 1 : Number(m[2]);
      const faces = Number(m[3]);
      const manter = m[4] ? { modo: m[4], quantidade: m[5] === '' ? 1 : Number(m[5]) } : null;
      const textoTermo = m[0].replace(/^[+-]/, '');

      if (faces < 1 || faces > LIMITE_FACES) throw new ErroNotacao('Os dados vão de d1 a d1000.');
      if (quantidade < 1) throw new ErroNotacao(`Em ${textoTermo}, role pelo menos 1 dado.`);
      if (manter && (manter.quantidade < 1 || manter.quantidade > quantidade)) {
        throw new ErroNotacao(`Em ${textoTermo}, mantenha de 1 a ${quantidade} dados.`);
      }
      termos.push({ sinal, tipo: 'dados', quantidade, faces, manter });
    } else {
      const valor = Number(m[6]);
      if (valor > LIMITE_CONSTANTE) throw new ErroNotacao(`Os números vão até ${LIMITE_CONSTANTE}.`);
      termos.push({ sinal, tipo: 'constante', valor });
    }
    posicao = fim;
  }

  if (termos.length > LIMITE_TERMOS) throw new ErroNotacao('Rolagem longa demais.');
  const totalDados = termos.reduce((soma, t) => soma + (t.tipo === 'dados' ? t.quantidade : 0), 0);
  if (totalDados > LIMITE_DADOS) throw new ErroNotacao(`No máximo ${LIMITE_DADOS} dados por rolagem.`);

  return termos;
}

// Escreve os termos de volta: minúsculo, sem espaços e com o 1 implícito ("d20" → "1d20").
export function normalizar(termos) {
  return termos
    .map((t, i) => {
      const sinal = t.sinal < 0 ? '-' : i > 0 ? '+' : '';
      const corpo = t.tipo === 'dados'
        ? `${t.quantidade}d${t.faces}${t.manter ? `${t.manter.modo}${t.manter.quantidade}` : ''}`
        : `${t.valor}`;
      return sinal + corpo;
    })
    .join('');
}
