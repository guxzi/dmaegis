import { useEffect, useState } from 'react';
import { listarModelos } from '../api/caderno.js';
import { mensagemDeErro } from '../api/erros.js';
import { atributosParaEnvio, mesclarAtributos } from './atributos.js';
import { separarTags } from './tags.js';
import { rotuloTipo } from './tipos.js';

// Formulário de registro (criar e editar). Os campos do modelo do tipo aparecem com rótulo fixo;
// os atributos livres têm chave e valor editáveis. Trocar o tipo traz o modelo novo e mantém
// os valores já digitados nas chaves iguais.
export default function FormularioRegistro({ campanha, tipos, tagsExistentes, inicial, textoBotao, aoSalvar, aoCancelar }) {
  const [idTipo, setIdTipo] = useState(inicial?.idTipoRegistro ?? tipos[0]?.id ?? 1);
  const [nome, setNome] = useState(inicial?.nome ?? '');
  const [resumo, setResumo] = useState(inicial?.resumo ?? '');
  const [descricao, setDescricao] = useState(inicial?.descricao ?? '');
  const [textoTags, setTextoTags] = useState((inicial?.tags ?? []).map((t) => t.nome).join(', '));
  const [linhas, setLinhas] = useState(
    (inicial?.atributos ?? []).map((a) => ({ chave: a.chave, valor: a.valor, doModelo: false })),
  );
  const [erro, setErro] = useState('');
  const [salvando, setSalvando] = useState(false);

  useEffect(() => {
    let ativo = true;
    listarModelos(campanha.idSistema, idTipo)
      .then((modelo) => ativo && setLinhas((atuais) => mesclarAtributos(modelo, atuais)))
      .catch((e) => ativo && setErro(mensagemDeErro(e)));
    return () => {
      ativo = false;
    };
  }, [campanha.idSistema, idTipo]);

  const alterarLinha = (indice, campo, valor) =>
    setLinhas((atuais) => atuais.map((l, i) => (i === indice ? { ...l, [campo]: valor } : l)));
  const removerLinha = (indice) => setLinhas((atuais) => atuais.filter((_, i) => i !== indice));
  const adicionarLinha = () => setLinhas((atuais) => [...atuais, { chave: '', valor: '', doModelo: false }]);

  async function aoEnviar(evento) {
    evento.preventDefault();
    setErro('');
    setSalvando(true);
    try {
      await aoSalvar({
        idTipoRegistro: idTipo,
        nome,
        resumo,
        descricao,
        atributos: atributosParaEnvio(linhas),
        tags: separarTags(textoTags),
      });
    } catch (e) {
      setErro(mensagemDeErro(e));
      setSalvando(false);
    }
  }

  return (
    <form onSubmit={aoEnviar} noValidate>
      {erro && <div className="alert alert-danger" role="alert">{erro}</div>}

      <div className="row g-3 mb-3">
        <div className="col-sm-4">
          <label htmlFor="tipoRegistro" className="form-label">Tipo</label>
          <select id="tipoRegistro" className="form-select" value={idTipo} onChange={(e) => setIdTipo(Number(e.target.value))}>
            {tipos.map((t) => <option key={t.id} value={t.id}>{rotuloTipo(t, campanha.idSistema)}</option>)}
          </select>
        </div>
        <div className="col-sm-8">
          <label htmlFor="nomeRegistro" className="form-label">Nome</label>
          <input id="nomeRegistro" className="form-control" maxLength={150} required
            value={nome} onChange={(e) => setNome(e.target.value)} />
        </div>
      </div>

      <div className="mb-3">
        <label htmlFor="resumoRegistro" className="form-label">Resumo <span className="text-secondary">(uma linha)</span></label>
        <input id="resumoRegistro" className="form-control" maxLength={200}
          value={resumo} onChange={(e) => setResumo(e.target.value)} />
      </div>

      <div className="mb-3">
        <label htmlFor="tagsRegistro" className="form-label">Tags <span className="text-secondary">(separadas por vírgula)</span></label>
        <input id="tagsRegistro" className="form-control" list="tagsDaCampanha" placeholder="vila, culto"
          value={textoTags} onChange={(e) => setTextoTags(e.target.value)} />
        <datalist id="tagsDaCampanha">
          {tagsExistentes.map((t) => <option key={t.id} value={t.nome} />)}
        </datalist>
      </div>

      <fieldset className="mb-3">
        <legend className="form-label fs-6 mb-2">Atributos</legend>
        {linhas.length === 0 && <p className="text-secondary small mb-2">Nenhum atributo.</p>}
        {linhas.map((linha, indice) => (
          <div className="row g-2 mb-2 align-items-center" key={indice}>
            <div className="col-5 col-sm-4">
              {linha.doModelo
                ? <label htmlFor={`valorAtributo${indice}`} className="col-form-label fw-semibold">{linha.chave}</label>
                : <input className="form-control" aria-label="Nome do atributo" placeholder="Nome" maxLength={60}
                    value={linha.chave} onChange={(e) => alterarLinha(indice, 'chave', e.target.value)} />}
            </div>
            <div className="col">
              <input id={`valorAtributo${indice}`} className="form-control" maxLength={500}
                aria-label={linha.doModelo ? undefined : 'Valor do atributo'} placeholder={linha.doModelo ? '' : 'Valor'}
                value={linha.valor} onChange={(e) => alterarLinha(indice, 'valor', e.target.value)} />
            </div>
            <div className="col-auto" style={{ width: 90 }}>
              {!linha.doModelo && (
                <button type="button" className="btn btn-sm btn-outline-danger" onClick={() => removerLinha(indice)}>
                  Remover
                </button>
              )}
            </div>
          </div>
        ))}
        <button type="button" className="btn btn-sm btn-outline-secondary" onClick={adicionarLinha}>
          Adicionar atributo
        </button>
      </fieldset>

      <div className="mb-3">
        <label htmlFor="descricaoRegistro" className="form-label">Descrição</label>
        <textarea id="descricaoRegistro" className="form-control" rows={6} maxLength={20000}
          value={descricao} onChange={(e) => setDescricao(e.target.value)} />
      </div>

      <div className="d-flex gap-2">
        <button type="submit" className="btn btn-primary" disabled={salvando}>{salvando ? 'Salvando…' : textoBotao}</button>
        <button type="button" className="btn btn-outline-secondary" onClick={aoCancelar} disabled={salvando}>Cancelar</button>
      </div>
    </form>
  );
}
