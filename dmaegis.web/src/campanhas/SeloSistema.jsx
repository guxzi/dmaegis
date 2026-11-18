// Selo com o nome do sistema. Cores fixas por id: 1 = D&D 5e (cobalto), 2 = Daggerheart (violeta).
const CORES = { 1: 'selo-dnd', 2: 'selo-daggerheart' };

export default function SeloSistema({ idSistema, nomeSistema }) {
  return <span className={`badge ${CORES[idSistema] ?? 'text-bg-secondary'}`}>{nomeSistema}</span>;
}
