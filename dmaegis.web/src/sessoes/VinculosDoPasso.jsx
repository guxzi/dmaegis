import { Link } from 'react-router';
import { opcoesParaVincular, vinculadosDoTipo } from './passos.js';

// Passos 5 a 8: registros de um tipo ligados à sessão (etiquetas) e um select para vincular mais.
export default function VinculosDoPasso({ idCampanha, idTipo, rotuloTipo, registros, idsRegistro, aoMudar }) {
  const vinculados = vinculadosDoTipo(idsRegistro, registros, idTipo);
  const opcoes = opcoesParaVincular(registros, idTipo, idsRegistro);
  const existeAlgumDoTipo = registros.some((r) => r.idTipoRegistro === idTipo);

  return (
    <div>
      <div className="d-flex flex-wrap gap-2 mb-2">
        {vinculados.length === 0 && <span className="text-secondary small">Nenhum vinculado.</span>}
        {vinculados.map((r) => (
          <span key={r.id} className="badge rounded-pill text-bg-light border d-inline-flex align-items-center gap-1 fs-6 fw-normal">
            <Link to={`/campanhas/${idCampanha}/caderno/${r.id}`} className="text-decoration-none">{r.nome}</Link>
            <button type="button" className="btn-close" style={{ fontSize: '0.6rem' }} aria-label={`Desvincular ${r.nome}`}
              onClick={() => aoMudar(idsRegistro.filter((id) => id !== r.id))} />
          </span>
        ))}
      </div>

      {existeAlgumDoTipo ? (
        <select className="form-select form-select-sm" style={{ maxWidth: 320 }} value=""
          aria-label={`Vincular ${rotuloTipo}`} disabled={opcoes.length === 0}
          onChange={(e) => e.target.value && aoMudar([...idsRegistro, Number(e.target.value)])}>
          <option value="">{opcoes.length === 0 ? 'Todos já vinculados' : `Vincular ${rotuloTipo}…`}</option>
          {opcoes.map((r) => <option key={r.id} value={r.id}>{r.nome}</option>)}
        </select>
      ) : (
        <Link to={`/campanhas/${idCampanha}/caderno/novo`} className="small">
          Nenhum registro do tipo {rotuloTipo}. Criar no caderno
        </Link>
      )}
    </div>
  );
}
