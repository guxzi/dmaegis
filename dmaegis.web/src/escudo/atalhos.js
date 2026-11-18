// Ctrl+K (Windows/Linux) ou ⌘K (Mac) abre a busca rápida.
export function ehAtalhoBusca(evento) {
  return Boolean(evento.ctrlKey || evento.metaKey) && evento.key?.toLowerCase() === 'k';
}

// Índice ativo depois de uma tecla: as setas giram pela lista; outras teclas não mudam nada.
export function proximoIndice(atual, tecla, total) {
  if (total === 0) return -1;
  if (tecla === 'ArrowDown') return atual < 0 || atual >= total - 1 ? 0 : atual + 1;
  if (tecla === 'ArrowUp') return atual <= 0 ? total - 1 : atual - 1;
  return atual;
}

// "2026-11-18T20:14:00" → "20:14". Sem fuso, a hora já é a local do servidor; com fuso, converte.
export function formatarHora(dataIso) {
  if (!dataIso) return '';
  const temFuso = /(Z|[+-]\d{2}:\d{2})$/.test(dataIso);
  if (!temFuso) return dataIso.slice(11, 16);
  const data = new Date(dataIso);
  return `${String(data.getHours()).padStart(2, '0')}:${String(data.getMinutes()).padStart(2, '0')}`;
}

// Sessão aberta por padrão no escudo: a de maior número.
export function sessaoPadrao(sessoes) {
  if (sessoes.length === 0) return null;
  return sessoes.reduce((maior, s) => (s.numero > maior.numero ? s : maior)).id;
}
