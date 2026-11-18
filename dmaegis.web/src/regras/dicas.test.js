import { expect, test } from 'vitest';
import { DICAS, dicaDoMomento, dicasDoSistema } from './dicas.js';

const lista = [{ texto: 'a' }, { texto: 'b' }, { texto: 'c' }];
const TRES_MINUTOS = 3 * 60 * 1000;

test('a dica troca a cada 3 minutos e dá a volta na lista', () => {
  expect(dicaDoMomento(0, lista)).toBe(lista[0]);
  expect(dicaDoMomento(TRES_MINUTOS - 1, lista)).toBe(lista[0]);
  expect(dicaDoMomento(TRES_MINUTOS, lista)).toBe(lista[1]);
  expect(dicaDoMomento(TRES_MINUTOS * 4, lista)).toBe(lista[1]);
  expect(dicaDoMomento(0, [])).toBeNull();
});

test('só as dicas gerais e as do sistema da mesa', () => {
  const dnd = dicasDoSistema(DICAS, 1);
  const daggerheart = dicasDoSistema(DICAS, 2);
  expect(dnd.every((d) => d.sistema === null || d.sistema === 1)).toBe(true);
  expect(daggerheart.every((d) => d.sistema === null || d.sistema === 2)).toBe(true);
  expect(dnd.some((d) => d.sistema === 1)).toBe(true);
  expect(daggerheart.some((d) => d.sistema === 2)).toBe(true);
});
