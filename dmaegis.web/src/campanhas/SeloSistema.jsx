// Selo com o nome do sistema. Cores fixas por id: 1 = D&D 5e, 2 = Daggerheart.
const CORES = { 1: 'text-bg-danger', 2: 'text-bg-primary' };

export default function SeloSistema({ idSistema, nomeSistema }) {
  return <span className={`badge ${CORES[idSistema] ?? 'text-bg-secondary'}`}>{nomeSistema}</span>;
}
