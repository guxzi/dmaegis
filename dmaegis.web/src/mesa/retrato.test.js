import { expect, test } from 'vitest';
import { dimensoesReduzidas } from './retrato.js';

test('o retrato cabe em 512 px mantendo a proporção, e imagem pequena não aumenta', () => {
  expect(dimensoesReduzidas(2048, 1024)).toEqual({ largura: 512, altura: 256 });
  expect(dimensoesReduzidas(600, 1200)).toEqual({ largura: 256, altura: 512 });
  expect(dimensoesReduzidas(300, 200)).toEqual({ largura: 300, altura: 200 });
  expect(dimensoesReduzidas(0, 200)).toEqual({ largura: 0, altura: 0 });
});
