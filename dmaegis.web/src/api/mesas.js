import { api } from './cliente.js';

const dados = (resposta) => resposta.data.data;

export const listarMesas = (idCampanha) => api.get(`/campanhas/${idCampanha}/mesas`).then(dados);
export const criarMesa = (idCampanha, mesa) => api.post(`/campanhas/${idCampanha}/mesas`, mesa).then(dados);
export const obterMesa = (id) => api.get(`/mesas/${id}`).then(dados);
export const salvarMesa = (id, mesa) => api.put(`/mesas/${id}`, mesa).then(dados);
export const excluirMesa = (id) => api.delete(`/mesas/${id}`).then(dados);
export const ativarMesa = (id) => api.put(`/mesas/${id}/ativa`).then(dados);

// { todosJogadores } | { idRegistro, quantidade, nome? } | { nome, quantidade }
export const adicionarFichas = (idMesa, pedido) => api.post(`/mesas/${idMesa}/participantes`, pedido).then(dados);
export const salvarFicha = (id, ficha) => api.put(`/participantes/${id}`, ficha).then(dados);
export const moverFicha = (id, coluna, linha) => api.put(`/participantes/${id}/posicao`, { coluna, linha }).then(dados);
export const excluirFicha = (id) => api.delete(`/participantes/${id}`).then(dados);

export const iniciarCombate = (idMesa, iniciativas) => api.post(`/mesas/${idMesa}/combate`, { iniciativas }).then(dados);
export const proximoTurno = (idMesa) => api.post(`/mesas/${idMesa}/combate/proximo`).then(dados);
export const encerrarCombate = (idMesa) => api.delete(`/mesas/${idMesa}/combate`).then(dados);

export const obterVisaoJogador = (idMesa) => api.get(`/mesas/${idMesa}/visao-jogador`).then(dados);

// Névoa: o que o pincel pintou de uma vez ({ coluna, linha }[]), e "esconder tudo".
export const alterarNevoa = (idMesa, revelar, esconder) => api.put(`/mesas/${idMesa}/nevoa`, { revelar, esconder }).then(dados);
export const esconderTudo = (idMesa) => api.delete(`/mesas/${idMesa}/nevoa`).then(dados);
