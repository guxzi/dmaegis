import { expect, test } from 'vitest';
import {
  caminhoEntre, celulasDoPincel, celulasDoRetangulo, conjuntoDe, diferenca, emLotes, limitesDaGrade, pintar, todasAsCelulas,
} from './nevoa.js';

const chaves = (celulas) => celulas.map((c) => `${c.coluna},${c.linha}`).sort();

test('a grade sobre o mapa conta colunas e linhas, considerando o deslocamento', () => {
  expect(limitesDaGrade(700, 350, { tamanhoCelula: 70 })).toEqual({ colunas: 10, linhas: 5 });
  expect(limitesDaGrade(710, 350, { tamanhoCelula: 70, deslocamentoX: 20 })).toEqual({ colunas: 10, linhas: 5 });
});

test('o pincel cobre um quadrado centrado e respeita a borda da grade', () => {
  expect(celulasDoPincel(5, 5, 1)).toEqual([{ coluna: 5, linha: 5 }]);
  expect(celulasDoPincel(5, 5, 3)).toHaveLength(9);
  expect(chaves(celulasDoPincel(0, 0, 3, { colunas: 10, linhas: 10 }))).toEqual(['0,0', '0,1', '1,0', '1,1']);
  expect(celulasDoPincel(9, 9, 3, { colunas: 10, linhas: 10 })).toHaveLength(4);
});

test('o retângulo pega todas as células entre os cantos, em qualquer direção', () => {
  expect(chaves(celulasDoRetangulo({ coluna: 2, linha: 3 }, { coluna: 1, linha: 2 }))).toEqual(['1,2', '1,3', '2,2', '2,3']);
});

test('o caminho entre duas células não pula buracos', () => {
  expect(caminhoEntre({ coluna: 0, linha: 0 }, { coluna: 3, linha: 0 })).toHaveLength(4);
  expect(caminhoEntre({ coluna: 0, linha: 0 }, { coluna: 3, linha: 3 }).map((c) => `${c.coluna},${c.linha}`))
    .toEqual(['0,0', '1,1', '2,2', '3,3']);
  expect(caminhoEntre({ coluna: 2, linha: 2 }, { coluna: 2, linha: 2 })).toEqual([{ coluna: 2, linha: 2 }]);
});

test('revelar e esconder mudam o conjunto sem mexer no original', () => {
  const antes = conjuntoDe([{ coluna: 1, linha: 1 }]);
  const revelado = pintar(antes, [{ coluna: 2, linha: 1 }], 'revelar');
  expect([...revelado].sort()).toEqual(['1,1', '2,1']);
  expect([...antes]).toEqual(['1,1']);
  expect([...pintar(revelado, [{ coluna: 1, linha: 1 }], 'esconder')]).toEqual(['2,1']);
});

test('a diferença vira o pedido da API', () => {
  const antes = conjuntoDe([{ coluna: 1, linha: 1 }, { coluna: 2, linha: 2 }]);
  const depois = conjuntoDe([{ coluna: 2, linha: 2 }, { coluna: 3, linha: 3 }]);
  expect(diferenca(antes, depois)).toEqual({ revelar: [{ coluna: 3, linha: 3 }], esconder: [{ coluna: 1, linha: 1 }] });
});

test('revelar tudo cobre a grade inteira e vai em lotes', () => {
  expect(todasAsCelulas({ colunas: 3, linhas: 2 })).toHaveLength(6);
  expect(emLotes([1, 2, 3, 4, 5], 2)).toEqual([[1, 2], [3, 4], [5]]);
});
