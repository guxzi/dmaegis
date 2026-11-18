import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router';
import { listarRegistros, listarTags } from '../api/caderno.js';
import { mensagemDeErro } from '../api/erros.js';
import { Carregando } from '../auth/RotaProtegida.jsx';
import { COR_TIPO } from '../caderno/tipos.js';
import { useCaderno } from '../caderno/useCaderno.js';
import Cabecalho from '../componentes/Cabecalho.jsx';
import CabecalhoSecao from '../visual/CabecalhoSecao.jsx';

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
        <CabecalhoSecao
          chamada={campanha?.nome ?? 'Campanha'}
          titulo="Caderno"
          subtitulo="NPCs, personagens, criaturas, locais, facções, itens e missões, com os mapas de cada um."
          acoes={<Link to={`/campanhas/${id}/caderno/novo`} className="btn btn-primary btn-lg">Novo registro</Link>}
        />

        {(erro || erroCaderno) && <div className="alert alert-danger" role="alert">{erro || erroCaderno}</div>}

        <div className="segmentado mb-3" role="group" aria-label="Filtrar por tipo">
          <button type="button" aria-pressed={idTipo === ''} onClick={() => setIdTipo('')}>Todos</button>
          {tipos.map((t) => (
            <button key={t.id} type="button" aria-pressed={String(idTipo) === String(t.id)} onClick={() => setIdTipo(t.id)}>
              {rotulo(t.id)}
            </button>
          ))}
        </div>

        <div className="row g-2 mb-4">
          <div className="col-sm-4 col-lg-3">
            <select className="form-select" aria-label="Filtrar por tag" value={idTag} onChange={(e) => setIdTag(e.target.value)}>
              <option value="">Todas as tags</option>
              {tags.map((t) => <option key={t.id} value={t.id}>{t.nome}</option>)}
            </select>
          </div>
          <div className="col-sm-8 col-lg-9">
            <input type="search" className="form-control" placeholder="Procurar no nome ou no resumo…"
              aria-label="Procurar" value={texto} onChange={(e) => setTexto(e.target.value)} />
          </div>
        </div>

        {registros === null && !erro && <Carregando />}
        {registros?.length === 0 && <div className="aviso-astral">Nenhum registro encontrado.</div>}

        {registros?.length > 0 && (
          <div className="row row-cols-1 row-cols-sm-2 row-cols-lg-3 row-cols-xl-4 g-3">
            {registros.map((r) => (
              <div className="col" key={r.id}>
                <Link to={`/campanhas/${id}/caderno/${r.id}`} className="cartao-registro"
                  style={{ '--cor-tipo': COR_TIPO[r.idTipoRegistro] ?? '#9aa3d4' }}>
                  <span className="tipo">{rotulo(r.idTipoRegistro)}</span>
                  <h3>{r.nome}</h3>
                  {r.resumo && <p className="resumo">{r.resumo}</p>}
                  {r.tags.length > 0 && (
                    <div className="d-flex flex-wrap gap-1 mt-auto pt-1">
                      {r.tags.map((t) => <span key={t.id} className="chip" style={{ fontSize: '0.75rem' }}>{t.nome}</span>)}
                    </div>
                  )}
                </Link>
              </div>
            ))}
          </div>
        )}
      </main>
    </>
  );
}
