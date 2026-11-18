import { useId } from 'react';

// Emblema do DMAegis: um escudo heráldico com uma constelação dentro e uma órbita atravessando.
export default function Emblema({ tamanho = 32, className = '' }) {
  const id = useId().replace(/:/g, '');
  const borda = `borda${id}`;
  const fundo = `fundo${id}`;
  const brilho = `brilho${id}`;

  return (
    <svg width={tamanho} height={tamanho * 1.04} viewBox="0 0 100 104" className={className} role="img" aria-label="DMAegis">
      <defs>
        <linearGradient id={borda} x1="0" y1="0" x2="1" y2="1">
          <stop offset="0" stopColor="#2f5bff" />
          <stop offset="1" stopColor="#8b5cf6" />
        </linearGradient>
        <radialGradient id={fundo} cx="0.5" cy="0.35" r="0.75">
          <stop offset="0" stopColor="#1b2766" />
          <stop offset="1" stopColor="#080c24" />
        </radialGradient>
        <filter id={brilho} x="-50%" y="-50%" width="200%" height="200%">
          <feGaussianBlur stdDeviation="1.6" />
        </filter>
      </defs>

      {/* Escudo */}
      <path d="M50 4 L90 17 V50 C90 76 72 92 50 100 C28 92 10 76 10 50 V17 Z"
        fill={`url(#${fundo})`} stroke={`url(#${borda})`} strokeWidth="4" strokeLinejoin="round" />

      {/* Órbita */}
      <ellipse cx="50" cy="54" rx="47" ry="13" fill="none" stroke="#a78bfa" strokeOpacity="0.55" strokeWidth="1.6"
        transform="rotate(-22 50 54)" />

      {/* Constelação */}
      <g stroke="#a9bbff" strokeOpacity="0.75" strokeWidth="1.3" fill="none">
        <polyline points="32,40 47,28 63,41 57,63 38,68 32,40" />
        <line x1="47" y1="28" x2="57" y2="63" strokeOpacity="0.35" />
      </g>
      <g fill="#e6e9ff">
        <circle cx="32" cy="40" r="2.2" />
        <circle cx="63" cy="41" r="2.2" />
        <circle cx="57" cy="63" r="2" />
        <circle cx="38" cy="68" r="1.8" />
      </g>
      <circle cx="47" cy="28" r="5" fill="#c4b5fd" filter={`url(#${brilho})`} />
      <circle cx="47" cy="28" r="2.8" fill="#ffffff" />
    </svg>
  );
}
