import { useId } from 'react';
import { constelacaoDaSemente } from './constelacao.js';

// Capa gerada para uma campanha (ou registro): céu com névoa na matiz da semente, uma constelação
// e um arco de órbita. Sempre igual para a mesma semente. Sem arte de terceiros.
export default function ArteCampanha({ semente, className = '' }) {
  const id = useId().replace(/:/g, '');
  const { estrelas, linhas, matiz } = constelacaoDaSemente(semente);
  const nevoa = `nevoa${id}`;
  const fundo = `fundo${id}`;
  const brilho = `brilho${id}`;
  const cor = `hsl(${matiz} 85% 72%)`;

  return (
    <svg viewBox="0 0 400 220" preserveAspectRatio="xMidYMid slice" className={className} aria-hidden="true">
      <defs>
        <linearGradient id={fundo} x1="0" y1="0" x2="1" y2="1">
          <stop offset="0" stopColor={`hsl(${matiz} 70% 14%)`} />
          <stop offset="1" stopColor="#070b1f" />
        </linearGradient>
        <radialGradient id={nevoa} cx="0.7" cy="0.35" r="0.65">
          <stop offset="0" stopColor={`hsl(${matiz} 90% 60%)`} stopOpacity="0.45" />
          <stop offset="1" stopColor={`hsl(${matiz} 90% 60%)`} stopOpacity="0" />
        </radialGradient>
        <filter id={brilho} x="-100%" y="-100%" width="300%" height="300%">
          <feGaussianBlur stdDeviation="2.2" />
        </filter>
      </defs>
      <rect width="400" height="220" fill={`url(#${fundo})`} />
      <rect width="400" height="220" fill={`url(#${nevoa})`} />
      {/* poeira de fundo, derivada das próprias estrelas para continuar determinística */}
      {estrelas.flatMap((e, i) => [0, 1, 2].map((k) => (
        <circle key={`p${i}-${k}`} cx={(e.x * (k + 3) + i * 47) % 400} cy={(e.y * (k + 2) + i * 31) % 220}
          r={0.6 + (k % 2) * 0.4} fill="#fff" opacity={0.25 + k * 0.12} />
      )))}
      <ellipse cx="200" cy="130" rx="230" ry="60" fill="none" stroke={cor} strokeOpacity="0.22"
        strokeWidth="1" strokeDasharray="2 6" transform={`rotate(${(matiz % 20) - 10} 200 130)`} />
      <g stroke={cor} strokeOpacity="0.6" strokeWidth="1.1">
        {linhas.map(([a, b], i) => (
          <line key={i} x1={estrelas[a].x} y1={estrelas[a].y} x2={estrelas[b].x} y2={estrelas[b].y} />
        ))}
      </g>
      {estrelas.map((e, i) => (
        <g key={i}>
          <circle cx={e.x} cy={e.y} r={e.raio * 2.6} fill={cor} opacity={0.35 * e.brilho} filter={`url(#${brilho})`} />
          <circle cx={e.x} cy={e.y} r={e.raio} fill="#fff" opacity={e.brilho} />
        </g>
      ))}
    </svg>
  );
}
