import { api } from './cliente.js';

const dados = (resposta) => resposta.data.data;

export const listarTipos = () => api.get('/tipos-registro').then(dados);

export const listarModelos = (idSistema, idTipoRegistro) =>
  api.get(`/sistemas/${idSistema}/modelos-atributo`, { params: { idTipoRegistro } }).then(dados);

export const listarTags = (idCampanha) => api.get(`/campanhas/${idCampanha}/tags`).then(dados);

// Filtros vazios não vão na URL.
export const listarRegistros = (idCampanha, { idTipoRegistro, idTag, texto } = {}) =>
  api.get(`/campanhas/${idCampanha}/registros`, {
    params: {
      idTipoRegistro: idTipoRegistro || undefined,
      idTag: idTag || undefined,
      texto: texto?.trim() || undefined,
    },
  }).then(dados);

export const obterRegistro = (id) => api.get(`/registros/${id}`).then(dados);
export const criarRegistro = (idCampanha, registro) => api.post(`/campanhas/${idCampanha}/registros`, registro).then(dados);
export const atualizarRegistro = (id, registro) => api.put(`/registros/${id}`, registro).then(dados);
export const excluirRegistro = (id) => api.delete(`/registros/${id}`).then(dados);
