// Dicas de condução, com texto próprio. sistema: null = vale para qualquer mesa; 1 = D&D; 2 = Daggerheart.
export const DICAS = [
  { sistema: null, texto: 'Antes de pedir um teste, pergunte-se: falhar aqui deixa a história mais interessante? Se não, deixe dar certo.' },
  { sistema: null, texto: 'Descreva com três sentidos: o que se vê, o que se ouve e um cheiro ou textura.' },
  { sistema: null, texto: 'Quando a mesa travar, revele um segredo. Ele não precisa sair do lugar que você planejou.' },
  { sistema: null, texto: 'Termine a cena quando a pergunta dela tiver resposta. Corte para a próxima.' },
  { sistema: null, texto: 'Dê nome e um trejeito a cada NPC que fala. Anote os dois para lembrar depois.' },
  { sistema: null, texto: 'Repare em quem falou menos nos últimos minutos e pergunte o que o personagem dele faz.' },
  { sistema: null, texto: 'Mostre a ameaça antes de usá-la: pegadas, um boato, um cheiro de enxofre.' },
  { sistema: null, texto: 'Diga o custo antes da escolha: "dá para passar, mas vai fazer barulho".' },
  { sistema: null, texto: 'Use as ideias dos jogadores. Se eles suspeitam de algo, talvez seja verdade.' },
  { sistema: null, texto: 'Faça uma pausa curta a cada uma hora e meia. A mesa volta com mais energia.' },
  { sistema: 1, texto: 'Na dúvida entre dois números, use CD 15. Fácil é 10; muito difícil é 25.' },
  { sistema: 1, texto: 'Vantagem e desvantagem não se somam: com qualquer quantidade de cada, elas se anulam.' },
  { sistema: 1, texto: 'Um teste passivo (10 + modificador) evita rolar Percepção a cada corredor.' },
  { sistema: 1, texto: 'Inimigos fogem ou se rendem quando a luta vira. Nem todo combate termina em zero PV.' },
  { sistema: 1, texto: 'A Inspiração Heroica é uma boa recompensa para uma ideia ousada ou uma cena bem interpretada.' },
  { sistema: 2, texto: 'Quando a rolagem sai com Medo, a mesa sente: descreva a complicação antes de gastar o Medo.' },
  { sistema: 2, texto: 'Passe o holofote: depois de uma ação, pergunte quem reage e deixe a cena girar pelos personagens.' },
  { sistema: 2, texto: 'Gaste Medo para dar a vez aos adversários, mas mostre a ameaça na ficção antes do golpe.' },
  { sistema: 2, texto: 'Sucesso com Medo ainda é sucesso: entregue o que pediram e acrescente uma nova pressão.' },
  { sistema: 2, texto: 'Pergunte aos jogadores sobre o passado dos personagens e transforme a resposta em gancho.' },
];

const TRES_MINUTOS = 3 * 60 * 1000;

// Só as dicas que fazem sentido na mesa: as gerais e as do sistema da campanha.
export function dicasDoSistema(dicas, idSistema) {
  return dicas.filter((d) => d.sistema === null || d.sistema === idSistema);
}

// A dica muda a cada 3 minutos e dá a volta na lista.
export function dicaDoMomento(agora, dicas) {
  if (dicas.length === 0) return null;
  return dicas[Math.floor(agora / TRES_MINUTOS) % dicas.length];
}
