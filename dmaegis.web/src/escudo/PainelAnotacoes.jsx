import { useEffect, useRef, useState } from 'react';
import { mensagemDeErro } from '../api/erros.js';
import { criarAnotacao, excluirAnotacao, listarAnotacoes } from '../api/escudo.js';
import { formatarHora } from './atalhos.js';
import { marcarMencoes } from './mencoes.js';

// Texto da anotação com os nomes do caderno como botões que abrem o cartão do registro.
function TextoComMencoes({ texto, registros, aoAbrirRegistro }) {
  return marcarMencoes(texto, registros).map((pedaco, i) => (pedaco.registro
    ? <button key={i} type="button" className="mencao" onClick={() => aoAbrirRegistro(pedaco.registro.id)}>{pedaco.texto}</button>
    : <span key={i}>{pedaco.texto}</span>));
}

// Anotações da sessão com horário. Enter grava e limpa o campo; a lista rola sozinha para o fim.
// Nomes de registros da campanha viram botões (anotações inteligentes); `aoMudar` avisa o resumo vivo.
export default function PainelAnotacoes({ idSessao, registros = [], aoAbrirRegistro, aoMudar }) {
  const [anotacoes, setAnotacoes] = useState([]);
  const [texto, setTexto] = useState('');
  const [erro, setErro] = useState('');
  const [gravando, setGravando] = useState(false);
  const fim = useRef(null);

  useEffect(() => {
    let ativo = true;
    listarAnotacoes(idSessao)
      .then((lista) => ativo && setAnotacoes(lista))
      .catch((e) => ativo && setErro(mensagemDeErro(e)));
    return () => {
      ativo = false;
    };
  }, [idSessao]);

  useEffect(() => {
    aoMudar?.(anotacoes);
  }, [anotacoes, aoMudar]);

  useEffect(() => {
    fim.current?.scrollIntoView({ block: 'end' });
  }, [anotacoes.length]);

  async function gravar(evento) {
    evento.preventDefault();
    if (!texto.trim() || gravando) return;
    setGravando(true);
    setErro('');
    try {
      const nova = await criarAnotacao(idSessao, texto);
      setAnotacoes((atuais) => [...atuais, nova]);
      setTexto('');
    } catch (e) {
      setErro(mensagemDeErro(e));
    } finally {
      setGravando(false);
    }
  }

  async function excluir(id) {
    setErro('');
    try {
      await excluirAnotacao(id);
      setAnotacoes((atuais) => atuais.filter((a) => a.id !== id));
    } catch (e) {
      setErro(mensagemDeErro(e));
    }
  }

  return (
    <div className="d-flex flex-column h-100">
      <div className="flex-grow-1 overflow-auto mb-2" style={{ minHeight: 0 }}>
        {anotacoes.length === 0 && <p className="text-secondary small">Nada anotado ainda.</p>}
        <ul className="list-unstyled mb-0">
          {anotacoes.map((a) => (
            <li key={a.id} className="d-flex gap-2 align-items-start py-1 border-bottom border-opacity-25">
              <span className="font-monospace small text-info flex-shrink-0 pt-1">{formatarHora(a.data)}</span>
              <span className="flex-grow-1" style={{ whiteSpace: 'pre-line' }}>
                <TextoComMencoes texto={a.texto} registros={registros} aoAbrirRegistro={aoAbrirRegistro} />
              </span>
              <button type="button" className="btn-close btn-sm flex-shrink-0 mt-1" style={{ fontSize: '0.55rem' }}
                aria-label={`Excluir anotação das ${formatarHora(a.data)}`} onClick={() => excluir(a.id)} />
            </li>
          ))}
        </ul>
        <div ref={fim} />
      </div>
      {erro && <div className="alert alert-danger py-1 small mb-2">{erro}</div>}
      <form onSubmit={gravar}>
        <input className="form-control" maxLength={2000} placeholder="Anotar… (Enter grava)" aria-label="Nova anotação"
          value={texto} onChange={(e) => setTexto(e.target.value)} aria-busy={gravando} />
        {/* Sem "disabled" enquanto grava: desabilitar tira o foco, e o mestre teria que clicar de novo a cada anotação. */}
      </form>
    </div>
  );
}
