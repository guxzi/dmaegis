import { useState } from 'react';
import { CLASSE_SAUDE, estadoSaude, fracaoPv } from './combate.js';

function CampoIniciativa({ ficha, aoSalvar }) {
  const [editando, setEditando] = useState(false);
  const [valor, setValor] = useState('');

  function confirmar() {
    setEditando(false);
    const numero = valor.trim() === '' ? null : Number.parseInt(valor, 10);
    if (Number.isNaN(numero) || numero === ficha.iniciativa) return;
    aoSalvar(ficha, { iniciativa: numero });
  }

  if (editando) {
    return (
      <input className="iniciativa-campo" type="number" inputMode="numeric" autoFocus value={valor}
        onChange={(e) => setValor(e.target.value)} onBlur={confirmar}
        onKeyDown={(e) => { if (e.key === 'Enter') confirmar(); if (e.key === 'Escape') setEditando(false); }}
        onClick={(e) => e.stopPropagation()} aria-label={`Iniciativa de ${ficha.nome}`} />
    );
  }
  return (
    <button type="button" className={`selo-escudo iniciativa-selo${ficha.iniciativa == null ? ' vazio' : ''}`}
      title="Clique para digitar a iniciativa"
      onClick={(e) => { e.stopPropagation(); setValor(ficha.iniciativa ?? ''); setEditando(true); }}>
      {ficha.iniciativa ?? '–'}
    </button>
  );
}

// A ordem do combate: número da iniciativa (clique para digitar), retrato, nome, PV e condições.
export default function ListaIniciativa({ mesa, urlImagem, idSelecionada, aoSelecionar, aoSalvarFicha }) {
  if (mesa.participantes.length === 0) {
    return (
      <div className="lista-iniciativa-vazia">
        <p className="mb-1">Nenhuma ficha na mesa.</p>
        <p className="small text-secondary mb-0">Adicione os PJs e os monstros logo abaixo.</p>
      </div>
    );
  }

  return (
    <ol className="lista-iniciativa">
      {mesa.participantes.map((ficha) => {
        const daVez = mesa.emCombate && ficha.id === mesa.idParticipanteVez;
        const fracao = fracaoPv(ficha.pvAtual, ficha.pvMaximo);
        const saude = estadoSaude(ficha.pvAtual, ficha.pvMaximo);
        const classes = [
          'linha-iniciativa',
          daVez && 'da-vez',
          ficha.id === idSelecionada && 'selecionada',
          !ficha.visivel && 'oculta',
          saude === 'Caído' && 'caida',
        ].filter(Boolean).join(' ');
        return (
          <li key={ficha.id} className={classes} onClick={() => aoSelecionar(ficha.id)}
            style={{ '--cor-ficha': ficha.jogador ? 'var(--dm-cobalto-claro)' : 'var(--dm-estrela)' }}>
            <CampoIniciativa ficha={ficha} aoSalvar={aoSalvarFicha} />
            <span className="linha-retrato">
              {ficha.idImagemRetrato
                ? <img src={urlImagem(ficha.idImagemRetrato)} alt="" />
                : <span>{ficha.nome.charAt(0)}</span>}
            </span>
            <span className="linha-corpo">
              <span className="linha-nome">
                {ficha.nome}
                {!ficha.visivel && <span className="linha-etiqueta">oculta</span>}
                {ficha.coluna == null && <span className="linha-etiqueta">fora do mapa</span>}
              </span>
              {fracao !== null && (
                <span className={`barra-pv saude-${CLASSE_SAUDE[saude]}`}>
                  <span style={{ width: `${fracao * 100}%` }} />
                </span>
              )}
              {ficha.condicoes.length > 0 && (
                <span className="linha-condicoes">
                  {ficha.condicoes.map((c) => <span key={c} className="selo-condicao">{c}</span>)}
                </span>
              )}
            </span>
            <span className="linha-numeros">
              {ficha.pvMaximo != null && <span className="pv">{ficha.pvAtual ?? 0}<small>/{ficha.pvMaximo}</small></span>}
              {ficha.ca != null && <span className="ca" title="Classe de Armadura">{ficha.ca}</span>}
            </span>
          </li>
        );
      })}
    </ol>
  );
}
