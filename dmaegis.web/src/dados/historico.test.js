import { expect, test } from 'vitest';
import { adicionarAoHistorico, expressaoRapida } from './rolar.js';

test('adicionarAoHistorico põe o novo no começo, corta em 20 e não altera o original', () => {
  const original = Array.from({ length: 20 }, (_, i) => i);
  const novo = adicionarAoHistorico(original, 'novo');
  expect(novo).toHaveLength(20);
  expect(novo[0]).toBe('novo');
  expect(novo[19]).toBe(18);
  expect(original).toHaveLength(20);
  expect(original[0]).toBe(0);
});

test('adicionarAoHistorico aceita outro limite', () => {
  expect(adicionarAoHistorico(['a', 'b'], 'c', 2)).toEqual(['c', 'a']);
});

test('expressaoRapida monta a rolagem dos botões', () => {
  expect(expressaoRapida('d20', 3)).toBe('1d20+3');
  expect(expressaoRapida('vantagem', -1)).toBe('2d20kh1-1');
  expect(expressaoRapida('desvantagem', 0)).toBe('2d20kl1');
  expect(expressaoRapida('d100', 0)).toBe('1d100');
  expect(expressaoRapida('d6', 2)).toBe('1d6+2');
});
