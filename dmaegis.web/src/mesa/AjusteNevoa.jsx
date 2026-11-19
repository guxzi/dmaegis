const TAMANHOS = [1, 3, 5, 9];

// Painel da névoa sobre o palco: liga a névoa para os jogadores e escolhe o pincel (revelar ou esconder, tamanho).
// Com o painel aberto, arrastar no mapa pinta a névoa; Shift + arrastar pinta um retângulo.
export default function AjusteNevoa({ mesa, pincel, aoMudarPincel, aoLigar, aoRevelarTudo, aoEsconderTudo, aoFechar, ocupado }) {
  return (
    <div className="ajuste-grade ajuste-nevoa cartao-astral" onPointerDown={(e) => e.stopPropagation()}>
      <div className="d-flex align-items-center mb-2">
        <strong className="flex-grow-1">Névoa de guerra</strong>
        <button type="button" className="btn-close btn-close-white btn-sm" onClick={aoFechar} aria-label="Fechar a névoa" />
      </div>

      <div className="form-check form-switch mb-2">
        <input className="form-check-input" type="checkbox" id="nevoaLigada" checked={mesa.nevoa}
          onChange={(e) => aoLigar(e.target.checked)} />
        <label className="form-check-label" htmlFor="nevoaLigada">Névoa para os jogadores</label>
      </div>

      <div className="segmentado w-100 mb-2" role="group" aria-label="O que o pincel faz">
        <button type="button" className="flex-fill" aria-pressed={pincel.modo === 'revelar'}
          onClick={() => aoMudarPincel({ ...pincel, modo: 'revelar' })}>Revelar</button>
        <button type="button" className="flex-fill" aria-pressed={pincel.modo === 'esconder'}
          onClick={() => aoMudarPincel({ ...pincel, modo: 'esconder' })}>Esconder</button>
      </div>

      <div className="d-flex align-items-center gap-2 mb-2">
        <span className="small text-secondary">Pincel</span>
        <div className="tamanhos-pincel" role="group" aria-label="Tamanho do pincel">
          {TAMANHOS.map((t) => (
            <button key={t} type="button" aria-pressed={pincel.tamanho === t} onClick={() => aoMudarPincel({ ...pincel, tamanho: t })}
              title={`${t}×${t} células`}>
              <span style={{ '--lado': `${6 + t * 1.6}px` }} />
            </button>
          ))}
        </div>
      </div>

      <div className="d-flex gap-2 mb-2">
        <button type="button" className="btn btn-sm btn-fantasma flex-fill" disabled={ocupado} onClick={aoRevelarTudo}>Revelar tudo</button>
        <button type="button" className="btn btn-sm btn-fantasma flex-fill" disabled={ocupado} onClick={aoEsconderTudo}>Esconder tudo</button>
      </div>
      <p className="small text-secondary mb-0">
        Arraste no mapa para pintar. Segure <kbd>Shift</kbd> para um retângulo.
        {!mesa.nevoa && ' A névoa só aparece para os jogadores quando estiver ligada.'}
      </p>
    </div>
  );
}
