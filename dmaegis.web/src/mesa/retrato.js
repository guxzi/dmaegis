// Retrato enviado pelo jogador: o navegador reduz a imagem antes de mandar (fica leve no banco e na mesa).

export const LADO_MAXIMO_RETRATO = 512;

// Largura e altura que cabem num quadrado de `maximo` px, mantendo a proporção (nunca aumenta).
export function dimensoesReduzidas(largura, altura, maximo = LADO_MAXIMO_RETRATO) {
  if (!largura || !altura) return { largura: 0, altura: 0 };
  const fator = Math.min(1, maximo / Math.max(largura, altura));
  return { largura: Math.round(largura * fator), altura: Math.round(altura * fator) };
}

// Lê o arquivo, desenha reduzido num canvas e devolve um Blob JPEG. GIF animado vira a primeira imagem.
export async function reduzirRetrato(arquivo, maximo = LADO_MAXIMO_RETRATO) {
  const bitmap = await createImageBitmap(arquivo);
  const { largura, altura } = dimensoesReduzidas(bitmap.width, bitmap.height, maximo);
  const canvas = document.createElement('canvas');
  canvas.width = largura;
  canvas.height = altura;
  canvas.getContext('2d').drawImage(bitmap, 0, 0, largura, altura);
  bitmap.close?.();
  return new Promise((resolver, rejeitar) => {
    canvas.toBlob((blob) => (blob ? resolver(blob) : rejeitar(new Error('Não deu para ler a imagem.'))), 'image/jpeg', 0.88);
  });
}
