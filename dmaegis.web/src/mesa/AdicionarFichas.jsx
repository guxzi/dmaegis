import { useState } from 'react';

const ID_TIPO_PJ = 2;

// Adiciona fichas: todos os PJs de uma vez, um registro do caderno com quantidade ou uma ficha avulsa.
export default function AdicionarFichas({ registros, rotulo, aoAdicionar, ocupado }) {
  const [idRegistro, setIdRegistro] = useState('');
  const [quantidade, setQuantidade] = useState(1);
  const [avulsa, setAvulsa] = useState('');

  const doCaderno = registros.filter((r) => [1, 2, 3].includes(r.idTipoRegistro));
  const grupos = [
    { titulo: `${rotulo?.(3) || 'Criatura'}s`, itens: doCaderno.filter((r) => r.idTipoRegistro === 3) },
    { titulo: 'NPCs', itens: doCaderno.filter((r) => r.idTipoRegistro === 1) },
    { titulo: 'PJs', itens: doCaderno.filter((r) => r.idTipoRegistro === ID_TIPO_PJ) },
  ].filter((g) => g.itens.length > 0);

  async function doRegistro(evento) {
    evento.preventDefault();
    if (!idRegistro) return;
    await aoAdicionar({ idRegistro: Number(idRegistro), quantidade: Number(quantidade) || 1 });
    setQuantidade(1);
  }

  async function avulsaSubmit(evento) {
    evento.preventDefault();
    if (!avulsa.trim()) return;
    await aoAdicionar({ nome: avulsa.trim(), quantidade: 1 });
    setAvulsa('');
  }

  return (
    <section className="adicionar-fichas" aria-label="Adicionar fichas">
      <p className="rotulo-painel">Adicionar à mesa</p>
      <button type="button" className="btn btn-sm btn-fantasma w-100 mb-2" disabled={ocupado}
        onClick={() => aoAdicionar({ todosJogadores: true })}>
        ✦ Todos os PJs
      </button>
      <form className="d-flex gap-2 mb-2" onSubmit={doRegistro}>
        <select className="form-select form-select-sm" value={idRegistro} onChange={(e) => setIdRegistro(e.target.value)}
          aria-label="Registro do caderno">
          <option value="">Do caderno…</option>
          {grupos.map((g) => (
            <optgroup key={g.titulo} label={g.titulo}>
              {g.itens.map((r) => <option key={r.id} value={r.id}>{r.nome}</option>)}
            </optgroup>
          ))}
        </select>
        <input type="number" min="1" max="20" className="form-control form-control-sm" style={{ width: 64 }}
          value={quantidade} onChange={(e) => setQuantidade(e.target.value)} aria-label="Quantidade" />
        <button type="submit" className="btn btn-sm btn-primary" disabled={!idRegistro || ocupado}>+</button>
      </form>
      <form className="d-flex gap-2" onSubmit={avulsaSubmit}>
        <input className="form-control form-control-sm" placeholder="Ficha avulsa (ex.: Barril explosivo)" maxLength={100}
          value={avulsa} onChange={(e) => setAvulsa(e.target.value)} aria-label="Nome da ficha avulsa" />
        <button type="submit" className="btn btn-sm btn-fantasma" disabled={!avulsa.trim() || ocupado}>+</button>
      </form>
    </section>
  );
}
