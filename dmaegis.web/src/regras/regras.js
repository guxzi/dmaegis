import { REGRAS_DAGGERHEART } from './daggerheart.js';
import { REGRAS_DND } from './dnd.js';

const POR_SISTEMA = { 1: REGRAS_DND, 2: REGRAS_DAGGERHEART };

// Só as regras do sistema da mesa: numa campanha de D&D não aparece nada de Daggerheart, e vice-versa.
export function regrasDoSistema(idSistema) {
  return POR_SISTEMA[idSistema] ?? { secoes: [], rodape: '' };
}
