import { api } from './cliente.js';

const dados = (resposta) => resposta.data.data;

// Entregas aos jogadores (handouts), do lado do mestre.
export const listarEntregas = (idCampanha) => api.get(`/campanhas/${idCampanha}/entregas`).then(dados);
// { titulo, texto?, idImagemRegistro?, idRegistroDestino? } — destino nulo = todos os jogadores.
export const criarEntrega = (idCampanha, entrega) => api.post(`/campanhas/${idCampanha}/entregas`, entrega).then(dados);
export const excluirEntrega = (id) => api.delete(`/entregas/${id}`).then(dados);
