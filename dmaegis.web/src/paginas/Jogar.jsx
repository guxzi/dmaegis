import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { mensagemDeErro } from '../api/erros.js';
import {
  enviarRetrato, enviarRolagemDoJogador, enviarSinalDoJogador, listarEntregasDoJogador, listarRolagensDoJogador, moverMinhaFicha,
  obterJogador, obterMesaDoJogador, renovarCsrfJogador, rolarIniciativaDoJogador, URL_EVENTOS_JOGADOR, urlImagemDoJogador,
} from '../api/jogador.js';
import Emblema from '../componentes/Emblema.jsx';
import { formatarRolagem, rolar } from '../dados/rolar.js';
import { ListaEntregas, PedidoIniciativa, RevelarEntrega } from '../mesa/EntregasJogador.jsx';
import { reduzirRetrato } from '../mesa/retrato.js';
import { CLASSE_SAUDE, estadoSaude, fracaoPv } from '../mesa/combate.js';
import { limitarTaxa, REACOES } from '../mesa/efeitos.js';
import { ID_TELA, ouvirMesa } from '../mesa/eventos.js';
import PainelRolagens from '../mesa/PainelRolagens.jsx';
import RolagensNaMesa from '../mesa/RolagensNaMesa.jsx';
import { useSinaisMesa } from '../mesa/useEfeitos.js';
import { useListaComNovas, useRolagens } from '../mesa/useRolagens.js';
import VisaoJogador from '../mesa/VisaoJogador.jsx';

// Uma reação por vez a cada esse tempo, para ninguém encher a tela dos outros.
const INTERVALO_REACOES = 700;

function Espera({ titulo, texto }) {
  return (
    <main className="tela-jogador-espera">
      <div className="cartao-astral tela-jogador-cartao text-center">
        <Emblema tamanho={96} className="projecao-emblema mb-3" />
        <h1 className="h4 titulo-estelar">{titulo}</h1>
        <p className="text-secondary mb-0">{texto}</p>
      </div>
    </main>
  );
}

// O cartão do próprio personagem, no canto: retrato (clique para trocar), PV, CA, iniciativa e condições.
function MinhaFicha({ ficha, enviandoRetrato, aoEscolherRetrato }) {
  const fracao = fracaoPv(ficha.pvAtual, ficha.pvMaximo);
  const saude = estadoSaude(ficha.pvAtual, ficha.pvMaximo);
  return (
    <aside className="minha-ficha" onPointerDown={(e) => e.stopPropagation()} aria-label="Minha ficha">
      <label className={`minha-ficha-retrato trocar-retrato${enviandoRetrato ? ' enviando' : ''}`}
        title={ficha.idImagemRetrato ? 'Trocar o retrato' : 'Enviar o retrato do seu personagem'}>
        {ficha.idImagemRetrato ? <img src={urlImagemDoJogador(ficha.idImagemRetrato)} alt="" /> : <span>{ficha.nome.charAt(0)}</span>}
        <span className="trocar-retrato-dica" aria-hidden="true">{enviandoRetrato ? '…' : '📷'}</span>
        <input type="file" accept="image/png,image/jpeg,image/webp,image/gif" className="visually-hidden"
          onChange={(e) => { const arquivo = e.target.files?.[0]; e.target.value = ''; if (arquivo) aoEscolherRetrato(arquivo); }}
          aria-label="Escolher o retrato" disabled={enviandoRetrato} />
      </label>
      <div className="minha-ficha-corpo">
        <strong>{ficha.nome}</strong>
        {ficha.iniciativa != null && <span className="minha-ficha-numeros">Iniciativa {ficha.iniciativa}</span>}
        {fracao !== null && (
          <>
            <span className={`barra-pv saude-${CLASSE_SAUDE[saude]}`}><span style={{ width: `${fracao * 100}%` }} /></span>
            <span className="minha-ficha-numeros">PV {ficha.pvAtual}/{ficha.pvMaximo}{ficha.ca != null && ` · CA ${ficha.ca}`}</span>
          </>
        )}
        {ficha.condicoes.length > 0 && (
          <span className="linha-condicoes">
            {ficha.condicoes.map((c) => <span key={c} className="selo-condicao">{c}</span>)}
          </span>
        )}
      </div>
    </aside>
  );
}

// /jogar — a tela do jogador: a mesa ativa em tempo real e a própria ficha, que ele pode arrastar.
export default function Jogar() {
  const [jogador, setJogador] = useState(null);
  const [visao, setVisao] = useState(undefined);
  const [erro, setErro] = useState('');
  const [conectado, setConectado] = useState(true);
  const [aviso, setAviso] = useState('');
  const [versao, setVersao] = useState(0);
  const [dadosAbertos, setDadosAbertos] = useState(false);
  const [reacoesAbertas, setReacoesAbertas] = useState(false);
  const [entregasAbertas, setEntregasAbertas] = useState(false);
  const [entregaAberta, setEntregaAberta] = useState(null);
  const [pedidoIniciativa, setPedidoIniciativa] = useState(false);
  const [erroIniciativa, setErroIniciativa] = useState('');
  const [enviandoRetrato, setEnviandoRetrato] = useState(false);
  const ultimaReacao = useRef(0);
  const rolagens = useRolagens(listarRolagensDoJogador, versao);
  const entregas = useListaComNovas(listarEntregasDoJogador, versao);
  // A entrega nova mais antiga ainda não vista abre sozinha; as outras esperam a vez.
  const entregaNova = entregas.novas[0] ?? null;
  const idMesa = visao?.idMesa ?? null;
  const sinais = useSinaisMesa(idMesa);
  const receberSinal = useRef(sinais.receber);
  useEffect(() => {
    // O pedido de iniciativa abre o cartão; o resto vai para os efeitos do palco.
    receberSinal.current = (sinal) => {
      if (sinal.tipo === 'pedir-iniciativa') {
        if (sinal.idMesa == null || sinal.idMesa === idMesa) {
          setErroIniciativa('');
          setPedidoIniciativa(true);
        }
        return;
      }
      sinais.receber(sinal);
    };
  }, [sinais, idMesa]);

  const carregar = useCallback(async () => {
    setVersao((v) => v + 1);
    try {
      setVisao(await obterMesaDoJogador());
      setErro('');
    } catch (e) {
      setErro(mensagemDeErro(e));
    }
  }, []);

  useEffect(() => {
    document.title = 'DMAegis — Mesa';
    (async () => {
      try {
        setJogador(await obterJogador());
        await renovarCsrfJogador();
        await carregar();
      } catch (e) {
        setErro(mensagemDeErro(e));
      }
    })();
  }, [carregar]);

  useEffect(() => {
    if (!jogador) return undefined;
    return ouvirMesa(URL_EVENTOS_JOGADOR, carregar, setConectado, (sinal) => receberSinal.current(sinal));
  }, [jogador, carregar]);

  useEffect(() => {
    if (!aviso) return undefined;
    const t = setTimeout(() => setAviso(''), 3000);
    return () => clearTimeout(t);
  }, [aviso]);

  // O vibrar avisa no celular quando chega a vez do jogador.
  const minhaVez = Boolean(visao?.emCombate && visao.participantes.some((p) => p.ehMinha && p.id === visao.idParticipanteVez));
  useEffect(() => {
    if (minhaVez) navigator.vibrate?.([120, 60, 120]);
  }, [minhaVez]);

  const nomeAutor = jogador?.nomePersonagem ?? '';
  const pingar = useCallback(({ coluna, linha }) => {
    if (!idMesa) return;
    const sinal = { tipo: 'ping', coluna, linha, origem: ID_TELA };
    sinais.receber({ ...sinal, autor: nomeAutor, idMesa });
    enviarSinalDoJogador(sinal).catch(() => {});
  }, [idMesa, nomeAutor, sinais]);

  const envioArrasto = useMemo(() => limitarTaxa((sinal) => enviarSinalDoJogador(sinal).catch(() => {}), 90), []);
  useEffect(() => () => envioArrasto.cancelar(), [envioArrasto]);
  const arrastar = useCallback((ficha, posicao, terminou) => {
    const sinal = { tipo: terminou ? 'arrasto-fim' : 'arrasto', coluna: posicao?.coluna ?? null, linha: posicao?.linha ?? null, origem: ID_TELA };
    if (!terminou) {
      envioArrasto.chamar(sinal);
      return;
    }
    envioArrasto.cancelar();
    enviarSinalDoJogador(sinal).catch(() => {});
  }, [envioArrasto]);

  function reagir(emoji) {
    const agora = Date.now();
    if (agora - ultimaReacao.current < INTERVALO_REACOES) return;
    ultimaReacao.current = agora;
    const minhaFicha = visao?.participantes.find((p) => p.ehMinha && p.coluna != null);
    sinais.receber({ tipo: 'reacao', emoji, idParticipante: minhaFicha?.id ?? null, autor: nomeAutor, idMesa, origem: ID_TELA });
    enviarSinalDoJogador({ tipo: 'reacao', emoji, origem: ID_TELA }).catch(() => {});
  }

  // d20 + bônus da ficha, rolado aqui; o resultado vira a iniciativa da ficha e aparece nas rolagens.
  async function rolarMinhaIniciativa() {
    const minhaFicha = visao?.participantes.find((p) => p.ehMinha);
    const bonus = minhaFicha?.bonusIniciativa ?? 0;
    const resultado = rolar(bonus ? `1d20${bonus > 0 ? '+' : ''}${bonus}` : '1d20');
    setErroIniciativa('');
    try {
      const nova = await rolarIniciativaDoJogador({ expressao: resultado.expressao, total: resultado.total, detalhe: formatarRolagem(resultado) });
      rolagens.adicionar(nova);
      setPedidoIniciativa(false);
      carregar();
    } catch (e) {
      setErroIniciativa(mensagemDeErro(e));
    }
  }

  async function trocarRetrato(arquivo) {
    setEnviandoRetrato(true);
    try {
      await enviarRetrato(await reduzirRetrato(arquivo));
      await carregar();
    } catch (e) {
      setAviso(e instanceof Error && !e.response ? 'Não deu para ler essa imagem.' : mensagemDeErro(e));
    } finally {
      setEnviandoRetrato(false);
    }
  }

  async function mover(ficha, coluna, linha) {
    setVisao((v) => v && ({ ...v, participantes: v.participantes.map((p) => (p.id === ficha.id ? { ...p, coluna, linha } : p)) }));
    try {
      await moverMinhaFicha(coluna, linha);
    } catch (e) {
      setAviso(mensagemDeErro(e));
      carregar();
    }
  }

  if (erro && !jogador) return <Espera titulo="Sem acesso à mesa" texto={erro} />;
  if (!jogador || visao === undefined) return <Espera titulo="Abrindo a mesa…" texto="Um instante." />;
  if (erro) return <Espera titulo="Sem acesso à mesa" texto={erro} />;
  if (!visao) {
    return <Espera titulo={`Olá, ${jogador.nomePersonagem}!`} texto={`O mestre de ${jogador.nomeCampanha} ainda não abriu a mesa. Ela aparece aqui sozinha.`} />;
  }

  const minha = visao.participantes.find((p) => p.ehMinha);

  return (
    <main className="tela-jogador">
      <VisaoJogador visao={visao} urlImagem={urlImagemDoJogador}
        podeArrastar={(ficha) => ficha.ehMinha && visao.movimentoLivre} aoMover={mover}
        sinais={sinais} aoPing={pingar} aoArrastar={arrastar} />
      <RolagensNaMesa novas={rolagens.novas} aoDispensar={rolagens.dispensar} />
      <div className="marca-jogador" aria-hidden="true">
        <Emblema tamanho={26} />
        <span>{jogador.nomeCampanha}</span>
      </div>
      {minha && <MinhaFicha ficha={minha} enviandoRetrato={enviandoRetrato} aoEscolherRetrato={trocarRetrato} />}
      <button type="button" className="botao-entregas-jogador" aria-expanded={entregasAbertas}
        onClick={() => { setEntregasAbertas((v) => !v); setDadosAbertos(false); setReacoesAbertas(false); }}
        title="Entregas do mestre (cartas, mapas, retratos)" aria-label="Entregas do mestre">
        <span aria-hidden="true">📜</span>
        {entregas.itens.length > 0 && <span className="contador-entregas">{entregas.itens.length}</span>}
      </button>
      {entregasAbertas && (
        <ListaEntregas entregas={entregas.itens} aoFechar={() => setEntregasAbertas(false)}
          aoAbrir={(e) => { setEntregaAberta(e); setEntregasAbertas(false); }} />
      )}
      {pedidoIniciativa && (
        <PedidoIniciativa bonus={minha?.bonusIniciativa ?? 0} erro={erroIniciativa}
          aoRolar={rolarMinhaIniciativa} aoFechar={() => setPedidoIniciativa(false)} />
      )}
      {(entregaAberta || entregaNova) && (
        <RevelarEntrega entrega={entregaAberta ?? entregaNova} nova={!entregaAberta} urlImagem={urlImagemDoJogador}
          aoFechar={() => (entregaAberta ? setEntregaAberta(null) : entregas.dispensar(entregaNova.id))} />
      )}
      <button type="button" className="botao-reacoes-jogador" aria-expanded={reacoesAbertas}
        onClick={() => { setReacoesAbertas((v) => !v); setDadosAbertos(false); setEntregasAbertas(false); }} title="Reagir (todos veem)" aria-label="Reações">
        <span aria-hidden="true">✨</span>
      </button>
      {reacoesAbertas && (
        <div className="barra-reacoes" role="group" aria-label="Reações" onPointerDown={(e) => e.stopPropagation()}>
          {REACOES.map((emoji) => (
            <button key={emoji} type="button" onClick={() => reagir(emoji)} aria-label={`Reagir com ${emoji}`}>{emoji}</button>
          ))}
        </div>
      )}
      <button type="button" className="botao-dados-jogador" aria-expanded={dadosAbertos}
        onClick={() => { setDadosAbertos((v) => !v); setReacoesAbertas(false); setEntregasAbertas(false); }}
        title="Rolar dados para a mesa" aria-label="Dados">
        <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinejoin="round" aria-hidden="true">
          <path d="M12 2 21 7v10l-9 5-9-5V7z" /><path d="M12 22V12M21 7l-9 5-9-5" />
        </svg>
      </button>
      {dadosAbertos && (
        <div className="janela-dados-jogador cartao-astral" onPointerDown={(e) => e.stopPropagation()}>
          <div className="d-flex align-items-center mb-2">
            <strong className="flex-grow-1">Dados da mesa</strong>
            <button type="button" className="btn-close btn-close-white btn-sm" onClick={() => setDadosAbertos(false)} aria-label="Fechar dados" />
          </div>
          {minha && (
            <button type="button" className="btn btn-sm btn-fantasma w-100 mb-2" onClick={rolarMinhaIniciativa}>
              Rolar iniciativa (d20{minha.bonusIniciativa ? `${minha.bonusIniciativa > 0 ? '+' : ''}${minha.bonusIniciativa}` : ''})
            </button>
          )}
          {erroIniciativa && !pedidoIniciativa && <p className="small text-danger mb-2">{erroIniciativa}</p>}
          <PainelRolagens rolagens={rolagens.itens} enviar={enviarRolagemDoJogador} aoEnviada={rolagens.adicionar} compacto />
        </div>
      )}
      {!conectado && <div className="aviso-conexao" role="status">Reconectando à mesa…</div>}
      {aviso && <div className="aviso-conexao aviso-erro" role="status">{aviso}</div>}
    </main>
  );
}
