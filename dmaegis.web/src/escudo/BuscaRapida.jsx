import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router';
import { mensagemDeErro } from '../api/erros.js';
import { buscar } from '../api/escudo.js';
import SeloTipo from '../caderno/SeloTipo.jsx';
import { useCaderno } from '../caderno/useCaderno.js';
import { ehAtalhoBusca, proximoIndice } from './atalhos.js';

// Janela da busca: só existe enquanto está aberta (assim os dados da campanha só carregam quando precisa).
function JanelaBusca({ idCampanha, aoEscolher, aoFechar }) {
  const { rotulo } = useCaderno(idCampanha);
  const [termo, setTermo] = useState('');
  const [resultados, setResultados] = useState([]);
  const [ativo, setAtivo] = useState(-1);
  const [buscando, setBuscando] = useState(false);
  const [erro, setErro] = useState('');
  const campo = useRef(null);

  useEffect(() => {
    campo.current?.focus();
  }, []);

  // Espera 200 ms sem digitar antes de buscar.
  useEffect(() => {
    if (!termo.trim()) return undefined;
    let ativa = true;
    const espera = setTimeout(() => {
      setBuscando(true);
      buscar(idCampanha, termo)
        .then((lista) => {
          if (!ativa) return;
          setResultados(lista);
          setAtivo(lista.length > 0 ? 0 : -1);
          setErro('');
        })
        .catch((e) => ativa && setErro(mensagemDeErro(e)))
        .finally(() => ativa && setBuscando(false));
    }, 200);
    return () => {
      ativa = false;
      clearTimeout(espera);
    };
  }, [idCampanha, termo]);

  const aoDigitar = (texto) => {
    setTermo(texto);
    if (!texto.trim()) {
      setResultados([]);
      setAtivo(-1);
    }
  };

  function aoTeclar(evento) {
    if (evento.key === 'ArrowDown' || evento.key === 'ArrowUp') {
      evento.preventDefault();
      setAtivo((atual) => proximoIndice(atual, evento.key, resultados.length));
    } else if (evento.key === 'Enter') {
      evento.preventDefault();
      if (ativo >= 0 && resultados[ativo]) aoEscolher(resultados[ativo]);
    } else if (evento.key === 'Escape') {
      evento.preventDefault();
      aoFechar();
    }
  }

  return (
    <>
      <div className="modal d-block" tabIndex={-1} role="dialog" aria-modal="true" aria-label="Busca rápida"
        onMouseDown={(e) => e.target === e.currentTarget && aoFechar()}>
        <div className="modal-dialog" style={{ marginTop: '12vh', maxWidth: 620 }}>
          <div className="modal-content cartao-astral">
            <div className="p-3 border-bottom">
              <input ref={campo} type="search" className="form-control form-control-lg" role="combobox"
                aria-expanded={resultados.length > 0} aria-controls="resultadosBusca"
                aria-activedescendant={ativo >= 0 ? `resultado${ativo}` : undefined}
                placeholder="Buscar NPC, local, criatura, tag…" value={termo}
                onChange={(e) => aoDigitar(e.target.value)} onKeyDown={aoTeclar} />
            </div>

            <div style={{ maxHeight: '50vh', overflowY: 'auto' }}>
              {erro && <div className="alert alert-danger m-3">{erro}</div>}
              {termo.trim() && !buscando && resultados.length === 0 && !erro && (
                <p className="text-secondary text-center my-4">Nenhum registro encontrado.</p>
              )}
              <ul id="resultadosBusca" role="listbox" className="list-unstyled mb-0">
                {resultados.map((r, indice) => (
                  <li key={r.id} id={`resultado${indice}`} role="option" aria-selected={indice === ativo}
                    className={`resultado-busca px-3 py-2 ${indice === ativo ? 'ativo' : ''}`}
                    style={{ cursor: 'pointer' }}
                    onMouseEnter={() => setAtivo(indice)} onClick={() => aoEscolher(r)}>
                    <div className="d-flex flex-wrap align-items-center gap-2">
                      <span className="fw-semibold">{r.nome}</span>
                      <SeloTipo idTipo={r.idTipoRegistro} rotulo={rotulo(r.idTipoRegistro)} />
                      {r.tags.map((t) => <span key={t.id} className="badge rounded-pill text-bg-light border">{t.nome}</span>)}
                    </div>
                    {r.resumo && <div className="text-secondary small text-truncate">{r.resumo}</div>}
                  </li>
                ))}
              </ul>
            </div>

            <div className="px-3 py-2 border-top small text-secondary d-flex flex-wrap gap-3">
              <span><kbd>↑</kbd> <kbd>↓</kbd> navegar</span>
              <span><kbd>Enter</kbd> abrir</span>
              <span><kbd>Esc</kbd> fechar</span>
            </div>
          </div>
        </div>
      </div>
      <div className="modal-backdrop show" />
    </>
  );
}

// Busca rápida da campanha: Ctrl+K (ou ⌘K) em qualquer tela da campanha, mesmo com o foco num campo.
// Sem `aoEscolherRegistro`, escolher um resultado abre o registro no caderno.
export default function BuscaRapida({ idCampanha, aoEscolherRegistro, aberta, aoMudarAberta }) {
  const navegar = useNavigate();

  useEffect(() => {
    const aoTeclar = (evento) => {
      if (ehAtalhoBusca(evento)) {
        evento.preventDefault();
        aoMudarAberta(true);
      }
    };
    window.addEventListener('keydown', aoTeclar);
    return () => window.removeEventListener('keydown', aoTeclar);
  }, [aoMudarAberta]);

  if (!aberta) return null;

  const escolher = (registro) => {
    aoMudarAberta(false);
    if (aoEscolherRegistro) aoEscolherRegistro(registro.id);
    else navegar(`/campanhas/${idCampanha}/caderno/${registro.id}`);
  };

  return <JanelaBusca idCampanha={idCampanha} aoEscolher={escolher} aoFechar={() => aoMudarAberta(false)} />;
}
