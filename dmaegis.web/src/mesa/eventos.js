// Marca desta aba: vai junto dos sinais que ela manda, para ignorar o próprio sinal quando ele voltar.
export const ID_TELA = Math.random().toString(36).slice(2, 12);

// Ouve os avisos da mesa (Server-Sent Events). O aviso "mesa" só diz "mudou": a tela busca o estado de novo.
// O aviso "sinal" traz um efeito passageiro (ping, reação, arrasto ao vivo) para `aoSinal`.
// O EventSource se reconecta sozinho; ao reconectar, chega um "pronto" e a tela também busca de novo.
// `aoMudarConexao(true|false)` mostra o aviso de "reconectando…".
export function ouvirMesa(url, aoMudar, aoMudarConexao = () => {}, aoSinal = () => {}) {
  if (typeof EventSource === 'undefined') return () => {};
  const fonte = new EventSource(url, { withCredentials: true });
  let espera = null;
  // Vários avisos em sequência (arrastar, PV, turno) viram uma busca só.
  const avisar = () => {
    clearTimeout(espera);
    espera = setTimeout(aoMudar, 120);
  };
  fonte.addEventListener('mesa', avisar);
  fonte.addEventListener('sinal', (evento) => {
    const sinal = lerSinal(evento.data);
    if (sinal && sinal.origem !== ID_TELA) aoSinal(sinal);
  });
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

// O sinal chega como texto JSON; um texto estragado é ignorado.
export function lerSinal(texto) {
  try {
    const sinal = JSON.parse(texto);
    return sinal && typeof sinal.tipo === 'string' ? sinal : null;
  } catch {
    return null;
  }
}
