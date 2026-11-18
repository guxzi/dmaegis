import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { mensagemDeErro } from '../api/erros.js';
import { criarSessao, listarSessoes } from '../api/sessoes.js';
import { Carregando } from '../auth/RotaProtegida.jsx';
import { useCaderno } from '../caderno/useCaderno.js';
import Cabecalho from '../componentes/Cabecalho.jsx';
import { formatarData } from '../sessoes/passos.js';

export default function Sessoes() {
  const { id } = useParams();
  const navegar = useNavigate();
  const { campanha, naoEncontrada } = useCaderno(id);
  const [sessoes, setSessoes] = useState(null);
  const [erro, setErro] = useState('');
  const [criando, setCriando] = useState(false);

  useEffect(() => {
    listarSessoes(id).then(setSessoes).catch((e) => setErro(mensagemDeErro(e)));
  }, [id]);

  async function nova() {
    setErro('');
    setCriando(true);
    try {
      const sessao = await criarSessao(id);
      navegar(`/campanhas/${id}/sessoes/${sessao.id}`);
    } catch (e) {
      setErro(mensagemDeErro(e));
      setCriando(false);
    }
  }

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
      <Cabecalho />
      <main className="container pb-5">
        <Link to={`/campanhas/${id}`} className="d-inline-block mb-3 text-decoration-none">← {campanha?.nome ?? 'Campanha'}</Link>
        <div className="d-flex justify-content-between align-items-center mb-3">
          <h1 className="h3 mb-0">Sessões</h1>
          <button type="button" className="btn btn-primary" onClick={nova} disabled={criando}>
            {criando ? 'Criando…' : 'Nova sessão'}
          </button>
        </div>

        {erro && <div className="alert alert-danger" role="alert">{erro}</div>}
        {sessoes === null && !erro && <Carregando />}
        {sessoes?.length === 0 && <p className="text-secondary">Nenhuma sessão ainda.</p>}

        <div className="list-group">
          {sessoes?.map((s) => (
            <Link key={s.id} to={`/campanhas/${id}/sessoes/${s.id}`} className="list-group-item list-group-item-action">
              <div className="d-flex justify-content-between">
                <span className="fw-semibold">Sessão {s.numero}</span>
                <span className="text-secondary small">{formatarData(s.data)}</span>
              </div>
              {s.comecoForte && <div className="text-secondary small text-truncate">{s.comecoForte}</div>}
            </Link>
          ))}
        </div>
      </main>
    </>
  );
}
