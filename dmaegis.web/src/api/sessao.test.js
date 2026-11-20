import { expect, test, vi } from 'vitest';
import { criarTratadorDe401, semConexao } from './sessao.js';

const erro = (status, url) => ({ response: { status }, config: { url } });

test('401 fora de /auth chama aoExpirar e repassa o erro', async () => {
  const aoExpirar = vi.fn().mockResolvedValue();
  const tratar = criarTratadorDe401(aoExpirar);
  const e = erro(401, '/campanhas');

  await expect(tratar(e)).rejects.toBe(e);
  expect(aoExpirar).toHaveBeenCalledTimes(1);
});

test('401 de /auth (login errado, /eu) não chama aoExpirar', async () => {
  const aoExpirar = vi.fn();
  const tratar = criarTratadorDe401(aoExpirar);

  await expect(tratar(erro(401, '/auth/login'))).rejects.toBeTruthy();
  await expect(tratar(erro(401, '/auth/eu'))).rejects.toBeTruthy();
  expect(aoExpirar).not.toHaveBeenCalled();
});

test('outros status não chamam aoExpirar', async () => {
  const aoExpirar = vi.fn();
  await expect(criarTratadorDe401(aoExpirar)(erro(500, '/campanhas'))).rejects.toBeTruthy();
  expect(aoExpirar).not.toHaveBeenCalled();
});

test('se renovar a sessão falhar, o erro original ainda é repassado', async () => {
  const aoExpirar = vi.fn().mockRejectedValue(new Error('rede'));
  const e = erro(401, '/campanhas');
  await expect(criarTratadorDe401(aoExpirar)(e)).rejects.toBe(e);
});

test('sem resposta ou erro 5xx é falta de conexão; 401 e 404 não são', () => {
  expect(semConexao({ message: 'Network Error' })).toBe(true);
  expect(semConexao({ response: { status: 502 } })).toBe(true);
  expect(semConexao({ response: { status: 500 } })).toBe(true);
  expect(semConexao({ response: { status: 401 } })).toBe(false);
  expect(semConexao({ response: { status: 404 } })).toBe(false);
  expect(semConexao(null)).toBe(false);
});
