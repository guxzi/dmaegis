import axios from 'axios';

// Única instância usada para falar com a API.
// - baseURL '/api': em desenvolvimento o Vite repassa para http://localhost:5080 (mesma origem).
// - withCredentials: envia o cookie de sessão (HttpOnly).
// - xsrf*: o Axios lê o cookie XSRF-TOKEN e devolve o valor no cabeçalho X-XSRF-TOKEN (proteção CSRF).
export const api = axios.create({
  baseURL: '/api',
  withCredentials: true,
  xsrfCookieName: 'XSRF-TOKEN',
  xsrfHeaderName: 'X-XSRF-TOKEN',
});
