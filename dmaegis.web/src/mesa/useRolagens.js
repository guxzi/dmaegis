import { useCallback, useEffect, useRef, useState } from 'react';

const maiorId = (lista) => lista.reduce((maior, r) => Math.max(maior, r.id), 0);

// Uma lista que chega do servidor (rolagens, entregas) e os itens que acabaram de chegar.
// A primeira busca só preenche a lista: item antigo não aparece como novo ao abrir a tela.
// `versao` muda a cada aviso da mesa, e aí a lista é buscada de novo.
export function useListaComNovas(listar, versao) {
  const [itens, setItens] = useState([]);
  const [novas, setNovas] = useState([]);
  const vistas = useRef(null);
  const fonte = useRef(listar);

  const anunciar = useCallback((chegaram) => {
    if (!chegaram.length) return;
    // Da mais velha para a mais nova; no máximo três cartões ao mesmo tempo.
    setNovas((atuais) => [...atuais, ...chegaram].slice(-3));
  }, []);

  useEffect(() => {
    let ativo = true;
    if (fonte.current !== listar) {
      fonte.current = listar;
      vistas.current = null;
    }
    listar().then((lista) => {
      if (!ativo) return;
      if (vistas.current === null) {
        vistas.current = maiorId(lista);
      } else {
        const chegaram = lista.filter((r) => r.id > vistas.current).reverse();
        vistas.current = Math.max(vistas.current, maiorId(lista));
        anunciar(chegaram);
      }
      setItens(lista);
    }).catch(() => {});
    return () => {
      ativo = false;
    };
  }, [listar, versao, anunciar]);

  // A própria rolagem entra na hora, sem esperar o aviso da mesa.
  const adicionar = useCallback((nova) => {
    setItens((lista) => [nova, ...lista.filter((r) => r.id !== nova.id)].slice(0, 30));
    if (vistas.current === null || nova.id > vistas.current) {
      vistas.current = Math.max(vistas.current ?? 0, nova.id);
      anunciar([nova]);
    }
  }, [anunciar]);

  const dispensar = useCallback((id) => setNovas((atuais) => atuais.filter((r) => r.id !== id)), []);

  return { itens, novas, adicionar, dispensar };
}

// As rolagens da mesa e as que acabaram de chegar (para o dado animado em cima do mapa).
export const useRolagens = useListaComNovas;
