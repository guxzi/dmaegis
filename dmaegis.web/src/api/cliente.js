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

// Instância das rotas de jogador (quem entra pelo link de convite). O token CSRF do jogador vem num cookie
// próprio (XSRF-TOKEN-JOGADOR), emitido com a identidade do cookie de jogador: assim o mestre e o jogador
// no mesmo navegador não sobrescrevem o token um do outro.
export const apiJogador = axios.create({
  baseURL: '/api',
  withCredentials: true,
  xsrfCookieName: 'XSRF-TOKEN-JOGADOR',
  xsrfHeaderName: 'X-XSRF-TOKEN',
});
