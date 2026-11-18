import { expect, test } from 'vitest';
import { acharRegistroPeloNome, agruparPorRegistro } from './galeria.js';

const imagens = [
  { id: 1, idRegistro: 10, titulo: 'Retrato', nomeRegistro: 'Sken', idTipoRegistro: 1 },
  { id: 2, idRegistro: 10, titulo: 'Disfarce', nomeRegistro: 'Sken', idTipoRegistro: 1 },
  { id: 3, idRegistro: 20, titulo: 'Mapa do cais', nomeRegistro: 'Porto Damali', idTipoRegistro: 4 },
];

test('agrupa as imagens pelo registro, mantendo a ordem', () => {
  const grupos = agruparPorRegistro(imagens);
  expect(grupos.map((g) => [g.idRegistro, g.nomeRegistro, g.idTipoRegistro, g.imagens.map((i) => i.id)])).toEqual([
    [10, 'Sken', 1, [1, 2]],
    [20, 'Porto Damali', 4, [3]],
  ]);
});

test('filtra pelo tipo e pelo termo (no nome do registro ou no título, sem acento)', () => {
  expect(agruparPorRegistro(imagens, { idTipo: 4 }).map((g) => g.idRegistro)).toEqual([20]);
  expect(agruparPorRegistro(imagens, { termo: 'skén' }).map((g) => g.imagens.length)).toEqual([2]);
  expect(agruparPorRegistro(imagens, { termo: 'cais' }).map((g) => g.idRegistro)).toEqual([20]);
  expect(agruparPorRegistro(imagens, { termo: 'disfarce' })[0].imagens.map((i) => i.id)).toEqual([2]);
  expect(agruparPorRegistro(imagens, { termo: 'nada' })).toEqual([]);
});

test('acha o registro pelo nome sem diferenciar maiúsculas, acentos nem espaços nas pontas', () => {
  const registros = [{ id: 10, nome: 'Sken' }, { id: 30, nome: 'Éli' }];
  expect(acharRegistroPeloNome(registros, '  sken ')).toBe(registros[0]);
  expect(acharRegistroPeloNome(registros, 'eli')).toBe(registros[1]);
  expect(acharRegistroPeloNome(registros, 'Novo')).toBeUndefined();
  expect(acharRegistroPeloNome(registros, '   ')).toBeUndefined();
});
