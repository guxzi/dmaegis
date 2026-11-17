import { expect, test } from 'vitest';
import { mensagemDeErro } from './erros.js';

test('usa a mensagem do envelope', () =>
  expect(mensagemDeErro({ response: { status: 401, data: { errorMessage: 'E-mail ou senha inválidos.' } } }))
    .toBe('E-mail ou senha inválidos.'));

test('sem resposta = servidor fora', () =>
  expect(mensagemDeErro({ request: {} })).toBe('Não foi possível falar com o servidor.'));

test('429 = muitas tentativas', () =>
  expect(mensagemDeErro({ response: { status: 429, data: {} } }))
    .toBe('Muitas tentativas. Aguarde um minuto e tente de novo.'));

test('outro erro = genérico', () =>
  expect(mensagemDeErro({ response: { status: 500, data: {} } })).toBe('Algo deu errado. Tente de novo.'));
