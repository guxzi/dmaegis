// Selo do tipo de registro. Cores fixas por id (1 NPC … 7 Missão).
const CORES = {
  1: 'text-bg-info',
  2: 'text-bg-success',
  3: 'text-bg-danger',
  4: 'text-bg-warning',
  5: 'text-bg-dark',
  6: 'text-bg-primary',
  7: 'text-bg-secondary',
};

export default function SeloTipo({ idTipo, rotulo }) {
  return <span className={`badge ${CORES[idTipo] ?? 'text-bg-light'}`}>{rotulo}</span>;
}
