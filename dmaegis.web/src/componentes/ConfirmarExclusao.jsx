import { useEffect, useState } from 'react';

// Modal de confirmação do Bootstrap, controlado pelo React (sem o JS do Bootstrap).
// Só aparece quando `aberto` é true. Esc ou "Cancelar" fecham.
export default function ConfirmarExclusao({ aberto, titulo, texto, aoConfirmar, aoCancelar }) {
  const [excluindo, setExcluindo] = useState(false);

  useEffect(() => {
    if (!aberto) return undefined;
    const aoTeclar = (e) => e.key === 'Escape' && aoCancelar();
    window.addEventListener('keydown', aoTeclar);
    return () => window.removeEventListener('keydown', aoTeclar);
  }, [aberto, aoCancelar]);

  if (!aberto) return null;

  async function confirmar() {
    setExcluindo(true);
    try {
      await aoConfirmar();
    } finally {
      setExcluindo(false);
    }
  }

  return (
    <>
      <div className="modal d-block" tabIndex={-1} role="dialog" aria-modal="true" aria-labelledby="tituloExclusao">
        <div className="modal-dialog modal-dialog-centered">
          <div className="modal-content">
            <div className="modal-header">
              <h2 id="tituloExclusao" className="modal-title h5">{titulo}</h2>
              <button type="button" className="btn-close" aria-label="Fechar" onClick={aoCancelar} />
            </div>
            <div className="modal-body">
              <p className="mb-0">{texto}</p>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-outline-secondary" onClick={aoCancelar} autoFocus>
                Cancelar
              </button>
              <button type="button" className="btn btn-danger" onClick={confirmar} disabled={excluindo}>
                {excluindo ? 'Excluindo…' : 'Excluir'}
              </button>
            </div>
          </div>
        </div>
      </div>
      <div className="modal-backdrop show" />
    </>
  );
}
