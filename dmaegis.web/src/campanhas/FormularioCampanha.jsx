import { useState } from 'react';
import { mensagemDeErro } from '../api/erros.js';

// Formulário de campanha. Com `sistemas`, mostra a escolha do sistema (criação);
// sem `sistemas`, só nome e descrição (edição: o sistema não pode ser trocado).
export default function FormularioCampanha({ sistemas, inicial, textoBotao, aoSalvar, aoCancelar }) {
  const [nome, setNome] = useState(inicial?.nome ?? '');
  const [descricao, setDescricao] = useState(inicial?.descricao ?? '');
  const [idSistema, setIdSistema] = useState(sistemas?.[0]?.id ?? null);
  const [erro, setErro] = useState('');
  const [salvando, setSalvando] = useState(false);

  async function aoEnviar(evento) {
    evento.preventDefault();
    setErro('');
    setSalvando(true);
    try {
      await aoSalvar({ nome, descricao, idSistema });
    } catch (e) {
      setErro(mensagemDeErro(e));
      setSalvando(false);
    }
  }

  return (
    <form onSubmit={aoEnviar} noValidate>
      {erro && <div className="alert alert-danger" role="alert">{erro}</div>}

      <div className="mb-3">
        <label htmlFor="nomeCampanha" className="form-label">Nome</label>
        <input id="nomeCampanha" className="form-control" maxLength={100} required autoFocus
          value={nome} onChange={(e) => setNome(e.target.value)} />
      </div>

      {sistemas && (
        <fieldset className="mb-3">
          <legend className="form-label fs-6">Sistema</legend>
          {sistemas.map((s) => (
            <div className="form-check form-check-inline" key={s.id}>
              <input className="form-check-input" type="radio" name="sistema" id={`sistema${s.id}`}
                checked={idSistema === s.id} onChange={() => setIdSistema(s.id)} />
              <label className="form-check-label" htmlFor={`sistema${s.id}`}>{s.nome}</label>
            </div>
          ))}
          <div className="form-text">O sistema não pode ser trocado depois.</div>
        </fieldset>
      )}

      <div className="mb-3">
        <label htmlFor="descricaoCampanha" className="form-label">Descrição <span className="text-secondary">(opcional)</span></label>
        <textarea id="descricaoCampanha" className="form-control" rows={3} maxLength={5000}
          value={descricao} onChange={(e) => setDescricao(e.target.value)} />
      </div>

      <div className="d-flex gap-2">
        <button type="submit" className="btn btn-primary" disabled={salvando}>
          {salvando ? 'Salvando…' : textoBotao}
        </button>
        <button type="button" className="btn btn-outline-secondary" onClick={aoCancelar} disabled={salvando}>
          Cancelar
        </button>
      </div>
    </form>
  );
}
