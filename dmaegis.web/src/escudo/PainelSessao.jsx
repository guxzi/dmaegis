import { useState } from 'react';
import { mensagemDeErro } from '../api/erros.js';
import { urlDaImagem } from '../api/imagens.js';
import { ocultarSegredo, revelarSegredo } from '../api/sessoes.js';
import { COR_TIPO } from '../caderno/tipos.js';
import { ID_TIPO_PJ, PASSOS_DE_VINCULO, titulosDosPassos, vinculadosDoTipo } from '../sessoes/passos.js';
import { gravarCenaAtual, lerCenaAtual } from './cenaAtual.js';
import DicaConducao from './DicaConducao.jsx';

const ID_TIPO_NPC = 1;
const ID_TIPO_LOCAL = 4;

function Bloco({ titulo, children, extra }) {
  return (
    <section className="mb-4">
      <div className="d-flex align-items-center gap-2 mb-2">
        <h3 className="h6 text-uppercase mb-0" style={{ letterSpacing: '0.12em', color: 'var(--dm-estrela)' }}>{titulo}</h3>
        {extra}
      </div>
      {children}
    </section>
  );
}

function Retrato({ registro, retratos }) {
  if (!retratos[registro.id]) return null;
  return <img className="retrato" src={urlDaImagem(retratos[registro.id])} alt="" loading="lazy" />;
}

function Etiquetas({ registros, retratos, aoAbrir }) {
  if (registros.length === 0) return <span className="text-secondary small">—</span>;
  return (
    <div className="d-flex flex-wrap gap-2">
      {registros.map((r) => (
        <button key={r.id} type="button" className="etiqueta-registro" onClick={() => aoAbrir(r.id)}
          style={{ '--cor-tipo': COR_TIPO[r.idTipoRegistro] }}>
          <Retrato registro={r} retratos={retratos} />{r.nome}
        </button>
      ))}
    </div>
  );
}

// Faixa do topo: onde a mesa está, o que já foi revelado, o que foi anotado e quem está em cena.
function ResumoVivo({ cenaAtual, totalCenas, reveladosAqui, pendentes, anotacoes, emCena, retratos, aoAbrir }) {
  const ultima = anotacoes.at(-1);
  return (
    <section className="resumo-vivo mb-4" aria-label="Resumo da sessão">
      <div className="resumo-vivo-numeros">
        <div>
          <strong>{totalCenas ? `${cenaAtual + 1}/${totalCenas}` : '—'}</strong>
          <span>cena</span>
        </div>
        <div>
          <strong>{reveladosAqui}<small>/{reveladosAqui + pendentes}</small></strong>
          <span>segredos revelados</span>
        </div>
        <div>
          <strong>{anotacoes.length}</strong>
          <span>anotações</span>
        </div>
      </div>
      {ultima && <p className="resumo-vivo-ultima text-truncate mb-0" title={ultima.texto}>Última anotação: {ultima.texto}</p>}
      {emCena.length > 0 && (
        <div className="resumo-vivo-rostos">
          {emCena.map((r) => (
            <button key={r.id} type="button" className="rosto" onClick={() => aoAbrir(r.id)} title={r.nome}
              style={{ '--cor-tipo': COR_TIPO[r.idTipoRegistro] }}>
              {retratos[r.id]
                ? <img className="retrato" src={urlDaImagem(retratos[r.id])} alt="" loading="lazy" />
                : <span className="retrato retrato-inicial" aria-hidden="true">{r.nome.charAt(0)}</span>}
              <span className="rosto-nome">{r.nome}</span>
            </button>
          ))}
        </div>
      )}
    </section>
  );
}

// "Sessão de hoje": o preparo à vista durante o jogo. Segredos são marcados na hora;
// clicar num registro abre o cartão lateral.
export default function PainelSessao({
  sessao, registros, idSistema, retratos = {}, anotacoes = [], aoAbrirRegistro, aoMudarSegredos,
}) {
  const [erro, setErro] = useState('');
  const [cenaAtual, setCenaAtual] = useState(() => lerCenaAtual(sessao.id, sessao.cenas.length));
  const titulos = titulosDosPassos(idSistema);
  const pjs = registros.filter((r) => r.idTipoRegistro === ID_TIPO_PJ);
  const reveladosAqui = sessao.segredos.filter((s) => s.idSessaoRevelacao === sessao.id).length;
  const emCena = [
    ...vinculadosDoTipo(sessao.idsRegistro, registros, ID_TIPO_NPC),
    ...vinculadosDoTipo(sessao.idsRegistro, registros, ID_TIPO_LOCAL),
  ];

  function irParaCena(indice) {
    const limitado = Math.max(0, Math.min(indice, sessao.cenas.length - 1));
    setCenaAtual(limitado);
    gravarCenaAtual(sessao.id, limitado);
  }

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
      <DicaConducao idSistema={idSistema} />

      <ResumoVivo cenaAtual={cenaAtual} totalCenas={sessao.cenas.length} reveladosAqui={reveladosAqui}
        pendentes={sessao.segredos.length - reveladosAqui} anotacoes={anotacoes} emCena={emCena}
        retratos={retratos} aoAbrir={aoAbrirRegistro} />

      {erro && <div className="alert alert-danger py-2">{erro}</div>}

      <Bloco titulo={titulos[1]}>
        {sessao.comecoForte
          ? <p className="fs-5 mb-0 border-start border-3 ps-3" style={{ borderColor: 'var(--dm-violeta) !important' }}>{sessao.comecoForte}</p>
          : <p className="text-secondary small mb-0">Sem começo forte.</p>}
      </Bloco>

      <Bloco titulo={titulos[2]} extra={sessao.cenas.length > 1 && (
        <div className="ms-auto d-flex gap-1">
          <button type="button" className="btn btn-sm btn-fantasma px-2 py-0" disabled={cenaAtual === 0}
            onClick={() => irParaCena(cenaAtual - 1)} aria-label="Cena anterior">←</button>
          <button type="button" className="btn btn-sm btn-fantasma px-2 py-0" disabled={cenaAtual >= sessao.cenas.length - 1}
            onClick={() => irParaCena(cenaAtual + 1)}>Próxima →</button>
        </div>
      )}>
        {sessao.cenas.length === 0 && <p className="text-secondary small mb-0">Nenhuma cena.</p>}
        {sessao.cenas.length > 0 && (
          <div className="progresso-cenas mb-2" aria-hidden="true">
            <span style={{ width: `${((cenaAtual + 1) / sessao.cenas.length) * 100}%` }} />
          </div>
        )}
        <ol className="list-unstyled mb-0 lista-cenas">
          {sessao.cenas.map((cena, i) => {
            const estado = i < cenaAtual ? 'passada' : i === cenaAtual ? 'atual' : 'futura';
            return (
              <li key={i}>
                <button type="button" className={`cena-escudo ${estado}`} onClick={() => irParaCena(i)}
                  aria-current={estado === 'atual' ? 'step' : undefined}>
                  <span className="selo-escudo">{estado === 'passada' ? '✓' : i + 1}</span>
                  <span className="flex-grow-1 text-start">{cena}</span>
                  {estado === 'atual' && <span className="cena-agora">agora</span>}
                </button>
              </li>
            );
          })}
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
            <Etiquetas registros={pjs} retratos={retratos} aoAbrir={aoAbrirRegistro} />
          </Bloco>
        </div>
        {PASSOS_DE_VINCULO.map(({ passo, idTipo }) => (
          <div className="col-sm-6" key={passo}>
            <Bloco titulo={titulos[passo - 1]}>
              <Etiquetas registros={vinculadosDoTipo(sessao.idsRegistro, registros, idTipo)} retratos={retratos}
                aoAbrir={aoAbrirRegistro} />
            </Bloco>
          </div>
        ))}
      </div>
    </>
  );
}
