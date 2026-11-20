import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { urlEventosDaMesa } from '../api/jogador.js';
import { enviarSinal, obterMesa } from '../api/mesas.js';
import { avisarSinalNaProjecao } from '../projecao/canal.js';
import { limitarTaxa } from './efeitos.js';
import { ID_TELA, ouvirMesa } from './eventos.js';
import { useSinaisMesa } from './useEfeitos.js';

// O tempo real da mesa do mestre: ouve o fluxo da campanha (busca a mesa de novo a cada aviso),
// recebe os sinais dos jogadores e manda os do mestre (ping, arrasto ao vivo, pedir iniciativa).
// `aoAtualizar(mesa)` recebe a mesa buscada de novo. `versao` muda a cada aviso (as rolagens usam).
export function useTempoRealMesa(idCampanha, idMesa, mesa, aoAtualizar) {
  const [versao, setVersao] = useState(0);
  const [pediuIniciativa, setPediuIniciativa] = useState(false);
  const sinais = useSinaisMesa(idMesa);
  const receberSinal = useRef(sinais.receber);
  const atualizar = useRef(aoAtualizar);
  useEffect(() => {
    receberSinal.current = sinais.receber;
    atualizar.current = aoAtualizar;
  }, [sinais.receber, aoAtualizar]);

  // Quando um jogador move a ficha (ou outra aba mexe na mesa), busca a mesa de novo.
  useEffect(() => {
    if (!idMesa) return undefined;
    return ouvirMesa(urlEventosDaMesa(idCampanha), () => {
      setVersao((v) => v + 1);
      obterMesa(idMesa).then((m) => atualizar.current(m)).catch(() => {});
    }, undefined, (sinal) => {
      receberSinal.current(sinal);
      // O que os jogadores podem ver também vai para a janela de projeção.
      if (sinal.publico) avisarSinalNaProjecao(sinal);
    });
  }, [idCampanha, idMesa]);

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

  return { sinais, versao, pingar, arrastar, pedirIniciativa, pediuIniciativa };
}
