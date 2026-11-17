import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router';
import * as campanhasApi from '../api/campanhas.js';
import { mensagemDeErro } from '../api/erros.js';
import { Carregando } from '../auth/RotaProtegida.jsx';
import FormularioCampanha from '../campanhas/FormularioCampanha.jsx';
import { separarCampanhas } from '../campanhas/organizar.js';
import SeloSistema from '../campanhas/SeloSistema.jsx';
import Cabecalho from '../componentes/Cabecalho.jsx';
import { textoExclusao } from '../campanhas/textos.js';
import ConfirmarExclusao from '../componentes/ConfirmarExclusao.jsx';

function CartaoCampanha({ campanha, aoArquivar, aoExcluir }) {
  return (
    <div className="col">
      <div className={`card h-100 ${campanha.arquivada ? 'bg-body-tertiary' : ''}`}>
        <div className="card-body">
          <div className="d-flex justify-content-between align-items-start gap-2 mb-2">
            <h3 className="h5 card-title mb-0">
              <Link to={`/campanhas/${campanha.id}`} className="stretched-link text-decoration-none">
                {campanha.nome}
              </Link>
            </h3>
            <SeloSistema idSistema={campanha.idSistema} nomeSistema={campanha.nomeSistema} />
          </div>
          {campanha.descricao && (
            <p className="card-text text-secondary small"
              style={{ display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical', overflow: 'hidden' }}>
              {campanha.descricao}
            </p>
          )}
        </div>
        {/* position-relative + z-index: os botões ficam acima do stretched-link do título */}
        <div className="card-footer bg-transparent d-flex gap-2 position-relative" style={{ zIndex: 2 }}>
          <button type="button" className="btn btn-sm btn-outline-secondary" onClick={() => aoArquivar(campanha)}>
            {campanha.arquivada ? 'Desarquivar' : 'Arquivar'}
          </button>
          <button type="button" className="btn btn-sm btn-outline-danger" onClick={() => aoExcluir(campanha)}>
            Excluir
          </button>
        </div>
      </div>
    </div>
  );
}

export default function Campanhas() {
  const [campanhas, setCampanhas] = useState(null);
  const [sistemas, setSistemas] = useState([]);
  const [erro, setErro] = useState('');
  const [criando, setCriando] = useState(false);
  const [mostrarArquivadas, setMostrarArquivadas] = useState(false);
  const [paraExcluir, setParaExcluir] = useState(null);

  const carregar = useCallback(async () => {
    try {
      const [lista, listaSistemas] = await Promise.all([campanhasApi.listarCampanhas(), campanhasApi.listarSistemas()]);
      setCampanhas(lista);
      setSistemas(listaSistemas);
    } catch (e) {
      setErro(mensagemDeErro(e));
    }
  }, []);

  useEffect(() => {
    carregar();
  }, [carregar]);

  async function criar(dados) {
    await campanhasApi.criarCampanha(dados);
    setCriando(false);
    await carregar();
  }

  async function alternarArquivada(campanha) {
    setErro('');
    try {
      if (campanha.arquivada) await campanhasApi.desarquivarCampanha(campanha.id);
      else await campanhasApi.arquivarCampanha(campanha.id);
      await carregar();
    } catch (e) {
      setErro(mensagemDeErro(e));
    }
  }

  async function excluir() {
    setErro('');
    try {
      await campanhasApi.excluirCampanha(paraExcluir.id);
      setParaExcluir(null);
      await carregar();
    } catch (e) {
      setParaExcluir(null);
      setErro(mensagemDeErro(e));
    }
  }

  const fecharExclusao = useCallback(() => setParaExcluir(null), []);
  const { ativas, arquivadas } = separarCampanhas(campanhas ?? []);

  return (
    <>
      <Cabecalho />
      <main className="container pb-5">
        <div className="d-flex justify-content-between align-items-center mb-3">
          <h1 className="h3 mb-0">Campanhas</h1>
          {!criando && (
            <button type="button" className="btn btn-primary" onClick={() => setCriando(true)}>
              Nova campanha
            </button>
          )}
        </div>

        {erro && <div className="alert alert-danger" role="alert">{erro}</div>}

        {criando && (
          <div className="card mb-4">
            <div className="card-body">
              <h2 className="h5 mb-3">Nova campanha</h2>
              <FormularioCampanha sistemas={sistemas} textoBotao="Criar campanha"
                aoSalvar={criar} aoCancelar={() => setCriando(false)} />
            </div>
          </div>
        )}

        {campanhas === null && !erro && <Carregando />}

        {campanhas !== null && ativas.length === 0 && !criando && (
          <p className="text-secondary">Nenhuma campanha ainda. Crie a primeira!</p>
        )}

        <div className="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-3">
          {ativas.map((c) => (
            <CartaoCampanha key={c.id} campanha={c} aoArquivar={alternarArquivada} aoExcluir={setParaExcluir} />
          ))}
        </div>

        {arquivadas.length > 0 && (
          <section className="mt-5">
            <button type="button" className="btn btn-link px-0 text-decoration-none"
              aria-expanded={mostrarArquivadas} onClick={() => setMostrarArquivadas((v) => !v)}>
              {mostrarArquivadas ? '▾' : '▸'} Arquivadas ({arquivadas.length})
            </button>
            {mostrarArquivadas && (
              <div className="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-3 mt-1">
                {arquivadas.map((c) => (
                  <CartaoCampanha key={c.id} campanha={c} aoArquivar={alternarArquivada} aoExcluir={setParaExcluir} />
                ))}
              </div>
            )}
          </section>
        )}
      </main>

      <ConfirmarExclusao
        aberto={paraExcluir !== null}
        titulo="Excluir campanha"
        texto={paraExcluir ? textoExclusao(paraExcluir.nome) : ''}
        aoConfirmar={excluir}
        aoCancelar={fecharExclusao}
      />
    </>
  );
}
