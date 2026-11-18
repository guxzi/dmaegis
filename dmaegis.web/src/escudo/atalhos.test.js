import { expect, test } from 'vitest';
import { ehAtalhoBusca, formatarHora, proximoIndice, sessaoPadrao } from './atalhos.js';

test('Ctrl+K e ⌘K abrem a busca; sem modificador ou outra letra, não', () => {
  expect(ehAtalhoBusca({ key: 'k', ctrlKey: true })).toBe(true);
  expect(ehAtalhoBusca({ key: 'K', metaKey: true })).toBe(true);
  expect(ehAtalhoBusca({ key: 'k' })).toBe(false);
  expect(ehAtalhoBusca({ key: 'j', ctrlKey: true })).toBe(false);
});

test('setas giram pela lista', () => {
  expect(proximoIndice(-1, 'ArrowDown', 3)).toBe(0);
  expect(proximoIndice(2, 'ArrowDown', 3)).toBe(0);
  expect(proximoIndice(0, 'ArrowUp', 3)).toBe(2);
  expect(proximoIndice(1, 'ArrowUp', 3)).toBe(0);
  expect(proximoIndice(1, 'Enter', 3)).toBe(1);
});

test('lista vazia → -1', () => {
  expect(proximoIndice(0, 'ArrowDown', 0)).toBe(-1);
});

test('formatarHora mostra HH:mm, com ou sem fuso', () => {
  expect(formatarHora('2026-11-18T20:14:00')).toBe('20:14');
  expect(formatarHora('2026-11-18T09:05:59')).toBe('09:05');
});

test('sessão padrão é a de maior número', () => {
  expect(sessaoPadrao([{ id: 7, numero: 2 }, { id: 1, numero: 1 }, { id: 9, numero: 3 }])).toBe(9);
  expect(sessaoPadrao([])).toBeNull();
});
