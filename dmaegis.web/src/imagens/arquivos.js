export const TAMANHO_MAXIMO = 15 * 1024 * 1024;
const TIPOS = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];

// "Prisao do Porto - DM Version.jpg" → "Prisao do Porto DM Version"
export function tituloDoArquivo(nome) {
  return nome
    .replace(/\.[^.]+$/, '')
    .replace(/[_-]+/g, ' ')
    .replace(/\s+/g, ' ')
    .trim();
}

// Aviso antes de enviar (a API valida de novo pelos bytes do arquivo).
export function validarArquivo(arquivo) {
  if (!TIPOS.includes(arquivo.type)) return 'Envie uma imagem JPEG, PNG, WebP ou GIF.';
  if (arquivo.size > TAMANHO_MAXIMO) return 'A imagem pode ter até 15 MB.';
  return null;
}

export function formatarTamanho(bytes) {
  if (bytes < 1024 * 1024) return `${Math.max(1, Math.round(bytes / 1024))} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1).replace('.', ',')} MB`;
}

const EXTENSAO = { 'image/jpeg': 'jpg', 'image/png': 'png', 'image/webp': 'webp', 'image/gif': 'gif' };

// Nome para o "Baixar": sem os caracteres que o Windows recusa, com a extensão do tipo real.
export function nomeArquivoDownload(titulo, tipo) {
  const limpo = (titulo ?? '').replace(/[\/:*?"<>|]+/g, ' ').replace(/\s+/g, ' ').trim();
  return `${limpo || 'imagem'}.${EXTENSAO[tipo] ?? 'img'}`;
}

// Tamanho da imagem copiada: no máximo 2048 px no lado maior, para caber no chat dos jogadores
// (um mapa de 5120 px viraria um PNG de uns 30 MB). Imagens menores ficam como estão.
export function dimensoesParaCopiar(largura, altura, maximo = 2048) {
  const escala = Math.min(1, maximo / Math.max(largura, altura));
  return { largura: Math.round(largura * escala), altura: Math.round(altura * escala) };
}
