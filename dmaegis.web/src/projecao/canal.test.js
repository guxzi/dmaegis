import { expect, test } from 'vitest';
import { gravarUltimaProjecao, lerUltimaProjecao } from './canal.js';

function memoria() {
  const dados = {};
  return { getItem: (k) => (k in dados ? dados[k] : null), setItem: (k, v) => { dados[k] = String(v); } };
}

test('lembra a última imagem mostrada, com ou sem título', () => {
  const armazenamento = memoria();
  gravarUltimaProjecao({ id: 4, titulo: 'Porto' }, armazenamento);
  expect(lerUltimaProjecao(armazenamento)).toEqual({ id: 4, titulo: 'Porto' });
  gravarUltimaProjecao({ id: 5, titulo: '' }, armazenamento);
  expect(lerUltimaProjecao(armazenamento)).toEqual({ id: 5, titulo: '' });
});

test('tela limpa, JSON estragado ou armazenamento bloqueado → nada', () => {
  const armazenamento = memoria();
  gravarUltimaProjecao(null, armazenamento);
  expect(lerUltimaProjecao(armazenamento)).toBeNull();
  armazenamento.setItem('dmaegis:projecao', '{quebrado');
  expect(lerUltimaProjecao(armazenamento)).toBeNull();
  armazenamento.setItem('dmaegis:projecao', '{"id":"x"}');
  expect(lerUltimaProjecao(armazenamento)).toBeNull();
  const bloqueado = { getItem: () => { throw new Error(); }, setItem: () => { throw new Error(); } };
  expect(lerUltimaProjecao(bloqueado)).toBeNull();
  expect(() => gravarUltimaProjecao({ id: 1, titulo: '' }, bloqueado)).not.toThrow();
});

test('a projeção também pode mostrar a mesa', () => {
  const armazenamento = memoria();
  gravarUltimaProjecao({ idMesa: 3 }, armazenamento);
  expect(lerUltimaProjecao(armazenamento)).toEqual({ idMesa: 3 });
  armazenamento.setItem('dmaegis:projecao', '{"idMesa":"3"}');
  expect(lerUltimaProjecao(armazenamento)).toBeNull();
});
