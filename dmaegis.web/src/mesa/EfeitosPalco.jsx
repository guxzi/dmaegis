import { pontoDaCelula } from './grade.js';
import { celulaDaChave } from './nevoa.js';
import { sementeDe } from './efeitos.js';

// Efeitos passageiros do palco. Os que ficam presos ao mapa (números, poeira, ping, reação, holofote)
// são desenhados dentro do conteúdo que move e dá zoom; o embrulho de fora posiciona e compensa o zoom
// (--zoom-inverso), e o de dentro anima, para as duas transformações não brigarem.

function centroDaCelula(coluna, linha, mesa) {
  const canto = pontoDaCelula(coluna, linha, mesa);
  return { x: canto.x + mesa.tamanhoCelula / 2, y: canto.y + mesa.tamanhoCelula / 2 };
}

const naPosicao = ({ x, y }) => ({ transform: `translate(${x}px, ${y}px) scale(var(--zoom-inverso, 1))` });

// Luz suave embaixo da ficha da vez. Ela desliza de uma ficha para a outra quando o turno passa.
export function Holofote({ mesa, posicaoDe }) {
  if (!mesa.emCombate) return null;
  const ficha = mesa.participantes.find((p) => p.id === mesa.idParticipanteVez && p.coluna != null);
  if (!ficha) return null;
  const lado = (ficha.tamanho || 1) * mesa.tamanhoCelula;
  const { x, y } = posicaoDe(ficha);
  return (
    <div className="holofote-vez" aria-hidden="true"
      style={{ width: lado * 3, height: lado * 3, transform: `translate(${x - lado}px, ${y - lado}px)` }} />
  );
}

// "−12" em vermelho, "+5" em verde (sem número quando o jogador não vê o PV), e "Caído" quando cai.
export function NumerosFlutuantes({ efeitos, mesa, posicaoDe }) {
  return efeitos.map((efeito) => {
    const ficha = mesa.participantes.find((p) => p.id === efeito.idFicha && p.coluna != null);
    if (!ficha) return null;
    const { x, y } = posicaoDe(ficha);
    const lado = (ficha.tamanho || 1) * mesa.tamanhoCelula;
    const texto = efeito.valor == null
      ? (efeito.tipo === 'dano' ? 'golpe!' : 'cura')
      : `${efeito.tipo === 'dano' ? '−' : '+'}${efeito.valor}`;
    return (
      <div key={efeito.chave} className="efeito-ancora" style={naPosicao({ x: x + lado / 2, y: y + lado * 0.15 })} aria-hidden="true">
        <span className={`numero-flutuante numero-${efeito.tipo}${efeito.valor == null ? ' sem-numero' : ''}`}>{texto}</span>
        {efeito.caiu && <span className="numero-flutuante numero-caiu">Caído</span>}
      </div>
    );
  });
}

// Poeira estelar onde a névoa acabou de se abrir: três faíscas por célula, espalhadas sem sorteio.
export function PoeiraNevoa({ efeitos, mesa }) {
  const cel = mesa.tamanhoCelula;
  return efeitos.flatMap((efeito) => efeito.celulas.flatMap((chave) => {
    const { coluna, linha } = celulaDaChave(chave);
    const canto = pontoDaCelula(coluna, linha, mesa);
    return [0, 1, 2].map((i) => {
      const s = sementeDe(`${chave}:${i}`);
      const t = sementeDe(`${i}:${chave}`);
      return (
        <span key={`${efeito.chave}-${chave}-${i}`} className="poeira-nevoa" aria-hidden="true"
          style={{
            transform: `translate(${canto.x + s * cel}px, ${canto.y + t * cel}px)`,
            '--atraso': `${Math.round(s * 380)}ms`,
            '--deriva': `${Math.round((t - 0.5) * cel * 0.6)}px`,
            '--tamanho': `${Math.max(3, cel * (0.05 + s * 0.05))}px`,
          }} />
      );
    });
  }));
}

// Ping: anéis de luz saindo do ponto, com o nome de quem chamou a atenção.
export function Pings({ pings, mesa }) {
  return pings.map((ping) => {
    const centro = centroDaCelula(ping.coluna, ping.linha, mesa);
    const doMestre = ping.autor === 'Mestre';
    return (
      <div key={ping.chave} className={`efeito-ancora ping-mapa${doMestre ? ' ping-mestre' : ''}`} style={naPosicao(centro)} role="status">
        <span className="ping-anel" />
        <span className="ping-anel ping-anel-2" />
        <span className="ping-anel ping-anel-3" />
        <span className="ping-estrela" aria-hidden="true">✦</span>
        {ping.autor && <span className="ping-autor">{ping.autor}</span>}
      </div>
    );
  });
}

// Reações que saem da ficha de quem reagiu. As de quem não está no mapa vão para `ReacoesSoltas`.
export function ReacoesNaFicha({ reacoes, mesa, posicaoDe }) {
  return reacoes.map((reacao, i) => {
    const ficha = mesa.participantes.find((p) => p.id === reacao.idParticipante && p.coluna != null);
    if (!ficha) return null;
    const { x, y } = posicaoDe(ficha);
    const lado = (ficha.tamanho || 1) * mesa.tamanhoCelula;
    const desvio = (sementeDe(String(reacao.chave)) - 0.5) * lado * 0.6;
    return (
      <div key={reacao.chave} className="efeito-ancora" style={naPosicao({ x: x + lado / 2 + desvio, y: y + lado * 0.1 })} aria-hidden="true">
        <span className="reacao-flutuante" style={{ '--balanco': `${i % 2 ? 1 : -1}` }}>{reacao.emoji}</span>
      </div>
    );
  });
}

// Fora do conteúdo do mapa: reações de quem não tem ficha no mapa sobem do pé da tela.
export function ReacoesSoltas({ reacoes, mesa }) {
  const soltas = reacoes.filter((r) => !mesa.participantes.some((p) => p.id === r.idParticipante && p.coluna != null));
  if (!soltas.length) return null;
  return (
    <div className="reacoes-soltas" aria-hidden="true">
      {soltas.map((r) => (
        <span key={r.chave} className="reacao-solta" style={{ left: `${20 + sementeDe(String(r.chave)) * 60}%` }}>
          <span className="reacao-flutuante">{r.emoji}</span>
          {r.autor && <small>{r.autor}</small>}
        </span>
      ))}
    </div>
  );
}

// Faixa que atravessa a tela quando a vez passa. Para o dono da ficha: "É a sua vez!".
export function AnuncioVez({ anuncios }) {
  const ultimo = anuncios[anuncios.length - 1];
  if (!ultimo) return null;
  return (
    <div key={ultimo.chave} className={`anuncio-vez${ultimo.minha ? ' anuncio-minha' : ''}${ultimo.depoisDaAbertura ? ' depois-da-abertura' : ''}`}
      aria-live="polite">
      <span className="anuncio-vez-linha" aria-hidden="true" />
      <span className="anuncio-vez-texto">
        {ultimo.minha ? 'É a sua vez!' : <>Vez de <strong>{ultimo.nome}</strong></>}
      </span>
      <span className="anuncio-vez-linha" aria-hidden="true" />
    </div>
  );
}

// Raios de estrela que saem do centro, como se a câmera acelerasse, e o anúncio do combate.
const RAIOS = Array.from({ length: 28 }, (_, i) => ({
  angulo: (i * 360) / 28 + sementeDe(`raio${i}`) * 9,
  atraso: Math.round(sementeDe(`atraso${i}`) * 260),
  comprimento: 30 + Math.round(sementeDe(`comp${i}`) * 25),
}));

export function AberturaCombate({ aberturas }) {
  const ultima = aberturas[aberturas.length - 1];
  if (!ultima) return null;
  return (
    <div key={ultima.chave} className="abertura-combate" aria-live="polite">
      <div className="abertura-raios" aria-hidden="true">
        {RAIOS.map((r, i) => (
          <span key={i} style={{ '--angulo': `${r.angulo}deg`, '--atraso': `${r.atraso}ms`, '--comprimento': `${r.comprimento}vmax` }} />
        ))}
      </div>
      <div className="abertura-centro">
        <svg className="abertura-espadas" width="64" height="64" viewBox="0 0 64 64" fill="none" aria-hidden="true">
          <path d="M14 10l26 26M50 10L24 36" stroke="currentColor" strokeWidth="3.2" strokeLinecap="round" />
          <path d="M33 39l-5 5 4 4M31 39l5 5-4 4" stroke="currentColor" strokeWidth="2.6" strokeLinecap="round" strokeLinejoin="round" />
          <path d="M20 44l-6 6M44 44l6 6" stroke="currentColor" strokeWidth="3.2" strokeLinecap="round" />
          <circle cx="32" cy="8" r="2.2" fill="currentColor" />
        </svg>
        <p className="abertura-titulo">Combate!</p>
        <p className="abertura-subtitulo">Rodada 1</p>
      </div>
    </div>
  );
}
