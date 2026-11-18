import { api } from './cliente.js';

const dados = (resposta) => resposta.data.data;

export const buscar = (idCampanha, termo) =>
  api.get(`/campanhas/${idCampanha}/busca`, { params: { q: termo } }).then(dados);

export const listarAnotacoes = (idSessao) => api.get(`/sessoes/${idSessao}/anotacoes`).then(dados);
export const criarAnotacao = (idSessao, texto) => api.post(`/sessoes/${idSessao}/anotacoes`, { texto }).then(dados);
export const excluirAnotacao = (id) => api.delete(`/anotacoes/${id}`).then(dados);
