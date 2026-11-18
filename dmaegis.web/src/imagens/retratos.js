// { idRegistro: idImagem }: a primeira imagem de cada registro (a API já manda em ordem) é o retrato.
export function retratosPorRegistro(imagens) {
  const retratos = {};
  for (const imagem of imagens) {
    if (!(imagem.idRegistro in retratos)) retratos[imagem.idRegistro] = imagem.id;
  }
  return retratos;
}
