import { expect, test } from 'vitest';
import { retratosPorRegistro } from './retratos.js';

test('a primeira imagem de cada registro é o retrato', () => {
  const imagens = [
    { id: 5, idRegistro: 1 },
    { id: 6, idRegistro: 1 },
    { id: 9, idRegistro: 2 },
  ];
  expect(retratosPorRegistro(imagens)).toEqual({ 1: 5, 2: 9 });
});

test('sem imagens, sem retratos', () => expect(retratosPorRegistro([])).toEqual({}));
