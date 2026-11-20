import { useState } from 'react';
import { mensagemDeErro } from '../api/erros.js';
import { ErroNotacao } from '../dados/notacao.js';
import { expressaoRapida, formatarRolagem, rolar } from '../dados/rolar.js';
import { criticoDaRolagem } from './efeitos.js';

const RAPIDOS = [
  { id: 'd20', rotulo: 'd20' },
  { id: 'vantagem', rotulo: 'vant', titulo: 'Vantagem: 2d20, fica o maior' },
  { id: 'desvantagem', rotulo: 'desv', titulo: 'Desvantagem: 2d20, fica o menor' },
  { id: 'd12', rotulo: 'd12' },
  { id: 'd8', rotulo: 'd8' },
  { id: 'd6', rotulo: 'd6' },
  { id: 'd4', rotulo: 'd4' },
];

function hora(data) {
  return new Date(data).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
}

// Rolagens da mesa: o dado rola no navegador e o resultado vai para todos (o mestre pode rolar em segredo).
// A lista vem da página (useRolagens), que também mostra o dado animado em cima do mapa.
export default function PainelRolagens({ rolagens, enviar, aoEnviada, comSecreta = false, compacto = false }) {
  const [expressao, setExpressao] = useState('1d20');
  const [modificador, setModificador] = useState(0);
  const [secreta, setSecreta] = useState(false);
  const [erro, setErro] = useState('');

  async function fazer(texto) {
    setErro('');
    let resultado;
    try {
      resultado = rolar(texto);
    } catch (e) {
      setErro(e instanceof ErroNotacao ? e.message : 'Expressão inválida.');
      return;
    }
    try {
      const nova = await enviar({
        expressao: resultado.expressao, total: resultado.total, detalhe: formatarRolagem(resultado), secreta: comSecreta && secreta,
      });
      aoEnviada(nova);
    } catch (e) {
      setErro(mensagemDeErro(e));
    }
  }

  return (
    <section className={`painel-rolagens${compacto ? ' compacto' : ''}`} aria-label="Rolagens da mesa">
      <form className="d-flex gap-2 mb-2" onSubmit={(e) => { e.preventDefault(); fazer(expressao); }}>
        <input className="form-control form-control-sm" value={expressao} onChange={(e) => setExpressao(e.target.value)}
          aria-label="Expressão de dados" placeholder="2d6+3" maxLength={200} />
        <button type="submit" className="btn btn-sm btn-primary">Rolar</button>
      </form>
      <div className="rapidos-rolagem mb-2">
        {RAPIDOS.map((b) => (
          <button key={b.id} type="button" title={b.titulo} onClick={() => fazer(expressaoRapida(b.id, modificador))}>{b.rotulo}</button>
        ))}
        <input type="number" className="form-control form-control-sm" value={modificador} aria-label="Modificador"
          title="Modificador somado aos botões" onChange={(e) => setModificador(e.target.value)} />
      </div>
      {comSecreta && (
        <div className="form-check form-switch small mb-2">
          <input className="form-check-input" type="checkbox" id="rolagemSecreta" checked={secreta} onChange={(e) => setSecreta(e.target.checked)} />
          <label className="form-check-label" htmlFor="rolagemSecreta">Rolar em segredo (só você vê)</label>
        </div>
      )}
      {erro && <p className="small text-danger mb-2">{erro}</p>}
      <ol className="lista-rolagens">
        {rolagens.length === 0 && <li className="text-secondary small">Nenhuma rolagem ainda.</li>}
        {rolagens.map((r) => {
          const critico = criticoDaRolagem(r.expressao, r.detalhe);
          return (
          <li key={r.id} className={[r.mestre && 'do-mestre', r.secreta && 'secreta', critico && `rolagem-${critico}`].filter(Boolean).join(' ')}>
            <span className="rolagem-total">{r.total}</span>
            <span className="rolagem-corpo">
              <span className="rolagem-autor">{r.autor}{r.secreta && <span className="linha-etiqueta ms-1">secreta</span>}</span>
              <span className="rolagem-detalhe" title={r.detalhe}>{r.detalhe}</span>
            </span>
            <span className="rolagem-hora">{hora(r.data)}</span>
          </li>
          );
        })}
      </ol>
    </section>
  );
}
