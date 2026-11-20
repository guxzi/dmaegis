import { dadoSeguro } from '../dados/rolar.js';

// Oráculo de sim ou não com seis respostas: a resposta pode trazer uma complicação ou um bônus.
// A escala é uma ideia genérica de jogos de improviso; os textos são nossos.
export const RESPOSTAS_ORACULO = [
  { resposta: 'Não, e…', dica: 'Não, e ainda piora: algo complica a situação.' },
  { resposta: 'Não', dica: 'Não. Simples assim.' },
  { resposta: 'Não, mas…', dica: 'Não, mas existe um consolo ou uma saída.' },
  { resposta: 'Sim, mas…', dica: 'Sim, mas com um custo ou um porém.' },
  { resposta: 'Sim', dica: 'Sim. Exatamente o que se esperava.' },
  { resposta: 'Sim, e…', dica: 'Sim, e ainda melhor: vem um bônus inesperado.' },
];

export const CHANCES = {
  improvavel: { rotulo: 'Improvável', modificador: -1 },
  meio: { rotulo: 'Meio a meio', modificador: 0 },
  provavel: { rotulo: 'Provável', modificador: 1 },
};

// Rola 1d6, soma o modificador da chance e limita entre 1 e 6.
export function consultarOraculo(chance = 'meio', sortear = dadoSeguro) {
  const dado = sortear(6);
  const valor = Math.min(6, Math.max(1, dado + (CHANCES[chance]?.modificador ?? 0)));
  return { dado, valor, ...RESPOSTAS_ORACULO[valor - 1] };
}

// Listas para o NPC rápido, escritas para o DMAegis (fantasia medieval, servem aos dois sistemas).
const INICIOS = ['Al', 'Bel', 'Cor', 'Dar', 'El', 'Fen', 'Gar', 'Hel', 'Ir', 'Jor', 'Kal', 'Lu', 'Mar', 'Nim', 'Or', 'Pel', 'Quen', 'Ros', 'Sel', 'Tam', 'Ul', 'Val', 'Wen', 'Zar'];
const FINAIS = ['a', 'ana', 'ard', 'ebel', 'en', 'ia', 'ino', 'is', 'ith', 'o', 'on', 'ora', 'os', 'ric', 'wen', 'yn'];
const SOBRENOMES = ['da Ponte Velha', 'do Moinho', 'Pedra-Funda', 'Folha-Rubra', 'dos Sete Sinos', 'Mão-de-Ferro', 'do Vale Cinzento', 'Corvo-Manso', 'da Maré Baixa', 'Lua-Torta', 'do Brejo', 'Pé-Ligeiro'];
const OFICIOS = ['taverneiro', 'ferreira', 'guarda da cidade', 'contrabandista', 'sacerdotisa', 'mercador de especiarias', 'caçadora', 'escriba', 'barqueiro', 'curandeira', 'menestrel', 'mendigo que já foi nobre', 'aprendiz de mago', 'coveiro', 'cartógrafa', 'domador de cavalos'];
const TRACOS = ['desconfiado com estranhos', 'fala sem parar', 'extremamente educado', 'ri de nervoso', 'mente mal', 'muito supersticioso', 'generoso até demais', 'orgulhoso e teimoso', 'sempre com pressa', 'curioso sobre tudo', 'amargo com o passado', 'otimista sem motivo'];
const APARENCIAS = ['cicatriz na sobrancelha', 'roupas remendadas com cuidado', 'anéis demais nos dedos', 'cheiro forte de fumaça', 'olhos de cores diferentes', 'tatuagem de estrela no pulso', 'manca de uma perna', 'cabelo trançado com contas', 'unhas sujas de tinta', 'voz rouca', 'chapéu enorme', 'sempre com um gato por perto'];
const DESEJOS = ['pagar uma dívida antiga', 'reencontrar um irmão desaparecido', 'sair desta cidade', 'provar que é inocente', 'ficar rico depressa', 'proteger a família', 'descobrir quem roubou sua loja', 'ser aceito por uma guilda', 'vingar um amigo', 'esconder um segredo do passado', 'conseguir um remédio raro', 'ganhar o respeito do pai'];
const MANIAS = ['tamborila os dedos na mesa', 'responde perguntas com perguntas', 'chama todo mundo de "chefe"', 'conta moedas enquanto fala', 'cita provérbios inventados', 'nunca olha nos olhos', 'cantarola baixinho', 'mastiga um graveto', 'repete a última palavra dos outros', 'ajeita o chapéu a cada frase'];

const escolher = (lista, sortear) => lista[sortear(lista.length) - 1];

export function gerarNome(sortear = dadoSeguro) {
  return `${escolher(INICIOS, sortear)}${escolher(FINAIS, sortear)} ${escolher(SOBRENOMES, sortear)}`;
}

// Um NPC completo para usar na hora. `sortear(n)` devolve de 1 a n (nos testes, uma sequência fixa).
export function gerarNpc(sortear = dadoSeguro) {
  return {
    nome: gerarNome(sortear),
    oficio: escolher(OFICIOS, sortear),
    traco: escolher(TRACOS, sortear),
    aparencia: escolher(APARENCIAS, sortear),
    desejo: escolher(DESEJOS, sortear),
    mania: escolher(MANIAS, sortear),
  };
}

// O registro de NPC para o caderno, a partir do NPC gerado.
export function registroDoNpc(npc) {
  const resumo = `${npc.oficio[0].toUpperCase()}${npc.oficio.slice(1)}, ${npc.traco}.`;
  return {
    idTipoRegistro: 1,
    nome: npc.nome,
    resumo: resumo.length > 200 ? resumo.slice(0, 200) : resumo,
    descricao: `Aparência: ${npc.aparencia}.\nQuer: ${npc.desejo}.\nMania: ${npc.mania}.`,
    atributos: [],
    tags: ['improvisado'],
  };
}
