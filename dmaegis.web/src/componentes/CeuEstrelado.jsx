// Fundo fixo de todas as páginas: névoa cobalto/violeta, três camadas de estrelas
// e constelações discretas com uma órbita que gira devagar (parada se o sistema pedir menos movimento).
function Constelacao({ className, comOrbita = false }) {
  const estrelas = [
    [40, 210], [120, 150], [200, 175], [260, 100], [340, 130], [410, 60], [470, 120], [300, 230],
  ];
  return (
    <svg className={className} viewBox="0 0 520 300" aria-hidden="true">
      {comOrbita && (
        <g className="orbita">
          <ellipse cx="260" cy="170" rx="230" ry="70" fill="none" stroke="#8b5cf6" strokeOpacity="0.35"
            strokeWidth="1" strokeDasharray="2 7" />
          <circle cx="490" cy="170" r="3" fill="#c4b5fd" />
        </g>
      )}
      <polyline points="40,210 120,150 200,175 260,100 340,130 410,60 470,120" fill="none"
        stroke="#7c8cff" strokeOpacity="0.55" strokeWidth="1" />
      <polyline points="200,175 300,230 340,130" fill="none" stroke="#7c8cff" strokeOpacity="0.35" strokeWidth="1" />
      {estrelas.map(([x, y], i) => (
        <g key={i}>
          <circle cx={x} cy={y} r={i % 3 === 0 ? 5 : 3.5} fill="#c4b5fd" opacity="0.25" />
          <circle cx={x} cy={y} r={i % 3 === 0 ? 2.2 : 1.6} fill="#ffffff" />
        </g>
      ))}
    </svg>
  );
}

export default function CeuEstrelado() {
  return (
    <div className="ceu-estrelado" aria-hidden="true">
      <div className="estrelas estrelas-pequenas" />
      <div className="estrelas estrelas-medias" />
      <div className="estrelas estrelas-grandes" />
      <Constelacao className="constelacao-alto" />
      <Constelacao className="constelacao" comOrbita />
    </div>
  );
}
