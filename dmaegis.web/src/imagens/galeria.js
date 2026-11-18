const dobrar = (texto) => (texto ?? '').normalize('NFD').replace(/\p{M}/gu, '').toLowerCase().trim();

// Grupos { idRegistro, nomeRegistro, idTipoRegistro, imagens } na ordem em que a API mandou.
// O termo procura no nome do registro (traz todas as imagens dele) ou no título de cada imagem.
export function agruparPorRegistro(imagens, { idTipo = '', termo = '' } = {}) {
  const procurado = dobrar(termo);
  const grupos = new Map();
  for (const imagem of imagens) {
    if (idTipo && imagem.idTipoRegistro !== Number(idTipo)) continue;
    const casaNome = dobrar(imagem.nomeRegistro).includes(procurado);
    if (procurado && !casaNome && !dobrar(imagem.titulo).includes(procurado)) continue;
    if (!grupos.has(imagem.idRegistro)) {
      grupos.set(imagem.idRegistro, {
        idRegistro: imagem.idRegistro,
        nomeRegistro: imagem.nomeRegistro,
        idTipoRegistro: imagem.idTipoRegistro,
        imagens: [],
      });
    }
    grupos.get(imagem.idRegistro).imagens.push(imagem);
  }
  return [...grupos.values()];
}

// O registro com esse nome (sem diferenciar maiúsculas nem acentos), ou undefined.
export function acharRegistroPeloNome(registros, nome) {
  const procurado = dobrar(nome);
  if (!procurado) return undefined;
  return registros.find((r) => dobrar(r.nome) === procurado);
}
