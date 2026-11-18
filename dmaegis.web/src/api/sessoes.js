import { api } from './cliente.js';

const dados = (resposta) => resposta.data.data;

export const listarSessoes = (idCampanha) => api.get(`/campanhas/${idCampanha}/sessoes`).then(dados);
export const criarSessao = (idCampanha, { numero, data } = {}) =>
  api.post(`/campanhas/${idCampanha}/sessoes`, { numero: numero || null, data: data || null }).then(dados);
export const obterSessao = (id) => api.get(`/sessoes/${id}`).then(dados);
export const salvarSessao = (id, sessao) => api.put(`/sessoes/${id}`, sessao).then(dados);
export const excluirSessao = (id) => api.delete(`/sessoes/${id}`).then(dados);

// Segredos: ações imediatas, fora do "Salvar preparo".
export const criarSegredo = (idSessao, texto) => api.post(`/sessoes/${idSessao}/segredos`, { texto }).then(dados);
export const editarSegredo = (id, texto) => api.put(`/segredos/${id}`, { texto }).then(dados);
export const revelarSegredo = (id, idSessao) => api.post(`/segredos/${id}/revelar`, { idSessao }).then(dados);
export const ocultarSegredo = (id) => api.post(`/segredos/${id}/ocultar`).then(dados);
export const excluirSegredo = (id) => api.delete(`/segredos/${id}`).then(dados);
