import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { Link, useParams, useSearchParams } from 'react-router';
import { listarRegistros } from '../api/caderno.js';
import { mensagemDeErro } from '../api/erros.js';
import { listarImagensDaCampanha, urlDaImagem } from '../api/imagens.js';
import {
  adicionarFichas, alterarNevoa, ativarMesa, criarMesa, encerrarCombate, esconderTudo, excluirFicha, excluirMesa,
  enviarRolagem, enviarSinal, iniciarCombate, listarMesas, listarRolagens, moverFicha, obterMesa, proximoTurno, salvarFicha, salvarMesa,
} from '../api/mesas.js';
import { Carregando } from '../auth/RotaProtegida.jsx';
import SeloSistema from '../campanhas/SeloSistema.jsx';
import { useCaderno } from '../caderno/useCaderno.js';
import Cabecalho from '../componentes/Cabecalho.jsx';
import ConfirmarExclusao from '../componentes/ConfirmarExclusao.jsx';
import AdicionarFichas from '../mesa/AdicionarFichas.jsx';
import AjusteNevoa from '../mesa/AjusteNevoa.jsx';
import { celulaDaChave, desfazerPintura, emLotes, todasAsCelulas } from '../mesa/nevoa.js';
import { corpoDaFicha, descreverMudanca, iniciativasParaComecar, valoresAnteriores } from '../mesa/combate.js';
import { condicoesDoSistema } from '../mesa/condicoes.js';
import EscolherMapa from '../mesa/EscolherMapa.jsx';
import { celulaLivrePerto } from '../mesa/grade.js';
import ListaIniciativa from '../mesa/ListaIniciativa.jsx';
import PainelFicha from '../mesa/PainelFicha.jsx';
import PalcoMapa from '../mesa/PalcoMapa.jsx';
import { urlEventosDaMesa } from '../api/jogador.js';
import { CLIMAS, limitarTaxa } from '../mesa/efeitos.js';
import { ID_TELA, ouvirMesa } from '../mesa/eventos.js';
import RolagensNaMesa from '../mesa/RolagensNaMesa.jsx';
import { useSinaisMesa } from '../mesa/useEfeitos.js';
import { useRolagens } from '../mesa/useRolagens.js';
import PainelEntregas from '../mesa/PainelEntregas.jsx';
import PainelJogadores from '../mesa/PainelJogadores.jsx';
import PainelRolagens from '../mesa/PainelRolagens.jsx';
import { abrirProjecao, avisarMesaMudou, avisarSinalNaProjecao, mostrarMesaNaProjecao } from '../projecao/canal.js';

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
    clima: m.clima ?? null,
  };
}

// Clima por cima do mapa (os jogadores também veem).
function AjusteClima({ clima, aoEscolher, aoFechar }) {
  return (
    <div className="ajuste-clima cartao-astral" onPointerDown={(e) => e.stopPropagation()}>
      <div className="d-flex align-items-center mb-2">
        <strong className="flex-grow-1">Clima</strong>
        <button type="button" className="btn-close btn-close-white btn-sm" onClick={aoFechar} aria-label="Fechar clima" />
      </div>
      <div className="segmentado" role="group" aria-label="Clima">
        {CLIMAS.map((c) => (
          <button key={c.rotulo} type="button" aria-pressed={(clima ?? null) === c.id} onClick={() => aoEscolher(c.id)}>{c.rotulo}</button>
        ))}
      </div>
    </div>
  );
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
  const [escolhendoClima, setEscolhendoClima] = useState(false);
  const [entregando, setEntregando] = useState(false);
  const [pediuIniciativa, setPediuIniciativa] = useState(false);
  // Com o painel da névoa aberto, o palco pinta em vez de mover a vista.
  const [pincel, setPincel] = useState(null);
  const [limitesGrade, setLimitesGrade] = useState(null);
  // Desfazer: as últimas ações reversíveis (mover, mudar a ficha, pintar a névoa), da mais nova para a mais velha.
  // O histórico é de uma mesa só: ao trocar de mesa, ele fica vazio.
  const historico = useRef({ idMesa: null, itens: [] });
  const [tamanhoHistorico, setTamanhoHistorico] = useState({ idMesa: null, n: 0 });
  // Muda a cada aviso da mesa, para o painel de rolagens buscar as novas.
  const [versaoMesa, setVersaoMesa] = useState(0);
  const [avisoDesfazer, setAvisoDesfazer] = useState('');

  const idMesa = Number(parametros.get('mesa')) || mesas?.[0]?.id || null;
  const fecharExclusao = useCallback(() => setExcluindo(null), []);
  const fecharJogadores = useCallback(() => setVendoJogadores(false), []);
  const fecharEntregas = useCallback(() => setEntregando(false), []);
  const podeDesfazer = tamanhoHistorico.idMesa === idMesa && tamanhoHistorico.n > 0;
  const listarRolagensDaMesa = useCallback(() => (idMesa ? listarRolagens(idMesa) : Promise.resolve([])), [idMesa]);
  const enviarRolagemDaMesa = useCallback((rolagem) => enviarRolagem(idMesa, rolagem), [idMesa]);
  const condicoes = useMemo(() => condicoesDoSistema(campanha?.idSistema), [campanha?.idSistema]);
  const rolagens = useRolagens(listarRolagensDaMesa, versaoMesa);
  // Pings, reações e arrastos ao vivo (os dos jogadores chegam pelo fluxo da mesa).
  const sinais = useSinaisMesa(idMesa);
  const receberSinal = useRef(sinais.receber);
  useEffect(() => {
    receberSinal.current = sinais.receber;
  }, [sinais.receber]);

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
      setVersaoMesa((v) => v + 1);
      obterMesa(idMesa).then((m) => {
        setMesa(m);
        avisarMesaMudou(m.id);
      }).catch(() => {});
    }, undefined, (sinal) => {
      receberSinal.current(sinal);
      // O que os jogadores podem ver também vai para a janela de projeção.
      if (sinal.publico) avisarSinalNaProjecao(sinal);
    });
  }, [id, idMesa]);

  // Ping do mestre: aparece aqui na hora e vai para os jogadores e para a projeção.
  const pingar = useCallback(({ coluna, linha }) => {
    if (!idMesa) return;
    const sinal = { tipo: 'ping', coluna, linha, origem: ID_TELA };
    const completo = { ...sinal, autor: 'Mestre', idMesa };
    sinais.receber(completo);
    avisarSinalNaProjecao(completo);
    enviarSinal(idMesa, sinal).catch(() => {});
  }, [idMesa, sinais]);

  // Pede a iniciativa aos jogadores: aparece um cartão na tela deles, e o que rolarem entra na ficha.
  const pedirIniciativa = useCallback(() => {
    if (!idMesa) return;
    enviarSinal(idMesa, { tipo: 'pedir-iniciativa', origem: ID_TELA }).catch(() => {});
    setPediuIniciativa(true);
  }, [idMesa]);

  useEffect(() => {
    if (!pediuIniciativa) return undefined;
    const t = setTimeout(() => setPediuIniciativa(false), 2500);
    return () => clearTimeout(t);
  }, [pediuIniciativa]);

  // Arrasto ao vivo: no máximo ~11 avisos por segundo; ao soltar, avisa a célula final na hora.
  const envioArrasto = useMemo(() => limitarTaxa((sinal, publico) => {
    enviarSinal(sinal.idMesa, sinal).catch(() => {});
    if (publico) avisarSinalNaProjecao({ ...sinal, autor: 'Mestre' });
  }, 90), []);
  useEffect(() => () => envioArrasto.cancelar(), [envioArrasto]);
  const arrastar = useCallback((ficha, posicao, terminou) => {
    if (!idMesa || !mesa) return;
    // Mesma regra do servidor: ficha oculta, ou monstro com a névoa ligada, não vai para a projeção.
    const publico = ficha.visivel !== false && (!mesa.nevoa || ficha.jogador);
    const sinal = {
      tipo: terminou ? 'arrasto-fim' : 'arrasto', idParticipante: ficha.id, coluna: posicao?.coluna ?? null, linha: posicao?.linha ?? null,
      origem: ID_TELA, idMesa,
    };
    if (!terminou) {
      envioArrasto.chamar(sinal, publico);
      return;
    }
    envioArrasto.cancelar();
    enviarSinal(idMesa, sinal).catch(() => {});
    if (publico) avisarSinalNaProjecao({ ...sinal, autor: 'Mestre' });
  }, [idMesa, mesa, envioArrasto]);

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

  const registrar = useCallback((descricao, desfazer) => {
    if (historico.current.idMesa !== idMesa) historico.current = { idMesa, itens: [] };
    const { itens } = historico.current;
    itens.push({ descricao, desfazer });
    if (itens.length > 30) itens.shift();
    setTamanhoHistorico({ idMesa, n: itens.length });
  }, [idMesa]);

  const salvarFichaComMudancas = useCallback(function salvar(ficha, mudancas, comHistorico = true) {
    if (comHistorico) {
      const anteriores = valoresAnteriores(ficha, mudancas);
      registrar(descreverMudanca(ficha, mudancas), () => salvar({ ...ficha, ...mudancas }, anteriores, false));
    }
    // Otimista: a tela muda já; a resposta do servidor confirma (ou o erro aparece e recarrega).
    setMesa((m) => m && ({ ...m, participantes: m.participantes.map((p) => (p.id === ficha.id ? { ...p, ...mudancas } : p)) }));
    return executar(() => salvarFicha(ficha.id, corpoDaFicha(ficha, mudancas)));
  }, [executar, registrar]);

  const moverNoMapa = useCallback(async function mover(ficha, coluna, linha, comHistorico = true) {
    if (comHistorico) {
      registrar(`mover ${ficha.nome}`, () => mover({ ...ficha, coluna, linha }, ficha.coluna ?? null, ficha.linha ?? null, false));
    }
    setMesa((m) => m && ({ ...m, participantes: m.participantes.map((p) => (p.id === ficha.id ? { ...p, coluna, linha } : p)) }));
    try {
      await moverFicha(ficha.id, coluna, linha);
      avisarMesaMudou(ficha.idMesa ?? idMesa);
    } catch (e) {
      setErro(mensagemDeErro(e));
      obterMesa(idMesa).then(setMesa).catch(() => {});
    }
  }, [idMesa, registrar]);

  // Fichas novas já entram no mapa: PJs a partir da coluna 2, os outros mais à direita, sem empilhar.
  async function adicionar(pedido) {
    const antes = new Set(mesa.participantes.map((p) => p.id));
    const nova = await executar(() => adicionarFichas(mesa.id, pedido));
    if (!nova) return;
    const ocupadas = new Set(nova.participantes.filter((p) => p.coluna != null).map((p) => `${p.coluna},${p.linha}`));
    for (const ficha of nova.participantes.filter((p) => !antes.has(p.id) && p.coluna == null)) {
      const { coluna, linha } = celulaLivrePerto(ficha.jogador ? 2 : 8, 2, ocupadas);
      ocupadas.add(`${coluna},${linha}`);
      await moverNoMapa(ficha, coluna, linha, false);
    }
  }

  function colocarNoMapa(ficha) {
    const ocupadas = new Set(mesa.participantes.filter((p) => p.coluna != null).map((p) => `${p.coluna},${p.linha}`));
    const { coluna, linha } = celulaLivrePerto(2, 2, ocupadas);
    moverNoMapa(ficha, coluna, linha);
  }

  // O pincel soltou: mostra já o resultado e manda só a diferença para o servidor.
  const pintarNevoa = useCallback(function pintar(mudancas, final, comHistorico = true) {
    if (comHistorico) {
      registrar('pintura da névoa', () => pintar(
        { revelar: mudancas.esconder, esconder: mudancas.revelar }, desfazerPintura(final, mudancas), false));
    }
    setMesa((m) => m && ({ ...m, celulasReveladas: [...final].map(celulaDaChave) }));
    return executar(() => alterarNevoa(idMesa, mudancas.revelar, mudancas.esconder));
  }, [executar, idMesa, registrar]);

  const desfazer = useCallback(async () => {
    if (historico.current.idMesa !== idMesa) return;
    const item = historico.current.itens.pop();
    setTamanhoHistorico({ idMesa, n: historico.current.itens.length });
    if (!item) return;
    await item.desfazer();
    setAvisoDesfazer(`Desfeito: ${item.descricao}`);
  }, [idMesa]);

  useEffect(() => {
    if (!avisoDesfazer) return undefined;
    const t = setTimeout(() => setAvisoDesfazer(''), 2500);
    return () => clearTimeout(t);
  }, [avisoDesfazer]);

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

  // Atalhos: N passa o turno; Ctrl+Z desfaz; Esc tira a seleção (fora dos campos de texto).
  useEffect(() => {
    function aoTeclar(evento) {
      const alvo = evento.target;
      if (alvo.closest?.('input, textarea, select, [contenteditable="true"]')) return;
      if (evento.key === 'Escape') setIdSelecionada(null);
      if ((evento.key === 'z' || evento.key === 'Z') && (evento.ctrlKey || evento.metaKey) && !evento.shiftKey) {
        evento.preventDefault();
        desfazer();
        return;
      }
      if ((evento.key === 'n' || evento.key === 'N') && mesa?.emCombate && !evento.ctrlKey && !evento.metaKey) {
        evento.preventDefault();
        executar(() => proximoTurno(mesa.id));
      }
    }
    window.addEventListener('keydown', aoTeclar);
    return () => window.removeEventListener('keydown', aoTeclar);
  }, [mesa?.emCombate, mesa?.id, executar, desfazer]);

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
                onClick={() => { setAjustandoGrade((v) => !v); setPincel(null); setEscolhendoClima(false); }}>Grade</button>
              <button type="button" className={`btn btn-sm btn-fantasma${mesa.clima ? ' com-nevoa' : ''}`} aria-pressed={escolhendoClima}
                onClick={() => { setEscolhendoClima((v) => !v); setAjustandoGrade(false); setPincel(null); }}
                title="Chuva, neve, neblina ou brasas por cima do mapa">Clima</button>
              <button type="button" className={`btn btn-sm btn-fantasma${mesa.nevoa ? ' com-nevoa' : ''}`} aria-pressed={Boolean(pincel)}
                onClick={() => { setPincel((p) => (p ? null : { modo: 'revelar', tamanho: 3 })); setAjustandoGrade(false); setEscolhendoClima(false); }}
                title="Névoa de guerra: revele o mapa aos poucos">Névoa</button>
              {mesa.ativa
                ? <span className="selo-ativa" title="É a mesa que os jogadores veem">◉ Ativa</span>
                : <button type="button" className="btn btn-sm btn-fantasma" onClick={() => executar(() => ativarMesa(mesa.id))}>Tornar ativa</button>}
              <button type="button" className="btn btn-sm btn-fantasma" onClick={() => setEntregando(true)}
                title="Entregue uma carta, um retrato ou um mapa (handout) aos jogadores">Entregar</button>
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
                pincel={pincel} aoPintarNevoa={pintarNevoa} aoMedirGrade={setLimitesGrade}
                pings={sinais.pings} reacoes={sinais.reacoes} arrastosRemotos={sinais.arrastos} aoPing={pingar} aoArrastar={arrastar}>
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
                {escolhendoClima && (
                  <AjusteClima clima={mesa.clima} aoFechar={() => setEscolhendoClima(false)}
                    aoEscolher={(clima) => executar(() => salvarMesa(mesa.id, corpoDaMesa(mesa, { clima })))} />
                )}
                <RolagensNaMesa novas={rolagens.novas} aoDispensar={rolagens.dispensar} />
                {avisoDesfazer && <div className="aviso-desfeito" role="status">{avisoDesfazer}</div>}
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
                  <button type="button" className="btn btn-sm btn-fantasma px-2" disabled={!podeDesfazer} onClick={desfazer}
                    title="Desfazer (Ctrl+Z)" aria-label="Desfazer">↶</button>
                  {mesa.emCombate ? (
                    <>
                      <button type="button" className="btn btn-sm btn-primary" disabled={ocupado}
                        onClick={() => executar(() => proximoTurno(mesa.id))} title="Atalho: N">Próximo turno →</button>
                      <button type="button" className="btn btn-sm btn-fantasma" disabled={ocupado}
                        onClick={() => executar(() => encerrarCombate(mesa.id))}>Encerrar</button>
                    </>
                  ) : (
                    <>
                    <button type="button" className="btn btn-sm btn-fantasma" disabled={!mesa.ativa || pediuIniciativa}
                      onClick={pedirIniciativa} title={mesa.ativa ? 'Os jogadores rolam a iniciativa na tela deles' : 'Só na mesa ativa'}>
                      {pediuIniciativa ? 'Pedido!' : 'Pedir iniciativa'}
                    </button>
                    <button type="button" className="btn btn-sm btn-primary" disabled={ocupado || mesa.participantes.length === 0}
                      onClick={() => executar(() => iniciarCombate(mesa.id, iniciativasParaComecar(mesa.participantes)))}
                      title="Monstros e NPCs rolam d20 + bônus; digite a iniciativa dos PJs">
                      ⚔ Iniciar combate
                    </button>
                    </>
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
                <p className="rotulo-painel">Rolagens da mesa</p>
                <PainelRolagens key={mesa.id} comSecreta rolagens={rolagens.itens}
                  enviar={enviarRolagemDaMesa} aoEnviada={rolagens.adicionar} />

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

      {entregando && (
        <PainelEntregas idCampanha={id} registros={registros} imagens={imagens} urlImagem={urlDaImagem} aoFechar={fecharEntregas} />
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
