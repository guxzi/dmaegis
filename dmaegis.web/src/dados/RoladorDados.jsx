import { useState } from 'react';
import { ErroNotacao } from './notacao.js';
import { adicionarAoHistorico, expressaoRapida, formatarRolagem, rolar } from './rolar.js';

const BOTOES = [
  { id: 'd20', rotulo: 'd20' },
  { id: 'vantagem', rotulo: 'Vant.', titulo: 'Vantagem: 2d20, fica o maior' },
  { id: 'desvantagem', rotulo: 'Desv.', titulo: 'Desvantagem: 2d20, fica o menor' },
  { id: 'd12', rotulo: 'd12' },
  { id: 'd10', rotulo: 'd10' },
  { id: 'd8', rotulo: 'd8' },
  { id: 'd6', rotulo: 'd6' },
  { id: 'd4', rotulo: 'd4' },
  { id: 'd100', rotulo: 'd100' },
];

let proximoId = 1;

// Um dado do resultado: máximo brilha, 1 fica avermelhado, descartado fica riscado.
function Dado({ dado, faces }) {
  const classes = ['dado-resultado'];
  if (!dado.mantido) classes.push('descartado');
  else if (dado.valor === faces && faces > 1) classes.push('maximo');
  else if (dado.valor === 1 && faces > 1) classes.push('minimo');
  return <span className={classes.join(' ')} title={dado.mantido ? `d${faces}` : `d${faces} descartado`}>{dado.valor}</span>;
}

function Detalhe({ resultado }) {
  return (
    <div className="d-flex flex-wrap align-items-center gap-1">
      {resultado.termos.map((t, i) => (
        <span key={i} className="d-inline-flex align-items-center gap-1">
          {(i > 0 || t.sinal < 0) && <span className="text-secondary px-1">{t.sinal < 0 ? '−' : '+'}</span>}
          {t.tipo === 'dados'
            ? t.dados.map((d, j) => <Dado key={j} dado={d} faces={t.faces} />)
            : <span className="fw-semibold">{t.valor}</span>}
        </span>
      ))}
    </div>
  );
}

// Rolador do escudo: expressão escrita ou botões rápidos. Roda só no navegador; o histórico fica na memória.
export default function RoladorDados() {
  const [expressao, setExpressao] = useState('');
  const [modificador, setModificador] = useState(0);
  const [historico, setHistorico] = useState([]);
  const [erro, setErro] = useState('');

  function executar(texto) {
    try {
      const resultado = rolar(texto);
      setHistorico((atual) => adicionarAoHistorico(atual, { id: proximoId++, resultado, texto: formatarRolagem(resultado) }));
      setErro('');
      return true;
    } catch (e) {
      if (!(e instanceof ErroNotacao)) throw e;
      setErro(e.message);
      return false;
    }
  }

  const aoEnviar = (evento) => {
    evento.preventDefault();
    executar(expressao);
  };

  const rapido = (botao) => {
    const texto = expressaoRapida(botao, modificador);
    setExpressao(texto);
    executar(texto);
  };

  const [ultimo, ...anteriores] = historico;

  return (
    <div className="d-flex flex-column h-100" style={{ minHeight: 0 }}>
      <form className="mb-2" onSubmit={aoEnviar} noValidate>
        <div className="input-group input-group-sm has-validation">
          <input className={`form-control font-monospace ${erro ? 'is-invalid' : ''}`} placeholder="1d20+5"
            aria-label="Expressão de dados" aria-describedby="erroDados" maxLength={200}
            value={expressao} onChange={(e) => setExpressao(e.target.value)} />
          <button type="submit" className="btn btn-primary">Rolar</button>
          <div id="erroDados" className="invalid-feedback">{erro}</div>
        </div>
      </form>

      <div className="d-flex flex-wrap align-items-center gap-1 mb-2">
        <label htmlFor="modDados" className="small text-secondary me-1">Mod</label>
        <input id="modDados" type="number" className="form-control form-control-sm" style={{ width: 64 }}
          min={-99} max={99} value={modificador} onChange={(e) => setModificador(e.target.value)} />
        {BOTOES.map((b) => (
          <button key={b.id} type="button" className="btn btn-sm btn-dado" title={b.titulo ?? `Rolar 1${b.id}`}
            onClick={() => rapido(b.id)}>
            {b.rotulo}
          </button>
        ))}
      </div>

      <div aria-live="polite" className="visually-hidden">{ultimo ? ultimo.texto : ''}</div>

      {ultimo ? (
        <div className="resultado-dados d-flex align-items-center gap-3 mb-2">
          <span className="total-dados">{ultimo.resultado.total}</span>
          <div className="flex-grow-1" style={{ minWidth: 0 }}>
            <div className="small font-monospace text-secondary text-truncate">{ultimo.resultado.expressao}</div>
            <Detalhe resultado={ultimo.resultado} />
          </div>
        </div>
      ) : (
        <p className="text-secondary small mb-2">Escreva uma rolagem ou use os botões.</p>
      )}

      {anteriores.length > 0 && (
        <ul className="list-unstyled small mb-0 overflow-auto flex-grow-1" style={{ minHeight: 0 }} aria-label="Histórico de rolagens">
          {anteriores.map((item) => (
            <li key={item.id}>
              <button type="button" className="btn btn-link btn-sm p-0 text-start text-decoration-none font-monospace historico-dados"
                title="Rolar de novo" onClick={() => { setExpressao(item.resultado.expressao); executar(item.resultado.expressao); }}>
                {item.texto}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
