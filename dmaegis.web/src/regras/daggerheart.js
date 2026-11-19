// Lembretes de Daggerheart, só as mecânicas, em texto próprio (sem copiar o SRD; veja docs/CONVENCOES.md).
export const REGRAS_DAGGERHEART = {
  secoes: [
    {
      titulo: 'Rolagem de dualidade',
      itens: [
        { nome: 'Dualidade', texto: 'Role os dois d12 (um de Esperança e um de Medo), some o traço e compare com a dificuldade.' },
        { nome: 'Com Esperança', texto: 'O dado de Esperança saiu maior: o jogador ganha 1 Esperança.' },
        { nome: 'Com Medo', texto: 'O dado de Medo saiu maior: o mestre ganha 1 Medo e pode tomar o holofote.' },
        { nome: 'Crítico', texto: 'Os dois dados iguais: sucesso crítico. O jogador ganha 1 Esperança e limpa 1 Estresse.' },
        { nome: 'Vantagem e desvantagem', texto: 'Some (vantagem) ou subtraia (desvantagem) um d6 do resultado.' },
      ],
    },
    {
      titulo: 'Medo e holofote',
      itens: [
        { nome: 'Medo', texto: 'Recurso do mestre (até 12). Gaste para fazer jogadas de mestre mais fortes, dar a vez aos adversários ou usar as habilidades de Medo deles.' },
        { nome: 'Holofote', texto: 'Não há iniciativa: o holofote passa pelos personagens. Ele vem para o mestre quando um jogador falha ou rola com Medo.' },
        { nome: 'Esperança', texto: 'Recurso de cada jogador (até 6). Serve para ajudar um aliado, usar uma Experiência ou ativar habilidades.' },
      ],
    },
    {
      titulo: 'Dano',
      itens: [
        { nome: 'Limiares', texto: 'Abaixo do limiar Maior, marque 1 PV; a partir do Maior, 2 PV; a partir do Grave, 3 PV.' },
        { nome: 'Armadura', texto: 'Marcar um espaço de armadura reduz o dano em um limiar.' },
        { nome: 'Estresse', texto: 'Com o Estresse cheio, cada Estresse a mais vira 1 PV marcado, e o personagem fica Vulnerável.' },
      ],
    },
  ],
  rodape: 'Resumo com palavras próprias, para lembrar na hora. Na dúvida, confira no livro.',
};
