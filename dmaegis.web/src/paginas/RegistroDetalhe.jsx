import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { excluirRegistro, obterRegistro } from '../api/caderno.js';
import { mensagemDeErro } from '../api/erros.js';
import { Carregando } from '../auth/RotaProtegida.jsx';
import { urlDaImagem } from '../api/imagens.js';
import { COR_TIPO } from '../caderno/tipos.js';
import { useCaderno } from '../caderno/useCaderno.js';
import Cabecalho from '../componentes/Cabecalho.jsx';
import ConfirmarExclusao from '../componentes/ConfirmarExclusao.jsx';
import Emblema from '../componentes/Emblema.jsx';
import GaleriaImagens from '../imagens/GaleriaImagens.jsx';
import VisualizadorImagem from '../imagens/VisualizadorImagem.jsx';
import ArteCampanha from '../visual/ArteCampanha.jsx';

export default function RegistroDetalhe() {
  const { id, idRegistro } = useParams();
  const navegar = useNavigate();
  const { rotulo } = useCaderno(id);
  const [registro, setRegistro] = useState(null);
  const [naoEncontrado, setNaoEncontrado] = useState(false);
  const [erro, setErro] = useState('');
  const [confirmando, setConfirmando] = useState(false);
  const [imagens, setImagens] = useState([]);
  const [imagemAberta, setImagemAberta] = useState(null);
  const fecharVisualizador = useCallback(() => setImagemAberta(null), []);

  useEffect(() => {
    obterRegistro(idRegistro)
      .then(setRegistro)
      .catch((e) => (e.response?.status === 404 ? setNaoEncontrado(true) : setErro(mensagemDeErro(e))));
  }, [idRegistro]);

  async function excluir() {
    try {
      await excluirRegistro(idRegistro);
      navegar(`/campanhas/${id}/caderno`, { replace: true });
    } catch (e) {
      setConfirmando(false);
      setErro(mensagemDeErro(e));
    }
  }

  const fechar = useCallback(() => setConfirmando(false), []);

  let conteudo;
  if (naoEncontrado) {
    conteudo = <div className="alert alert-warning">Registro não encontrado.</div>;
  } else if (!registro) {
    conteudo = erro ? null : <Carregando />;
  } else {
    const corTipo = COR_TIPO[registro.idTipoRegistro] ?? '#9aa3d4';
    const capa = imagens[0];
    conteudo = (
      <article>
        <div className="card cartao-astral overflow-hidden surgir mb-4">
          <div className="row g-0">
            <div className="col-lg-5 position-relative" style={{ minHeight: 280 }}>
              <div className="destaque-registro" style={{ '--cor-tipo': corTipo }}>
                {capa ? (
                  <button type="button" className="miniatura-botao h-100 border-0 rounded-0" style={{ aspectRatio: 'auto' }}
                    onClick={() => setImagemAberta(0)} aria-label={`Abrir ${capa.titulo}`}>
                    <img src={urlDaImagem(capa.id)} alt="" />
                  </button>
                ) : (
                  <>
                    <div style={{ position: 'absolute', inset: 0, opacity: 0.45 }}>
                      <ArteCampanha semente={registro.id * 7 + registro.idTipoRegistro} />
                    </div>
                    <div className="emblema-destaque">
                      <Emblema tamanho={150} />
                    </div>
                  </>
                )}
              </div>
            </div>
            <div className="col-lg-7">
              <div className="p-4 p-lg-5">
                <div className="d-flex flex-wrap justify-content-between align-items-start gap-2">
                  <span className="chip" style={{ color: corTipo, borderColor: corTipo }}>{rotulo(registro.idTipoRegistro)}</span>
                  <div className="d-flex gap-2">
                    <Link to={`/campanhas/${id}/caderno/${idRegistro}/editar`} className="btn btn-sm btn-fantasma">Editar</Link>
                    <button type="button" className="btn btn-sm btn-outline-danger" onClick={() => setConfirmando(true)}>Excluir</button>
                  </div>
                </div>
                <h1 className="titulo-estelar mt-3 mb-2" style={{ fontSize: 'clamp(2rem, 4vw, 2.9rem)' }}>{registro.nome}</h1>
                {registro.resumo && <p className="fs-5 mb-3" style={{ color: '#d8ccff' }}>{registro.resumo}</p>}
                {registro.tags.length > 0 && (
                  <div className="d-flex flex-wrap gap-1 mb-4">
                    {registro.tags.map((t) => <span key={t.id} className="chip">{t.nome}</span>)}
                  </div>
                )}

                {registro.atributos.length > 0 && (
                  <>
                    <div className="rotulo-secao">Atributos</div>
                    <dl className="caixas-atributo mb-4">
                      {registro.atributos.map((a) => (
                        <div className="caixa-atributo" key={a.chave}>
                          <dt>{a.chave}</dt>
                          <dd>{a.valor}</dd>
                        </div>
                      ))}
                    </dl>
                  </>
                )}

                <div className="rotulo-secao">Sobre</div>
                {registro.descricao
                  ? <p className="mb-0" style={{ whiteSpace: 'pre-line', lineHeight: 1.65 }}>{registro.descricao}</p>
                  : <p className="text-secondary fst-italic mb-0">Sem descrição.</p>}
              </div>
            </div>
          </div>
        </div>

        <GaleriaImagens idRegistro={registro.id} editavel colunas={3} aoMudar={setImagens} />

        {imagemAberta !== null && imagens[imagemAberta] && (
          <VisualizadorImagem imagens={imagens} indice={imagemAberta} aoMudar={setImagemAberta} aoFechar={fecharVisualizador} />
        )}
      </article>
    );
  }

  return (
    <>
      <Cabecalho idCampanha={id} />
      <main className="container pb-5">
        <Link to={`/campanhas/${id}/caderno`} className="d-inline-block mb-3 text-decoration-none">← Caderno</Link>
        {erro && <div className="alert alert-danger" role="alert">{erro}</div>}
        {conteudo}
      </main>
      <ConfirmarExclusao
        aberto={confirmando}
        titulo="Excluir registro"
        texto={registro ? `Excluir "${registro.nome}"? Isso não pode ser desfeito.` : ''}
        aoConfirmar={excluir}
        aoCancelar={fechar}
      />
    </>
  );
}
