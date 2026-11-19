import { CLASSE_SAUDE } from './combate.js';
import PalcoMapa from './PalcoMapa.jsx';

// O que os jogadores veem: o mapa com as fichas visíveis e, em combate, a ordem dos turnos no topo.
// Recebe o estado já filtrado pelo servidor (sem fichas ocultas nem PV de monstros).
export default function VisaoJogador({ visao, urlImagem, podeArrastar, aoMover }) {
  const mesa = { ...visao, id: visao.idMesa };
  const naOrdem = visao.participantes;
  const daVez = naOrdem.find((p) => p.id === visao.idParticipanteVez);

  return (
    <div className="visao-jogador">
      <PalcoMapa mesa={mesa} urlImagem={urlImagem} modo="jogador" podeArrastar={podeArrastar ?? (() => false)} aoMover={aoMover}>
        {visao.emCombate && naOrdem.length > 0 && (
          <div className="faixa-turnos" onPointerDown={(e) => e.stopPropagation()}>
            <span className="faixa-turnos-rodada">Rodada {visao.rodada}</span>
            <ol>
              {naOrdem.map((p) => (
                <li key={p.id} className={[p.id === visao.idParticipanteVez && 'da-vez', p.estadoSaude === 'Caído' && 'caida', p.ehMinha && 'minha'].filter(Boolean).join(' ')}
                  style={{ '--cor-ficha': p.jogador ? 'var(--dm-cobalto-claro)' : 'var(--dm-estrela)' }}>
                  <span className="turno-retrato">
                    {p.idImagemRetrato ? <img src={urlImagem(p.idImagemRetrato)} alt="" /> : <span>{p.nome.charAt(0)}</span>}
                  </span>
                  <span className="turno-nome">{p.nome}</span>
                  {!p.jogador && p.estadoSaude && p.estadoSaude !== 'Ileso' && (
                    <span className={`turno-saude saude-${CLASSE_SAUDE[p.estadoSaude]}`}>{p.estadoSaude}</span>
                  )}
                </li>
              ))}
            </ol>
          </div>
        )}
        {visao.emCombate && daVez && (
          <div className="anuncio-vez" key={`${visao.rodada}-${daVez.id}`} aria-live="polite">
            {daVez.ehMinha ? 'É a sua vez!' : <>Vez de <strong>{daVez.nome}</strong></>}
          </div>
        )}
      </PalcoMapa>
    </div>
  );
}
