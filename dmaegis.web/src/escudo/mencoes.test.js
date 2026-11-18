import { expect, test } from 'vitest';
import { marcarMencoes } from './mencoes.js';

const sken = { id: 1, nome: 'Sken' };
const porto = { id: 2, nome: 'Porto Damali' };
const portoVelho = { id: 3, nome: 'Porto' };
const eli = { id: 4, nome: 'Éli' };

test('nome no texto vira menção, o resto fica como texto', () => {
  expect(marcarMencoes('Falar com Sken amanhã', [sken])).toEqual([
    { texto: 'Falar com ' },
    { texto: 'Sken', registro: sken },
    { texto: ' amanhã' },
  ]);
});

test('não diferencia maiúsculas nem acentos e mantém a grafia do texto', () => {
  expect(marcarMencoes('eli e SKEN', [sken, eli])).toEqual([
    { texto: 'eli', registro: eli },
    { texto: ' e ' },
    { texto: 'SKEN', registro: sken },
  ]);
});

test('o nome mais longo vence', () => {
  expect(marcarMencoes('Chegaram ao porto damali.', [portoVelho, porto])).toEqual([
    { texto: 'Chegaram ao ' },
    { texto: 'porto damali', registro: porto },
    { texto: '.' },
  ]);
});

test('só vale a palavra inteira', () => {
  expect(marcarMencoes('Skenário', [sken])).toEqual([{ texto: 'Skenário' }]);
});

test('texto sem nomes ou lista vazia', () => {
  expect(marcarMencoes('nada aqui', [sken])).toEqual([{ texto: 'nada aqui' }]);
  expect(marcarMencoes('Sken', [])).toEqual([{ texto: 'Sken' }]);
  expect(marcarMencoes('', [sken])).toEqual([]);
});
