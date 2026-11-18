const dobrar = (texto) => (texto ?? '').normalize('NFD').replace(/\p{M}/gu, '').toLowerCase();

// Regras cujo nome, termo em inglês ou texto contém o termo (sem acento nem maiúscula).
export function filtrarRegras(lista, termo) {
  const procurado = dobrar(termo).trim();
  if (!procurado) return lista;
  return lista.filter((regra) => [regra.nome, regra.termo, regra.texto].some((campo) => dobrar(campo).includes(procurado)));
}
