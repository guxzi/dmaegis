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
