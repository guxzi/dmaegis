import { useEffect, useState } from 'react';
import { criticoDaRolagem } from './efeitos.js';

const TEMPO_GIRANDO = 750;
const TEMPO_NA_TELA = 5200;

const poucoMovimento = () => typeof window !== 'undefined' && window.matchMedia?.('(prefers-reduced-motion: reduce)').matches;

// Os números que passam no dado enquanto ele gira são só enfeite (o resultado já veio pronto).
function numeroQualquer(maximo) {
  return 1 + Math.floor(Math.random() * maximo);
}

function DadoSvg() {
  return (
    <svg className="dado-icone" width="34" height="34" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinejoin="round" aria-hidden="true">
      <path d="M12 2 21 7v10l-9 5-9-5V7z" />
      <path d="M12 2 7.5 9.5h9zM3 7l4.5 2.5M21 7l-4.5 2.5M7.5 9.5 12 22l4.5-12.5M3 17l4.5-7.5M21 17l-4.5-7.5" opacity="0.6" />
    </svg>
  );
}

// Um cartão: o dado gira, o número embaralha e então para no total. Crítico brilha em dourado; falha racha.
function DadoRolando({ rolagem, aoDispensar }) {
  const [girando, setGirando] = useState(() => !poucoMovimento());
  const [embaralhado, setEmbaralhado] = useState(rolagem.total);
  const critico = criticoDaRolagem(rolagem.expressao, rolagem.detalhe);

  useEffect(() => {
    if (!girando) return undefined;
    const maximo = Math.max(6, Math.abs(rolagem.total) + 4);
    const intervalo = setInterval(() => setEmbaralhado(numeroQualquer(maximo)), 55);
    const parar = setTimeout(() => setGirando(false), TEMPO_GIRANDO);
    return () => {
      clearInterval(intervalo);
      clearTimeout(parar);
    };
  }, [girando, rolagem.total]);

  useEffect(() => {
    const t = setTimeout(() => aoDispensar(rolagem.id), TEMPO_NA_TELA);
    return () => clearTimeout(t);
  }, [aoDispensar, rolagem.id]);

  const classes = [
    'dado-rolando',
    girando ? 'girando' : 'parou',
    !girando && critico && `rolagem-${critico}`,
    rolagem.mestre && 'do-mestre',
    rolagem.secreta && 'secreta',
  ].filter(Boolean).join(' ');

  return (
    <li className={classes} onClick={() => aoDispensar(rolagem.id)} title="Clique para dispensar">
      <span className="dado-rolando-total">
        <DadoSvg />
        <strong>{girando ? embaralhado : rolagem.total}</strong>
        {!girando && critico === 'falha' && (
          <svg className="dado-racha" viewBox="0 0 40 40" aria-hidden="true">
            <path d="M20 2 17 12l6 5-5 7 4 6-3 8" />
            <path d="M17 12 9 15M23 17l8-2M18 24l-7 4" />
          </svg>
        )}
        {!girando && critico === 'critico' && (
          <span className="dado-faiscas" aria-hidden="true">
            {Array.from({ length: 10 }, (_, i) => <span key={i} style={{ '--angulo': `${i * 36}deg` }} />)}
          </span>
        )}
      </span>
      <span className="dado-rolando-corpo">
        <span className="dado-rolando-autor">
          {rolagem.autor}
          {rolagem.secreta && <span className="linha-etiqueta ms-1">secreta</span>}
        </span>
        <span className="dado-rolando-detalhe">
          {!girando && critico === 'critico' ? 'Crítico!' : !girando && critico === 'falha' ? 'Falha crítica!' : rolagem.expressao}
        </span>
      </span>
    </li>
  );
}

// Os dados que acabaram de ser rolados por alguém da mesa, por cima do mapa, para todos verem.
export default function RolagensNaMesa({ novas, aoDispensar }) {
  if (!novas.length) return null;
  return (
    <ol className="rolagens-na-mesa" aria-live="polite" onPointerDown={(e) => e.stopPropagation()}>
      {novas.map((r) => <DadoRolando key={r.id} rolagem={r} aoDispensar={aoDispensar} />)}
    </ol>
  );
}
