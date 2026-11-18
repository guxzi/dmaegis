import { moverItem } from './passos.js';

// Passo 3: lista de cenas editável, com subir/descer, remover e adicionar.
export default function ListaCenas({ cenas, aoMudar }) {
  const alterar = (indice, texto) => aoMudar(cenas.map((c, i) => (i === indice ? texto : c)));

  return (
    <div>
      {cenas.length === 0 && <p className="text-secondary small mb-2">Nenhuma cena.</p>}
      {cenas.map((cena, indice) => (
        <div className="input-group mb-2" key={indice}>
          <span className="input-group-text">{indice + 1}</span>
          <input className="form-control" maxLength={500} aria-label={`Cena ${indice + 1}`}
            value={cena} onChange={(e) => alterar(indice, e.target.value)} />
          <button type="button" className="btn btn-outline-secondary" aria-label="Subir cena"
            disabled={indice === 0} onClick={() => aoMudar(moverItem(cenas, indice, -1))}>↑</button>
          <button type="button" className="btn btn-outline-secondary" aria-label="Descer cena"
            disabled={indice === cenas.length - 1} onClick={() => aoMudar(moverItem(cenas, indice, 1))}>↓</button>
          <button type="button" className="btn btn-outline-danger"
            onClick={() => aoMudar(cenas.filter((_, i) => i !== indice))}>Remover</button>
        </div>
      ))}
      <button type="button" className="btn btn-sm btn-outline-secondary" disabled={cenas.length >= 30}
        onClick={() => aoMudar([...cenas, ''])}>
        Adicionar cena
      </button>
    </div>
  );
}
