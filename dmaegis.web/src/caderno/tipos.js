const CRIATURA = 3;
const DAGGERHEART = 2;

// Nome do tipo como aparece na tela: no Daggerheart, Criatura se chama "Adversário".
export function rotuloTipo(tipo, idSistema) {
  if (tipo.id === CRIATURA && idSistema === DAGGERHEART) return 'Adversário';
  return tipo.nome;
}
