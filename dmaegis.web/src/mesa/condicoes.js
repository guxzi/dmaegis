import { REGRAS_DND } from '../regras/dnd.js';

const DAGGERHEART = 2;

// Condições do Daggerheart, com texto próprio (sem copiar o SRD). Confira no livro.
const CONDICOES_DAGGERHEART = [
  { nome: 'Contido', texto: 'Não consegue se mover do lugar, mas ainda pode agir de onde está.' },
  { nome: 'Oculto', texto: 'Fora da vista dos inimigos: rolagens contra ele têm desvantagem até que se revele.' },
  { nome: 'Vulnerável', texto: 'Exposto: as rolagens contra ele têm vantagem.' },
];

// Condições da mesa, só do sistema da campanha: [{ nome, texto }], em ordem alfabética.
export function condicoesDoSistema(idSistema) {
  if (idSistema === DAGGERHEART) return CONDICOES_DAGGERHEART;
  const secao = REGRAS_DND.secoes.find((s) => s.titulo === 'Condições');
  return (secao?.itens ?? []).map(({ nome, texto }) => ({ nome, texto }));
}
