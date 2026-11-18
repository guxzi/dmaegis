// Tira acento e maiúscula letra por letra, para que o texto dobrado tenha o mesmo tamanho do original
// (assim os índices de um valem no outro).
function dobrar(texto) {
  let resultado = '';
  for (const letra of texto) {
    const simples = letra.normalize('NFD').replace(/\p{M}/gu, '').toLowerCase();
    resultado += simples.length === letra.length ? simples : letra;
  }
  return resultado;
}

const ehLetra = (c) => c !== undefined && /[\p{L}\p{N}]/u.test(c);

// Divide o texto em pedaços; os nomes de registros viram { texto, registro }.
// Não diferencia maiúsculas nem acentos, o nome mais longo vence e só vale a palavra inteira.
export function marcarMencoes(texto, registros) {
  if (!texto) return [];
  const dobrado = dobrar(texto);
  const candidatos = registros
    .map((registro) => ({ registro, nome: dobrar(registro.nome.trim()) }))
    .filter((c) => c.nome.length >= 2)
    .sort((a, b) => b.nome.length - a.nome.length);

  const pedacos = [];
  let inicioTexto = 0;
  let i = 0;
  while (i < texto.length) {
    const achado = !ehLetra(texto[i - 1]) && candidatos.find((c) =>
      dobrado.startsWith(c.nome, i) && !ehLetra(texto[i + c.nome.length]));
    if (achado) {
      if (i > inicioTexto) pedacos.push({ texto: texto.slice(inicioTexto, i) });
      const fim = i + achado.nome.length;
      pedacos.push({ texto: texto.slice(i, fim), registro: achado.registro });
      i = fim;
      inicioTexto = fim;
    } else {
      i += 1;
    }
  }
  if (inicioTexto < texto.length) pedacos.push({ texto: texto.slice(inicioTexto) });
  return pedacos;
}
