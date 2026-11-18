// Passos 5 a 8 do Lazy DM: cada um vincula registros de um tipo do caderno.
export const PASSOS_DE_VINCULO = [
  { passo: 5, titulo: 'Locais fantásticos', idTipo: 4 },
  { passo: 6, titulo: 'NPCs importantes', idTipo: 1 },
  { passo: 7, titulo: 'Monstros possíveis', idTipo: 3 },
  { passo: 8, titulo: 'Tesouros possíveis', idTipo: 6 },
];

export const ID_TIPO_PJ = 2;

// Os 8 passos; no Daggerheart o passo 7 fala de adversários.
export function titulosDosPassos(idSistema) {
  return [
    'Personagens',
    'Começo forte',
    'Cenas',
    'Segredos e pistas',
    'Locais fantásticos',
    'NPCs importantes',
    idSistema === 2 ? 'Adversários possíveis' : 'Monstros possíveis',
    'Tesouros possíveis',
  ];
}

// Move o item `indice` em `delta` posições (−1 sobe, +1 desce). Fora dos limites, devolve uma cópia igual.
export function moverItem(lista, indice, delta) {
  const copia = [...lista];
  const destino = indice + delta;
  if (destino < 0 || destino >= copia.length) return copia;
  [copia[indice], copia[destino]] = [copia[destino], copia[indice]];
  return copia;
}

// Registros vinculados de um tipo, na ordem dos ids.
export function vinculadosDoTipo(idsRegistro, registros, idTipo) {
  return idsRegistro
    .map((id) => registros.find((r) => r.id === id))
    .filter((r) => r && r.idTipoRegistro === idTipo);
}

// Registros do tipo que ainda podem ser vinculados, em ordem alfabética.
export function opcoesParaVincular(registros, idTipo, idsRegistro) {
  return registros
    .filter((r) => r.idTipoRegistro === idTipo && !idsRegistro.includes(r.id))
    .sort((a, b) => a.nome.localeCompare(b.nome, 'pt-BR', { sensitivity: 'base' }));
}

// "2026-11-20" → "20/11/2026".
export function formatarData(iso) {
  if (!iso) return '';
  const [ano, mes, dia] = iso.split('-');
  return `${dia}/${mes}/${ano}`;
}
