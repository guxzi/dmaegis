const mesmaTag = (a, b) => a.localeCompare(b, 'pt-BR', { sensitivity: 'base' }) === 0;

// "Vila, culto ,, vila" → ["Vila", "culto"]: separa por vírgula, apara e remove repetidas
// (sem diferenciar maiúsculas nem acentos, como o banco), mantendo a primeira grafia.
export function separarTags(texto) {
  const resultado = [];
  for (const tag of texto.split(',').map((t) => t.trim()).filter(Boolean)) {
    if (!resultado.some((existente) => mesmaTag(existente, tag))) resultado.push(tag);
  }
  return resultado;
}
