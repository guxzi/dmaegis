import { useEffect, useState } from 'react';
import { urlDaImagem } from '../api/imagens.js';
import Emblema from '../componentes/Emblema.jsx';
import { lerUltimaProjecao, ouvirProjecao } from '../projecao/canal.js';

// Janela para os jogadores: sem cabeçalho, só a imagem que o mestre mandou (ou a tela de espera).
// Duplo clique liga e desliga a tela cheia.
export default function Projecao() {
  const [projecao, setProjecao] = useState(() => lerUltimaProjecao());

  useEffect(() => {
    document.title = 'DMAegis — Projeção';
    return ouvirProjecao(setProjecao);
  }, []);

  function alternarTelaCheia() {
    if (document.fullscreenElement) document.exitFullscreen?.();
    else document.documentElement.requestFullscreen?.().catch(() => {});
  }

  return (
    <main className="projecao" onDoubleClick={alternarTelaCheia}>
      {projecao ? (
        <figure className="projecao-figura surgir" key={projecao.id}>
          <img src={urlDaImagem(projecao.id)} alt={projecao.titulo || 'Imagem mostrada pelo mestre'} />
          {projecao.titulo && <figcaption className="titulo-estelar">{projecao.titulo}</figcaption>}
        </figure>
      ) : (
        <div className="projecao-espera surgir">
          <Emblema tamanho={140} className="projecao-emblema" />
          <p className="titulo-estelar h2 mb-1">DMAegis</p>
          <p className="text-secondary mb-0">Aguardando o mestre…</p>
        </div>
      )}
      <p className="projecao-dica small">Duplo clique: tela cheia</p>
    </main>
  );
}
