import { api } from './cliente.js';

// O token CSRF é ligado ao usuário: depois de entrar ou sair, é preciso pedir um novo.
export async function renovarCsrf() {
  await api.get('/auth/csrf');
}

export async function cadastrar({ nome, email, senha }) {
  const resposta = await api.post('/auth/cadastro', { nome, email, senha });
  await renovarCsrf();
  return resposta.data.data;
}

export async function entrar({ email, senha }) {
  const resposta = await api.post('/auth/login', { email, senha });
  await renovarCsrf();
  return resposta.data.data;
}

export async function sair() {
  await api.post('/auth/logout');
  await renovarCsrf();
}

// Usuário logado, ou null se não houver sessão.
export async function obterUsuarioAtual() {
  try {
    const resposta = await api.get('/auth/eu');
    return resposta.data.data;
  } catch (erro) {
    if (erro.response?.status === 401) return null;
    throw erro;
  }
}
