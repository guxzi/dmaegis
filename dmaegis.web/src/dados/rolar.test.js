import { expect, test, vi } from 'vitest';
import { dadoSeguro, formatarRolagem, rolar } from './rolar.js';

// Fonte de aleatoriedade previsível: devolve os valores da lista, em ordem.
const sequencia = (...valores) => {
  let i = 0;
  return vi.fn(() => valores[i++]);
};

test('1d20+5', () => {
  const r = rolar('1d20+5', sequencia(14));
  expect(r.total).toBe(19);
  expect(r.expressao).toBe('1d20+5');
  expect(formatarRolagem(r)).toBe('1d20+5 → (14) + 5 = 19');
});

test('2d20kh1+5 descarta o menor e mostra entre colchetes', () => {
  const r = rolar('2d20kh1+5', sequencia(4, 17));
  expect(r.total).toBe(22);
  expect(r.termos[0].dados).toEqual([{ valor: 4, mantido: false }, { valor: 17, mantido: true }]);
  expect(formatarRolagem(r)).toBe('2d20kh1+5 → ([4], 17) + 5 = 22');
});

test('2d20kl1 fica com o menor', () => {
  expect(rolar('2d20kl1', sequencia(9, 3)).total).toBe(3);
});

test('2d20kh1 com empate mantém só um', () => {
  const r = rolar('2d20kh1', sequencia(12, 12));
  expect(r.total).toBe(12);
  expect(r.termos[0].dados.filter((d) => d.mantido)).toHaveLength(1);
});

test('4d6kh3', () => {
  expect(rolar('4d6kh3', sequencia(1, 5, 3, 6)).total).toBe(14);
});

test('termo de dados negativo', () => {
  const r = rolar('1d8-1d4', sequencia(6, 3));
  expect(r.total).toBe(3);
  expect(formatarRolagem(r)).toBe('1d8-1d4 → (6) - (3) = 3');
});

test('total negativo não é erro', () => {
  expect(rolar('1d4-10', sequencia(2)).total).toBe(-8);
});

test('pede ao sorteio o número de faces de cada dado', () => {
  const sortear = sequencia(3, 7, 1);
  rolar('2d8+1d4', sortear);
  expect(sortear.mock.calls).toEqual([[8], [8], [4]]);
});

test('expressão inválida propaga o erro da notação', () => {
  expect(() => rolar('2d', sequencia())).toThrow('Não entendi');
});

test('dadoSeguro: 6000 d6 ficam entre 1 e 6 e todas as faces aparecem bastante', () => {
  const contagem = [0, 0, 0, 0, 0, 0];
  for (let i = 0; i < 6000; i++) {
    const valor = dadoSeguro(6);
    expect(Number.isInteger(valor)).toBe(true);
    expect(valor).toBeGreaterThanOrEqual(1);
    expect(valor).toBeLessThanOrEqual(6);
    contagem[valor - 1]++;
  }
  for (const vezes of contagem) expect(vezes).toBeGreaterThanOrEqual(800);
});
