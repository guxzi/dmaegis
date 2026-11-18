import { expect, test } from 'vitest';
import { formatarData, moverItem, opcoesParaVincular, titulosDosPassos, vinculadosDoTipo } from './passos.js';

test('moverItem sobe e desce sem alterar a original', () => {
  const lista = ['a', 'b', 'c'];
  expect(moverItem(lista, 1, -1)).toEqual(['b', 'a', 'c']);
  expect(moverItem(lista, 1, 1)).toEqual(['a', 'c', 'b']);
  expect(lista).toEqual(['a', 'b', 'c']);
});

test('moverItem fora dos limites devolve cópia igual', () => {
  const lista = ['a', 'b'];
  const subir = moverItem(lista, 0, -1);
  expect(subir).toEqual(['a', 'b']);
  expect(subir).not.toBe(lista);
  expect(moverItem(lista, 1, 1)).toEqual(['a', 'b']);
});

const registros = [
  { id: 1, idTipoRegistro: 1, nome: 'Ulm' },
  { id: 2, idTipoRegistro: 4, nome: 'Mina' },
  { id: 3, idTipoRegistro: 1, nome: 'Bram' },
  { id: 4, idTipoRegistro: 1, nome: 'Ana' },
];

test('vinculadosDoTipo filtra pelo tipo na ordem dos ids', () => {
  expect(vinculadosDoTipo([3, 2, 1], registros, 1).map((r) => r.nome)).toEqual(['Bram', 'Ulm']);
});

test('opcoesParaVincular exclui as já vinculadas e as de outro tipo, por nome', () => {
  expect(opcoesParaVincular(registros, 1, [1]).map((r) => r.nome)).toEqual(['Ana', 'Bram']);
});

test('passo 7 muda de nome no Daggerheart', () => {
  expect(titulosDosPassos(2)[6]).toBe('Adversários possíveis');
  expect(titulosDosPassos(1)[6]).toBe('Monstros possíveis');
  expect(titulosDosPassos(1)).toHaveLength(8);
});

test('formatarData mostra dd/mm/aaaa e vazio sem data', () => {
  expect(formatarData('2026-11-20')).toBe('20/11/2026');
  expect(formatarData(null)).toBe('');
});
