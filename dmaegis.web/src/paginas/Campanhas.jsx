import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router';
import * as campanhasApi from '../api/campanhas.js';
import { mensagemDeErro } from '../api/erros.js';
import { Carregando } from '../auth/RotaProtegida.jsx';
import FormularioCampanha from '../campanhas/FormularioCampanha.jsx';
import { separarCampanhas } from '../campanhas/organizar.js';
import SeloSistema from '../campanhas/SeloSistema.jsx';
import { textoExclusao } from '../campanhas/textos.js';
import Cabecalho from '../componentes/Cabecalho.jsx';
import ConfirmarExclusao from '../componentes/ConfirmarExclusao.jsx';
import ArteCampanha from '../visual/ArteCampanha.jsx';
import CabecalhoSecao from '../visual/CabecalhoSecao.jsx';

function CartaoCampanha({ campanha, aoArquivar, aoExcluir }) {
  return (
    <div className="col surgir">
      <article className={`cartao-arte ${campanha.arquivada ? 'opacity-75' : ''}`}>
        <div className="arte">
          <ArteCampanha semente={campanha.id} />
        </div>
        <span className="selo-canto">
          <SeloSistema idSistema={campanha.idSistema} nomeSistema={campanha.nomeSistema} />
        </span>
        <div className="corpo">
          <h3>
            <Link to={`/campanhas/${campanha.id}`} className="stretched-link text-decoration-none titulo-estelar">
              {campanha.nome}
            </Link>
          </h3>
          {campanha.descricao && (
            <p className="text-secondary mb-0"
              style={{ display: '-webkit-box', WebkitLineClamp: 3, WebkitBoxOrient: 'vertical', overflow: 'hidden' }}>
              {campanha.descricao}
            </p>
          )}
        </div>
        <div className="rodape">
          <Link to={`/campanhas/${campanha.id}/escudo`} className="btn btn-sm btn-primary">Escudo</Link>
          <button type="button" className="btn btn-sm btn-fantasma ms-auto" onClick={() => aoArquivar(campanha)}>
            {campanha.arquivada ? 'Desarquivar' : 'Arquivar'}
          </button>
          <button type="button" className="btn btn-sm btn-outline-danger" onClick={() => aoExcluir(campanha)}>Excluir</button>
        </div>
      </article>
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
        <CabecalhoSecao
          chamada="Suas mesas"
          titulo="Campanhas"
          subtitulo="Cada campanha guarda o caderno, as sessões e os mapas. Abra o escudo e conduza o jogo."
          acoes={!criando && (
            <button type="button" className="btn btn-primary btn-lg" onClick={() => setCriando(true)}>Nova campanha</button>
          )}
        >
          {campanhas && (
            <div className="numeros-destaque">
              <div className="numero-destaque"><strong>{ativas.length}</strong><span>ativas</span></div>
              <div className="numero-destaque"><strong>{arquivadas.length}</strong><span>arquivadas</span></div>
              <div className="numero-destaque"><strong>{new Set(ativas.map((c) => c.idSistema)).size}</strong><span>sistemas em jogo</span></div>
            </div>
          )}
        </CabecalhoSecao>

        {erro && <div className="alert alert-danger" role="alert">{erro}</div>}

        {criando && (
          <div className="card cartao-astral mb-4 surgir">
            <div className="card-body p-4">
              <h2 className="h4 mb-3">Nova campanha</h2>
              <FormularioCampanha sistemas={sistemas} textoBotao="Criar campanha"
                aoSalvar={criar} aoCancelar={() => setCriando(false)} />
            </div>
          </div>
        )}

        {campanhas === null && !erro && <Carregando />}

        {campanhas !== null && ativas.length === 0 && !criando && (
          <div className="aviso-astral">Nenhuma campanha ainda. Crie a primeira!</div>
        )}

        <div className="row row-cols-1 row-cols-md-2 row-cols-xl-3 g-4">
          {ativas.map((c) => (
            <CartaoCampanha key={c.id} campanha={c} aoArquivar={alternarArquivada} aoExcluir={setParaExcluir} />
          ))}
        </div>

        {arquivadas.length > 0 && (
          <section className="mt-5">
            <button type="button" className="btn btn-link px-0 text-decoration-none rotulo-secao"
              aria-expanded={mostrarArquivadas} onClick={() => setMostrarArquivadas((v) => !v)}>
              {mostrarArquivadas ? '▾' : '▸'} Arquivadas ({arquivadas.length})
            </button>
            {mostrarArquivadas && (
              <div className="row row-cols-1 row-cols-md-2 row-cols-xl-3 g-4 mt-1">
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
