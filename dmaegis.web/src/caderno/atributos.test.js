import { expect, test } from 'vitest';
import { atributosParaEnvio, mesclarAtributos } from './atributos.js';

const modelo = (...chaves) => chaves.map((chave, i) => ({ chave, ordem: i + 1 }));

test('modelo primeiro, com valores existentes sem diferenciar maiúsculas; depois os livres', () => {
  const resultado = mesclarAtributos(modelo('CA', 'PV'), [{ chave: 'pv', valor: '10' }, { chave: 'Notas', valor: 'x' }]);
  expect(resultado).toEqual([
    { chave: 'CA', valor: '', doModelo: true },
    { chave: 'PV', valor: '10', doModelo: true },
    { chave: 'Notas', valor: 'x', doModelo: false },
  ]);
});

test('trocar o tipo mantém os valores de chaves iguais e o resto vira livre', () => {
  const antes = [
    { chave: 'CA', valor: '15', doModelo: true },
    { chave: 'PV', valor: '10', doModelo: true },
  ];
  const depois = mesclarAtributos(modelo('PV', 'Estresse'), antes);
  expect(depois).toEqual([
    { chave: 'PV', valor: '10', doModelo: true },
    { chave: 'Estresse', valor: '', doModelo: true },
    { chave: 'CA', valor: '15', doModelo: false },
  ]);
});

test('campos do modelo vazios não viram livres ao trocar de tipo', () => {
  const antes = [{ chave: 'CA', valor: '', doModelo: true }];
  expect(mesclarAtributos([], antes)).toEqual([]);
});

test('sem modelo, só os livres', () => {
  expect(mesclarAtributos([], [{ chave: 'Idade', valor: '40' }])).toEqual([
    { chave: 'Idade', valor: '40', doModelo: false },
  ]);
});

test('atributosParaEnvio apara e remove valores vazios', () => {
  expect(atributosParaEnvio([
    { chave: ' CA ', valor: ' 15 ', doModelo: true },
    { chave: 'PV', valor: '  ', doModelo: true },
    { chave: '', valor: '', doModelo: false },
  ])).toEqual([{ chave: 'CA', valor: '15' }]);
});
