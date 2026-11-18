import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { excluirRegistro, obterRegistro } from '../api/caderno.js';
import { mensagemDeErro } from '../api/erros.js';
import { Carregando } from '../auth/RotaProtegida.jsx';
import SeloTipo from '../caderno/SeloTipo.jsx';
import { useCaderno } from '../caderno/useCaderno.js';
import Cabecalho from '../componentes/Cabecalho.jsx';
import ConfirmarExclusao from '../componentes/ConfirmarExclusao.jsx';

export default function RegistroDetalhe() {
  const { id, idRegistro } = useParams();
  const navegar = useNavigate();
  const { rotulo } = useCaderno(id);
  const [registro, setRegistro] = useState(null);
  const [naoEncontrado, setNaoEncontrado] = useState(false);
  const [erro, setErro] = useState('');
  const [confirmando, setConfirmando] = useState(false);

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
    conteudo = (
      <article>
        <div className="d-flex flex-wrap justify-content-between align-items-start gap-2 mb-2">
          <h1 className="h3 mb-0">{registro.nome}</h1>
          <div className="d-flex gap-2">
            <Link to={`/campanhas/${id}/caderno/${idRegistro}/editar`} className="btn btn-sm btn-outline-primary">Editar</Link>
            <button type="button" className="btn btn-sm btn-outline-danger" onClick={() => setConfirmando(true)}>Excluir</button>
          </div>
        </div>
        <div className="d-flex flex-wrap gap-2 mb-3">
          <SeloTipo idTipo={registro.idTipoRegistro} rotulo={rotulo(registro.idTipoRegistro)} />
          {registro.tags.map((t) => <span key={t.id} className="badge rounded-pill text-bg-light border">{t.nome}</span>)}
        </div>
        {registro.resumo && <p className="lead">{registro.resumo}</p>}

        {registro.atributos.length > 0 && (
          <dl className="row border rounded-3 py-2 mx-0 mb-3 bg-body-tertiary">
            {registro.atributos.map((a) => (
              <div className="col-6 col-md-4 col-lg-3 mb-2" key={a.chave}>
                <dt className="small text-secondary">{a.chave}</dt>
                <dd className="mb-0">{a.valor}</dd>
              </div>
            ))}
          </dl>
        )}

        {registro.descricao
          ? <p style={{ whiteSpace: 'pre-line' }}>{registro.descricao}</p>
          : <p className="text-secondary fst-italic">Sem descrição.</p>}
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
