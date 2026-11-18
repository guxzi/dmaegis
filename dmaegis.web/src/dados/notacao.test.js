import { describe, expect, test } from 'vitest';
import { ErroNotacao, interpretar, normalizar } from './notacao.js';

const dados = (sinal, quantidade, faces, manter = null) => ({ sinal, tipo: 'dados', quantidade, faces, manter });
const constante = (sinal, valor) => ({ sinal, tipo: 'constante', valor });

describe('XdY', () => {
  test('d20 sem quantidade vale 1d20', () => expect(interpretar('d20')).toEqual([dados(1, 1, 20)]));
  test('3d6', () => expect(interpretar('3d6')).toEqual([dados(1, 3, 6)]));
  test('maiúscula', () => expect(interpretar('D8')).toEqual([dados(1, 1, 8)]));
});

describe('+/-', () => {
  test('1d20+5', () => expect(interpretar('1d20+5')).toEqual([dados(1, 1, 20), constante(1, 5)]));
  test('vários termos com espaços', () =>
    expect(interpretar('2d6 + 1d4 - 1')).toEqual([dados(1, 2, 6), dados(1, 1, 4), constante(-1, 1)]));
  test('primeiro termo negativo', () => expect(interpretar('-1+1d4')).toEqual([constante(-1, 1), dados(1, 1, 4)]));
});

describe('kh/kl', () => {
  test('2d20kh1', () => expect(interpretar('2d20kh1')).toEqual([dados(1, 2, 20, { modo: 'kh', quantidade: 1 })]));
  test('4d6kh3', () => expect(interpretar('4d6kh3')).toEqual([dados(1, 4, 6, { modo: 'kh', quantidade: 3 })]));
  test('2d20kl1', () => expect(interpretar('2d20kl1')).toEqual([dados(1, 2, 20, { modo: 'kl', quantidade: 1 })]));
  test('kh sem número mantém 1', () =>
    expect(interpretar('2d20kh')).toEqual([dados(1, 2, 20, { modo: 'kh', quantidade: 1 })]));
});

describe('expressão inválida', () => {
  const casos = [
    ['', 'Digite uma rolagem, como 1d20+5.'],
    ['   ', 'Digite uma rolagem, como 1d20+5.'],
    ['2d', 'Não entendi “2d”. Use algo como 2d6+3.'],
    ['d0', 'Os dados vão de d1 a d1000.'],
    ['1d1001', 'Os dados vão de d1 a d1000.'],
    ['101d6', 'No máximo 100 dados por rolagem.'],
    ['60d6+50d4', 'No máximo 100 dados por rolagem.'],
    ['2d20kh3', 'Em 2d20kh3, mantenha de 1 a 2 dados.'],
    ['2d20kh0', 'Em 2d20kh0, mantenha de 1 a 2 dados.'],
    ['1d20++3', 'Não entendi “++3”. Use algo como 2d6+3.'],
    ['abc', 'Não entendi “abc”. Use algo como 2d6+3.'],
    ['1d20+', 'Não entendi “+”. Use algo como 2d6+3.'],
    ['5x', 'Não entendi “5x”. Use algo como 2d6+3.'],
    ['1'.repeat(201), 'Rolagem longa demais.'],
    [Array.from({ length: 21 }, () => '1').join('+'), 'Rolagem longa demais.'],
  ];

  test.each(casos)('%j → %s', (texto, mensagem) => {
    expect(() => interpretar(texto)).toThrow(ErroNotacao);
    expect(() => interpretar(texto)).toThrow(mensagem);
  });
});

test('normalizar: minúsculo, sem espaços e com o 1 implícito', () => {
  expect(normalizar(interpretar(' D20 + 5 '))).toBe('1d20+5');
  expect(normalizar(interpretar('-1+2d20KH'))).toBe('-1+2d20kh1');
});
