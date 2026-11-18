import { useCallback, useState } from 'react';
import { mensagemDeErro } from '../api/erros.js';
import * as sessoesApi from '../api/sessoes.js';
import ConfirmarExclusao from '../componentes/ConfirmarExclusao.jsx';

// Passo 4: segredos e pistas. Tudo aqui é salvo na hora (não depende do "Salvar preparo").
// A lista vem da sessão: pendentes da campanha + revelados nesta sessão.
export default function ListaSegredos({ idSessao, segredos, aoMudar }) {
  const [novo, setNovo] = useState('');
  const [editando, setEditando] = useState(null); // { id, texto }
  const [paraExcluir, setParaExcluir] = useState(null);
  const [erro, setErro] = useState('');

  const substituir = (atualizado) => aoMudar(segredos.map((s) => (s.id === atualizado.id ? atualizado : s)));

  async function executar(acao) {
    setErro('');
    try {
      await acao();
    } catch (e) {
      setErro(mensagemDeErro(e));
    }
  }

  const alternarRevelado = (segredo) => executar(async () => {
    substituir(segredo.idSessaoRevelacao === idSessao
      ? await sessoesApi.ocultarSegredo(segredo.id)
      : await sessoesApi.revelarSegredo(segredo.id, idSessao));
  });

  const adicionar = (evento) => {
    evento.preventDefault();
    if (!novo.trim()) return;
    executar(async () => {
      aoMudar([...segredos, await sessoesApi.criarSegredo(idSessao, novo)]);
      setNovo('');
    });
  };

  const salvarEdicao = () => executar(async () => {
    substituir(await sessoesApi.editarSegredo(editando.id, editando.texto));
    setEditando(null);
  });

  const excluir = () => executar(async () => {
    await sessoesApi.excluirSegredo(paraExcluir.id);
    aoMudar(segredos.filter((s) => s.id !== paraExcluir.id));
    setParaExcluir(null);
  });

  const fecharExclusao = useCallback(() => setParaExcluir(null), []);

  return (
    <div>
      {erro && <div className="alert alert-danger py-2" role="alert">{erro}</div>}
      <p className="text-secondary small mb-2">
        Segredos não revelados aparecem nas próximas sessões até serem revelados.
      </p>

      {segredos.length === 0 && <p className="text-secondary small">Nenhum segredo pendente.</p>}

      <ul className="list-group mb-2">
        {segredos.map((segredo) => {
          const revelado = segredo.idSessaoRevelacao === idSessao;
          const emEdicao = editando?.id === segredo.id;
          return (
            <li key={segredo.id} className="list-group-item d-flex align-items-center gap-2">
              <input className="form-check-input mt-0" type="checkbox" id={`segredo${segredo.id}`}
                checked={revelado} onChange={() => alternarRevelado(segredo)}
                aria-label={revelado ? 'Marcar como não revelado' : 'Marcar como revelado'} />
              {emEdicao ? (
                <>
                  <input className="form-control form-control-sm" maxLength={500} autoFocus value={editando.texto}
                    aria-label="Texto do segredo" onChange={(e) => setEditando({ ...editando, texto: e.target.value })} />
                  <button type="button" className="btn btn-sm btn-primary" onClick={salvarEdicao}>Salvar</button>
                  <button type="button" className="btn btn-sm btn-outline-secondary" onClick={() => setEditando(null)}>Cancelar</button>
                </>
              ) : (
                <>
                  <label htmlFor={`segredo${segredo.id}`}
                    className={`flex-grow-1 mb-0 ${revelado ? 'text-decoration-line-through text-secondary' : ''}`}>
                    {segredo.texto}
                    {revelado && <span className="badge text-bg-success ms-2">revelado nesta sessão</span>}
                  </label>
                  <button type="button" className="btn btn-sm btn-link"
                    onClick={() => setEditando({ id: segredo.id, texto: segredo.texto })}>Editar</button>
                  <button type="button" className="btn btn-sm btn-link text-danger" onClick={() => setParaExcluir(segredo)}>
                    Excluir
                  </button>
                </>
              )}
            </li>
          );
        })}
      </ul>

      <form className="input-group" onSubmit={adicionar}>
        <input className="form-control" maxLength={500} placeholder="Novo segredo ou pista…" aria-label="Novo segredo"
          value={novo} onChange={(e) => setNovo(e.target.value)} />
        <button type="submit" className="btn btn-outline-primary" disabled={!novo.trim()}>Adicionar segredo</button>
      </form>

      <ConfirmarExclusao
        aberto={paraExcluir !== null}
        titulo="Excluir segredo"
        texto={paraExcluir ? `Excluir o segredo "${paraExcluir.texto}"? Ele some de todas as sessões.` : ''}
        aoConfirmar={excluir}
        aoCancelar={fecharExclusao}
      />
    </div>
  );
}
