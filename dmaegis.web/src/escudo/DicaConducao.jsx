import { useEffect, useMemo, useState } from 'react';
import { DICAS, dicaDoMomento, dicasDoSistema } from '../regras/dicas.js';

const TRES_MINUTOS = 3 * 60 * 1000;

// Uma dica de condução (gerais + as do sistema da mesa) que troca sozinha a cada 3 minutos.
export default function DicaConducao({ idSistema }) {
  const dicas = useMemo(() => dicasDoSistema(DICAS, idSistema), [idSistema]);
  const [agora, setAgora] = useState(() => Date.now());
  const [pulos, setPulos] = useState(0);

  useEffect(() => {
    const relogio = setInterval(() => setAgora(Date.now()), 30 * 1000);
    return () => clearInterval(relogio);
  }, []);

  // "Outra" avança 3 minutos no relógio da dica, sem atrapalhar a troca automática.
  const dica = dicaDoMomento(agora + pulos * TRES_MINUTOS, dicas);
  if (!dica) return null;

  return (
    <aside className="dica-conducao" aria-live="polite">
      <span className="dica-conducao-rotulo">Dica</span>
      <span className="flex-grow-1">{dica.texto}</span>
      <button type="button" className="btn btn-sm btn-fantasma px-2 py-0" onClick={() => setPulos((p) => p + 1)}>outra</button>
    </aside>
  );
}
