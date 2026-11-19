import { memo } from 'react';
import { celulaDaChave } from './nevoa.js';

// A névoa por cima do mapa: um véu inteiro com buracos nas células reveladas (máscara SVG).
// Para os jogadores: escura, com estrelas tênues e bordas suaves (a máscara é desfocada).
// Para o mestre: um véu violeta translúcido, nítido, para enxergar o mapa e saber o que está coberto.
function CamadaNevoa({ largura, altura, mesa, reveladas, modo }) {
  const cel = mesa.tamanhoCelula;
  const id = `nevoa-${modo}`;
  const suave = modo === 'jogador';

  return (
    <svg className={`palco-nevoa nevoa-${modo}`} width={largura} height={altura} aria-hidden="true">
      <defs>
        {suave && (
          <filter id={`${id}-borda`} x="-10%" y="-10%" width="120%" height="120%">
            <feGaussianBlur stdDeviation={cel * 0.32} />
          </filter>
        )}
        <mask id={`${id}-mascara`} maskUnits="userSpaceOnUse" x="0" y="0" width={largura} height={altura}>
          <rect width={largura} height={altura} fill="#fff" />
          <g filter={suave ? `url(#${id}-borda)` : undefined}>
            {[...reveladas].map((chave) => {
              const { coluna, linha } = celulaDaChave(chave);
              // Um pouco maior que a célula no modo suave, para o desfoque não deixar fresta entre vizinhas.
              const folga = suave ? cel * 0.12 : 0;
              return (
                <rect key={chave} className="nevoa-buraco" fill="#000"
                  x={mesa.deslocamentoX + coluna * cel - folga} y={mesa.deslocamentoY + linha * cel - folga}
                  width={cel + folga * 2} height={cel + folga * 2} />
              );
            })}
          </g>
        </mask>
        {suave && (
          <pattern id={`${id}-estrelas`} width="160" height="160" patternUnits="userSpaceOnUse">
            <circle cx="22" cy="31" r="1" fill="#c4b5fd" opacity="0.5" />
            <circle cx="97" cy="14" r="0.8" fill="#fff" opacity="0.45" />
            <circle cx="133" cy="88" r="1.2" fill="#a9bbff" opacity="0.5" />
            <circle cx="61" cy="119" r="0.8" fill="#fff" opacity="0.35" />
            <circle cx="12" cy="143" r="1" fill="#c4b5fd" opacity="0.4" />
          </pattern>
        )}
      </defs>
      <g mask={`url(#${id}-mascara)`}>
        <rect className="nevoa-veu" width={largura} height={altura} />
        {suave && <rect width={largura} height={altura} fill={`url(#${id}-estrelas)`} />}
      </g>
    </svg>
  );
}

export default memo(CamadaNevoa);
