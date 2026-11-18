import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { mensagemDeErro } from '../api/erros.js';
import { criarSessao, listarSessoes } from '../api/sessoes.js';
import { Carregando } from '../auth/RotaProtegida.jsx';
import { useCaderno } from '../caderno/useCaderno.js';
import Cabecalho from '../componentes/Cabecalho.jsx';
import { formatarData } from '../sessoes/passos.js';
import CabecalhoSecao from '../visual/CabecalhoSecao.jsx';

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
      <Cabecalho idCampanha={id} />
      <main className="container pb-5">
        <CabecalhoSecao
          chamada={campanha?.nome ?? 'Campanha'}
          titulo="Sessões"
          subtitulo="Prepare cada sessão nos 8 passos. Os segredos não revelados seguem para a próxima."
          acoes={(
            <button type="button" className="btn btn-primary btn-lg" onClick={nova} disabled={criando}>
              {criando ? 'Criando…' : 'Nova sessão'}
            </button>
          )}
        />

        {erro && <div className="alert alert-danger" role="alert">{erro}</div>}
        {sessoes === null && !erro && <Carregando />}
        {sessoes?.length === 0 && <div className="aviso-astral">Nenhuma sessão ainda. Crie a primeira!</div>}

        <div className="d-grid gap-3">
          {sessoes?.map((s, i) => (
            <div key={s.id} className="cartao-registro surgir" style={{ '--cor-tipo': i === 0 ? '#c4b5fd' : '#60a5fa' }}>
              <div className="d-flex flex-wrap align-items-center gap-3">
                <span className="selo-escudo" style={{ fontSize: '1.05rem' }}>{s.numero}</span>
                <div className="flex-grow-1" style={{ minWidth: 0 }}>
                  <div className="d-flex flex-wrap align-items-baseline gap-2">
                    <h3 className="mb-0">Sessão {s.numero}</h3>
                    {i === 0 && <span className="chip">mais recente</span>}
                    {s.data && <span className="text-secondary small">{formatarData(s.data)}</span>}
                  </div>
                  <p className="resumo mb-0">{s.comecoForte || 'Sem começo forte ainda.'}</p>
                </div>
                <div className="d-flex gap-2 position-relative" style={{ zIndex: 2 }}>
                  <Link to={`/campanhas/${id}/escudo?sessao=${s.id}`} className="btn btn-sm btn-primary">Escudo</Link>
                  <Link to={`/campanhas/${id}/sessoes/${s.id}`} className="btn btn-sm btn-fantasma">Preparar</Link>
                </div>
              </div>
            </div>
          ))}
        </div>
      </main>
    </>
  );
}
