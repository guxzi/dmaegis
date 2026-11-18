import { useEffect, useState } from 'react';
import { createPortal } from 'react-dom';
import { urlDaImagem } from '../api/imagens.js';
import AcoesImagem from './AcoesImagem.jsx';

// Visualizador em tela cheia. A imagem começa ajustada à tela; clicar alterna para o tamanho real (com rolagem).
// ← → trocam de imagem, Esc fecha. Fica acima do cartão lateral do escudo.
export default function VisualizadorImagem({ imagens, indice, aoMudar, aoFechar }) {
  const [tamanhoReal, setTamanhoReal] = useState(false);
  const imagem = imagens[indice];

  useEffect(() => {
    // Captura + stopImmediatePropagation: o Esc fecha só o visualizador, não o cartão que está atrás.
    const aoTeclar = (e) => {
      if (e.key === 'Escape') {
        e.stopImmediatePropagation();
        aoFechar();
      } else if (e.key === 'ArrowRight' && imagens.length > 1) {
        setTamanhoReal(false);
        aoMudar((indice + 1) % imagens.length);
      } else if (e.key === 'ArrowLeft' && imagens.length > 1) {
        setTamanhoReal(false);
        aoMudar((indice - 1 + imagens.length) % imagens.length);
      }
    };
    window.addEventListener('keydown', aoTeclar, true);
    return () => window.removeEventListener('keydown', aoTeclar, true);
  }, [imagens.length, indice, aoMudar, aoFechar]);

  if (!imagem) return null;

  return createPortal(
    <div className="visualizador-imagem" role="dialog" aria-modal="true" aria-label={imagem.titulo}>
      <div className="visualizador-palco" style={{ overflow: tamanhoReal ? 'auto' : 'hidden' }}
        onClick={(e) => e.target === e.currentTarget && aoFechar()}>
        <img src={urlDaImagem(imagem.id)} alt={imagem.titulo}
          className={tamanhoReal ? 'tamanho-real' : 'ajustada'}
          onClick={() => setTamanhoReal((v) => !v)}
          title={tamanhoReal ? 'Clique para ajustar à tela' : 'Clique para ver no tamanho real'} />
      </div>
      <div className="visualizador-barra">
        {imagens.length > 1 && (
          <button type="button" className="btn btn-sm btn-outline-secondary" aria-label="Imagem anterior"
            onClick={() => { setTamanhoReal(false); aoMudar((indice - 1 + imagens.length) % imagens.length); }}>←</button>
        )}
        <span className="titulo-runico flex-grow-1 text-truncate">
          {imagem.titulo}
          {imagem.nomeRegistro && <span className="small text-secondary ms-2">{imagem.nomeRegistro}</span>}
        </span>
        <AcoesImagem key={imagem.id} imagem={imagem} comOpcaoTitulo />
        <span className="small text-secondary text-nowrap">{indice + 1} de {imagens.length}</span>
        {imagens.length > 1 && (
          <button type="button" className="btn btn-sm btn-outline-secondary" aria-label="Próxima imagem"
            onClick={() => { setTamanhoReal(false); aoMudar((indice + 1) % imagens.length); }}>→</button>
        )}
        <button type="button" className="btn-close ms-2" aria-label="Fechar" onClick={aoFechar} />
      </div>
    </div>,
    document.body,
  );
}
