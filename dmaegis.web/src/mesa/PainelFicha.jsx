import { useState } from 'react';
import { aplicarPv } from './combate.js';

function numeroOuNulo(texto) {
  if (String(texto).trim() === '') return null;
  const numero = Number.parseInt(texto, 10);
  return Number.isNaN(numero) ? null : numero;
}

// A ficha selecionada: dano e cura rápidos, números, condições e as ações (ocultar, mapa, remover).
export default function PainelFicha({ ficha, condicoes, aoSalvar, aoColocarNoMapa, aoTirarDoMapa, aoRemover, aoFechar }) {
  const [quanto, setQuanto] = useState('');
  const [efeito, setEfeito] = useState(null);

  function aplicar(modo) {
    if (!quanto || (ficha.pvMaximo == null && ficha.pvAtual == null)) return;
    aoSalvar(ficha, { pvAtual: aplicarPv(ficha.pvAtual, ficha.pvMaximo, quanto, modo) });
    setEfeito(modo);
    setQuanto('');
    setTimeout(() => setEfeito(null), 700);
  }

  function campo(chave, valor) {
    aoSalvar(ficha, { [chave]: numeroOuNulo(valor) });
  }

  function alternarCondicao(nome) {
    const atuais = ficha.condicoes;
    aoSalvar(ficha, { condicoes: atuais.includes(nome) ? atuais.filter((c) => c !== nome) : [...atuais, nome] });
  }

  const temPv = ficha.pvMaximo != null || ficha.pvAtual != null;

  return (
    <section className={`painel-ficha${efeito ? ` efeito-${efeito}` : ''}`} aria-label={`Ficha de ${ficha.nome}`}>
      <header className="d-flex align-items-center gap-2 mb-2">
        <h3 className="h6 mb-0 flex-grow-1 text-truncate">{ficha.nome}</h3>
        <button type="button" className="btn-close btn-close-white btn-sm" onClick={aoFechar} aria-label="Fechar ficha" />
      </header>

      {temPv && (
        <div className="dano-cura mb-3">
          <input type="number" inputMode="numeric" min="0" className="form-control form-control-sm" placeholder="Quanto?"
            value={quanto} onChange={(e) => setQuanto(e.target.value)} aria-label="Valor de dano ou cura"
            onKeyDown={(e) => { if (e.key === 'Enter') aplicar(e.shiftKey ? 'cura' : 'dano'); }} />
          <button type="button" className="btn btn-sm botao-dano" onClick={() => aplicar('dano')}>Dano</button>
          <button type="button" className="btn btn-sm botao-cura" onClick={() => aplicar('cura')}>Cura</button>
        </div>
      )}

      <div className="numeros-ficha mb-3">
        <label>
          <span>PV</span>
          <input key={`pv-${ficha.pvAtual}`} type="number" className="form-control form-control-sm" defaultValue={ficha.pvAtual ?? ''}
            onBlur={(e) => campo('pvAtual', e.target.value)} />
        </label>
        <label>
          <span>PV máx.</span>
          <input key={`pvm-${ficha.pvMaximo}`} type="number" min="1" className="form-control form-control-sm" defaultValue={ficha.pvMaximo ?? ''}
            onBlur={(e) => campo('pvMaximo', e.target.value)} />
        </label>
        <label>
          <span>CA</span>
          <input key={`ca-${ficha.ca}`} type="number" min="0" className="form-control form-control-sm" defaultValue={ficha.ca ?? ''}
            onBlur={(e) => campo('ca', e.target.value)} />
        </label>
        <label>
          <span>Bônus inic.</span>
          <input key={`bi-${ficha.bonusIniciativa}`} type="number" className="form-control form-control-sm" defaultValue={ficha.bonusIniciativa}
            onBlur={(e) => aoSalvar(ficha, { bonusIniciativa: numeroOuNulo(e.target.value) ?? 0 })} />
        </label>
        <label>
          <span>Tamanho</span>
          <select className="form-select form-select-sm" value={ficha.tamanho}
            onChange={(e) => aoSalvar(ficha, { tamanho: Number(e.target.value) })}>
            <option value={1}>1×1</option>
            <option value={2}>2×2</option>
            <option value={3}>3×3</option>
            <option value={4}>4×4</option>
          </select>
        </label>
      </div>

      <p className="rotulo-painel">Condições</p>
      <div className="grade-condicoes mb-3">
        {condicoes.map((c) => (
          <button key={c.nome} type="button" title={c.texto} aria-pressed={ficha.condicoes.includes(c.nome)}
            className="botao-condicao" onClick={() => alternarCondicao(c.nome)}>
            {c.nome}
          </button>
        ))}
      </div>

      <div className="d-flex flex-wrap gap-2">
        <button type="button" className="btn btn-sm btn-fantasma" onClick={() => aoSalvar(ficha, { visivel: !ficha.visivel })}>
          {ficha.visivel ? 'Ocultar dos jogadores' : 'Mostrar aos jogadores'}
        </button>
        {ficha.coluna == null
          ? <button type="button" className="btn btn-sm btn-fantasma" onClick={() => aoColocarNoMapa(ficha)}>Colocar no mapa</button>
          : <button type="button" className="btn btn-sm btn-fantasma" onClick={() => aoTirarDoMapa(ficha)}>Tirar do mapa</button>}
        <button type="button" className="btn btn-sm btn-outline-danger ms-auto" onClick={() => aoRemover(ficha)}>Remover</button>
      </div>
    </section>
  );
}
