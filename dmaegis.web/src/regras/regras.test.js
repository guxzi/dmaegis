import { expect, test } from 'vitest';
import { regrasDoSistema } from './regras.js';

const nomes = (regras) => regras.secoes.flatMap((s) => s.itens.map((i) => i.nome));

test('mesa de D&D: só regras de D&D, com as 15 condições, as CDs e a atribuição do SRD', () => {
  const regras = regrasDoSistema(1);
  const condicoes = regras.secoes.find((s) => s.titulo === 'Condições');
  expect(condicoes.itens).toHaveLength(15);
  expect(condicoes.itens.map((c) => c.termo)).toContain('Prone');
  expect(nomes(regras)).toContain('Médio');
  expect(nomes(regras).join(' ')).not.toMatch(/Esperança|Medo \(/);
  expect(regras.rodape).toMatch(/SRD 5\.2\.1/);
  expect(regras.rodape).toMatch(/CC-BY|Creative Commons/);
});

test('mesa de Daggerheart: só regras de Daggerheart, com aviso para conferir no livro', () => {
  const regras = regrasDoSistema(2);
  expect(nomes(regras)).toContain('Dualidade');
  expect(nomes(regras)).not.toContain('Caído');
  expect(regras.rodape).toMatch(/livro/);
});

test('sistema desconhecido: nada', () => expect(regrasDoSistema(99).secoes).toEqual([]));

test('todo item tem nome e texto', () => {
  for (const id of [1, 2]) {
    for (const secao of regrasDoSistema(id).secoes) {
      for (const item of secao.itens) {
        expect(item.nome.length).toBeGreaterThan(0);
        expect(item.texto.length).toBeGreaterThan(0);
      }
    }
  }
});
