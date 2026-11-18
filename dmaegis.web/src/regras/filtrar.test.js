import { expect, test } from 'vitest';
import { filtrarRegras } from './filtrar.js';

const lista = [
  { nome: 'Agarrado', termo: 'Grappled', texto: 'Deslocamento 0.' },
  { nome: 'Caído', termo: 'Prone', texto: 'Só rasteja; ataques corpo a corpo contra você têm vantagem.' },
];

test('termo vazio devolve tudo', () => expect(filtrarRegras(lista, '  ')).toEqual(lista));

test('procura no nome, no termo em inglês e no texto, sem acento nem maiúscula', () => {
  expect(filtrarRegras(lista, 'caido')).toEqual([lista[1]]);
  expect(filtrarRegras(lista, 'GRAPP')).toEqual([lista[0]]);
  expect(filtrarRegras(lista, 'vantagem')).toEqual([lista[1]]);
  expect(filtrarRegras(lista, 'xyz')).toEqual([]);
});
