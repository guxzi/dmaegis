import { useEffect, useState } from 'react';
import { mensagemDeErro } from '../api/erros.js';
import { gerarConvite, listarConvites, revogarConvite } from '../api/jogador.js';
import { linkDeConvite } from './eventos.js';

const CHAVE_ENDERECO = 'dmaegis:endereco-mesa';

function lerEndereco() {
  try {
    return globalThis.localStorage?.getItem(CHAVE_ENDERECO) || window.location.origin;
  } catch {
    return window.location.origin;
  }
}

function gravarEndereco(valor) {
  try {
    globalThis.localStorage?.setItem(CHAVE_ENDERECO, valor);
  } catch {
    // Sem armazenamento: o campo só não é lembrado.
  }
}

// Janela "Jogadores": o endereço da mesa (o do túnel), um link por PJ e a trava de movimento.
// O código do link só existe na resposta de "Gerar": o banco guarda o hash. Gerar de novo troca o link.
export default function PainelJogadores({ idCampanha, mesa, aoMudarMovimento, aoFechar }) {
  const [convites, setConvites] = useState(null);
  const [endereco, setEndereco] = useState(lerEndereco);
  const [codigos, setCodigos] = useState({});
  const [copiado, setCopiado] = useState(null);
  const [erro, setErro] = useState('');

  useEffect(() => {
    listarConvites(idCampanha).then(setConvites).catch((e) => setErro(mensagemDeErro(e)));
  }, [idCampanha]);

  useEffect(() => {
    const aoTeclar = (e) => e.key === 'Escape' && aoFechar();
    window.addEventListener('keydown', aoTeclar);
    return () => window.removeEventListener('keydown', aoTeclar);
  }, [aoFechar]);

  async function gerar(idRegistro) {
    setErro('');
    try {
      const { codigo } = await gerarConvite(idRegistro);
      setCodigos((c) => ({ ...c, [idRegistro]: codigo }));
      setConvites((lista) => lista.map((c) => (c.idRegistro === idRegistro ? { ...c, temConvite: true, dataUltimoAcesso: null } : c)));
    } catch (e) {
      setErro(mensagemDeErro(e));
    }
  }

  async function revogar(idRegistro) {
    setErro('');
    try {
      await revogarConvite(idRegistro);
      setCodigos((atuais) => {
        const resto = { ...atuais };
        delete resto[idRegistro];
        return resto;
      });
      setConvites((lista) => lista.map((c) => (c.idRegistro === idRegistro ? { ...c, temConvite: false } : c)));
    } catch (e) {
      setErro(mensagemDeErro(e));
    }
  }

  async function copiar(idRegistro) {
    try {
      await navigator.clipboard.writeText(linkDeConvite(endereco, codigos[idRegistro]));
      setCopiado(idRegistro);
      setTimeout(() => setCopiado(null), 2000);
    } catch {
      setErro('Não deu para copiar. Selecione o link e copie com Ctrl+C.');
    }
  }

  const ehLocal = /localhost|127\.0\.0\.1/.test(endereco);

  return (
    <>
      <div className="modal d-block" tabIndex={-1} role="dialog" aria-modal="true" aria-labelledby="tituloJogadores"
        onClick={(e) => e.target === e.currentTarget && aoFechar()}>
        <div className="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
          <div className="modal-content cartao-astral">
            <div className="modal-header">
              <h2 id="tituloJogadores" className="modal-title h5">Jogadores na mesa</h2>
              <button type="button" className="btn-close btn-close-white" onClick={aoFechar} aria-label="Fechar" />
            </div>
            <div className="modal-body">
              <label className="form-label rotulo-painel" htmlFor="enderecoMesa">Endereço da mesa</label>
              <input id="enderecoMesa" className="form-control" value={endereco}
                onChange={(e) => { setEndereco(e.target.value); gravarEndereco(e.target.value); }}
                placeholder="https://….trycloudflare.com" />
              <p className={`small mt-1 ${ehLocal ? 'text-warning' : 'text-secondary'}`}>
                {ehLocal
                  ? 'Este endereço só funciona no seu computador. Abra a mesa pelo "Abrir mesa para os jogadores.cmd" e cole aqui o endereço que ele mostrar.'
                  : 'Os links abaixo usam este endereço. O endereço do túnel muda a cada sessão: gere os links de novo quando ele mudar.'}
              </p>

              <div className="form-check form-switch mb-3">
                <input className="form-check-input" type="checkbox" id="movimentoLivre" checked={mesa.movimentoLivre}
                  onChange={(e) => aoMudarMovimento(e.target.checked)} />
                <label className="form-check-label" htmlFor="movimentoLivre">Jogadores podem mover a própria ficha</label>
              </div>

              {erro && <div className="alert alert-danger py-2">{erro}</div>}
              {convites?.length === 0 && (
                <p className="text-secondary">Nenhum PJ no caderno. Crie os personagens dos jogadores (tipo PJ) para gerar os links.</p>
              )}

              <ul className="lista-convites">
                {convites?.map((c) => (
                  <li key={c.idRegistro}>
                    <div className="lista-convites-nome">
                      <strong>{c.nomePersonagem}</strong>
                      <span className="small text-secondary">
                        {!c.temConvite && 'sem link'}
                        {c.temConvite && (c.dataUltimoAcesso ? 'já entrou na mesa' : 'link gerado, ainda não usado')}
                      </span>
                    </div>
                    {codigos[c.idRegistro] && (
                      <input className="form-control form-control-sm lista-convites-link" readOnly
                        value={linkDeConvite(endereco, codigos[c.idRegistro])} onFocus={(e) => e.target.select()}
                        aria-label={`Link de ${c.nomePersonagem}`} />
                    )}
                    <div className="d-flex gap-2 flex-shrink-0">
                      {codigos[c.idRegistro] && (
                        <button type="button" className="btn btn-sm btn-primary" onClick={() => copiar(c.idRegistro)}>
                          {copiado === c.idRegistro ? 'Copiado!' : 'Copiar'}
                        </button>
                      )}
                      <button type="button" className="btn btn-sm btn-fantasma" onClick={() => gerar(c.idRegistro)}
                        title={c.temConvite ? 'Gera um link novo; o antigo para de funcionar' : 'Gera o link deste personagem'}>
                        {c.temConvite ? 'Trocar link' : 'Gerar link'}
                      </button>
                      {c.temConvite && (
                        <button type="button" className="btn btn-sm btn-outline-danger px-2" onClick={() => revogar(c.idRegistro)}
                          title="Tira o acesso deste jogador">✕</button>
                      )}
                    </div>
                  </li>
                ))}
              </ul>
              <p className="small text-secondary mb-0">
                O jogador vê só a mesa ativa: o mapa, as fichas visíveis e a ordem dos turnos. PV de monstros, fichas ocultas, anotações e segredos ficam com você.
              </p>
            </div>
          </div>
        </div>
      </div>
      <div className="modal-backdrop show" />
    </>
  );
}
