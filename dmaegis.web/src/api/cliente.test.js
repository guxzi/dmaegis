import { expect, test } from 'vitest';
import { api } from './cliente.js';

test('cliente usa /api, envia cookies e o cabeçalho CSRF', () => {
  expect(api.defaults.baseURL).toBe('/api');
  expect(api.defaults.withCredentials).toBe(true);
  expect(api.defaults.xsrfCookieName).toBe('XSRF-TOKEN');
  expect(api.defaults.xsrfHeaderName).toBe('X-XSRF-TOKEN');
});
