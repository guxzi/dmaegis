import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams, useSearchParams } from 'react-router';
import { listarRegistros } from '../api/caderno.js';
import { mensagemDeErro } from '../api/erros.js';
import { listarImagensDaCampanha } from '../api/imagens.js';
import { criarSessao, listarSessoes, obterSessao } from '../api/sessoes.js';
import { Carregando } from '../auth/RotaProtegida.jsx';
import SeloSistema from '../campanhas/SeloSistema.jsx';
import { useCaderno } from '../caderno/useCaderno.js';
import RoladorDados from '../dados/RoladorDados.jsx';
import Cabecalho from '../componentes/Cabecalho.jsx';
import { sessaoPadrao } from '../escudo/atalhos.js';
import CartaoRegistro from '../escudo/CartaoRegistro.jsx';
import PainelAnotacoes from '../escudo/PainelAnotacoes.jsx';
import PainelRegras from '../escudo/PainelRegras.jsx';
import PainelImproviso from '../improviso/PainelImproviso.jsx';
import PainelSessao from '../escudo/PainelSessao.jsx';
import { retratosPorRegistro } from '../imagens/retratos.js';
import { abrirProjecao } from '../projecao/canal.js';

// Altura disponível abaixo do cabeçalho e da barra do escudo: os painéis rolam por dentro.
const ALTURA_PAINEIS = 'calc(100vh - 130px)';

// O escudo: a tela da sessão. Tudo o que o mestre precisa sem abrir outra aba.
export default function Escudo() {
  const { id } = useParams();
  const navegar = useNavigate();
  const [parametros, setParametros] = useSearchParams();
  const { campanha, rotulo, naoEncontrada } = useCaderno(id);
  const [sessoes, setSessoes] = useState(null);
  const [sessao, setSessao] = useState(null);
  const [registros, setRegistros] = useState([]);
  const [idRegistroAberto, setIdRegistroAberto] = useState(null);
  const [erro, setErro] = useState('');
  const [retratos, setRetratos] = useState({});
  const [anotacoes, setAnotacoes] = useState([]);
  const [aba, setAba] = useState('anotacoes');

  const idSessao = Number(parametros.get('sessao')) || (sessoes ? sessaoPadrao(sessoes) : null);

  useEffect(() => {
    listarSessoes(id).then(setSessoes).catch((e) => setErro(mensagemDeErro(e)));
    listarRegistros(id).then(setRegistros).catch(() => {});
    listarImagensDaCampanha(id).then((lista) => setRetratos(retratosPorRegistro(lista))).catch(() => {});
  }, [id]);

  useEffect(() => {
    if (!idSessao) return undefined;
    let ativo = true;
    obterSessao(idSessao)
      .then((s) => ativo && setSessao(s))
      .catch((e) => ativo && setErro(mensagemDeErro(e)));
    return () => {
      ativo = false;
    };
  }, [idSessao]);

  const escolherSessao = (valor) => {
    setSessao(null);
    setParametros({ sessao: valor }, { replace: true });
  };

  async function criarPrimeira() {
    try {
      const nova = await criarSessao(id);
      navegar(`/campanhas/${id}/sessoes/${nova.id}`);
    } catch (e) {
      setErro(mensagemDeErro(e));
    }
  }

  const fecharCartao = useCallback(() => setIdRegistroAberto(null), []);

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
      <Cabecalho idCampanha={id} aoEscolherRegistro={setIdRegistroAberto} className="mb-2" />
      <main className="container-fluid px-3 px-lg-4">
        <div className="d-flex flex-wrap align-items-center gap-3 mb-2">
          <Link to={`/campanhas/${id}`} className="text-decoration-none small">← Campanha</Link>
          <h1 className="h4 mb-0">{campanha?.nome ?? '…'}</h1>
          {campanha && <SeloSistema idSistema={campanha.idSistema} nomeSistema={campanha.nomeSistema} />}
          {sessoes?.length > 0 && (
            <div className="d-flex align-items-center gap-2 ms-auto">
              <label htmlFor="sessaoEscudo" className="small text-secondary">Sessão</label>
              <select id="sessaoEscudo" className="form-select form-select-sm" style={{ width: 'auto' }}
                value={idSessao ?? ''} onChange={(e) => escolherSessao(e.target.value)}>
                {sessoes.map((s) => <option key={s.id} value={s.id}>Sessão {s.numero}</option>)}
              </select>
              {idSessao && (
                <Link to={`/campanhas/${id}/sessoes/${idSessao}`} className="btn btn-sm btn-outline-secondary">Preparar</Link>
              )}
              <button type="button" className="btn btn-sm btn-fantasma" onClick={abrirProjecao}
                title="Janela para compartilhar a tela ou levar ao segundo monitor">Abrir projeção</button>
            </div>
          )}
        </div>

        {erro && <div className="alert alert-danger py-2">{erro}</div>}

        {sessoes === null && !erro && <Carregando />}

        {sessoes?.length === 0 && (
          <div className="card cartao-astral mx-auto mt-5 text-center" style={{ maxWidth: 480 }}>
            <div className="card-body p-4">
              <h2 className="h5">Nenhuma sessão ainda</h2>
              <p className="text-secondary">Crie a primeira sessão para preparar e conduzir o jogo por aqui.</p>
              <button type="button" className="btn btn-primary" onClick={criarPrimeira}>Criar a primeira sessão</button>
            </div>
          </div>
        )}

        {sessoes?.length > 0 && (
          <div className="row g-3">
            <div className="col-lg-7">
              <div className="card cartao-astral painel-escudo" style={{ height: ALTURA_PAINEIS }}>
                <div className="card-header d-flex align-items-center gap-2">
                  <span className="titulo-runico">Sessão de hoje</span>
                </div>
                <div className="card-body">
                  {sessao && campanha
                    ? <PainelSessao key={sessao.id} sessao={sessao} registros={registros} idSistema={campanha.idSistema}
                        retratos={retratos} anotacoes={anotacoes}
                        aoAbrirRegistro={setIdRegistroAberto}
                        aoMudarSegredos={(segredos) => setSessao((s) => ({ ...s, segredos }))} />
                    : <Carregando />}
                </div>
              </div>
            </div>

            <div className="col-lg-5 d-flex flex-column gap-3" style={{ height: ALTURA_PAINEIS }}>
              <div className="card painel-escudo flex-grow-1" style={{ minHeight: 0 }}>
                <div className="card-header">
                  <div className="abas-escudo" role="tablist" aria-label="Anotações, regras ou improviso">
                    <button type="button" role="tab" id="abaAnotacoes" aria-selected={aba === 'anotacoes'}
                      aria-controls="painelAnotacoes" onClick={() => setAba('anotacoes')}>Anotações</button>
                    <button type="button" role="tab" id="abaRegras" aria-selected={aba === 'regras'}
                      aria-controls="painelRegras" onClick={() => setAba('regras')}>Regras</button>
                    <button type="button" role="tab" id="abaImproviso" aria-selected={aba === 'improviso'}
                      aria-controls="painelImproviso" onClick={() => setAba('improviso')}>Improviso</button>
                  </div>
                </div>
                {/* As anotações continuam montadas na aba de regras, para o resumo vivo seguir atualizado. */}
                <div className={`card-body flex-column ${aba === 'anotacoes' ? 'd-flex' : 'd-none'}`} style={{ minHeight: 0 }}
                  role="tabpanel" id="painelAnotacoes" aria-labelledby="abaAnotacoes">
                  {idSessao && (
                    <PainelAnotacoes key={idSessao} idSessao={idSessao} registros={registros}
                      aoAbrirRegistro={setIdRegistroAberto} aoMudar={setAnotacoes} />
                  )}
                </div>
                {aba === 'regras' && (
                  <div className="card-body d-flex flex-column" style={{ minHeight: 0 }}
                    role="tabpanel" id="painelRegras" aria-labelledby="abaRegras">
                    {campanha && <PainelRegras idSistema={campanha.idSistema} />}
                  </div>
                )}
                {aba === 'improviso' && (
                  <div className="card-body overflow-auto" style={{ minHeight: 0 }}
                    role="tabpanel" id="painelImproviso" aria-labelledby="abaImproviso">
                    <PainelImproviso idCampanha={id}
                      aoSalvarRegistro={(registro) => setRegistros((lista) => [...lista, registro])} />
                  </div>
                )}
              </div>
              <div className="card painel-escudo flex-shrink-0" style={{ height: 300 }}>
                <div className="card-header"><span className="titulo-runico">Dados</span></div>
                <div className="card-body d-flex flex-column" style={{ minHeight: 0 }}>
                  <RoladorDados />
                </div>
              </div>
            </div>
          </div>
        )}
      </main>

      {idRegistroAberto && (
        <CartaoRegistro key={idRegistroAberto} idCampanha={id} idRegistro={idRegistroAberto} rotulo={rotulo}
          aoFechar={fecharCartao} />
      )}
    </>
  );
}
