import { useEffect, useState } from 'react';
import { criarEntrega, excluirEntrega, listarEntregas } from '../api/entregas.js';
import { mensagemDeErro } from '../api/erros.js';

const TIPO_PJ = 2;

function hora(data) {
  return new Date(data).toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' });
}

// Janela "Entregar": o mestre manda um handout (título, texto e/ou imagem da galeria) para todos ou para um PJ.
// Embaixo, o que já foi entregue, com a opção de recolher.
export default function PainelEntregas({ idCampanha, registros, imagens, urlImagem, aoFechar }) {
  const [entregas, setEntregas] = useState(null);
  const [titulo, setTitulo] = useState('');
  const [texto, setTexto] = useState('');
  const [idImagem, setIdImagem] = useState(null);
  const [destino, setDestino] = useState('');
  const [erro, setErro] = useState('');
  const [enviando, setEnviando] = useState(false);
  const pjs = registros.filter((r) => r.idTipoRegistro === TIPO_PJ);

  useEffect(() => {
    listarEntregas(idCampanha).then(setEntregas).catch((e) => setErro(mensagemDeErro(e)));
  }, [idCampanha]);

  useEffect(() => {
    const aoTeclar = (e) => e.key === 'Escape' && aoFechar();
    window.addEventListener('keydown', aoTeclar);
    return () => window.removeEventListener('keydown', aoTeclar);
  }, [aoFechar]);

  async function entregar(evento) {
    evento.preventDefault();
    setErro('');
    setEnviando(true);
    try {
      const nova = await criarEntrega(idCampanha, {
        titulo, texto: texto || null, idImagemRegistro: idImagem, idRegistroDestino: destino ? Number(destino) : null,
      });
      setEntregas((lista) => [nova, ...(lista ?? [])]);
      setTitulo('');
      setTexto('');
      setIdImagem(null);
    } catch (e) {
      setErro(mensagemDeErro(e));
    } finally {
      setEnviando(false);
    }
  }

  async function recolher(id) {
    setErro('');
    try {
      await excluirEntrega(id);
      setEntregas((lista) => lista.filter((e) => e.id !== id));
    } catch (e) {
      setErro(mensagemDeErro(e));
    }
  }

  return (
    <>
      <div className="modal d-block" tabIndex={-1} role="dialog" aria-modal="true" aria-labelledby="tituloEntregas"
        onClick={(e) => e.target === e.currentTarget && aoFechar()}>
        <div className="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
          <div className="modal-content cartao-astral">
            <div className="modal-header">
              <h2 id="tituloEntregas" className="modal-title h5">Entregar aos jogadores</h2>
              <button type="button" className="btn-close btn-close-white" onClick={aoFechar} aria-label="Fechar" />
            </div>
            <div className="modal-body">
              <form onSubmit={entregar} className="form-entrega">
                <div className="row g-2">
                  <div className="col-md-8">
                    <label className="form-label rotulo-painel" htmlFor="tituloEntrega">Título</label>
                    <input id="tituloEntrega" className="form-control" value={titulo} maxLength={120} required
                      onChange={(e) => setTitulo(e.target.value)} placeholder="Carta lacrada do prefeito" />
                  </div>
                  <div className="col-md-4">
                    <label className="form-label rotulo-painel" htmlFor="destinoEntrega">Para</label>
                    <select id="destinoEntrega" className="form-select" value={destino} onChange={(e) => setDestino(e.target.value)}>
                      <option value="">Todos os jogadores</option>
                      {pjs.map((r) => <option key={r.id} value={r.id}>Só {r.nome}</option>)}
                    </select>
                  </div>
                </div>
                <label className="form-label rotulo-painel mt-2" htmlFor="textoEntrega">Texto</label>
                <textarea id="textoEntrega" className="form-control" rows={4} maxLength={4000} value={texto}
                  onChange={(e) => setTexto(e.target.value)} placeholder="O que está escrito, o que eles encontram…" />

                <p className="rotulo-painel mt-3 mb-1">Imagem (opcional)</p>
                {imagens.length === 0
                  ? <p className="small text-secondary">A galeria da campanha está vazia.</p>
                  : (
                    <div className="grade-imagens-entrega" role="listbox" aria-label="Imagem da entrega">
                      {imagens.map((img) => (
                        <button key={img.id} type="button" role="option" aria-selected={idImagem === img.id} title={img.titulo}
                          onClick={() => setIdImagem((atual) => (atual === img.id ? null : img.id))}>
                          <img src={urlImagem(img.id)} alt={img.titulo} loading="lazy" />
                        </button>
                      ))}
                    </div>
                  )}

                {erro && <div className="alert alert-danger py-2 mt-2">{erro}</div>}
                <div className="d-flex justify-content-end mt-3">
                  <button type="submit" className="btn btn-primary" disabled={enviando || !titulo.trim() || (!texto.trim() && !idImagem)}>
                    {enviando ? 'Entregando…' : 'Entregar'}
                  </button>
                </div>
              </form>

              <div className="divisor-estelar my-3" />
              <p className="rotulo-painel">Já entregues</p>
              {entregas?.length === 0 && <p className="small text-secondary mb-0">Nada entregue ainda.</p>}
              <ul className="lista-entregas">
                {entregas?.map((e) => (
                  <li key={e.id}>
                    {e.idImagemRegistro && <img src={urlImagem(e.idImagemRegistro)} alt="" />}
                    <div className="flex-grow-1 min-w-0">
                      <strong>{e.titulo}</strong>
                      <span className="small text-secondary d-block">{e.nomeDestino ? `Só ${e.nomeDestino}` : 'Todos'} · {hora(e.data)}</span>
                    </div>
                    <button type="button" className="btn btn-sm btn-outline-danger" onClick={() => recolher(e.id)}
                      title="Some da tela dos jogadores">Recolher</button>
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </div>
      </div>
      <div className="modal-backdrop show" />
    </>
  );
}
