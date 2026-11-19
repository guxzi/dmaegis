// Ouve os avisos da mesa (Server-Sent Events). Cada aviso só diz "mudou": a tela busca o estado de novo.
// O EventSource se reconecta sozinho; ao reconectar, chega um "pronto" e a tela também busca de novo.
// `aoMudarConexao(true|false)` mostra o aviso de "reconectando…".
export function ouvirMesa(url, aoMudar, aoMudarConexao = () => {}) {
  if (typeof EventSource === 'undefined') return () => {};
  const fonte = new EventSource(url, { withCredentials: true });
  let espera = null;
  // Vários avisos em sequência (arrastar, PV, turno) viram uma busca só.
  const avisar = () => {
    clearTimeout(espera);
    espera = setTimeout(aoMudar, 120);
  };
  fonte.addEventListener('mesa', avisar);
  fonte.addEventListener('pronto', () => {
    aoMudarConexao(true);
    avisar();
  });
  fonte.onerror = () => aoMudarConexao(false);
  return () => {
    clearTimeout(espera);
    fonte.close();
  };
}

// Link de convite a partir do endereço da mesa (o do túnel muda a cada sessão).
export function linkDeConvite(endereco, codigo) {
  const base = String(endereco || '').trim().replace(/\/+$/, '');
  return `${base}/convite/${encodeURIComponent(codigo)}`;
}
