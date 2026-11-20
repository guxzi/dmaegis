import { api, apiJogador } from './cliente.js';

const dados = (resposta) => resposta.data.data;

// Lado do jogador (cookie do convite): só a mesa ativa, já filtrada pelo servidor.
// Pedir o token de novo depois de entrar: ele é ligado à identidade do jogador.
export const renovarCsrfJogador = () => apiJogador.get('/jogador/csrf');
export const entrarComConvite = (codigo) => apiJogador.post('/jogador/entrar', { codigo }).then(dados);
export const obterJogador = () => apiJogador.get('/jogador/eu').then(dados);
export const sairDaMesa = () => apiJogador.post('/jogador/sair').then(dados);
export const obterMesaDoJogador = () => apiJogador.get('/jogador/mesa').then(dados);
export const moverMinhaFicha = (coluna, linha) => apiJogador.put('/jogador/ficha/posicao', { coluna, linha }).then(dados);
export const listarRolagensDoJogador = () => apiJogador.get('/jogador/rolagens').then(dados);
export const enviarRolagemDoJogador = (rolagem) => apiJogador.post('/jogador/rolagens', rolagem).then(dados);
export const enviarSinalDoJogador = (sinal) => apiJogador.post('/jogador/sinais', sinal).then(dados);
export const urlImagemDoJogador = (id) => `/api/jogador/imagens/${id}`;
export const URL_EVENTOS_JOGADOR = '/api/jogador/eventos';

// Lado do mestre: os links de convite dos PJs.
export const listarConvites = (idCampanha) => api.get(`/campanhas/${idCampanha}/convites`).then(dados);
export const gerarConvite = (idRegistro) => api.post(`/registros/${idRegistro}/convite`).then(dados);
export const revogarConvite = (idRegistro) => api.delete(`/registros/${idRegistro}/convite`).then(dados);
export const urlEventosDaMesa = (idCampanha) => `/api/campanhas/${idCampanha}/mesa/eventos`;
