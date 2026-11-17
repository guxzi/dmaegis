const mesmaChave = (a, b) => a.trim().localeCompare(b.trim(), 'pt-BR', { sensitivity: 'base' }) === 0;

// Monta as linhas do formulário: primeiro os campos do modelo do tipo (na ordem do modelo,
// com o valor já existente de mesma chave), depois os atributos livres.
// Ao trocar de tipo, um campo do modelo antigo que tinha valor vira livre; vazio some.
export function mesclarAtributos(modelo, existentes) {
  const doModelo = [...modelo]
    .sort((a, b) => a.ordem - b.ordem)
    .map((m) => ({
      chave: m.chave,
      valor: existentes.find((e) => mesmaChave(e.chave, m.chave))?.valor ?? '',
      doModelo: true,
    }));

  const livres = existentes
    .filter((e) => !modelo.some((m) => mesmaChave(m.chave, e.chave)))
    .filter((e) => !e.doModelo || e.valor.trim() !== '')
    .map((e) => ({ chave: e.chave, valor: e.valor, doModelo: false }));

  return [...doModelo, ...livres];
}

// O que vai para a API: chave e valor aparados, sem os de valor vazio.
export function atributosParaEnvio(linhas) {
  return linhas
    .map((l) => ({ chave: l.chave.trim(), valor: l.valor.trim() }))
    .filter((l) => l.valor !== '');
}
