import { expect, test } from 'vitest';
import { rotuloTipo } from './tipos.js';

const criatura = { id: 3, nome: 'Criatura' };
const npc = { id: 1, nome: 'NPC' };

test('Criatura no Daggerheart vira Adversário', () => expect(rotuloTipo(criatura, 2)).toBe('Adversário'));
test('Criatura no D&D continua Criatura', () => expect(rotuloTipo(criatura, 1)).toBe('Criatura'));
test('NPC no Daggerheart continua NPC', () => expect(rotuloTipo(npc, 2)).toBe('NPC'));
