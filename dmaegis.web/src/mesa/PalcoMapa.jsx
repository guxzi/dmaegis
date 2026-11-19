import { useCallback, useEffect, useLayoutEffect, useRef, useState } from 'react';
import { COR_TIPO } from '../caderno/tipos.js';
import { CLASSE_SAUDE, estadoSaude, fracaoPv } from './combate.js';
import { celulaDoPonto, enquadrar, pontoDaCelula, zoomNoPonto } from './grade.js';

// Sem mapa, a mesa é um campo estrelado com esta grade (em células).
const COLUNAS_SEM_MAPA = 30;
const LINHAS_SEM_MAPA = 18;
const LIMIAR_ARRASTO = 4;

// Cor do anel: PJ em cobalto, criatura em vermelho suave, o resto na cor do tipo do caderno.
function corDaFicha(ficha) {
  if (ficha.jogador) return 'var(--dm-cobalto-claro)';
  return COR_TIPO[ficha.idTipoRegistro] ?? 'var(--dm-estrela)';
}

function Ficha({ ficha, mesa, urlImagem, posicao, selecionada, daVez, arrastando, modo, aoApertar }) {
  const lado = (ficha.tamanho || 1) * mesa.tamanhoCelula;
  const saude = ficha.estadoSaude ?? estadoSaude(ficha.pvAtual, ficha.pvMaximo);
  const fracao = fracaoPv(ficha.pvAtual, ficha.pvMaximo);
  const mostrarBarra = fracao !== null && (modo === 'mestre' || ficha.jogador);
  const classes = [
    'ficha',
    selecionada && 'ficha-selecionada',
    daVez && 'ficha-vez',
    arrastando && 'ficha-arrastando',
    ficha.visivel === false && 'ficha-oculta',
    saude === 'Caído' && 'ficha-caida',
    ficha.ehMinha && 'ficha-minha',
  ].filter(Boolean).join(' ');

  return (
    <div className={classes} onPointerDown={(e) => aoApertar(e, ficha)}
      style={{ width: lado, height: lado, transform: `translate(${posicao.x}px, ${posicao.y}px)`, '--cor-ficha': corDaFicha(ficha) }}
      title={ficha.nome} role="button" aria-label={ficha.nome} aria-pressed={selecionada}>
      <div className="ficha-disco">
        {ficha.idImagemRetrato
          ? <img src={urlImagem(ficha.idImagemRetrato)} alt="" draggable={false} />
          : <span className="ficha-inicial">{ficha.nome.charAt(0)}</span>}
      </div>
      {daVez && <span className="ficha-orbita" aria-hidden="true" />}
      {ficha.condicoes?.length > 0 && (
        <span className="ficha-condicoes" title={ficha.condicoes.join(', ')}>{ficha.condicoes.length}</span>
      )}
      {modo === 'jogador' && !ficha.jogador && saude && saude !== 'Ileso' && (
        <span className={`ficha-saude saude-${CLASSE_SAUDE[saude]}`} title={saude} />
      )}
      <span className="ficha-nome">{ficha.nome}</span>
      {mostrarBarra && (
        <span className="ficha-pv" aria-hidden="true"><span style={{ width: `${fracao * 100}%` }} /></span>
      )}
    </div>
  );
}

// O mapa com a grade e as fichas: arrastar o fundo move a vista, a roda dá zoom no cursor,
// e as fichas permitidas são arrastadas e encaixadas na grade. Serve ao mestre e aos jogadores.
export default function PalcoMapa({
  mesa, urlImagem, modo = 'mestre', podeArrastar = () => false, aoMover, idSelecionada = null,
  aoSelecionar = () => {}, children,
}) {
  const palcoRef = useRef(null);
  const [vista, setVista] = useState({ x: 0, y: 0, escala: 1 });
  const [imagemCarregada, setImagemCarregada] = useState(null);
  const [arrasto, setArrasto] = useState(null);
  const gesto = useRef(null);
  const enquadrouRef = useRef(null);

  // Só vale o tamanho da imagem que está na mesa agora (ao trocar o mapa, espera a nova carregar).
  const tamanhoImagem = imagemCarregada?.id === mesa.idImagemRegistro ? imagemCarregada : null;
  const largura = tamanhoImagem?.largura ?? COLUNAS_SEM_MAPA * mesa.tamanhoCelula + mesa.deslocamentoX;
  const altura = tamanhoImagem?.altura ?? LINHAS_SEM_MAPA * mesa.tamanhoCelula + mesa.deslocamentoY;
  const temImagem = Boolean(mesa.idImagemRegistro);

  const centralizar = useCallback(() => {
    const palco = palcoRef.current;
    if (!palco) return;
    setVista(enquadrar(largura, altura, palco.clientWidth, palco.clientHeight));
  }, [largura, altura]);

  // Enquadra uma vez por mapa (e quando a imagem termina de carregar).
  useLayoutEffect(() => {
    const chave = `${mesa.idImagemRegistro ?? 'estrelas'}:${tamanhoImagem ? 'pronto' : 'carregando'}`;
    if (enquadrouRef.current === chave) return;
    if (temImagem && !tamanhoImagem) return;
    enquadrouRef.current = chave;
    centralizar();
  }, [mesa.idImagemRegistro, tamanhoImagem, temImagem, centralizar]);

  // Roda do mouse: zoom em direção ao cursor. Ouvinte não passivo para poder impedir a rolagem da página.
  useEffect(() => {
    const palco = palcoRef.current;
    if (!palco) return undefined;
    function aoRolar(evento) {
      evento.preventDefault();
      const caixa = palco.getBoundingClientRect();
      const fator = Math.exp(-evento.deltaY * 0.0015);
      setVista((v) => zoomNoPonto(v, v.escala * fator, { x: evento.clientX - caixa.left, y: evento.clientY - caixa.top }));
    }
    palco.addEventListener('wheel', aoRolar, { passive: false });
    return () => palco.removeEventListener('wheel', aoRolar);
  }, []);

  function pontoNoMapa(evento) {
    const caixa = palcoRef.current.getBoundingClientRect();
    return {
      x: (evento.clientX - caixa.left - vista.x) / vista.escala,
      y: (evento.clientY - caixa.top - vista.y) / vista.escala,
    };
  }

  function apertarFundo(evento) {
    if (evento.button !== 0) return;
    gesto.current = { tipo: 'mover-vista', inicioX: evento.clientX, inicioY: evento.clientY, vista, moveu: false };
    evento.currentTarget.setPointerCapture(evento.pointerId);
  }

  function apertarFicha(evento, ficha) {
    if (evento.button !== 0) return;
    evento.stopPropagation();
    const ponto = pontoNoMapa(evento);
    const canto = pontoDaCelula(ficha.coluna, ficha.linha, mesa);
    gesto.current = {
      tipo: 'ficha', ficha, inicioX: evento.clientX, inicioY: evento.clientY, moveu: false,
      folgaX: ponto.x - canto.x, folgaY: ponto.y - canto.y,
    };
    palcoRef.current.setPointerCapture(evento.pointerId);
  }

  function mover(evento) {
    const g = gesto.current;
    if (!g) return;
    const distancia = Math.hypot(evento.clientX - g.inicioX, evento.clientY - g.inicioY);
    if (distancia > LIMIAR_ARRASTO) g.moveu = true;
    if (g.tipo === 'mover-vista') {
      setVista({ ...g.vista, x: g.vista.x + evento.clientX - g.inicioX, y: g.vista.y + evento.clientY - g.inicioY });
    } else if (g.moveu && podeArrastar(g.ficha)) {
      const ponto = pontoNoMapa(evento);
      setArrasto({ id: g.ficha.id, x: ponto.x - g.folgaX, y: ponto.y - g.folgaY });
    }
  }

  function soltar(evento) {
    const g = gesto.current;
    gesto.current = null;
    if (!g) return;
    if (g.tipo === 'mover-vista') {
      if (!g.moveu) aoSelecionar(null);
      return;
    }
    if (!g.moveu) {
      aoSelecionar(g.ficha.id === idSelecionada && modo === 'mestre' ? null : g.ficha.id);
      setArrasto(null);
      return;
    }
    if (arrasto && podeArrastar(g.ficha)) {
      const lado = (g.ficha.tamanho || 1) * mesa.tamanhoCelula;
      const { coluna, linha } = celulaDoPonto(arrasto.x + lado / 2, arrasto.y + lado / 2, mesa, g.ficha.tamanho || 1);
      if (coluna !== g.ficha.coluna || linha !== g.ficha.linha) aoMover?.(g.ficha, coluna, linha);
    }
    setArrasto(null);
    evento.preventDefault();
  }

  function zoomBotao(fator) {
    const palco = palcoRef.current;
    setVista((v) => zoomNoPonto(v, v.escala * fator, { x: palco.clientWidth / 2, y: palco.clientHeight / 2 }));
  }

  const noMapa = mesa.participantes.filter((p) => p.coluna != null && p.linha != null);
  const fichaArrastada = arrasto && noMapa.find((p) => p.id === arrasto.id);
  const destino = fichaArrastada && (() => {
    const lado = (fichaArrastada.tamanho || 1) * mesa.tamanhoCelula;
    const celula = celulaDoPonto(arrasto.x + lado / 2, arrasto.y + lado / 2, mesa, fichaArrastada.tamanho || 1);
    return { ...pontoDaCelula(celula.coluna, celula.linha, mesa), lado };
  })();

  return (
    <div className={`palco-mapa palco-${modo}${arrasto ? ' arrastando' : ''}`} ref={palcoRef}
      onPointerDown={apertarFundo} onPointerMove={mover} onPointerUp={soltar} onPointerCancel={soltar}>
      <div className="palco-conteudo"
        style={{ width: largura, height: altura, transform: `translate(${vista.x}px, ${vista.y}px) scale(${vista.escala})` }}>
        {temImagem
          ? <img className="palco-imagem" src={urlImagem(mesa.idImagemRegistro)} alt="" draggable={false}
              onLoad={(e) => setImagemCarregada({
                id: mesa.idImagemRegistro, largura: e.currentTarget.naturalWidth, altura: e.currentTarget.naturalHeight,
              })} />
          : <div className="palco-sem-mapa" />}

        {mesa.gradeVisivel && (
          <svg className="palco-grade" width={largura} height={altura} aria-hidden="true">
            <defs>
              <pattern id={`grade-${modo}`} width={mesa.tamanhoCelula} height={mesa.tamanhoCelula} patternUnits="userSpaceOnUse"
                x={mesa.deslocamentoX} y={mesa.deslocamentoY}>
                <path d={`M ${mesa.tamanhoCelula} 0 L 0 0 0 ${mesa.tamanhoCelula}`} fill="none" />
              </pattern>
            </defs>
            <rect width="100%" height="100%" fill={`url(#grade-${modo})`} />
          </svg>
        )}

        {destino && (
          <div className="palco-destino" style={{ width: destino.lado, height: destino.lado, transform: `translate(${destino.x}px, ${destino.y}px)` }} />
        )}

        {noMapa.map((ficha) => {
          const posicao = arrasto?.id === ficha.id ? arrasto : pontoDaCelula(ficha.coluna, ficha.linha, mesa);
          return (
            <Ficha key={ficha.id} ficha={ficha} mesa={mesa} urlImagem={urlImagem} posicao={posicao} modo={modo}
              selecionada={ficha.id === idSelecionada} daVez={mesa.emCombate && ficha.id === mesa.idParticipanteVez}
              arrastando={arrasto?.id === ficha.id} aoApertar={apertarFicha} />
          );
        })}
      </div>

      <div className="palco-vinheta" aria-hidden="true" />

      <div className="palco-zoom" onPointerDown={(e) => e.stopPropagation()}>
        <button type="button" onClick={() => zoomBotao(1 / 1.25)} aria-label="Afastar">−</button>
        <button type="button" onClick={centralizar} title="Enquadrar o mapa">{Math.round(vista.escala * 100)}%</button>
        <button type="button" onClick={() => zoomBotao(1.25)} aria-label="Aproximar">+</button>
      </div>
      {children}
    </div>
  );
}
