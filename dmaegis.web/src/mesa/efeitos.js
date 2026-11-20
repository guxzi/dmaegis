// Funções puras dos efeitos da mesa: o que mudou entre dois estados (dano, cura, turno, combate, névoa)
// e o crítico do d20. Os componentes só desenham o que estas funções devolvem.

const ORDEM_SAUDE = { Ileso: 0, Ferido: 1, Sangrando: 2, 'Caído': 3 };

const temPv = (p) => Number.isFinite(p?.pvAtual) && Number.isFinite(p?.pvMaximo) && p.pvMaximo > 0;

// Dano e cura de cada ficha entre dois estados da mesa. Com PV (mestre, ou PJ na tela do jogador) vem o número;
// sem PV (monstro na tela do jogador) só a direção, pela saúde em palavras. Mesa trocada = nada mudou.
export function mudancasDeSaude(antes, depois) {
  if (!antes || !depois || (antes.id ?? antes.idMesa) !== (depois.id ?? depois.idMesa)) return [];
  const anteriores = new Map(antes.participantes.map((p) => [p.id, p]));
  const mudancas = [];
  for (const atual of depois.participantes) {
    const velho = anteriores.get(atual.id);
    if (!velho) continue;
    if (temPv(velho) && temPv(atual)) {
      const diferenca = atual.pvAtual - velho.pvAtual;
      if (diferenca === 0) continue;
      mudancas.push({
        idFicha: atual.id,
        tipo: diferenca < 0 ? 'dano' : 'cura',
        valor: Math.abs(diferenca),
        caiu: velho.pvAtual > 0 && atual.pvAtual <= 0,
      });
      continue;
    }
    const a = ORDEM_SAUDE[velho.estadoSaude];
    const b = ORDEM_SAUDE[atual.estadoSaude];
    if (a === undefined || b === undefined || a === b) continue;
    mudancas.push({ idFicha: atual.id, tipo: b > a ? 'dano' : 'cura', valor: null, caiu: atual.estadoSaude === 'Caído' });
  }
  return mudancas;
}

// O combate acabou de começar (na mesma mesa).
export function combateComecou(antes, depois) {
  return Boolean(antes && depois && (antes.id ?? antes.idMesa) === (depois.id ?? depois.idMesa) && !antes.emCombate && depois.emCombate);
}

// A vez passou para outra ficha (ou começou uma rodada nova com a mesma). Devolve a ficha da vez, ou null.
export function novaVez(antes, depois) {
  if (!depois?.emCombate || depois.idParticipanteVez == null) return null;
  const mesmaMesa = antes && (antes.id ?? antes.idMesa) === (depois.id ?? depois.idMesa);
  if (mesmaMesa && antes.emCombate && antes.idParticipanteVez === depois.idParticipanteVez && antes.rodada === depois.rodada) return null;
  if (!mesmaMesa) return null;
  return depois.participantes.find((p) => p.id === depois.idParticipanteVez) ?? null;
}

// Células que passaram a estar reveladas (chaves "coluna,linha"). Com muitas, fica uma amostra espalhada.
export function celulasRecemReveladas(antes, depois, limite = 120) {
  if (!antes || !depois) return [];
  const novas = [];
  for (const chave of depois) if (!antes.has(chave)) novas.push(chave);
  if (novas.length <= limite) return novas;
  const passo = novas.length / limite;
  return Array.from({ length: limite }, (_, i) => novas[Math.floor(i * passo)]);
}

// Crítico e falha crítica: só quando a rolagem tem um único d20 que conta (1d20, 2d20kh1, 2d20kl1, com ou sem bônus).
// O detalhe vem de formatarRolagem: "1d20+5 → (20) + 5 = 25"; dados entre colchetes foram descartados.
export function criticoDaRolagem(expressao, detalhe) {
  const termos = String(expressao ?? '').toLowerCase().match(/\d*d\d+(k[hl]\d+)?/g) ?? [];
  if (termos.length !== 1) return null;
  const [, quantidade = '1', faces, manter] = termos[0].match(/^(\d*)d(\d+)(k[hl]\d+)?$/) ?? [];
  if (faces !== '20') return null;
  const contam = manter ? Number(manter.slice(2)) : Number(quantidade || 1);
  if (contam !== 1) return null;
  const grupo = String(detalhe ?? '').match(/\(([^)]*)\)/);
  if (!grupo) return null;
  const mantido = grupo[1].split(',').map((v) => v.trim()).find((v) => !v.startsWith('['));
  if (mantido === '20') return 'critico';
  if (mantido === '1') return 'falha';
  return null;
}

// Chama `funcao` no máximo uma vez a cada `intervalo` ms, sempre com os argumentos mais recentes
// (o último valor nunca se perde). `cancelar()` descarta o que estiver pendente.
export function limitarTaxa(funcao, intervalo, relogio = () => Date.now(), agendar = setTimeout, desagendar = clearTimeout) {
  let ultima = -Infinity;
  let pendente = null;
  let timer = null;
  function disparar() {
    timer = null;
    ultima = relogio();
    const argumentos = pendente;
    pendente = null;
    funcao(...argumentos);
  }
  return {
    chamar(...argumentos) {
      pendente = argumentos;
      const espera = intervalo - (relogio() - ultima);
      if (espera <= 0) {
        if (timer) desagendar(timer);
        disparar();
      } else if (!timer) {
        timer = agendar(disparar, espera);
      }
    },
    cancelar() {
      if (timer) desagendar(timer);
      timer = null;
      pendente = null;
    },
  };
}

// Um número de 0 a 1 que depende só do texto (para espalhar faíscas sem sortear durante o desenho).
export function sementeDe(texto) {
  let h = 2166136261;
  for (let i = 0; i < texto.length; i++) {
    h ^= texto.charCodeAt(i);
    h = Math.imul(h, 16777619);
  }
  return ((h >>> 0) % 10000) / 10000;
}

// Climas que o mestre pode ligar por cima do mapa (os mesmos que o servidor aceita).
export const CLIMAS = [
  { id: null, rotulo: 'Nenhum' },
  { id: 'chuva', rotulo: 'Chuva' },
  { id: 'neve', rotulo: 'Neve' },
  { id: 'neblina', rotulo: 'Neblina' },
  { id: 'brasas', rotulo: 'Brasas' },
];

// Reações rápidas dos jogadores (as mesmas que o servidor aceita).
export const REACOES = ['😱', '🔥', '⭐', '💀', '😂', '👏', '❤️', '🎲'];
