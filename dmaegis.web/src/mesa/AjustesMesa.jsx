import { CLIMAS } from './efeitos.js';

// Os painéis pequenos que abrem por cima do palco do mestre: grade e clima.

// Ajuste da grade com prévia ao vivo: os controles mudam a mesa na hora e salvam ao soltar.
export function AjusteGrade({ mesa, aoMudar, aoSalvar, aoFechar }) {
  const controle = (chave, rotulo, min, max) => (
    <label className="ajuste-grade-controle">
      <span>{rotulo} <strong>{mesa[chave]}</strong></span>
      <input type="range" className="form-range" min={min} max={max} value={mesa[chave]}
        onChange={(e) => aoMudar({ [chave]: Number(e.target.value) })}
        onPointerUp={() => aoSalvar()} onKeyUp={() => aoSalvar()} />
    </label>
  );
  return (
    <div className="ajuste-grade cartao-astral" onPointerDown={(e) => e.stopPropagation()}>
      <div className="d-flex align-items-center mb-2">
        <strong className="flex-grow-1">Grade</strong>
        <button type="button" className="btn-close btn-close-white btn-sm" onClick={aoFechar} aria-label="Fechar ajuste da grade" />
      </div>
      {controle('tamanhoCelula', 'Célula', 20, 200)}
      {controle('deslocamentoX', 'Mover →', -100, 100)}
      {controle('deslocamentoY', 'Mover ↓', -100, 100)}
      <div className="form-check form-switch mt-1">
        <input className="form-check-input" type="checkbox" id="gradeVisivel" checked={mesa.gradeVisivel}
          onChange={(e) => { aoMudar({ gradeVisivel: e.target.checked }); aoSalvar({ gradeVisivel: e.target.checked }); }} />
        <label className="form-check-label" htmlFor="gradeVisivel">Mostrar a grade</label>
      </div>
      <p className="small text-secondary mb-0 mt-2">Encaixe as linhas nos quadrados desenhados no mapa.</p>
    </div>
  );
}

// Clima por cima do mapa (os jogadores também veem).
export function AjusteClima({ clima, aoEscolher, aoFechar }) {
  return (
    <div className="ajuste-clima cartao-astral" onPointerDown={(e) => e.stopPropagation()}>
      <div className="d-flex align-items-center mb-2">
        <strong className="flex-grow-1">Clima</strong>
        <button type="button" className="btn-close btn-close-white btn-sm" onClick={aoFechar} aria-label="Fechar clima" />
      </div>
      <div className="segmentado" role="group" aria-label="Clima">
        {CLIMAS.map((c) => (
          <button key={c.rotulo} type="button" aria-pressed={(clima ?? null) === c.id} onClick={() => aoEscolher(c.id)}>{c.rotulo}</button>
        ))}
      </div>
    </div>
  );
}
