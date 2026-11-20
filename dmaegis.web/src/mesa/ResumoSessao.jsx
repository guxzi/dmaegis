import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router';
import { mensagemDeErro } from '../api/erros.js';
import { anexarAoResumo, gerarResumo } from '../api/mesas.js';
import { listarSessoes } from '../api/sessoes.js';

const PERIODOS = [
  { horas: 4, rotulo: '4 h' },
  { horas: 8, rotulo: '8 h' },
  { horas: 12, rotulo: '12 h' },
  { horas: 24, rotulo: '24 h' },
];

// Janela "Resumo da sessão": o rascunho do que aconteceu na mesa (combates, quedas, críticos, entregas),
// que o mestre revisa e acrescenta ao resumo de uma sessão do caderno.
export default function ResumoSessao({ idCampanha, idMesa, aoFechar }) {
  const [horas, setHoras] = useState(8);
  const [texto, setTexto] = useState('');
  const [carregando, setCarregando] = useState(true);
  const [sessoes, setSessoes] = useState([]);
  const [idSessao, setIdSessao] = useState('');
  const [salvo, setSalvo] = useState(null);
  const [erro, setErro] = useState('');

  // Busca o rascunho (o "carregando" já começa ligado; ao trocar o período, o clique liga de novo).
  const buscar = useCallback((periodo) => gerarResumo(idMesa, periodo)
    .then((r) => setTexto(r.texto))
    .catch((e) => setErro(mensagemDeErro(e)))
    .finally(() => setCarregando(false)), [idMesa]);

  function gerar(periodo) {
    setCarregando(true);
    setSalvo(null);
    buscar(periodo);
  }

  useEffect(() => {
    buscar(8);
    // A sessão de número mais alto costuma ser a de hoje.
    listarSessoes(idCampanha).then((lista) => {
      const ordenadas = [...lista].sort((a, b) => b.numero - a.numero);
      setSessoes(ordenadas);
      if (ordenadas[0]) setIdSessao(String(ordenadas[0].id));
    }).catch(() => {});
  }, [buscar, idCampanha]);

  useEffect(() => {
    const aoTeclar = (e) => e.key === 'Escape' && aoFechar();
    window.addEventListener('keydown', aoTeclar);
    return () => window.removeEventListener('keydown', aoTeclar);
  }, [aoFechar]);

  async function anexar() {
    setErro('');
    try {
      await anexarAoResumo(Number(idSessao), texto);
      setSalvo(sessoes.find((s) => String(s.id) === idSessao) ?? null);
    } catch (e) {
      setErro(mensagemDeErro(e));
    }
  }

  return (
    <>
      <div className="modal d-block" tabIndex={-1} role="dialog" aria-modal="true" aria-labelledby="tituloResumo"
        onClick={(e) => e.target === e.currentTarget && aoFechar()}>
        <div className="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
          <div className="modal-content cartao-astral">
            <div className="modal-header">
              <h2 id="tituloResumo" className="modal-title h5">Resumo da sessão</h2>
              <button type="button" className="btn-close btn-close-white" onClick={aoFechar} aria-label="Fechar" />
            </div>
            <div className="modal-body">
              <div className="d-flex flex-wrap align-items-center gap-2 mb-2">
                <span className="small text-secondary">O que aconteceu nas últimas</span>
                <div className="segmentado" role="group" aria-label="Período">
                  {PERIODOS.map((p) => (
                    <button key={p.horas} type="button" aria-pressed={horas === p.horas}
                      onClick={() => { setHoras(p.horas); gerar(p.horas); }}>{p.rotulo}</button>
                  ))}
                </div>
              </div>
              <textarea className="form-control texto-resumo" rows={12} value={carregando ? 'Montando o resumo…' : texto}
                onChange={(e) => { setTexto(e.target.value); setSalvo(null); }} disabled={carregando} aria-label="Texto do resumo" />
              <p className="small text-secondary mt-1">É um rascunho: escreva por cima o que a história teve de importante.</p>

              {erro && <div className="alert alert-danger py-2">{erro}</div>}
              {sessoes.length === 0
                ? <p className="small text-secondary mb-0">Crie uma sessão no caderno para guardar este resumo.</p>
                : (
                  <div className="d-flex flex-wrap align-items-center gap-2">
                    <select className="form-select form-select-sm" style={{ width: 'auto' }} value={idSessao}
                      onChange={(e) => { setIdSessao(e.target.value); setSalvo(null); }} aria-label="Sessão">
                      {sessoes.map((s) => <option key={s.id} value={s.id}>Sessão {s.numero}</option>)}
                    </select>
                    <button type="button" className="btn btn-sm btn-primary" disabled={carregando || !texto.trim() || Boolean(salvo)} onClick={anexar}>
                      Acrescentar ao resumo da sessão
                    </button>
                    {salvo && (
                      <span className="small text-success">
                        Guardado. <Link to={`/campanhas/${idCampanha}/sessoes/${salvo.id}`}>Abrir a sessão {salvo.numero}</Link>
                      </span>
                    )}
                  </div>
                )}
            </div>
          </div>
        </div>
      </div>
      <div className="modal-backdrop show" />
    </>
  );
}
