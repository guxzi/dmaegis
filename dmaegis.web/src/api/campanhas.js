import { api } from './cliente.js';

const dados = (resposta) => resposta.data.data;

export const listarSistemas = () => api.get('/sistemas').then(dados);
export const listarCampanhas = () => api.get('/campanhas').then(dados);
export const obterCampanha = (id) => api.get(`/campanhas/${id}`).then(dados);

export const criarCampanha = ({ idSistema, nome, descricao }) =>
  api.post('/campanhas', { idSistema, nome, descricao }).then(dados);

// O sistema não vai no corpo: não pode ser trocado depois de criada a campanha.
export const atualizarCampanha = (id, { nome, descricao }) =>
  api.put(`/campanhas/${id}`, { nome, descricao }).then(dados);

export const arquivarCampanha = (id) => api.post(`/campanhas/${id}/arquivar`).then(dados);
export const desarquivarCampanha = (id) => api.post(`/campanhas/${id}/desarquivar`).then(dados);
export const excluirCampanha = (id) => api.delete(`/campanhas/${id}`).then(dados);
