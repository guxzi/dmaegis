import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router';
import { listarRegistros, listarTags } from '../api/caderno.js';
import { mensagemDeErro } from '../api/erros.js';
import { Carregando } from '../auth/RotaProtegida.jsx';
import SeloTipo from '../caderno/SeloTipo.jsx';
import { useCaderno } from '../caderno/useCaderno.js';
import Cabecalho from '../componentes/Cabecalho.jsx';

export default function Caderno() {
  const { id } = useParams();
  const { campanha, tipos, rotulo, erro: erroCaderno, naoEncontrada } = useCaderno(id);
  const [tags, setTags] = useState([]);
  const [registros, setRegistros] = useState(null);
  const [erro, setErro] = useState('');
  const [idTipo, setIdTipo] = useState('');
  const [idTag, setIdTag] = useState('');
  const [texto, setTexto] = useState('');
  const [textoBusca, setTextoBusca] = useState('');

  // Espera 300 ms sem digitar antes de buscar.
  useEffect(() => {
    const espera = setTimeout(() => setTextoBusca(texto), 300);
    return () => clearTimeout(espera);
  }, [texto]);

  useEffect(() => {
    listarTags(id).then(setTags).catch(() => {});
  }, [id]);

  useEffect(() => {
    let ativo = true;
    listarRegistros(id, { idTipoRegistro: idTipo, idTag, texto: textoBusca })
      .then((lista) => ativo && setRegistros(lista))
      .catch((e) => ativo && setErro(mensagemDeErro(e)));
    return () => {
      ativo = false;
    };
  }, [id, idTipo, idTag, textoBusca]);

  if (naoEncontrada) {
    return (
      <>
        <Cabecalho />
        <main className="container"><div className="alert alert-warning">Campanha não encontrada. <Link to="/">Voltar</Link></div></main>
      </>
    );
  }

  return (
    <>
      <Cabecalho idCampanha={id} />
      <main className="container pb-5">
        <Link to={`/campanhas/${id}`} className="d-inline-block mb-3 text-decoration-none">
          ← {campanha?.nome ?? 'Campanha'}
        </Link>
        <div className="d-flex justify-content-between align-items-center mb-3">
          <h1 className="h3 mb-0">Caderno</h1>
          <Link to={`/campanhas/${id}/caderno/novo`} className="btn btn-primary">Novo registro</Link>
        </div>

        {(erro || erroCaderno) && <div className="alert alert-danger" role="alert">{erro || erroCaderno}</div>}

        <div className="row g-2 mb-3">
          <div className="col-sm-3">
            <select className="form-select" aria-label="Filtrar por tipo" value={idTipo} onChange={(e) => setIdTipo(e.target.value)}>
              <option value="">Todos os tipos</option>
              {tipos.map((t) => <option key={t.id} value={t.id}>{rotulo(t.id)}</option>)}
            </select>
          </div>
          <div className="col-sm-3">
            <select className="form-select" aria-label="Filtrar por tag" value={idTag} onChange={(e) => setIdTag(e.target.value)}>
              <option value="">Todas as tags</option>
              {tags.map((t) => <option key={t.id} value={t.id}>{t.nome}</option>)}
            </select>
          </div>
          <div className="col-sm-6">
            <input type="search" className="form-control" placeholder="Procurar no nome ou no resumo…"
              aria-label="Procurar" value={texto} onChange={(e) => setTexto(e.target.value)} />
          </div>
        </div>

        {registros === null && !erro && <Carregando />}
        {registros?.length === 0 && <p className="text-secondary">Nenhum registro encontrado.</p>}

        {registros?.length > 0 && (
          <div className="list-group">
            {registros.map((r) => (
              <Link key={r.id} to={`/campanhas/${id}/caderno/${r.id}`} className="list-group-item list-group-item-action">
                <div className="d-flex flex-wrap align-items-center gap-2">
                  <span className="fw-semibold">{r.nome}</span>
                  <SeloTipo idTipo={r.idTipoRegistro} rotulo={rotulo(r.idTipoRegistro)} />
                  {r.tags.map((t) => <span key={t.id} className="badge rounded-pill text-bg-light border">{t.nome}</span>)}
                </div>
                {r.resumo && <div className="text-secondary small">{r.resumo}</div>}
              </Link>
            ))}
          </div>
        )}
      </main>
    </>
  );
}
