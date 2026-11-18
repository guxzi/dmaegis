import { formaDoDado } from './formas.js';

// Dado poligonal (d4 triângulo, d6 quadrado, d8 losango, d10 pipa, d12 pentágono, d20 hexágono).
// Sem `valor`, mostra "d20" etc. `estado`: 'destaque' (valor máximo), 'minimo' (1) ou 'descartado'.
export default function DadoSvg({ faces, valor, tamanho = 44, estado = '', rotulo }) {
  const forma = formaDoDado(faces);
  const texto = valor ?? `d${faces}`;
  const fonte = String(texto).length > 2 ? 22 : 30;

  return (
    <svg className={`dado-svg ${estado}`} viewBox="0 0 100 100" width={tamanho} height={tamanho}
      style={{ '--dado': forma.cor }} role={rotulo ? 'img' : undefined} aria-label={rotulo} aria-hidden={rotulo ? undefined : true}>
      {/* trechos de SVG estáticos, definidos em formas.js (nunca vêm do usuário) */}
      <g className="forma" dangerouslySetInnerHTML={{ __html: forma.elementos.join('') }} />
      <text x="50" y={forma.textoY} textAnchor="middle" dominantBaseline="middle" fontSize={fonte}>{texto}</text>
    </svg>
  );
}
