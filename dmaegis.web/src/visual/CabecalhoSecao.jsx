// "— CHAMADA —", título grande e subtítulo (padrão do Dungeons and Newbies). `acoes` fica à direita.
export default function CabecalhoSecao({ chamada, titulo, subtitulo, centralizado = false, acoes, children }) {
  return (
    <header className={`cabecalho-secao surgir ${centralizado ? 'centralizado' : ''}`}>
      <div className={`d-flex flex-wrap gap-3 ${centralizado ? 'justify-content-center' : 'justify-content-between align-items-end'}`}>
        <div style={{ minWidth: 0 }}>
          {chamada && <span className="chamada">{chamada}</span>}
          <h1 className="titulo-estelar">{titulo}</h1>
          {subtitulo && <p className="subtitulo">{subtitulo}</p>}
        </div>
        {acoes && <div className="d-flex flex-wrap gap-2">{acoes}</div>}
      </div>
      {children}
    </header>
  );
}
