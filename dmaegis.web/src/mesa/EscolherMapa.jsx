import { useEffect, useMemo, useState } from 'react';
import { urlDaImagem } from '../api/imagens.js';

// Janela com as imagens da galeria em miniatura, para escolher o mapa da mesa.
export default function EscolherMapa({ imagens, idAtual, aoEscolher, aoFechar }) {
  const [termo, setTermo] = useState('');

  useEffect(() => {
    const aoTeclar = (e) => e.key === 'Escape' && aoFechar();
    window.addEventListener('keydown', aoTeclar);
    return () => window.removeEventListener('keydown', aoTeclar);
  }, [aoFechar]);

  const filtradas = useMemo(() => {
    const t = termo.trim().toLowerCase();
    if (!t) return imagens;
    return imagens.filter((i) => `${i.titulo} ${i.nomeRegistro}`.toLowerCase().includes(t));
  }, [imagens, termo]);

  return (
    <>
      <div className="modal d-block" tabIndex={-1} role="dialog" aria-modal="true" aria-labelledby="tituloEscolherMapa"
        onClick={(e) => e.target === e.currentTarget && aoFechar()}>
        <div className="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
          <div className="modal-content cartao-astral">
            <div className="modal-header">
              <h2 id="tituloEscolherMapa" className="modal-title h5">Escolher o mapa</h2>
              <button type="button" className="btn-close btn-close-white" onClick={aoFechar} aria-label="Fechar" />
            </div>
            <div className="modal-body">
              <input type="search" className="form-control mb-3" placeholder="Procurar pelo título ou pelo registro…" autoFocus
                value={termo} onChange={(e) => setTermo(e.target.value)} aria-label="Procurar imagem" />
              {imagens.length === 0 && (
                <p className="text-secondary mb-0">Nenhuma imagem na galeria. Envie um mapa pela página Galeria.</p>
              )}
              <div className="grade-mapas">
                <button type="button" className={`opcao-mapa sem-mapa${idAtual == null ? ' escolhida' : ''}`}
                  onClick={() => aoEscolher(null)}>
                  <span className="opcao-mapa-arte" aria-hidden="true" />
                  <span className="opcao-mapa-titulo">Sem mapa (céu estrelado)</span>
                </button>
                {filtradas.map((imagem) => (
                  <button key={imagem.id} type="button" className={`opcao-mapa${imagem.id === idAtual ? ' escolhida' : ''}`}
                    onClick={() => aoEscolher(imagem.id)}>
                    <img src={urlDaImagem(imagem.id)} alt="" loading="lazy" />
                    <span className="opcao-mapa-titulo">{imagem.titulo}</span>
                    <span className="opcao-mapa-registro">{imagem.nomeRegistro}</span>
                  </button>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>
      <div className="modal-backdrop show" />
    </>
  );
}
