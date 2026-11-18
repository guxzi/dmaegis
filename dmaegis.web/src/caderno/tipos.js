const CRIATURA = 3;
const DAGGERHEART = 2;

// Cor de cada tipo nos cartões do caderno (1 NPC … 7 Missão), dentro da paleta cósmica.
export const COR_TIPO = {
  1: '#60a5fa',
  2: '#34d399',
  3: '#f87171',
  4: '#fbbf24',
  5: '#c4b5fd',
  6: '#93a8ff',
  7: '#f472b6',
};

// Nome do tipo como aparece na tela: no Daggerheart, Criatura se chama "Adversário".
export function rotuloTipo(tipo, idSistema) {
  if (tipo.id === CRIATURA && idSistema === DAGGERHEART) return 'Adversário';
  return tipo.nome;
}
