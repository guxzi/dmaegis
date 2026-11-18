import { expect, test } from 'vitest';
import { constelacaoDaSemente } from './constelacao.js';

test('mesma semente gera a mesma constelação', () => {
  expect(constelacaoDaSemente(5)).toEqual(constelacaoDaSemente(5));
});

test('sementes diferentes geram constelações diferentes', () => {
  expect(constelacaoDaSemente(5).estrelas).not.toEqual(constelacaoDaSemente(6).estrelas);
});

test('estrelas ficam dentro da área 400x220 e as linhas ligam estrelas existentes', () => {
  for (const semente of [1, 2, 3, 42, 999]) {
    const { estrelas, linhas, matiz } = constelacaoDaSemente(semente);
    expect(estrelas.length).toBeGreaterThanOrEqual(6);
    expect(estrelas.length).toBeLessThanOrEqual(9);
    for (const e of estrelas) {
      expect(e.x).toBeGreaterThanOrEqual(20);
      expect(e.x).toBeLessThanOrEqual(380);
      expect(e.y).toBeGreaterThanOrEqual(20);
      expect(e.y).toBeLessThanOrEqual(200);
    }
    for (const [a, b] of linhas) {
      expect(estrelas[a]).toBeDefined();
      expect(estrelas[b]).toBeDefined();
    }
    expect(matiz).toBeGreaterThanOrEqual(215);
    expect(matiz).toBeLessThanOrEqual(275);
  }
});
