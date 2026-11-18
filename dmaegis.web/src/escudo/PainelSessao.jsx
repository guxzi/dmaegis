import { useState } from 'react';
import { mensagemDeErro } from '../api/erros.js';
import { ocultarSegredo, revelarSegredo } from '../api/sessoes.js';
import { ID_TIPO_PJ, PASSOS_DE_VINCULO, titulosDosPassos, vinculadosDoTipo } from '../sessoes/passos.js';

function Bloco({ titulo, children }) {
  return (
    <section className="mb-4">
      <h3 className="h6 text-uppercase text-secondary mb-2" style={{ letterSpacing: '0.12em' }}>{titulo}</h3>
      {children}
    </section>
  );
}

function Etiquetas({ registros, aoAbrir }) {
  if (registros.length === 0) return <span className="text-secondary small">—</span>;
  return (
    <div className="d-flex flex-wrap gap-2">
      {registros.map((r) => (
        <button key={r.id} type="button" className="etiqueta-registro" onClick={() => aoAbrir(r.id)}>{r.nome}</button>
      ))}
    </div>
  );
}

// "Sessão de hoje": o preparo à vista durante o jogo. Segredos são marcados na hora;
// clicar num registro abre o cartão lateral.
export default function PainelSessao({ sessao, registros, idSistema, aoAbrirRegistro, aoMudarSegredos }) {
  const [erro, setErro] = useState('');
  const titulos = titulosDosPassos(idSistema);
  const pjs = registros.filter((r) => r.idTipoRegistro === ID_TIPO_PJ);

  async function alternar(segredo) {
    setErro('');
    try {
      const atualizado = segredo.idSessaoRevelacao === sessao.id
        ? await ocultarSegredo(segredo.id)
        : await revelarSegredo(segredo.id, sessao.id);
      aoMudarSegredos(sessao.segredos.map((s) => (s.id === atualizado.id ? atualizado : s)));
    } catch (e) {
      setErro(mensagemDeErro(e));
    }
  }

  return (
    <>
      {erro && <div className="alert alert-danger py-2">{erro}</div>}

      <Bloco titulo={titulos[1]}>
        {sessao.comecoForte
          ? <p className="fs-5 mb-0 border-start border-3 ps-3" style={{ borderColor: 'var(--dm-violeta) !important' }}>{sessao.comecoForte}</p>
          : <p className="text-secondary small mb-0">Sem começo forte.</p>}
      </Bloco>

      <Bloco titulo={titulos[2]}>
        {sessao.cenas.length === 0 && <p className="text-secondary small mb-0">Nenhuma cena.</p>}
        <ol className="list-unstyled mb-0">
          {sessao.cenas.map((cena, i) => (
            <li key={i} className="d-flex align-items-center gap-2 mb-1">
              <span className="selo-escudo">{i + 1}</span>{cena}
            </li>
          ))}
        </ol>
      </Bloco>

      <Bloco titulo={titulos[3]}>
        {sessao.segredos.length === 0 && <p className="text-secondary small mb-0">Nenhum segredo pendente.</p>}
        {sessao.segredos.map((segredo) => {
          const revelado = segredo.idSessaoRevelacao === sessao.id;
          return (
            <div className="form-check mb-1" key={segredo.id}>
              <input className="form-check-input" type="checkbox" id={`escudoSegredo${segredo.id}`}
                checked={revelado} onChange={() => alternar(segredo)} />
              <label className={`form-check-label ${revelado ? 'text-decoration-line-through text-secondary' : ''}`}
                htmlFor={`escudoSegredo${segredo.id}`}>
                {segredo.texto}
              </label>
            </div>
          );
        })}
      </Bloco>

      <div className="row g-3">
        <div className="col-sm-6">
          <Bloco titulo={titulos[0]}>
            <Etiquetas registros={pjs} aoAbrir={aoAbrirRegistro} />
          </Bloco>
        </div>
        {PASSOS_DE_VINCULO.map(({ passo, idTipo }) => (
          <div className="col-sm-6" key={passo}>
            <Bloco titulo={titulos[passo - 1]}>
              <Etiquetas registros={vinculadosDoTipo(sessao.idsRegistro, registros, idTipo)} aoAbrir={aoAbrirRegistro} />
            </Bloco>
          </div>
        ))}
      </div>
    </>
  );
}
