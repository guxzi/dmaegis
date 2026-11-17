import { expect, test } from 'vitest';
import { separarCampanhas } from './organizar.js';

const c = (nome, arquivada = false) => ({ nome, arquivada });

test('separa ativas e arquivadas', () => {
  const { ativas, arquivadas } = separarCampanhas([c('A'), c('B', true), c('C')]);
  expect(ativas.map((x) => x.nome)).toEqual(['A', 'C']);
  expect(arquivadas.map((x) => x.nome)).toEqual(['B']);
});

test('ordena por nome ignorando acentos e maiúsculas', () => {
  const { ativas } = separarCampanhas([c('Órbita'), c('abismo'), c('Mina')]);
  expect(ativas.map((x) => x.nome)).toEqual(['abismo', 'Mina', 'Órbita']);
});

test('lista vazia', () => {
  expect(separarCampanhas([])).toEqual({ ativas: [], arquivadas: [] });
});

test('não altera a lista original', () => {
  const lista = [c('B'), c('A')];
  separarCampanhas(lista);
  expect(lista.map((x) => x.nome)).toEqual(['B', 'A']);
});
