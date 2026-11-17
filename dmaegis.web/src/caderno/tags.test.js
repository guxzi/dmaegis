import { expect, test } from 'vitest';
import { separarTags } from './tags.js';

test('separa por vírgula, apara e remove repetidas sem acento nem maiúscula', () =>
  expect(separarTags('Vila, culto ,, vila, Víla')).toEqual(['Vila', 'culto']));

test('texto vazio', () => expect(separarTags('')).toEqual([]));
