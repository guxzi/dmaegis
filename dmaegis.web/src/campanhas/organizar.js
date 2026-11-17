const porNome = (a, b) => a.nome.localeCompare(b.nome, 'pt-BR', { sensitivity: 'base' });

// Divide as campanhas em ativas e arquivadas, cada grupo em ordem alfabética
// (ignorando acentos e maiúsculas). Não altera a lista recebida.
export function separarCampanhas(lista) {
  return {
    ativas: lista.filter((c) => !c.arquivada).sort(porNome),
    arquivadas: lista.filter((c) => c.arquivada).sort(porNome),
  };
}
