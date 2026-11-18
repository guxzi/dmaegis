import { expect, test } from 'vitest';
import { formaDoDado } from './formas.js';

test('cada dado comum tem a sua forma e cor', () => {
  for (const faces of [4, 6, 8, 10, 12, 20, 100]) {
    const forma = formaDoDado(faces);
    expect(forma.faces).toBe(faces);
    expect(forma.cor).toMatch(/^#[0-9a-f]{6}$/i);
    expect(forma.elementos.length).toBeGreaterThan(0);
  }
  expect(formaDoDado(4).elementos[0]).toContain('polygon');
  expect(formaDoDado(6).elementos[0]).toContain('rect');
});

test('dado incomum (d3, d7) usa um círculo neutro', () => {
  const forma = formaDoDado(7);
  expect(forma.faces).toBe(7);
  expect(forma.elementos[0]).toContain('circle');
});
