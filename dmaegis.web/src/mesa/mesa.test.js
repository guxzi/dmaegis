import { expect, test } from 'vitest';
import { aplicarPv, corpoDaFicha, estadoSaude, fracaoPv, iniciativasParaComecar, rolarIniciativa } from './combate.js';
import { condicoesDoSistema } from './condicoes.js';
import { celulaDoPonto, celulaLivrePerto, enquadrar, pontoDaCelula, zoomNoPonto } from './grade.js';

const grade = { tamanhoCelula: 50, deslocamentoX: 10, deslocamentoY: 0 };

test('o ponto cai na célula certa, considerando o deslocamento da grade', () => {
  expect(celulaDoPonto(35, 25, grade)).toEqual({ coluna: 0, linha: 0 });
  expect(celulaDoPonto(140, 120, grade)).toEqual({ coluna: 2, linha: 2 });
  expect(celulaDoPonto(-30, -30, grade)).toEqual({ coluna: 0, linha: 0 });
});

test('ficha grande (2×2) encaixa pelo centro', () => {
  // Centro em (110, 100) = encontro das células 1 e 2 → canto na célula (1, 1).
  expect(celulaDoPonto(110, 100, grade, 2)).toEqual({ coluna: 1, linha: 1 });
});

test('o canto da célula volta para pixels', () => {
  expect(pontoDaCelula(2, 3, grade)).toEqual({ x: 110, y: 150 });
});

test('zoom em direção ao cursor mantém o ponto sob o cursor e respeita os limites', () => {
  const novo = zoomNoPonto({ x: 0, y: 0, escala: 1 }, 2, { x: 100, y: 50 });
  expect(novo).toEqual({ escala: 2, x: -100, y: -50 });
  // O ponto do mapa sob o cursor: (100 - x) / escala continua 100.
  expect((100 - novo.x) / novo.escala).toBe(100);
  expect(zoomNoPonto({ x: 0, y: 0, escala: 1 }, 99, { x: 0, y: 0 }).escala).toBe(4);
  expect(zoomNoPonto({ x: 0, y: 0, escala: 1 }, 0.01, { x: 0, y: 0 }).escala).toBe(0.2);
});

test('enquadrar centraliza o mapa inteiro na tela', () => {
  const vista = enquadrar(2000, 1000, 1048, 548, 24);
  expect(vista.escala).toBe(0.5);
  expect(vista).toEqual({ escala: 0.5, x: 24, y: 24 });
  expect(enquadrar(0, 0, 100, 100)).toEqual({ x: 0, y: 0, escala: 1 });
});

test('procura uma célula livre perto, em espiral', () => {
  expect(celulaLivrePerto(2, 2, new Set())).toEqual({ coluna: 2, linha: 2 });
  const livre = celulaLivrePerto(0, 0, new Set(['0,0']));
  expect(['1,0', '0,1', '1,1']).toContain(`${livre.coluna},${livre.linha}`);
});

test('dano e cura ficam entre 0 e o máximo', () => {
  expect(aplicarPv(10, 12, 4, 'dano')).toBe(6);
  expect(aplicarPv(3, 12, 10, 'dano')).toBe(0);
  expect(aplicarPv(10, 12, 7, 'cura')).toBe(12);
  expect(aplicarPv(5, null, 8, 'cura')).toBe(13);
  expect(aplicarPv(10, 12, '-4', 'dano')).toBe(6);
  expect(aplicarPv(10, 12, 'abc', 'dano')).toBe(10);
});

test('saúde em palavras e fração da barra', () => {
  expect([estadoSaude(10, 10), estadoSaude(9, 10), estadoSaude(5, 10), estadoSaude(0, 10), estadoSaude(5, null)])
    .toEqual(['Ileso', 'Ferido', 'Sangrando', 'Caído', null]);
  expect(fracaoPv(5, 10)).toBe(0.5);
  expect(fracaoPv(15, 10)).toBe(1);
  expect(fracaoPv(5, null)).toBeNull();
});

test('ao começar o combate, monstros rolam d20 + bônus e PJs sem número esperam o mestre', () => {
  const sortear = () => 11;
  expect(rolarIniciativa(3, sortear)).toBe(14);
  expect(iniciativasParaComecar([
    { id: 1, jogador: true, iniciativa: null },
    { id: 2, jogador: false, iniciativa: null, bonusIniciativa: 2 },
    { id: 3, jogador: false, iniciativa: 7 },
  ], sortear)).toEqual([
    { idParticipante: 1, iniciativa: null },
    { idParticipante: 2, iniciativa: 13 },
    { idParticipante: 3, iniciativa: 7 },
  ]);
});

test('o corpo do PUT leva a ficha com as mudanças e valores padrão', () => {
  expect(corpoDaFicha({ id: 4, nome: 'Goblin', pvAtual: 7, pvMaximo: 7, condicoes: ['Caído'] }, { pvAtual: 3 })).toEqual({
    nome: 'Goblin', iniciativa: null, bonusIniciativa: 0, ca: null, pvAtual: 3, pvMaximo: 7,
    tamanho: 1, visivel: true, ordem: 0, condicoes: ['Caído'],
  });
});

test('cada sistema tem as próprias condições', () => {
  const dnd = condicoesDoSistema(1).map((c) => c.nome);
  expect(dnd).toHaveLength(15);
  expect(dnd).toContain('Caído');
  const dh = condicoesDoSistema(2).map((c) => c.nome);
  expect(dh).toEqual(['Contido', 'Oculto', 'Vulnerável']);
  expect(dh).not.toContain('Caído');
});

test('o link de convite usa o endereço da mesa sem barra sobrando', async () => {
  const { linkDeConvite } = await import('./eventos.js');
  expect(linkDeConvite('https://abc.trycloudflare.com/', 'x_y-1')).toBe('https://abc.trycloudflare.com/convite/x_y-1');
  expect(linkDeConvite(' http://localhost:5080 ', 'c')).toBe('http://localhost:5080/convite/c');
});
