// Referência rápida de D&D 5e (regras de 2024), traduzida e resumida do SRD 5.2.1 (CC-BY 4.0).
// O termo em inglês acompanha cada item para facilitar a conferência no SRD.
export const REGRAS_DND = {
  secoes: [
    {
      titulo: 'Condições',
      itens: [
        { nome: 'Agarrado', termo: 'Grappled', texto: 'Deslocamento 0. Desvantagem nos ataques contra qualquer alvo que não seja quem agarra. Quem agarra pode arrastar ou carregar você, gastando 1 metro a mais de movimento por metro (a não ser que você seja Miúdo ou dois tamanhos menor).' },
        { nome: 'Amedrontado', termo: 'Frightened', texto: 'Desvantagem em testes de atributo e ataques enquanto a fonte do medo estiver à vista. Não pode se aproximar dela por vontade própria.' },
        { nome: 'Atordoado', termo: 'Stunned', texto: 'Incapacitado. Falha automática em salvaguardas de Força e Destreza. Ataques contra você têm vantagem.' },
        { nome: 'Caído', termo: 'Prone', texto: 'Só pode rastejar ou gastar metade do deslocamento para levantar. Desvantagem nos seus ataques. Ataques contra você têm vantagem a até 1,5 m e desvantagem de mais longe.' },
        { nome: 'Cego', termo: 'Blinded', texto: 'Não enxerga e falha em testes que dependem da visão. Ataques contra você têm vantagem; os seus têm desvantagem.' },
        { nome: 'Enfeitiçado', termo: 'Charmed', texto: 'Não pode atacar quem o enfeitiçou nem mirá-lo com efeitos de dano. Essa criatura tem vantagem em testes de interação social com você.' },
        { nome: 'Envenenado', termo: 'Poisoned', texto: 'Desvantagem em ataques e testes de atributo.' },
        { nome: 'Exaustão', termo: 'Exhaustion', texto: 'Cumulativa, de 1 a 6 níveis. Cada nível tira 2 de todo Teste de d20 e 1,5 m do deslocamento. No nível 6, a criatura morre. Um descanso longo remove 1 nível.' },
        { nome: 'Impedido', termo: 'Restrained', texto: 'Deslocamento 0. Ataques contra você têm vantagem; os seus têm desvantagem. Desvantagem em salvaguardas de Destreza.' },
        { nome: 'Incapacitado', termo: 'Incapacitated', texto: 'Não pode fazer ações, ações bônus nem reações, nem falar. A Concentração termina. Desvantagem na Iniciativa.' },
        { nome: 'Inconsciente', termo: 'Unconscious', texto: 'Incapacitado e Caído; solta o que segura. Deslocamento 0. Falha automática em salvaguardas de Força e Destreza. Ataques contra você têm vantagem, e acertos a até 1,5 m são críticos.' },
        { nome: 'Invisível', termo: 'Invisible', texto: 'Vantagem na Iniciativa. Não é afetado por efeitos que exigem ver o alvo. Ataques contra você têm desvantagem; os seus têm vantagem (a menos que a criatura consiga vê-lo).' },
        { nome: 'Paralisado', termo: 'Paralyzed', texto: 'Incapacitado e com deslocamento 0. Falha automática em salvaguardas de Força e Destreza. Ataques contra você têm vantagem, e acertos a até 1,5 m são críticos.' },
        { nome: 'Petrificado', termo: 'Petrified', texto: 'Vira pedra (ou outra substância). Incapacitado, deslocamento 0, falha automática em salvaguardas de Força e Destreza, ataques contra você têm vantagem. Resistência a todo dano e imune a Envenenado.' },
        { nome: 'Surdo', termo: 'Deafened', texto: 'Não ouve e falha em testes que dependem da audição.' },
      ],
    },
    {
      titulo: 'Classe de Dificuldade',
      itens: [
        { nome: 'Muito fácil', termo: 'Very Easy', texto: 'CD 5' },
        { nome: 'Fácil', termo: 'Easy', texto: 'CD 10' },
        { nome: 'Médio', termo: 'Medium', texto: 'CD 15' },
        { nome: 'Difícil', termo: 'Hard', texto: 'CD 20' },
        { nome: 'Muito difícil', termo: 'Very Hard', texto: 'CD 25' },
        { nome: 'Quase impossível', termo: 'Nearly Impossible', texto: 'CD 30' },
      ],
    },
    {
      titulo: 'Ações',
      itens: [
        { nome: 'Atacar', termo: 'Attack', texto: 'Um ataque com arma ou desarmado (mais, com Ataque Extra).' },
        { nome: 'Correr', termo: 'Dash', texto: 'Ganha deslocamento extra igual ao seu deslocamento neste turno.' },
        { nome: 'Desengajar', termo: 'Disengage', texto: 'Seu movimento não provoca ataques de oportunidade até o fim do turno.' },
        { nome: 'Esquivar', termo: 'Dodge', texto: 'Até o seu próximo turno, ataques contra você têm desvantagem (se você vê o atacante) e você tem vantagem em salvaguardas de Destreza. Perde o efeito se ficar Incapacitado ou com deslocamento 0.' },
        { nome: 'Ajudar', termo: 'Help', texto: 'Dá vantagem no próximo teste de atributo de um aliado (numa perícia ou ferramenta que você domina) ou no próximo ataque de um aliado contra um inimigo a até 1,5 m de você.' },
        { nome: 'Esconder', termo: 'Hide', texto: 'Teste de Destreza (Furtividade) CD 15, fora da linha de visão e com escuridão ou cobertura de três quartos. Com sucesso, fica Invisível; o total do teste é a CD para encontrá-lo.' },
        { nome: 'Influenciar', termo: 'Influence', texto: 'Teste de Carisma ou Sabedoria para mudar a atitude de uma criatura.' },
        { nome: 'Magia', termo: 'Magic', texto: 'Conjurar uma magia, usar um item mágico ou um traço mágico.' },
        { nome: 'Preparar', termo: 'Ready', texto: 'Escolhe um gatilho e uma ação; quando o gatilho acontece, usa a reação para agir.' },
        { nome: 'Procurar', termo: 'Search', texto: 'Teste de Sabedoria (Intuição, Medicina, Percepção ou Sobrevivência).' },
        { nome: 'Estudar', termo: 'Study', texto: 'Teste de Inteligência (Arcanismo, História, Investigação, Natureza ou Religião).' },
        { nome: 'Usar', termo: 'Utilize', texto: 'Usar um objeto não mágico.' },
      ],
    },
    {
      titulo: 'Cobertura e vantagem',
      itens: [
        { nome: 'Meia cobertura', termo: 'Half Cover', texto: '+2 na CA e nas salvaguardas de Destreza.' },
        { nome: 'Cobertura de três quartos', termo: 'Three-Quarters Cover', texto: '+5 na CA e nas salvaguardas de Destreza.' },
        { nome: 'Cobertura total', termo: 'Total Cover', texto: 'Não pode ser alvo direto de ataques nem de magias.' },
        { nome: 'Vantagem e desvantagem', termo: 'Advantage / Disadvantage', texto: 'Role dois d20 e fique com o maior (vantagem) ou o menor (desvantagem). Várias não se somam, e uma vantagem e uma desvantagem se anulam.' },
      ],
    },
    {
      titulo: 'Na hora do aperto',
      itens: [
        { nome: 'Concentração', termo: 'Concentration', texto: 'Ao sofrer dano, salvaguarda de Constituição com CD 10 ou metade do dano (o que for maior, até CD 30). Termina se ficar Incapacitado.' },
        { nome: 'Ataque de oportunidade', termo: 'Opportunity Attack', texto: 'Reação quando uma criatura que você vê sai do seu alcance: um ataque corpo a corpo.' },
        { nome: 'Salvaguarda contra a morte', termo: 'Death Saving Throw', texto: 'Com 0 PV, d20 no início do turno: 10 ou mais é sucesso. Três sucessos estabilizam; três falhas, morte. 1 conta como duas falhas; 20 recupera 1 PV.' },
      ],
    },
  ],
  rodape:
    'Traduzido e resumido do System Reference Document 5.2.1 ("SRD 5.2.1"), de Wizards of the Coast LLC, '
    + 'disponível em https://www.dndbeyond.com/srd, sob a licença Creative Commons Attribution 4.0 International (CC-BY 4.0). '
    + 'É um lembrete rápido: na dúvida, confira no SRD.',
};
