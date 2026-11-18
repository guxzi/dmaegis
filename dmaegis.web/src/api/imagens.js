import { api } from './cliente.js';

const dados = (resposta) => resposta.data.data;

export const urlDaImagem = (id) => `/api/imagens/${id}`;
export const listarImagens = (idRegistro) => api.get(`/registros/${idRegistro}/imagens`).then(dados);
export const excluirImagem = (id) => api.delete(`/imagens/${id}`).then(dados);

export function enviarImagem(idRegistro, arquivo, titulo) {
  const formulario = new FormData();
  formulario.append('arquivo', arquivo);
  if (titulo) formulario.append('titulo', titulo);
  return api.post(`/registros/${idRegistro}/imagens`, formulario).then(dados);
}
