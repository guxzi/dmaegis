import { useCallback, useEffect, useRef, useState } from 'react';
import { urlDaImagem } from '../api/imagens.js';
import { listarRolagens, obterVisaoJogador } from '../api/mesas.js';
import Emblema from '../componentes/Emblema.jsx';
import RolagensNaMesa from '../mesa/RolagensNaMesa.jsx';
import { useSinaisMesa } from '../mesa/useEfeitos.js';
import { useRolagens } from '../mesa/useRolagens.js';
import VisaoJogador from '../mesa/VisaoJogador.jsx';
import { lerUltimaProjecao, ouvirProjecao } from '../projecao/canal.js';

// A mesa na projeção: busca a visão filtrada e busca de novo a cada aviso de mudança do mestre.
function ProjecaoMesa({ idMesa, versao, sinais }) {
  const [visao, setVisao] = useState(null);
  const [erro, setErro] = useState(false);
  // Na projeção, só as rolagens abertas (as secretas são do mestre).
  const listar = useCallback(() => listarRolagens(idMesa).then((lista) => lista.filter((r) => !r.secreta)), [idMesa]);
  const rolagens = useRolagens(listar, versao);

  useEffect(() => {
    let ativo = true;
    obterVisaoJogador(idMesa)
      .then((v) => { if (ativo) { setVisao(v); setErro(false); } })
      .catch(() => ativo && setErro(true));
    return () => {
      ativo = false;
    };
  }, [idMesa, versao]);

  if (erro && !visao) return <p className="projecao-dica">Não deu para abrir a mesa. Volte à página Mesa e clique em "Mostrar aos jogadores".</p>;
  if (!visao) return null;
  return (
    <>
      <VisaoJogador visao={visao} urlImagem={urlDaImagem} sinais={sinais} />
      <RolagensNaMesa novas={rolagens.novas} aoDispensar={rolagens.dispensar} />
    </>
  );
}

// Janela para os jogadores: sem cabeçalho, só a imagem ou a mesa que o mestre mandou (ou a tela de espera).
// Duplo clique liga e desliga a tela cheia.
export default function Projecao() {
  const [projecao, setProjecao] = useState(() => lerUltimaProjecao());
  const [versao, setVersao] = useState(0);
  const sinais = useSinaisMesa(projecao?.idMesa ?? null);
  const receberSinal = useRef(sinais.receber);
  useEffect(() => {
    receberSinal.current = sinais.receber;
  }, [sinais.receber]);

  useEffect(() => {
    document.title = 'DMAegis — Projeção';
    return ouvirProjecao(setProjecao, () => setVersao((v) => v + 1), (sinal) => receberSinal.current(sinal));
  }, []);

  function alternarTelaCheia(evento) {
    if (evento.target.closest?.('.palco-mapa')) return;
    if (document.fullscreenElement) document.exitFullscreen?.();
    else document.documentElement.requestFullscreen?.().catch(() => {});
  }

  return (
    <main className={`projecao${projecao?.idMesa ? ' projecao-com-mesa' : ''}`} onDoubleClick={alternarTelaCheia}>
      {projecao?.idMesa && <ProjecaoMesa idMesa={projecao.idMesa} versao={versao} sinais={sinais} />}
      {projecao && !projecao.idMesa && (
        <figure className="projecao-figura surgir" key={projecao.id}>
          <img src={urlDaImagem(projecao.id)} alt={projecao.titulo || 'Imagem mostrada pelo mestre'} />
          {projecao.titulo && <figcaption className="titulo-estelar">{projecao.titulo}</figcaption>}
        </figure>
      )}
      {!projecao && (
        <div className="projecao-espera surgir">
          <Emblema tamanho={140} className="projecao-emblema" />
          <p className="titulo-estelar h2 mb-1">DMAegis</p>
          <p className="text-secondary mb-0">Aguardando o mestre…</p>
        </div>
      )}
      {!projecao?.idMesa && <p className="projecao-dica small">Duplo clique: tela cheia</p>}
    </main>
  );
}
