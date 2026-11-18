import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { listarRegistros } from '../api/caderno.js';
import { mensagemDeErro } from '../api/erros.js';
import * as sessoesApi from '../api/sessoes.js';
import { Carregando } from '../auth/RotaProtegida.jsx';
import { useCaderno } from '../caderno/useCaderno.js';
import Cabecalho from '../componentes/Cabecalho.jsx';
import ConfirmarExclusao from '../componentes/ConfirmarExclusao.jsx';
import ListaCenas from '../sessoes/ListaCenas.jsx';
import ListaSegredos from '../sessoes/ListaSegredos.jsx';
import { ID_TIPO_PJ, PASSOS_DE_VINCULO, titulosDosPassos } from '../sessoes/passos.js';
import VinculosDoPasso from '../sessoes/VinculosDoPasso.jsx';

// Campos salvos pelo botão "Salvar preparo" (os segredos são salvos na hora, à parte).
const paraFormulario = (sessao) => ({
  numero: sessao.numero,
  data: sessao.data ?? '',
  notasPersonagens: sessao.notasPersonagens ?? '',
  comecoForte: sessao.comecoForte ?? '',
  resumo: sessao.resumo ?? '',
  cenas: sessao.cenas,
  idsRegistro: sessao.idsRegistro,
});

function Passo({ numero, titulo, children }) {
  return (
    <section className="card mb-3">
      <div className="card-body">
        <h2 className="h6 text-uppercase text-secondary mb-3 d-flex align-items-center">
          <span className="selo-escudo me-2">{numero}</span>{titulo}
        </h2>
        {children}
      </div>
    </section>
  );
}

export default function SessaoPreparo() {
  const { id, idSessao } = useParams();
  const idSessaoNumero = Number(idSessao);
  const navegar = useNavigate();
  const { campanha, rotulo } = useCaderno(id);
  const [registros, setRegistros] = useState([]);
  const [form, setForm] = useState(null);
  const [segredos, setSegredos] = useState([]);
  const [alterado, setAlterado] = useState(false);
  const [estadoSalvar, setEstadoSalvar] = useState(''); // '', 'salvando', 'salvo'
  const [erro, setErro] = useState('');
  const [naoEncontrada, setNaoEncontrada] = useState(false);
  const [confirmandoExclusao, setConfirmandoExclusao] = useState(false);

  useEffect(() => {
    sessoesApi.obterSessao(idSessao)
      .then((sessao) => {
        setForm(paraFormulario(sessao));
        setSegredos(sessao.segredos);
        setAlterado(false);
      })
      .catch((e) => (e.response?.status === 404 ? setNaoEncontrada(true) : setErro(mensagemDeErro(e))));
  }, [idSessao]);

  useEffect(() => {
    listarRegistros(id).then(setRegistros).catch(() => {});
  }, [id]);

  // Avisa ao fechar ou recarregar a aba com alterações não salvas.
  useEffect(() => {
    if (!alterado) return undefined;
    const avisar = (e) => e.preventDefault();
    window.addEventListener('beforeunload', avisar);
    return () => window.removeEventListener('beforeunload', avisar);
  }, [alterado]);

  const mudar = (campo, valor) => {
    setForm((atual) => ({ ...atual, [campo]: valor }));
    setAlterado(true);
    setEstadoSalvar('');
  };

  async function salvar() {
    setErro('');
    setEstadoSalvar('salvando');
    try {
      const sessao = await sessoesApi.salvarSessao(idSessao, {
        ...form,
        numero: Number(form.numero),
        data: form.data || null,
      });
      setForm(paraFormulario(sessao));
      setSegredos(sessao.segredos);
      setAlterado(false);
      setEstadoSalvar('salvo');
    } catch (e) {
      setErro(mensagemDeErro(e));
      setEstadoSalvar('');
    }
  }

  async function excluir() {
    try {
      await sessoesApi.excluirSessao(idSessao);
      setAlterado(false);
      navegar(`/campanhas/${id}/sessoes`, { replace: true });
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
          <div className="alert alert-warning">Sessão não encontrada. <Link to={`/campanhas/${id}/sessoes`}>Voltar</Link></div>
        </main>
      </>
    );
  }

  const titulos = titulosDosPassos(campanha?.idSistema);
  const pjs = registros.filter((r) => r.idTipoRegistro === ID_TIPO_PJ);

  return (
    <>
      <Cabecalho idCampanha={id} />
      <main className="container" style={{ maxWidth: 900, paddingBottom: 96 }}>
        <div className="d-flex justify-content-between align-items-center mb-3">
          <Link to={`/campanhas/${id}/sessoes`} className="text-decoration-none">← Sessões</Link>
          <Link to={`/campanhas/${id}/escudo?sessao=${idSessao}`} className="btn btn-sm btn-outline-primary">Abrir no escudo</Link>
        </div>
        {erro && <div className="alert alert-danger" role="alert">{erro}</div>}
        {!form || !campanha ? (!erro && <Carregando />) : (
          <>
            <div className="d-flex flex-wrap align-items-end gap-3 mb-4">
              <h1 className="h3 mb-0 me-auto">Sessão {form.numero}</h1>
              <div>
                <label htmlFor="numeroSessao" className="form-label small mb-1">Número</label>
                <input id="numeroSessao" type="number" min={1} max={9999} className="form-control form-control-sm" style={{ width: 90 }}
                  value={form.numero} onChange={(e) => mudar('numero', e.target.value)} />
              </div>
              <div>
                <label htmlFor="dataSessao" className="form-label small mb-1">Data</label>
                <input id="dataSessao" type="date" className="form-control form-control-sm"
                  value={form.data} onChange={(e) => mudar('data', e.target.value)} />
              </div>
            </div>

            <Passo numero={1} titulo={titulos[0]}>
              <div className="d-flex flex-wrap gap-2 mb-2">
                {pjs.length === 0 && <span className="text-secondary small">Nenhum PJ no caderno.</span>}
                {pjs.map((pj) => (
                  <Link key={pj.id} to={`/campanhas/${id}/caderno/${pj.id}`} className="badge text-bg-success text-decoration-none">
                    {pj.nome}
                  </Link>
                ))}
              </div>
              <textarea className="form-control" rows={2} maxLength={5000} aria-label="Notas sobre os personagens"
                placeholder="O que cada personagem quer ou está passando…"
                value={form.notasPersonagens} onChange={(e) => mudar('notasPersonagens', e.target.value)} />
            </Passo>

            <Passo numero={2} titulo={titulos[1]}>
              <textarea className="form-control" rows={2} maxLength={5000} aria-label="Começo forte"
                placeholder="Algo acontece logo de cara…"
                value={form.comecoForte} onChange={(e) => mudar('comecoForte', e.target.value)} />
            </Passo>

            <Passo numero={3} titulo={titulos[2]}>
              <ListaCenas cenas={form.cenas} aoMudar={(cenas) => mudar('cenas', cenas)} />
            </Passo>

            <Passo numero={4} titulo={titulos[3]}>
              <ListaSegredos idSessao={idSessaoNumero} segredos={segredos} aoMudar={setSegredos} />
            </Passo>

            {PASSOS_DE_VINCULO.map(({ passo, idTipo }) => (
              <Passo key={passo} numero={passo} titulo={titulos[passo - 1]}>
                <VinculosDoPasso idCampanha={id} idTipo={idTipo} rotuloTipo={rotulo(idTipo)} registros={registros}
                  idsRegistro={form.idsRegistro} aoMudar={(ids) => mudar('idsRegistro', ids)} />
              </Passo>
            ))}

            <section className="card mb-3">
              <div className="card-body">
                <h2 className="h6 text-uppercase text-secondary mb-3">Resumo pós-sessão</h2>
                <textarea className="form-control" rows={4} maxLength={20000} aria-label="Resumo pós-sessão"
                  placeholder="O que aconteceu na mesa…"
                  value={form.resumo} onChange={(e) => mudar('resumo', e.target.value)} />
              </div>
            </section>
          </>
        )}
      </main>

      {form && (
        <div className="position-fixed bottom-0 start-0 end-0 bg-body border-top py-2" style={{ zIndex: 1000 }}>
          <div className="container d-flex align-items-center gap-3" style={{ maxWidth: 900 }}>
            <button type="button" className="btn btn-primary" onClick={salvar} disabled={estadoSalvar === 'salvando'}>
              {estadoSalvar === 'salvando' ? 'Salvando…' : 'Salvar preparo'}
            </button>
            <span className={`small ${alterado ? 'text-warning-emphasis' : 'text-success'}`} role="status">
              {alterado ? 'Alterações não salvas' : estadoSalvar === 'salvo' ? 'Salvo' : ''}
            </span>
            <button type="button" className="btn btn-outline-danger ms-auto" onClick={() => setConfirmandoExclusao(true)}>
              Excluir sessão
            </button>
          </div>
        </div>
      )}

      <ConfirmarExclusao
        aberto={confirmandoExclusao}
        titulo="Excluir sessão"
        texto={form ? `Excluir a sessão ${form.numero}? As cenas, os vínculos e as anotações dela serão apagados. Os segredos continuam na campanha.` : ''}
        aoConfirmar={excluir}
        aoCancelar={fecharExclusao}
      />
    </>
  );
}
