import { dadoSeguro } from '../dados/rolar.js';

// Iniciativa de um monstro ou NPC: d20 + bônus, rolado no navegador.
export const rolarIniciativa = (bonus = 0, sortear = dadoSeguro) => sortear(20) + (Number(bonus) || 0);

// Aplica dano ou cura ao PV. Fica entre 0 e o máximo (sem máximo, só não passa de 0 para baixo).
export function aplicarPv(pvAtual, pvMaximo, valor, modo) {
  const quanto = Math.abs(Math.trunc(Number(valor) || 0));
  const atual = pvAtual ?? pvMaximo ?? 0;
  const novo = modo === 'cura' ? atual + quanto : atual - quanto;
  const comTeto = pvMaximo == null ? novo : Math.min(novo, pvMaximo);
  return Math.max(0, comTeto);
}

// Fração do PV (0 a 1) para a barra. Sem PV controlado, null.
export function fracaoPv(pvAtual, pvMaximo) {
  if (!pvMaximo || pvMaximo <= 0) return null;
  return Math.min(1, Math.max(0, (pvAtual ?? 0) / pvMaximo));
}

// Saúde em palavras, igual à regra do servidor (RegrasMesa.EstadoSaude).
export function estadoSaude(pvAtual, pvMaximo) {
  if (!pvMaximo || pvMaximo <= 0) return null;
  const atual = pvAtual ?? 0;
  if (atual <= 0) return 'Caído';
  if (atual * 2 <= pvMaximo) return 'Sangrando';
  return atual < pvMaximo ? 'Ferido' : 'Ileso';
}

// Classe CSS de cada estado, para a cor do selo.
export const CLASSE_SAUDE = { Ileso: 'ileso', Ferido: 'ferido', Sangrando: 'sangrando', 'Caído': 'caido' };

// Iniciativas para começar o combate: quem já tem número mantém; PJs sem número ficam como estão
// (o mestre digita o que os jogadores rolaram); os outros rolam d20 + bônus.
export function iniciativasParaComecar(participantes, sortear = dadoSeguro) {
  return participantes.map((p) => ({
    idParticipante: p.id,
    iniciativa: p.iniciativa ?? (p.jogador ? null : rolarIniciativa(p.bonusIniciativa, sortear)),
  }));
}

// Corpo do PUT de uma ficha, a partir dela mesma com as mudanças.
export function corpoDaFicha(ficha, mudancas = {}) {
  const f = { ...ficha, ...mudancas };
  return {
    nome: f.nome,
    iniciativa: f.iniciativa ?? null,
    bonusIniciativa: f.bonusIniciativa ?? 0,
    ca: f.ca ?? null,
    pvAtual: f.pvAtual ?? null,
    pvMaximo: f.pvMaximo ?? null,
    tamanho: f.tamanho ?? 1,
    visivel: f.visivel ?? true,
    ordem: f.ordem ?? 0,
    condicoes: f.condicoes ?? [],
  };
}

// Corpo do PUT da mesa, a partir dela mesma com as mudanças (grade, mapa, névoa, clima...).
export function corpoDaMesa(mesa, mudancas = {}) {
  const m = { ...mesa, ...mudancas };
  return {
    nome: m.nome,
    idImagemRegistro: m.idImagemRegistro ?? null,
    tamanhoCelula: m.tamanhoCelula,
    deslocamentoX: m.deslocamentoX,
    deslocamentoY: m.deslocamentoY,
    gradeVisivel: m.gradeVisivel,
    movimentoLivre: m.movimentoLivre ?? true,
    nevoa: m.nevoa ?? false,
    clima: m.clima ?? null,
  };
}

// Para desfazer uma mudança na ficha: os valores que ela tinha antes, só dos campos que mudaram.
export function valoresAnteriores(ficha, mudancas) {
  return Object.fromEntries(Object.keys(mudancas).map((chave) => [chave, ficha[chave] ?? null]));
}

// Descrição curta da ação, para o aviso "Desfeito: …".
export function descreverMudanca(ficha, mudancas) {
  const chaves = Object.keys(mudancas);
  if (chaves.length === 1 && chaves[0] === 'pvAtual') return `PV de ${ficha.nome}`;
  if (chaves.length === 1 && chaves[0] === 'condicoes') return `condições de ${ficha.nome}`;
  if (chaves.length === 1 && chaves[0] === 'visivel') return `visibilidade de ${ficha.nome}`;
  return `mudança em ${ficha.nome}`;
}
