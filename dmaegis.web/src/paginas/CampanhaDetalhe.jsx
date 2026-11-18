import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { listarRegistros } from '../api/caderno.js';
import * as campanhasApi from '../api/campanhas.js';
import { mensagemDeErro } from '../api/erros.js';
import { listarSessoes } from '../api/sessoes.js';
import { Carregando } from '../auth/RotaProtegida.jsx';
import FormularioCampanha from '../campanhas/FormularioCampanha.jsx';
import SeloSistema from '../campanhas/SeloSistema.jsx';
import { textoExclusao } from '../campanhas/textos.js';
import Cabecalho from '../componentes/Cabecalho.jsx';
import ConfirmarExclusao from '../componentes/ConfirmarExclusao.jsx';
import ArteCampanha from '../visual/ArteCampanha.jsx';
import PoeiraEstelar from '../visual/PoeiraEstelar.jsx';

const NPC = 1;
const LOCAL = 4;

// Visão geral da campanha: hero com a capa gerada, números e os caminhos principais.
export default function CampanhaDetalhe() {
  const { id } = useParams();
  const navegar = useNavigate();
  const [campanha, setCampanha] = useState(null);
  const [registros, setRegistros] = useState(null);
  const [sessoes, setSessoes] = useState(null);
  const [naoEncontrada, setNaoEncontrada] = useState(false);
  const [erro, setErro] = useState('');
  const [editando, setEditando] = useState(false);
  const [confirmandoExclusao, setConfirmandoExclusao] = useState(false);

  useEffect(() => {
    campanhasApi
      .obterCampanha(id)
      .then(setCampanha)
      .catch((e) => (e.response?.status === 404 ? setNaoEncontrada(true) : setErro(mensagemDeErro(e))));
    listarRegistros(id).then(setRegistros).catch(() => setRegistros([]));
    listarSessoes(id).then(setSessoes).catch(() => setSessoes([]));
  }, [id]);

  async function salvar(dados) {
    setCampanha(await campanhasApi.atualizarCampanha(id, dados));
    setEditando(false);
  }

  async function alternarArquivada() {
    setErro('');
    try {
      setCampanha(campanha.arquivada
        ? await campanhasApi.desarquivarCampanha(id)
        : await campanhasApi.arquivarCampanha(id));
    } catch (e) {
      setErro(mensagemDeErro(e));
    }
  }

  async function excluir() {
    try {
      await campanhasApi.excluirCampanha(id);
      navegar('/', { replace: true });
    } catch (e) {
      setConfirmandoExclusao(false);
      setErro(mensagemDeErro(e));
    }
  }

  const fecharExclusao = useCallback(() => setConfirmandoExclusao(false), []);

  if (naoEncontrada) {
    return (
      <>
        <Cabecalho />
        <main className="container">
          <div className="aviso-astral">Campanha não encontrada. <Link to="/">Voltar para as campanhas</Link></div>
        </main>
      </>
    );
  }

  const ultimaSessao = sessoes?.[0];
  const contar = (idTipo) => registros?.filter((r) => r.idTipoRegistro === idTipo).length ?? '–';

  return (
    <>
      <Cabecalho idCampanha={id} />
      <main className="container pb-5">
        {erro && <div className="alert alert-danger" role="alert">{erro}</div>}
        {!campanha && !erro && <Carregando />}

        {campanha && !editando && (
          <section className="hero surgir mb-4">
            <div className="hero-capa"><ArteCampanha semente={campanha.id} /></div>
            <PoeiraEstelar quantidade={35} />
            <div className="conteudo-hero">
              <div className="d-flex flex-wrap align-items-center gap-2">
                <span className="chamada">Campanha</span>
                <SeloSistema idSistema={campanha.idSistema} nomeSistema={campanha.nomeSistema} />
                {campanha.arquivada && <span className="chip">Arquivada</span>}
              </div>
              <h1 className="hero-titulo titulo-estelar" style={{ fontSize: 'clamp(2.2rem, 5vw, 3.8rem)' }}>{campanha.nome}</h1>
              {campanha.descricao
                ? <p className="fs-5 mb-0" style={{ maxWidth: 680, whiteSpace: 'pre-line', color: '#c9cff2' }}>{campanha.descricao}</p>
                : <p className="text-secondary fst-italic mb-0">Sem descrição.</p>}

              <div className="numeros-destaque">
                <div className="numero-destaque"><strong>{registros?.length ?? '–'}</strong><span>registros</span></div>
                <div className="numero-destaque"><strong>{contar(NPC)}</strong><span>NPCs</span></div>
                <div className="numero-destaque"><strong>{contar(LOCAL)}</strong><span>locais</span></div>
                <div className="numero-destaque"><strong>{sessoes?.length ?? '–'}</strong><span>sessões</span></div>
              </div>

              <div className="d-flex flex-wrap gap-2 mt-4">
                <Link to={`/campanhas/${campanha.id}/escudo`} className="btn btn-primary btn-lg">Abrir escudo</Link>
                {ultimaSessao && (
                  <Link to={`/campanhas/${campanha.id}/sessoes/${ultimaSessao.id}`} className="btn btn-fantasma btn-lg">
                    Preparar sessão {ultimaSessao.numero}
                  </Link>
                )}
                <Link to={`/campanhas/${campanha.id}/caderno`} className="btn btn-fantasma btn-lg">Caderno</Link>
              </div>
            </div>
          </section>
        )}

        {campanha && editando && (
          <div className="card cartao-astral surgir">
            <div className="card-body p-4">
              <h1 className="h3 mb-3">Editar campanha</h1>
              <p className="text-secondary small">O sistema ({campanha.nomeSistema}) não pode ser trocado.</p>
              <FormularioCampanha inicial={campanha} textoBotao="Salvar" aoSalvar={salvar} aoCancelar={() => setEditando(false)} />
            </div>
          </div>
        )}

        {campanha && !editando && (
          <div className="d-flex flex-wrap gap-2 justify-content-end">
            <button type="button" className="btn btn-sm btn-fantasma" onClick={() => setEditando(true)}>Editar</button>
            <button type="button" className="btn btn-sm btn-fantasma" onClick={alternarArquivada}>
              {campanha.arquivada ? 'Desarquivar' : 'Arquivar'}
            </button>
            <button type="button" className="btn btn-sm btn-outline-danger" onClick={() => setConfirmandoExclusao(true)}>Excluir</button>
          </div>
        )}
      </main>
      <ConfirmarExclusao
        aberto={confirmandoExclusao}
        titulo="Excluir campanha"
        texto={campanha ? textoExclusao(campanha.nome) : ''}
        aoConfirmar={excluir}
        aoCancelar={fecharExclusao}
      />
    </>
  );
}
