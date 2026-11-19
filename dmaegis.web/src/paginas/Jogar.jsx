import { useCallback, useEffect, useState } from 'react';
import { mensagemDeErro } from '../api/erros.js';
import {
  moverMinhaFicha, obterJogador, obterMesaDoJogador, renovarCsrfJogador, URL_EVENTOS_JOGADOR, urlImagemDoJogador,
} from '../api/jogador.js';
import Emblema from '../componentes/Emblema.jsx';
import { CLASSE_SAUDE, estadoSaude, fracaoPv } from '../mesa/combate.js';
import { ouvirMesa } from '../mesa/eventos.js';
import VisaoJogador from '../mesa/VisaoJogador.jsx';

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

// O cartão do próprio personagem, no canto: PV, CA e condições.
function MinhaFicha({ ficha }) {
  const fracao = fracaoPv(ficha.pvAtual, ficha.pvMaximo);
  const saude = estadoSaude(ficha.pvAtual, ficha.pvMaximo);
  return (
    <aside className="minha-ficha" onPointerDown={(e) => e.stopPropagation()} aria-label="Minha ficha">
      <span className="minha-ficha-retrato">
        {ficha.idImagemRetrato ? <img src={urlImagemDoJogador(ficha.idImagemRetrato)} alt="" /> : <span>{ficha.nome.charAt(0)}</span>}
      </span>
      <div className="minha-ficha-corpo">
        <strong>{ficha.nome}</strong>
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

  const carregar = useCallback(async () => {
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
    return ouvirMesa(URL_EVENTOS_JOGADOR, carregar, setConectado);
  }, [jogador, carregar]);

  useEffect(() => {
    if (!aviso) return undefined;
    const t = setTimeout(() => setAviso(''), 3000);
    return () => clearTimeout(t);
  }, [aviso]);

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
        podeArrastar={(ficha) => ficha.ehMinha && visao.movimentoLivre} aoMover={mover} />
      <div className="marca-jogador" aria-hidden="true">
        <Emblema tamanho={26} />
        <span>{jogador.nomeCampanha}</span>
      </div>
      {minha && <MinhaFicha ficha={minha} />}
      {!conectado && <div className="aviso-conexao" role="status">Reconectando à mesa…</div>}
      {aviso && <div className="aviso-conexao aviso-erro" role="status">{aviso}</div>}
    </main>
  );
}
