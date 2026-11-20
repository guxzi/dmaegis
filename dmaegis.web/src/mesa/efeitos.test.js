import { expect, test, vi } from 'vitest';
import {
  celulasRecemReveladas, combateComecou, criticoDaRolagem, limitarTaxa, mudancasDeSaude, novaVez, sementeDe,
} from './efeitos.js';

const mesa = (participantes, extra = {}) => ({ id: 5, participantes, ...extra });

test('dano e cura com PV trazem o número, e cair é quando o PV chega a zero', () => {
  const antes = mesa([{ id: 1, pvAtual: 10, pvMaximo: 20 }, { id: 2, pvAtual: 3, pvMaximo: 7 }, { id: 3, pvAtual: 5, pvMaximo: 5 }]);
  const depois = mesa([{ id: 1, pvAtual: 14, pvMaximo: 20 }, { id: 2, pvAtual: 0, pvMaximo: 7 }, { id: 3, pvAtual: 5, pvMaximo: 5 }]);
  expect(mudancasDeSaude(antes, depois)).toEqual([
    { idFicha: 1, tipo: 'cura', valor: 4, caiu: false },
    { idFicha: 2, tipo: 'dano', valor: 3, caiu: true },
  ]);
});

test('sem PV (monstro na tela do jogador), a direção vem da saúde em palavras', () => {
  const antes = mesa([{ id: 2, estadoSaude: 'Ferido' }, { id: 4, estadoSaude: 'Sangrando' }]);
  const depois = mesa([{ id: 2, estadoSaude: 'Caído' }, { id: 4, estadoSaude: 'Ferido' }]);
  expect(mudancasDeSaude(antes, depois)).toEqual([
    { idFicha: 2, tipo: 'dano', valor: null, caiu: true },
    { idFicha: 4, tipo: 'cura', valor: null, caiu: false },
  ]);
});

test('trocar de mesa ou ficha nova não conta como dano', () => {
  expect(mudancasDeSaude(mesa([{ id: 1, pvAtual: 9, pvMaximo: 9 }]), { id: 6, participantes: [{ id: 1, pvAtual: 1, pvMaximo: 9 }] })).toEqual([]);
  expect(mudancasDeSaude(mesa([]), mesa([{ id: 1, pvAtual: 1, pvMaximo: 9 }]))).toEqual([]);
  // A visão do jogador usa idMesa no lugar de id.
  expect(mudancasDeSaude({ idMesa: 5, participantes: [{ id: 1, pvAtual: 9, pvMaximo: 9 }] },
    { idMesa: 5, participantes: [{ id: 1, pvAtual: 4, pvMaximo: 9 }] })).toHaveLength(1);
});

test('começo do combate e nova vez', () => {
  const fora = mesa([{ id: 1 }, { id: 2 }], { emCombate: false });
  const vez1 = mesa([{ id: 1 }, { id: 2 }], { emCombate: true, rodada: 1, idParticipanteVez: 1 });
  const vez2 = { ...vez1, idParticipanteVez: 2 };
  expect(combateComecou(fora, vez1)).toBe(true);
  expect(combateComecou(vez1, vez2)).toBe(false);
  expect(novaVez(fora, vez1)?.id).toBe(1);
  expect(novaVez(vez1, vez2)?.id).toBe(2);
  expect(novaVez(vez2, { ...vez2 })).toBeNull();
  expect(novaVez(vez2, { ...vez2, rodada: 2 })?.id).toBe(2);
  expect(novaVez(vez2, { ...vez2, emCombate: false })).toBeNull();
});

test('células recém-reveladas, com amostra quando são muitas', () => {
  expect(celulasRecemReveladas(new Set(['1,1']), new Set(['1,1', '2,1', '3,1']))).toEqual(['2,1', '3,1']);
  const muitas = new Set(Array.from({ length: 1000 }, (_, i) => `${i},0`));
  expect(celulasRecemReveladas(new Set(), muitas, 50)).toHaveLength(50);
});

test('crítico e falha só com um d20 que conta', () => {
  expect(criticoDaRolagem('1d20+5', '1d20+5 → (20) + 5 = 25')).toBe('critico');
  expect(criticoDaRolagem('1d20', '1d20 → (1) = 1')).toBe('falha');
  expect(criticoDaRolagem('2d20kh1+3', '2d20kh1+3 → ([4], 20) + 3 = 23')).toBe('critico');
  expect(criticoDaRolagem('2d20kl1', '2d20kl1 → (1, [17]) = 1')).toBe('falha');
  expect(criticoDaRolagem('1d20', '1d20 → (12) = 12')).toBeNull();
  expect(criticoDaRolagem('2d20', '2d20 → (20, 20) = 40')).toBeNull();
  expect(criticoDaRolagem('1d12', '1d12 → (1) = 1')).toBeNull();
  expect(criticoDaRolagem('1d20+1d4', '1d20+1d4 → (20) + (2) = 22')).toBeNull();
});

test('limitarTaxa manda no máximo um por intervalo e não perde o último valor', () => {
  let agora = 0;
  const agendados = [];
  const chamadas = [];
  const limitado = limitarTaxa((v) => chamadas.push(v), 100, () => agora,
    (f, ms) => { agendados.push({ f, quando: agora + ms }); return agendados.length; }, vi.fn());
  limitado.chamar('a');
  limitado.chamar('b');
  agora = 50;
  limitado.chamar('c');
  expect(chamadas).toEqual(['a']);
  agora = 100;
  agendados[0].f();
  expect(chamadas).toEqual(['a', 'c']);
  limitado.chamar('d');
  limitado.cancelar();
  expect(chamadas).toEqual(['a', 'c']);
});

test('a semente é estável e fica entre 0 e 1', () => {
  expect(sementeDe('3,4')).toBe(sementeDe('3,4'));
  expect(sementeDe('3,4')).not.toBe(sementeDe('4,3'));
  expect(sementeDe('x')).toBeGreaterThanOrEqual(0);
  expect(sementeDe('x')).toBeLessThan(1);
});

test('o sinal do fluxo vem em JSON; texto estragado é ignorado', async () => {
  const { lerSinal } = await import('./eventos.js');
  expect(lerSinal('{"tipo":"ping","coluna":2,"linha":3}')).toMatchObject({ tipo: 'ping', coluna: 2 });
  expect(lerSinal('não é json')).toBeNull();
  expect(lerSinal('{"coluna":2}')).toBeNull();
});
