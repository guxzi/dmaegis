import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link, useParams, useSearchParams } from 'react-router';
import { listarRegistros } from '../api/caderno.js';
import { mensagemDeErro } from '../api/erros.js';
import { listarImagensDaCampanha, urlDaImagem } from '../api/imagens.js';
import {
  adicionarFichas, alterarNevoa, ativarMesa, criarMesa, encerrarCombate, esconderTudo, excluirFicha, excluirMesa,
  iniciarCombate, listarMesas, moverFicha, obterMesa, proximoTurno, salvarFicha, salvarMesa,
} from '../api/mesas.js';
import { Carregando } from '../auth/RotaProtegida.jsx';
import SeloSistema from '../campanhas/SeloSistema.jsx';
import { useCaderno } from '../caderno/useCaderno.js';
import Cabecalho from '../componentes/Cabecalho.jsx';
import ConfirmarExclusao from '../componentes/ConfirmarExclusao.jsx';
import AdicionarFichas from '../mesa/AdicionarFichas.jsx';
import AjusteNevoa from '../mesa/AjusteNevoa.jsx';
import { celulaDaChave, emLotes, todasAsCelulas } from '../mesa/nevoa.js';
import { corpoDaFicha, iniciativasParaComecar } from '../mesa/combate.js';
import { condicoesDoSistema } from '../mesa/condicoes.js';
import EscolherMapa from '../mesa/EscolherMapa.jsx';
import { celulaLivrePerto } from '../mesa/grade.js';
import ListaIniciativa from '../mesa/ListaIniciativa.jsx';
import PainelFicha from '../mesa/PainelFicha.jsx';
import PalcoMapa from '../mesa/PalcoMapa.jsx';
import { urlEventosDaMesa } from '../api/jogador.js';
import { ouvirMesa } from '../mesa/eventos.js';
import PainelJogadores from '../mesa/PainelJogadores.jsx';
import { abrirProjecao, avisarMesaMudou, mostrarMesaNaProjecao } from '../projecao/canal.js';

const GRADE_PADRAO = { tamanhoCelula: 70, deslocamentoX: 0, deslocamentoY: 0, gradeVisivel: true };

function corpoDaMesa(mesa, mudancas = {}) {
  const m = { ...mesa, ...mudancas };
  return {
    nome: m.nome,
    idImagemRegistro: m.idImagemRegistro ?? null,
    tamanhoCelula: m.tamanhoCelula,
    deslocamentoX: m.deslocamentoX,
    deslocamentoY: m.deslocamentoY,
    gradeVisivel: m.gradeVisivel,
    movimentoLivre: m.movimentoLivre ?? true,
    nevoa: m.nevoa ?? false,
  };
}

// Ajuste da grade com prévia ao vivo: os controles mudam a mesa na hora e salvam ao soltar.
function AjusteGrade({ mesa, aoMudar, aoSalvar, aoFechar }) {
  const controle = (chave, rotulo, min, max) => (
    <label className="ajuste-grade-controle">
      <span>{rotulo} <strong>{mesa[chave]}</strong></span>
      <input type="range" className="form-range" min={min} max={max} value={mesa[chave]}
        onChange={(e) => aoMudar({ [chave]: Number(e.target.value) })}
        onPointerUp={() => aoSalvar()} onKeyUp={() => aoSalvar()} />
    </label>
  );
  return (
    <div className="ajuste-grade cartao-astral" onPointerDown={(e) => e.stopPropagation()}>
      <div className="d-flex align-items-center mb-2">
        <strong className="flex-grow-1">Grade</strong>
        <button type="button" className="btn-close btn-close-white btn-sm" onClick={aoFechar} aria-label="Fechar ajuste da grade" />
      </div>
      {controle('tamanhoCelula', 'Célula', 20, 200)}
      {controle('deslocamentoX', 'Mover →', -100, 100)}
      {controle('deslocamentoY', 'Mover ↓', -100, 100)}
      <div className="form-check form-switch mt-1">
        <input className="form-check-input" type="checkbox" id="gradeVisivel" checked={mesa.gradeVisivel}
          onChange={(e) => { aoMudar({ gradeVisivel: e.target.checked }); aoSalvar({ gradeVisivel: e.target.checked }); }} />
        <label className="form-check-label" htmlFor="gradeVisivel">Mostrar a grade</label>
      </div>
      <p className="small text-secondary mb-0 mt-2">Encaixe as linhas nos quadrados desenhados no mapa.</p>
    </div>
  );
}

// A mesa: mapa com grade e fichas, combate ao lado, e a visão dos jogadores na projeção.
export default function Mesa() {
  const { id } = useParams();
  const [parametros, setParametros] = useSearchParams();
  const { campanha, rotulo, naoEncontrada } = useCaderno(id);
  const [mesas, setMesas] = useState(null);
  const [mesa, setMesa] = useState(null);
  const [registros, setRegistros] = useState([]);
  const [imagens, setImagens] = useState([]);
  const [idSelecionada, setIdSelecionada] = useState(null);
  const [erro, setErro] = useState('');
  const [ocupado, setOcupado] = useState(false);
  const [escolhendoMapa, setEscolhendoMapa] = useState(false);
  const [ajustandoGrade, setAjustandoGrade] = useState(false);
  const [excluindo, setExcluindo] = useState(null);
  const [renomeando, setRenomeando] = useState(false);
  const [vendoJogadores, setVendoJogadores] = useState(false);
  // Com o painel da névoa aberto, o palco pinta em vez de mover a vista.
  const [pincel, setPincel] = useState(null);
  const [limitesGrade, setLimitesGrade] = useState(null);

  const idMesa = Number(parametros.get('mesa')) || mesas?.[0]?.id || null;
  const fecharExclusao = useCallback(() => setExcluindo(null), []);
  const fecharJogadores = useCallback(() => setVendoJogadores(false), []);
  const condicoes = useMemo(() => condicoesDoSistema(campanha?.idSistema), [campanha?.idSistema]);

  useEffect(() => {
    listarMesas(id).then(setMesas).catch((e) => setErro(mensagemDeErro(e)));
    listarRegistros(id).then(setRegistros).catch(() => {});
    listarImagensDaCampanha(id).then(setImagens).catch(() => {});
  }, [id]);

  useEffect(() => {
    if (!idMesa) return undefined;
    let ativo = true;
    obterMesa(idMesa).then((m) => ativo && setMesa(m)).catch((e) => ativo && setErro(mensagemDeErro(e)));
    return () => {
      ativo = false;
    };
  }, [idMesa]);

  // Tempo real: quando um jogador move a ficha (ou outra aba mexe na mesa), busca a mesa de novo.
  useEffect(() => {
    if (!idMesa) return undefined;
    return ouvirMesa(urlEventosDaMesa(id), () => {
      obterMesa(idMesa).then((m) => {
        setMesa(m);
        avisarMesaMudou(m.id);
      }).catch(() => {});
    });
  }, [id, idMesa]);

  // Toda resposta da API traz a mesa inteira: atualiza a tela e avisa a projeção.
  const aplicar = useCallback((nova) => {
    setMesa(nova);
    setMesas((lista) => lista?.map((m) => (m.id === nova.id ? { ...m, nome: nova.nome, ativa: nova.ativa, emCombate: nova.emCombate } : m)));
    avisarMesaMudou(nova.id);
  }, []);

  const executar = useCallback(async (acao) => {
    setErro('');
    setOcupado(true);
    try {
      const resultado = await acao();
      if (resultado?.participantes) aplicar(resultado);
      return resultado;
    } catch (e) {
      setErro(mensagemDeErro(e));
      return null;
    } finally {
      setOcupado(false);
    }
  }, [aplicar]);

  async function novaMesa() {
    const nome = mesas?.length ? `Mesa ${mesas.length + 1}` : 'Primeira mesa';
    const criada = await executar(() => criarMesa(id, { nome, ...GRADE_PADRAO }));
    if (!criada) return;
    setMesas((lista) => [...(lista ?? []), { id: criada.id, nome: criada.nome, ativa: criada.ativa, emCombate: false }]);
    setParametros({ mesa: criada.id }, { replace: true });
  }

  const salvarFichaComMudancas = useCallback((ficha, mudancas) => {
    // Otimista: a tela muda já; a resposta do servidor confirma (ou o erro aparece e recarrega).
    setMesa((m) => m && ({ ...m, participantes: m.participantes.map((p) => (p.id === ficha.id ? { ...p, ...mudancas } : p)) }));
    executar(() => salvarFicha(ficha.id, corpoDaFicha(ficha, mudancas)));
  }, [executar]);

  const moverNoMapa = useCallback(async (ficha, coluna, linha) => {
    setMesa((m) => m && ({ ...m, participantes: m.participantes.map((p) => (p.id === ficha.id ? { ...p, coluna, linha } : p)) }));
    try {
      await moverFicha(ficha.id, coluna, linha);
      avisarMesaMudou(ficha.idMesa ?? idMesa);
    } catch (e) {
      setErro(mensagemDeErro(e));
      obterMesa(idMesa).then(setMesa).catch(() => {});
    }
  }, [idMesa]);

  // Fichas novas já entram no mapa: PJs a partir da coluna 2, os outros mais à direita, sem empilhar.
  async function adicionar(pedido) {
    const antes = new Set(mesa.participantes.map((p) => p.id));
    const nova = await executar(() => adicionarFichas(mesa.id, pedido));
    if (!nova) return;
    const ocupadas = new Set(nova.participantes.filter((p) => p.coluna != null).map((p) => `${p.coluna},${p.linha}`));
    for (const ficha of nova.participantes.filter((p) => !antes.has(p.id) && p.coluna == null)) {
      const { coluna, linha } = celulaLivrePerto(ficha.jogador ? 2 : 8, 2, ocupadas);
      ocupadas.add(`${coluna},${linha}`);
      await moverNoMapa(ficha, coluna, linha);
    }
  }

  function colocarNoMapa(ficha) {
    const ocupadas = new Set(mesa.participantes.filter((p) => p.coluna != null).map((p) => `${p.coluna},${p.linha}`));
    const { coluna, linha } = celulaLivrePerto(2, 2, ocupadas);
    moverNoMapa(ficha, coluna, linha);
  }

  // O pincel soltou: mostra já o resultado e manda só a diferença para o servidor.
  const pintarNevoa = useCallback((mudancas, final) => {
    setMesa((m) => m && ({ ...m, celulasReveladas: [...final].map(celulaDaChave) }));
    executar(() => alterarNevoa(idMesa, mudancas.revelar, mudancas.esconder));
  }, [executar, idMesa]);

  async function revelarTudo() {
    if (!limitesGrade) return;
    for (const lote of emLotes(todasAsCelulas(limitesGrade))) {
      const ok = await executar(() => alterarNevoa(mesa.id, lote, []));
      if (!ok) return;
    }
  }

  function salvarGrade(mudancas = {}) {
    if (!mesa) return;
    executar(() => salvarMesa(mesa.id, corpoDaMesa(mesa, mudancas)));
  }

  async function confirmarExclusao() {
    const alvo = excluindo;
    setExcluindo(null);
    if (alvo.tipo === 'ficha') {
      if (idSelecionada === alvo.ficha.id) setIdSelecionada(null);
      await executar(() => excluirFicha(alvo.ficha.id));
    } else {
      const ok = await executar(async () => {
        await excluirMesa(mesa.id);
        return true;
      });
      if (!ok) return;
      setMesa(null);
      setMesas((lista) => lista.filter((m) => m.id !== alvo.id));
      setParametros({}, { replace: true });
    }
  }

  // Atalhos: N passa o turno; Esc tira a seleção (fora dos campos de texto).
  useEffect(() => {
    function aoTeclar(evento) {
      const alvo = evento.target;
      if (alvo.closest?.('input, textarea, select, [contenteditable="true"]')) return;
      if (evento.key === 'Escape') setIdSelecionada(null);
      if ((evento.key === 'n' || evento.key === 'N') && mesa?.emCombate && !evento.ctrlKey && !evento.metaKey) {
        evento.preventDefault();
        executar(() => proximoTurno(mesa.id));
      }
    }
    window.addEventListener('keydown', aoTeclar);
    return () => window.removeEventListener('keydown', aoTeclar);
  }, [mesa?.emCombate, mesa?.id, executar]);

  if (naoEncontrada) {
    return (
      <>
        <Cabecalho />
        <main className="container"><div className="alert alert-warning">Campanha não encontrada. <Link to="/">Voltar</Link></div></main>
      </>
    );
  }

  const selecionada = mesa?.participantes.find((p) => p.id === idSelecionada) ?? null;
  const daVez = mesa?.participantes.find((p) => p.id === mesa.idParticipanteVez);
  const imagemAtual = imagens.find((i) => i.id === mesa?.idImagemRegistro);

  return (
    <>
      <Cabecalho idCampanha={id} className="mb-2" />
      <main className="container-fluid px-3 px-lg-4 pagina-mesa">
        <div className="barra-mesa">
          <h1 className="h4 mb-0 d-flex align-items-center gap-2">
            <span className="titulo-estelar">Mesa</span>
            {campanha && <SeloSistema idSistema={campanha.idSistema} nomeSistema={campanha.nomeSistema} />}
          </h1>

          {mesas?.length > 0 && (
            <div className="d-flex align-items-center gap-2">
              {renomeando && mesa ? (
                <input className="form-control form-control-sm" autoFocus defaultValue={mesa.nome} maxLength={100}
                  aria-label="Nome da mesa" style={{ width: 200 }}
                  onBlur={(e) => {
                    setRenomeando(false);
                    const nome = e.target.value.trim();
                    if (nome && nome !== mesa.nome) executar(() => salvarMesa(mesa.id, corpoDaMesa(mesa, { nome })));
                  }}
                  onKeyDown={(e) => { if (e.key === 'Enter') e.currentTarget.blur(); if (e.key === 'Escape') setRenomeando(false); }} />
              ) : (
                <select className="form-select form-select-sm" style={{ width: 'auto' }} value={idMesa ?? ''}
                  onChange={(e) => { setIdSelecionada(null); setMesa(null); setParametros({ mesa: e.target.value }, { replace: true }); }}
                  aria-label="Mesa">
                  {mesas.map((m) => <option key={m.id} value={m.id}>{m.ativa ? '◉ ' : ''}{m.nome}</option>)}
                </select>
              )}
              {mesa && !renomeando && (
                <button type="button" className="btn btn-sm btn-fantasma px-2" onClick={() => setRenomeando(true)} title="Renomear a mesa">✎</button>
              )}
            </div>
          )}
          <button type="button" className="btn btn-sm btn-fantasma" onClick={novaMesa} disabled={ocupado}>+ Nova mesa</button>

          {mesa && (
            <div className="d-flex flex-wrap align-items-center gap-2 ms-lg-auto">
              <button type="button" className="btn btn-sm btn-fantasma" onClick={() => setEscolhendoMapa(true)}>
                {imagemAtual ? `Mapa: ${imagemAtual.titulo}` : 'Escolher mapa'}
              </button>
              <button type="button" className="btn btn-sm btn-fantasma" aria-pressed={ajustandoGrade}
                onClick={() => { setAjustandoGrade((v) => !v); setPincel(null); }}>Grade</button>
              <button type="button" className={`btn btn-sm btn-fantasma${mesa.nevoa ? ' com-nevoa' : ''}`} aria-pressed={Boolean(pincel)}
                onClick={() => { setPincel((p) => (p ? null : { modo: 'revelar', tamanho: 3 })); setAjustandoGrade(false); }}
                title="Névoa de guerra: revele o mapa aos poucos">Névoa</button>
              {mesa.ativa
                ? <span className="selo-ativa" title="É a mesa que os jogadores veem">◉ Ativa</span>
                : <button type="button" className="btn btn-sm btn-fantasma" onClick={() => executar(() => ativarMesa(mesa.id))}>Tornar ativa</button>}
              <button type="button" className="btn btn-sm btn-fantasma" onClick={() => setVendoJogadores(true)}
                title="Links de convite para os jogadores abrirem a mesa no navegador">Jogadores</button>
              <button type="button" className="btn btn-sm btn-primary"
                onClick={() => { mostrarMesaNaProjecao(mesa.id); abrirProjecao(); }}
                title="Abre a janela de projeção com o mapa, as fichas visíveis e a ordem dos turnos">
                Mostrar aos jogadores
              </button>
              <button type="button" className="btn btn-sm btn-outline-danger px-2" title="Excluir esta mesa"
                onClick={() => setExcluindo({ tipo: 'mesa', id: mesa.id, nome: mesa.nome })}>✕</button>
            </div>
          )}
        </div>

        {erro && <div className="alert alert-danger py-2 my-2">{erro}</div>}
        {mesas === null && !erro && <Carregando />}

        {mesas?.length === 0 && (
          <div className="card cartao-astral mx-auto mt-5 text-center convite-mesa" style={{ maxWidth: 520 }}>
            <div className="card-body p-4">
              <h2 className="h4 titulo-estelar">Monte a primeira mesa</h2>
              <p className="text-secondary">Escolha um mapa da galeria, ajuste a grade e coloque as fichas. Os jogadores veem tudo pela projeção, sem os segredos.</p>
              <button type="button" className="btn btn-primary" onClick={novaMesa} disabled={ocupado}>Criar a mesa</button>
            </div>
          </div>
        )}

        {mesas?.length > 0 && !mesa && !erro && <Carregando />}

        {mesa && (
          <div className="mesa-area">
            <div className="mesa-palco-coluna">
              <PalcoMapa mesa={mesa} urlImagem={urlDaImagem} modo="mestre" podeArrastar={() => true}
                aoMover={moverNoMapa} idSelecionada={idSelecionada} aoSelecionar={setIdSelecionada}
                pincel={pincel} aoPintarNevoa={pintarNevoa} aoMedirGrade={setLimitesGrade}>
                {ajustandoGrade && (
                  <AjusteGrade mesa={mesa} aoMudar={(m) => setMesa((atual) => ({ ...atual, ...m }))}
                    aoSalvar={salvarGrade} aoFechar={() => setAjustandoGrade(false)} />
                )}
                {pincel && (
                  <AjusteNevoa mesa={mesa} pincel={pincel} aoMudarPincel={setPincel} ocupado={ocupado}
                    aoLigar={(ligada) => executar(() => salvarMesa(mesa.id, corpoDaMesa(mesa, { nevoa: ligada })))}
                    aoRevelarTudo={revelarTudo} aoEsconderTudo={() => executar(() => esconderTudo(mesa.id))}
                    aoFechar={() => setPincel(null)} />
                )}
                {mesa.emCombate && daVez && (
                  <div className="faixa-vez" key={daVez.id}>
                    <span className="faixa-vez-rodada">Rodada {mesa.rodada}</span>
                    <span>Vez de <strong>{daVez.nome}</strong></span>
                  </div>
                )}
              </PalcoMapa>
            </div>

            <aside className="mesa-lateral cartao-astral">
              <div className="mesa-lateral-cabecalho">
                <span className="titulo-runico">{mesa.emCombate ? `Combate · Rodada ${mesa.rodada}` : 'Fichas'}</span>
                <div className="ms-auto d-flex gap-2">
                  {mesa.emCombate ? (
                    <>
                      <button type="button" className="btn btn-sm btn-primary" disabled={ocupado}
                        onClick={() => executar(() => proximoTurno(mesa.id))} title="Atalho: N">Próximo turno →</button>
                      <button type="button" className="btn btn-sm btn-fantasma" disabled={ocupado}
                        onClick={() => executar(() => encerrarCombate(mesa.id))}>Encerrar</button>
                    </>
                  ) : (
                    <button type="button" className="btn btn-sm btn-primary" disabled={ocupado || mesa.participantes.length === 0}
                      onClick={() => executar(() => iniciarCombate(mesa.id, iniciativasParaComecar(mesa.participantes)))}
                      title="Monstros e NPCs rolam d20 + bônus; digite a iniciativa dos PJs">
                      ⚔ Iniciar combate
                    </button>
                  )}
                </div>
              </div>

              <div className="mesa-lateral-corpo">
                <ListaIniciativa mesa={mesa} urlImagem={urlDaImagem} idSelecionada={idSelecionada}
                  aoSelecionar={(idFicha) => setIdSelecionada((atual) => (atual === idFicha ? null : idFicha))}
                  aoSalvarFicha={salvarFichaComMudancas} />

                {selecionada && (
                  <PainelFicha key={selecionada.id} ficha={selecionada} condicoes={condicoes} aoSalvar={salvarFichaComMudancas}
                    aoColocarNoMapa={colocarNoMapa} aoTirarDoMapa={(f) => moverNoMapa(f, null, null)}
                    aoRemover={(f) => setExcluindo({ tipo: 'ficha', ficha: f, nome: f.nome })}
                    aoFechar={() => setIdSelecionada(null)} />
                )}

                <div className="divisor-estelar my-3" />
                <AdicionarFichas registros={registros} rotulo={rotulo} ocupado={ocupado}
                  aoAdicionar={adicionar} />
              </div>
            </aside>
          </div>
        )}
      </main>

      {escolhendoMapa && mesa && (
        <EscolherMapa imagens={imagens} idAtual={mesa.idImagemRegistro} aoFechar={() => setEscolhendoMapa(false)}
          aoEscolher={(idImagem) => {
            setEscolhendoMapa(false);
            executar(() => salvarMesa(mesa.id, corpoDaMesa(mesa, { idImagemRegistro: idImagem })));
          }} />
      )}

      {vendoJogadores && mesa && (
        <PainelJogadores idCampanha={id} mesa={mesa} aoFechar={fecharJogadores}
          aoMudarMovimento={(livre) => executar(() => salvarMesa(mesa.id, corpoDaMesa(mesa, { movimentoLivre: livre })))} />
      )}

      <ConfirmarExclusao aberto={Boolean(excluindo)}
        titulo={excluindo?.tipo === 'mesa' ? 'Excluir a mesa?' : 'Remover a ficha?'}
        texto={excluindo?.tipo === 'mesa'
          ? `A mesa "${excluindo.nome}" e todas as fichas dela serão apagadas. O mapa continua na galeria.`
          : `"${excluindo?.nome}" sai da mesa. O registro do caderno continua lá.`}
        aoConfirmar={confirmarExclusao} aoCancelar={fecharExclusao} />
    </>
  );
}
