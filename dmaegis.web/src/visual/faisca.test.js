import { expect, test } from 'vitest';
import { alvoDaFaisca, raiosDaFaisca } from './faisca.js';

test('os raios da faísca se espalham em volta do clique, à mesma distância', () => {
  const raios = raiosDaFaisca(6, 20);
  expect(raios).toHaveLength(6);
  for (const { x, y } of raios) expect(Math.hypot(x, y)).toBeCloseTo(20, 5);
  expect(raios[0].x).toBeCloseTo(0, 5);
  expect(raios[0].y).toBeCloseTo(-20, 5);
  expect(raios[3].y).toBeCloseTo(20, 5);
});

function elemento(tag, atributos = {}, pai = null) {
  return {
    tagName: tag.toUpperCase(),
    disabled: atributos.disabled ?? false,
    getAttribute: (nome) => atributos[nome] ?? null,
    closest(seletor) {
      const tags = seletor.split(',').map((s) => s.trim());
      for (let atual = this; atual; atual = atual.pai) {
        if (tags.includes(atual.tagName.toLowerCase()) || (atual.getAttribute('role') === 'button' && tags.includes('[role="button"]'))) return atual;
      }
      return null;
    },
    pai,
  };
}

test('a faísca nasce em botões e links, mesmo clicando num ícone dentro deles', () => {
  const botao = elemento('button');
  const icone = elemento('svg', {}, botao);
  expect(alvoDaFaisca(icone)).toBe(botao);
  expect(alvoDaFaisca(elemento('a'))).not.toBeNull();
});

test('não há faísca em botão desativado nem fora de algo clicável', () => {
  expect(alvoDaFaisca(elemento('button', { disabled: true }))).toBeNull();
  expect(alvoDaFaisca(elemento('button', { 'aria-disabled': 'true' }))).toBeNull();
  expect(alvoDaFaisca(elemento('p'))).toBeNull();
  expect(alvoDaFaisca(null)).toBeNull();
});
