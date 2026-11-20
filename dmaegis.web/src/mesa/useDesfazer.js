import { useCallback, useEffect, useRef, useState } from 'react';

const LIMITE = 30;

// Desfazer da mesa: as últimas ações reversíveis (mover, mudar a ficha, pintar a névoa), da mais nova para a mais velha.
// O histórico é de uma mesa só: ao trocar de mesa, ele fica vazio. `registrar(descricao, desfazer)` guarda a ação;
// `desfazer()` roda a mais nova e mostra "Desfeito: …" por um instante.
export function useDesfazer(idMesa) {
  const historico = useRef({ idMesa: null, itens: [] });
  const [tamanho, setTamanho] = useState({ idMesa: null, n: 0 });
  const [aviso, setAviso] = useState('');

  const registrar = useCallback((descricao, desfazer) => {
    if (historico.current.idMesa !== idMesa) historico.current = { idMesa, itens: [] };
    const { itens } = historico.current;
    itens.push({ descricao, desfazer });
    if (itens.length > LIMITE) itens.shift();
    setTamanho({ idMesa, n: itens.length });
  }, [idMesa]);

  const desfazer = useCallback(async () => {
    if (historico.current.idMesa !== idMesa) return;
    const item = historico.current.itens.pop();
    setTamanho({ idMesa, n: historico.current.itens.length });
    if (!item) return;
    await item.desfazer();
    setAviso(`Desfeito: ${item.descricao}`);
  }, [idMesa]);

  useEffect(() => {
    if (!aviso) return undefined;
    const t = setTimeout(() => setAviso(''), 2500);
    return () => clearTimeout(t);
  }, [aviso]);

  return { registrar, desfazer, podeDesfazer: tamanho.idMesa === idMesa && tamanho.n > 0, aviso };
}
