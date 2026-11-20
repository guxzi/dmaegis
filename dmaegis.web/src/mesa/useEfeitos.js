import { useCallback, useEffect, useRef, useState } from 'react';

let proximaChave = 1;

// Efeitos passageiros derivados de um valor que muda (a mesa, as células reveladas):
// guarda o valor anterior e, quando ele muda, `comparar(antes, depois)` diz que efeitos nascem.
// A comparação roda durante o desenho (o jeito do React de "ajustar o estado quando a prop muda"),
// e os efeitos se apagam sozinhos depois de `duracao` ms.
export function useEfeitosPassageiros(valor, comparar, duracao) {
  const [anterior, setAnterior] = useState(valor);
  const [efeitos, setEfeitos] = useState([]);
  if (valor !== anterior) {
    setAnterior(valor);
    const novos = comparar(anterior, valor);
    if (novos.length) setEfeitos((lista) => [...lista, ...novos.map((e) => ({ ...e, chave: proximaChave++ }))]);
  }
  useEffect(() => {
    if (!efeitos.length) return undefined;
    const ultima = efeitos[efeitos.length - 1].chave;
    const t = setTimeout(() => setEfeitos((lista) => lista.filter((e) => e.chave > ultima)), duracao);
    return () => clearTimeout(t);
  }, [efeitos, duracao]);
  return efeitos;
}

const DURACAO_PING = 2400;
const DURACAO_REACAO = 2600;
// Sem notícia do arrasto por esse tempo, a ficha volta para a posição salva.
const VALIDADE_ARRASTO = 2500;
// Depois de soltar, a ficha fica na célula de destino até a mesa nova chegar.
const ESPERA_DEPOIS_DE_SOLTAR = 1500;

// Sinais passageiros da mesa (ping, reação, arrasto ao vivo) de uma mesa só.
// `receber(sinal)` serve tanto para os que chegam do servidor quanto para os da própria tela.
export function useSinaisMesa(idMesa) {
  const [pings, setPings] = useState([]);
  const [reacoes, setReacoes] = useState([]);
  const [arrastos, setArrastos] = useState({});
  const timers = useRef(new Map());

  useEffect(() => () => {
    for (const t of timers.current.values()) clearTimeout(t);
    timers.current.clear();
  }, []);

  const tirarArrasto = useCallback((id) => {
    setArrastos((atuais) => {
      if (!(id in atuais)) return atuais;
      const { [id]: _removido, ...resto } = atuais;
      return resto;
    });
  }, []);

  const receber = useCallback((sinal) => {
    if (!sinal || (sinal.idMesa != null && sinal.idMesa !== idMesa)) return;
    const chave = proximaChave++;
    if (sinal.tipo === 'ping') {
      setPings((lista) => [...lista.slice(-6), { ...sinal, chave }]);
      setTimeout(() => setPings((lista) => lista.filter((p) => p.chave !== chave)), DURACAO_PING);
    } else if (sinal.tipo === 'reacao') {
      setReacoes((lista) => [...lista.slice(-12), { ...sinal, chave }]);
      setTimeout(() => setReacoes((lista) => lista.filter((r) => r.chave !== chave)), DURACAO_REACAO);
    } else if (sinal.tipo === 'arrasto' || sinal.tipo === 'arrasto-fim') {
      const id = sinal.idParticipante;
      if (id == null) return;
      clearTimeout(timers.current.get(id));
      const fim = sinal.tipo === 'arrasto-fim';
      if (fim && sinal.coluna == null) {
        tirarArrasto(id);
        return;
      }
      setArrastos((atuais) => ({ ...atuais, [id]: { coluna: sinal.coluna, linha: sinal.linha, fim } }));
      timers.current.set(id, setTimeout(() => tirarArrasto(id), fim ? ESPERA_DEPOIS_DE_SOLTAR : VALIDADE_ARRASTO));
    }
  }, [idMesa, tirarArrasto]);

  return { pings, reacoes, arrastos, receber };
}
