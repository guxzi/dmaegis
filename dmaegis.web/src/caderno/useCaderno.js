import { useEffect, useState } from 'react';
import { obterCampanha } from '../api/campanhas.js';
import { listarTipos } from '../api/caderno.js';
import { mensagemDeErro } from '../api/erros.js';
import { rotuloTipo } from './tipos.js';

// Carrega o que toda página do caderno precisa: a campanha (para saber o sistema) e os tipos.
// `rotulo(idTipo)` devolve o nome do tipo como aparece na tela ("Adversário" no Daggerheart).
export function useCaderno(idCampanha) {
  const [estado, setEstado] = useState({ campanha: null, tipos: [], erro: '', naoEncontrada: false });

  useEffect(() => {
    let ativo = true;
    Promise.all([obterCampanha(idCampanha), listarTipos()])
      .then(([campanha, tipos]) => ativo && setEstado({ campanha, tipos, erro: '', naoEncontrada: false }))
      .catch((e) => ativo && setEstado((s) => ({
        ...s,
        naoEncontrada: e.response?.status === 404,
        erro: e.response?.status === 404 ? '' : mensagemDeErro(e),
      })));
    return () => {
      ativo = false;
    };
  }, [idCampanha]);

  const rotulo = (idTipo) => {
    const tipo = estado.tipos.find((t) => t.id === idTipo);
    return tipo && estado.campanha ? rotuloTipo(tipo, estado.campanha.idSistema) : '';
  };

  return { ...estado, rotulo };
}
