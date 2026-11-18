import { expect, test } from 'vitest';
import { gravarCenaAtual, lerCenaAtual } from './cenaAtual.js';

function memoria() {
  const dados = {};
  return { getItem: (k) => (k in dados ? dados[k] : null), setItem: (k, v) => { dados[k] = String(v); } };
}

test('grava e lê a cena da sessão', () => {
  const armazenamento = memoria();
  gravarCenaAtual(7, 2, armazenamento);
  expect(lerCenaAtual(7, 5, armazenamento)).toBe(2);
  expect(lerCenaAtual(8, 5, armazenamento)).toBe(0);
});

test('cena fora da lista volta para dentro dos limites', () => {
  const armazenamento = memoria();
  gravarCenaAtual(7, 9, armazenamento);
  expect(lerCenaAtual(7, 3, armazenamento)).toBe(2);
  expect(lerCenaAtual(7, 0, armazenamento)).toBe(0);
});

test('valor estragado ou armazenamento que falha → 0', () => {
  const armazenamento = memoria();
  armazenamento.setItem('dmaegis:cena:7', 'abc');
  expect(lerCenaAtual(7, 3, armazenamento)).toBe(0);
  const quebrado = { getItem: () => { throw new Error('bloqueado'); }, setItem: () => { throw new Error('bloqueado'); } };
  expect(lerCenaAtual(7, 3, quebrado)).toBe(0);
  expect(() => gravarCenaAtual(7, 1, quebrado)).not.toThrow();
});
