import { expect, test } from 'vitest';
import { consultarOraculo, gerarNome, gerarNpc, registroDoNpc } from './improviso.js';

const sempre = (valor) => () => valor;

test('o oráculo soma a chance ao d6 e fica entre "Não, e…" e "Sim, e…"', () => {
  expect(consultarOraculo('meio', sempre(4)).resposta).toBe('Sim, mas…');
  expect(consultarOraculo('provavel', sempre(4)).resposta).toBe('Sim');
  expect(consultarOraculo('improvavel', sempre(1)).resposta).toBe('Não, e…');
  expect(consultarOraculo('provavel', sempre(6)).resposta).toBe('Sim, e…');
  expect(consultarOraculo('improvavel', sempre(4))).toMatchObject({ dado: 4, valor: 3, resposta: 'Não, mas…' });
});

test('o NPC rápido usa a fonte de sorte para cada parte', () => {
  const npc = gerarNpc(sempre(1));
  expect(npc).toEqual({
    nome: 'Ala da Ponte Velha',
    oficio: 'taverneiro',
    traco: 'desconfiado com estranhos',
    aparencia: 'cicatriz na sobrancelha',
    desejo: 'pagar uma dívida antiga',
    mania: 'tamborila os dedos na mesa',
  });
  expect(gerarNome(sempre(1))).toBe('Ala da Ponte Velha');
});

test('nomes variam com dados diferentes', () => {
  let i = 0;
  const sequencia = (n) => ((i++ * 7) % n) + 1;
  const nomes = new Set(Array.from({ length: 10 }, () => gerarNome(sequencia)));
  expect(nomes.size).toBeGreaterThan(5);
});

test('o NPC vira um registro de NPC para o caderno, com a tag "improvisado"', () => {
  const registro = registroDoNpc(gerarNpc(sempre(1)));
  expect(registro).toMatchObject({ idTipoRegistro: 1, nome: 'Ala da Ponte Velha', resumo: 'Taverneiro, desconfiado com estranhos.', tags: ['improvisado'] });
  expect(registro.descricao).toContain('Quer: pagar uma dívida antiga.');
});
