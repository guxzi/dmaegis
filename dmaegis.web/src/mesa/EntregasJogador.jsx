import { useEffect } from 'react';

// A entrega aberta em tela cheia: o cartão se desdobra com o título, a imagem e o texto.
export function RevelarEntrega({ entrega, nova = false, urlImagem, aoFechar }) {
  useEffect(() => {
    const aoTeclar = (e) => e.key === 'Escape' && aoFechar();
    window.addEventListener('keydown', aoTeclar);
    return () => window.removeEventListener('keydown', aoTeclar);
  }, [aoFechar]);

  return (
    <div className="revelar-entrega" role="dialog" aria-modal="true" aria-labelledby="tituloEntregaAberta"
      onPointerDown={(e) => e.stopPropagation()} onClick={(e) => e.target === e.currentTarget && aoFechar()}>
      <article className="cartao-entrega" key={entrega.id}>
        {nova && <p className="entrega-chamada">O mestre entregou algo a você</p>}
        <h2 id="tituloEntregaAberta" className="entrega-titulo">{entrega.titulo}</h2>
        {entrega.idImagemRegistro && (
          <img className="entrega-imagem" src={urlImagem(entrega.idImagemRegistro)} alt={entrega.titulo} />
        )}
        {entrega.texto && <p className="entrega-texto">{entrega.texto}</p>}
        <button type="button" className="btn btn-primary mt-3" onClick={aoFechar} autoFocus>Guardar</button>
      </article>
    </div>
  );
}

// A lista do que o jogador já recebeu, para rever.
export function ListaEntregas({ entregas, aoAbrir, aoFechar }) {
  return (
    <div className="janela-entregas cartao-astral" onPointerDown={(e) => e.stopPropagation()}>
      <div className="d-flex align-items-center mb-2">
        <strong className="flex-grow-1">Entregas do mestre</strong>
        <button type="button" className="btn-close btn-close-white btn-sm" onClick={aoFechar} aria-label="Fechar entregas" />
      </div>
      {entregas.length === 0
        ? <p className="small text-secondary mb-0">Nada por aqui ainda. Quando o mestre entregar uma carta ou um mapa, aparece aqui.</p>
        : (
          <ul className="lista-entregas compacta">
            {entregas.map((e) => (
              <li key={e.id}>
                <button type="button" onClick={() => aoAbrir(e)}>
                  <span className="entrega-icone" aria-hidden="true">{e.idImagemRegistro ? '🖼' : '📜'}</span>
                  <span className="text-start">{e.titulo}</span>
                </button>
              </li>
            ))}
          </ul>
        )}
    </div>
  );
}

// O mestre pediu iniciativa: um cartão com o botão de rolar (d20 + bônus da ficha).
export function PedidoIniciativa({ bonus, erro, aoRolar, aoFechar }) {
  const sinal = bonus >= 0 ? `+${bonus}` : `${bonus}`;
  return (
    <div className="pedido-iniciativa cartao-astral" role="dialog" aria-label="Pedido de iniciativa" onPointerDown={(e) => e.stopPropagation()}>
      <p className="pedido-iniciativa-titulo">Rolem iniciativa!</p>
      <p className="small text-secondary mb-2">O mestre está montando a ordem do combate.</p>
      <div className="d-flex gap-2 justify-content-center">
        <button type="button" className="btn btn-primary" onClick={aoRolar}>Rolar d20{bonus ? sinal : ''}</button>
        <button type="button" className="btn btn-fantasma" onClick={aoFechar}>Depois</button>
      </div>
      {erro && <p className="small text-danger mt-2 mb-0">{erro}</p>}
    </div>
  );
}
