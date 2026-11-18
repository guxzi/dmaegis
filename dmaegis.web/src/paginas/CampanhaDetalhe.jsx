import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import * as campanhasApi from '../api/campanhas.js';
import { mensagemDeErro } from '../api/erros.js';
import { Carregando } from '../auth/RotaProtegida.jsx';
import FormularioCampanha from '../campanhas/FormularioCampanha.jsx';
import SeloSistema from '../campanhas/SeloSistema.jsx';
import Cabecalho from '../componentes/Cabecalho.jsx';
import { textoExclusao } from '../campanhas/textos.js';
import ConfirmarExclusao from '../componentes/ConfirmarExclusao.jsx';

// Página da campanha. Nas próximas etapas ganha o caderno, as sessões e o escudo.
export default function CampanhaDetalhe() {
  const { id } = useParams();
  const navegar = useNavigate();
  const [campanha, setCampanha] = useState(null);
  const [naoEncontrada, setNaoEncontrada] = useState(false);
  const [erro, setErro] = useState('');
  const [editando, setEditando] = useState(false);
  const [confirmandoExclusao, setConfirmandoExclusao] = useState(false);

  useEffect(() => {
    campanhasApi
      .obterCampanha(id)
      .then(setCampanha)
      .catch((e) => (e.response?.status === 404 ? setNaoEncontrada(true) : setErro(mensagemDeErro(e))));
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

  let conteudo;
  if (naoEncontrada) {
    conteudo = (
      <div className="alert alert-warning">
        Campanha não encontrada. <Link to="/">Voltar para as campanhas</Link>
      </div>
    );
  } else if (!campanha) {
    conteudo = erro ? null : <Carregando />;
  } else if (editando) {
    conteudo = (
      <div className="card">
        <div className="card-body">
          <h1 className="h4 mb-3">Editar campanha</h1>
          <FormularioCampanha inicial={campanha} textoBotao="Salvar" aoSalvar={salvar}
            aoCancelar={() => setEditando(false)} />
        </div>
      </div>
    );
  } else {
    conteudo = (
      <>
        <div className="d-flex flex-wrap justify-content-between align-items-start gap-2 mb-2">
          <h1 className="h3 mb-0">
            {campanha.nome}
            {campanha.arquivada && <span className="badge text-bg-secondary ms-2 fs-6 align-middle">Arquivada</span>}
          </h1>
          <div className="d-flex gap-2">
            <button type="button" className="btn btn-sm btn-outline-primary" onClick={() => setEditando(true)}>Editar</button>
            <button type="button" className="btn btn-sm btn-outline-secondary" onClick={alternarArquivada}>
              {campanha.arquivada ? 'Desarquivar' : 'Arquivar'}
            </button>
            <button type="button" className="btn btn-sm btn-outline-danger" onClick={() => setConfirmandoExclusao(true)}>
              Excluir
            </button>
          </div>
        </div>
        <p className="mb-3">
          <SeloSistema idSistema={campanha.idSistema} nomeSistema={campanha.nomeSistema} />
          <span className="text-secondary small ms-2">O sistema não pode ser trocado.</span>
        </p>
        {campanha.descricao
          ? <p style={{ whiteSpace: 'pre-line' }}>{campanha.descricao}</p>
          : <p className="text-secondary fst-italic">Sem descrição.</p>}
        <div className="d-flex flex-wrap gap-2 mt-4">
          <Link to={`/campanhas/${campanha.id}/escudo`} className="btn btn-primary">Abrir escudo</Link>
          <Link to={`/campanhas/${campanha.id}/caderno`} className="btn btn-outline-primary">Caderno</Link>
          <Link to={`/campanhas/${campanha.id}/sessoes`} className="btn btn-outline-primary">Sessões</Link>
        </div>
      </>
    );
  }

  return (
    <>
      <Cabecalho idCampanha={id} />
      <main className="container pb-5">
        <Link to="/" className="d-inline-block mb-3 text-decoration-none">← Campanhas</Link>
        {erro && <div className="alert alert-danger" role="alert">{erro}</div>}
        {conteudo}
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
