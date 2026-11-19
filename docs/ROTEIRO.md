# Pesquisa e roteiro do DMAegis

Comecei este documento em 17/11/2026, depois de pesquisar o que já existe para mestres de RPG. O app é um escudo do mestre digital para D&D 5e (regras de 2024) e Daggerheart, ligado a um caderno de campanha. O nome DMAegis veio no mesmo dia; a pasta do projeto ainda se chama `AuxiliarDoMestre`.

## 1. A ideia

Um site em português que faz o papel do escudo do mestre: a tela que fica na minha frente durante a sessão, com o que preciso para lembrar das coisas e improvisar sem parar o jogo. Atrás do escudo fica um caderno com toda a informação da campanha.

Cada campanha é de D&D 5e ou de Daggerheart, e o escudo muda conforme o sistema.

| Parte | Para quê | Igual nos dois sistemas? |
| --- | --- | --- |
| Caderno | NPCs, locais, facções, itens, missões, segredos e o histórico das sessões | Sim. Só os atributos das criaturas mudam. |
| Improviso | Tabelas aleatórias, oráculo de sim ou não, nomes, NPC rápido | Sim |
| Escudo | A tela da sessão: preparo, busca rápida, anotações e os painéis de regra | Não. Cada sistema tem os próprios painéis (seção 3). |

O que liga as partes: o que eu improvisar no escudo deve ir para o caderno com um clique. O NPC inventado na hora vira um registro e fica ligado à sessão.

No começo eu deixei a mesa virtual de fora, porque Foundry, Roll20, Demiplane e Owlbear já fazem isso. Em 19/11/2026 mudei de ideia: meu grupo joga pelo Discord, sem mesa virtual, e eu queria que os jogadores vissem o mapa no próprio navegador. O design dessa parte está em [`docs/design/mesa-virtual.md`](design/mesa-virtual.md). Ficha completa dos jogadores continua fora.

## 2. O que a pesquisa mostrou

### 2.1 O que atrapalha o mestre durante a sessão

- Abas demais: a gente fica pulando entre ferramentas.
- Esquecer coisas: o nome de um NPC some da cabeça no meio da cena, ou leva tempo achar nas anotações.
- Parar para consultar regra, o que quebra o ritmo.
- Cansaço de ferramenta: configurar mesa virtual e preparar sessão vira um segundo hobby.

Boa parte dessas fontes vem de quem cria ou vende ferramentas, então leve com um grão de sal.

### 2.2 Escudos digitais que já existem

| Ferramenta | O que dá para aproveitar |
| --- | --- |
| Game Master Screen (plugin do Obsidian) | Telas em abas com blocos que se movem, mudam de tamanho, se recolhem e podem ser travados. Tem blocos de dados, notas, tabelas aleatórias, relógios e trilha sonora. É o mais parecido com o que eu quero. |
| Virtual GM Screen (Android, Pathfinder 2e) | Painéis ligados entre si: abrir uma cena carrega o que é dela. |
| Overseer Studio (desktop) | Funciona offline. |
| PrismScrollDM (celular, D&D 5e) | Mostrar e esconder blocos para limpar a tela. |
| Fear Tracker (módulo do Foundry) | Contador de Medo do Daggerheart. Mostra que essa mecânica pede um painel próprio. |
| Escudos em PDF de Daggerheart (de fãs, um deles brasileiro, no itch.io) | O que os mestres querem à vista: regras de Medo, resultados das rolagens, contagens regressivas, custo dos adversários. |
| Kanka, LegendKeeper, World Anvil | Wikis de campanha, o "caderno". O Kanka tem atributos livres por registro. |

Não achei nenhum app em português que junte escudo, improviso e caderno, nem um que atenda D&D e Daggerheart no mesmo lugar.

### 2.3 Ferramentas de improviso

- Oráculo de sim ou não com a escala "Não, e…", "Não", "Não, mas…", "Sim, mas…", "Sim" e "Sim, e…". A resposta traz uma complicação ou uma vantagem em vez de um sim seco. A ideia vem das regras do FU e é genérica, então dá para escrever com texto próprio.
- Tabelas aleatórias (NPCs, tavernas, eventos, clima, complicações) para fugir da primeira ideia.
- Lista de nomes. O Sly Flourish diz que uma boa lista de nomes é a ferramenta mais valiosa do mestre.
- Tabelas que chamam outras, como "O taverneiro {nome} parece {humor}". O projeto `rpg-table-randomizer` (derikb) serve de referência.

### 2.4 Um método de preparo: os 8 passos do Lazy DM

O livro *Return of the Lazy Dungeon Master*, do Sly Flourish, divide o preparo em 8 passos opcionais pensados para quem improvisa. Funciona nos dois sistemas e é a base do painel "Sessão de hoje":

1. Personagens. 2. Começo forte. 3. Cenas. 4. Segredos e pistas (o passo mais importante, segundo o autor). 5. Locais fantásticos. 6. NPCs importantes. 7. Monstros ou adversários possíveis. 8. Tesouros possíveis.

No escudo, cada segredo tem um botão "revelado", e os que não forem revelados passam sozinhos para a próxima sessão.

## 3. O que cada sistema precisa no escudo

Parte dos números abaixo veio de SRDs espelhados por fãs e de escudos não oficiais. Antes de programar qualquer regra, eu confiro no SRD oficial.

### 3.1 D&D 5e (regras de 2024, SRD 5.2.1)

| Painel | O que mostra |
| --- | --- |
| Iniciativa | Ordem por rolagem de d20, rodada atual, PV, PV temporário, CA, condições e concentração de cada criatura. |
| Condições | O efeito de cada condição do SRD (agarrado, amedrontado, caído, envenenado, exaustão etc.). |
| Classes de Dificuldade | A tabela de CD do SRD, de muito fácil a quase impossível. |
| Montar encontro | Orçamento de XP por personagem nas regras de 2024 (baixo, moderado ou alto, sem multiplicador de grupo) e XP por Nível de Desafio. Plugins do Obsidian e do Foundry já fazem essa conta a partir do SRD 5.2.1. |
| Monstros | Bloco de estatísticas do SRD, importado da API Open5e, ou criado por mim. |
| Dados | `1d20+5`, vantagem e desvantagem (`2d20kh1` e `2d20kl1`), dano. |

### 3.2 Daggerheart

O Daggerheart funciona de outro jeito: não tem iniciativa fixa. O holofote passa entre os jogadores, e o mestre age quando alguém falha ou rola com Medo.

| Painel | O que mostra |
| --- | --- |
| Medo do mestre | Contador de fichas de Medo, que sobe quando um jogador rola com Medo. Um guia de fãs fala em máximo de 10; preciso confirmar no SRD. Um botão "gastar" registra no que o Medo foi usado (movimento extra, adversário extra, habilidade de Medo, interrupção). |
| Dados de dualidade | Dois d12 (Esperança e Medo) somados ao modificador, contra uma Dificuldade. O resultado é sucesso ou falha, com Esperança ou com Medo, e dados iguais são crítico. Vantagem e desvantagem usam um d6 a mais. O app mostra o resultado já interpretado e soma 1 de Medo sozinho. |
| Adversários | Ficha com Dificuldade, limiares de dano (Maior e Grave), PV e Estresse para marcar, além do tipo (lacaio, horda, líder, bruto, solo…). |
| Montar encontro | Pontos de Batalha. Um escudo de fãs fala em 3 × número de PJs + 2, com custos de 1 (lacaios, social ou suporte) a 5 (solo). Preciso confirmar no livro. |
| Contagens regressivas | Relógios que descem a cada gatilho até uma consequência. |
| Ambientes | Cartões de ambiente com as habilidades que gastam Medo. |
| Holofote | Uma lista de quem já teve a vez na cena, para distribuir o holofote. |

### 3.3 Português e nome dos termos

- D&D: a Wizards anunciou o fim das traduções para o português em fevereiro de 2024. Há notícias de que a Galápagos/Asmodee trouxe a edição de 2024, mas as fontes se contradizem. Como o SRD 5.2.1 é em inglês, a tradução dos termos é minha. O app mostra o termo em inglês ao lado (por exemplo, "Agarrado (*Grappled*)"), o que ajuda quem joga com material em inglês.
- Daggerheart: a Jambô Editora é a editora oficial no Brasil. Uso os termos da edição da Jambô (conferindo no livro), para não confundir quem joga com o livro em português.

## 4. Licenças

| Fonte | Licença | Uso no app |
| --- | --- | --- |
| SRD 5.2.1 de D&D | CC-BY-4.0, irrevogável | Posso usar e traduzir, com crédito: *"Dungeons & Dragons SRD 5.2 by Wizards of the Coast LLC is licensed under CC BY 4.0."* A CC-BY permite adaptações (traduções) desde que eu dê o crédito e indique que houve mudança. Vale só para o SRD, não para a marca D&D. A frase exata está no PDF oficial. |
| API Open5e | Gratuita, sem chave | Monstros e magias do SRD. Melhor importar para o próprio banco. |
| SRD do Daggerheart | Darrington Press Community Gaming License (DPCGL), versão 2.0 de 25/08/2026 | Tem ressalvas; veja abaixo. |
| Lazy GM's Resource Document | CC-BY 4.0 | Posso usar nas tabelas iniciais, com a frase de crédito que o documento pede. |
| Gerador de nomes do Sly Flourish (o script) | CC BY | Posso usar, com crédito. A *página* de nomes é CC BY-NC, então fico longe dela. |
| Mythic GM Emulator | Produto comercial | Não copio as tabelas. |

### O caso do Daggerheart

O que a DPCGL diz, pela página oficial e pelo FAQ:

- Permite criar e vender conteúdo baseado nas regras do Daggerheart, sem royalties, em formatos permitidos. Exige frases de crédito e o logo de conteúdo da comunidade.
- Mesas virtuais entraram como formato permitido em junho de 2025, mas só as de uma lista aprovada (Roll20, Demiplane, Foundry, Alchemy e Fantasy Grounds, em 30/07/2025). As outras precisam pedir por e-mail.
- Não fala nada específico sobre apps ou sites, e o FAQ manda perguntar em `info@darringtonpress.com`.
- Proíbe usar logos, arte, personagens e lugares da Darrington Press ou da Critical Role, e reivindicar como seus o sistema e o modelo de bloco de estatísticas. "Daggerheart™ Compatible" só pode aparecer de forma discreta, nunca no título.

Por isso decidi:

1. Programar as mecânicas como ferramentas (contador de Medo, dados de dualidade, contagens regressivas, ficha de adversário em branco), com texto meu. São ferramentas para o mestre, não uma cópia do livro.
2. Não pré-carregar adversários, ambientes nem textos do SRD do Daggerheart até ter resposta da Darrington Press. Quem usa cadastra os adversários.
3. Antes de publicar o site, mandar um e-mail para a Darrington Press descrevendo o app. Para uso pessoal, os itens 1 e 2 bastam.
4. Não usar o nome "Daggerheart" no nome do app.

Isto é o resumo da minha pesquisa, não uma orientação jurídica.

## 5. Decisões

| # | Decisão | O que escolhi |
| --- | --- | --- |
| 1 | Stack | .NET 10, MySQL e React com Vite e Bootstrap, a mesma que eu já usava no RetroGames. |
| 2 | Projeto do curso ou pessoal | Pessoal. |
| 3 | Qual sistema primeiro | D&D. A licença é clara e o conteúdo do SRD pode vir pronto. O Daggerheart entra depois, começando pelo contador de Medo e pelos dados de dualidade. |
| 4 | D&D 2014 ou 2024 | 2024 (SRD 5.2.1). Muitas mesas ainda jogam 2014; no escudo, as diferenças principais são a exaustão e a conta de encontro. |
| 5 | Funcionar offline | Depois, na fase 4. |
| 6 | Onde rodam os dados e as tabelas | No navegador: só o mestre vê, e continua funcionando se a internet cair. |

## 6. Funcionalidades por fase

### Fase 1: MVP, caderno e escudo básico (comum aos dois sistemas)

1. Conta e login (cookie com proteção CSRF, senha guardada só como hash).
2. Campanhas com o sistema escolhido (D&D 5e 2024 ou Daggerheart).
3. Registros: NPCs, personagens dos jogadores, criaturas ou adversários, locais, facções, itens e missões. Cada um tem nome, resumo de uma linha, descrição, tags e atributos. Os atributos de criatura vêm de um modelo do sistema (no D&D, CA, PV, ND e deslocamento; no Daggerheart, Dificuldade, limiares Maior e Grave, PV, Estresse e tipo).
4. Sessões com o preparo nos 8 passos e segredos com status.
5. Escudo v1: painel "Sessão de hoje", busca rápida (Ctrl+K), anotações com horário e rolador de dados.

### Fase 2: improviso (comum aos dois sistemas)

1. Tabelas aleatórias próprias, com pesos e tabelas aninhadas `{…}`.
2. Oráculo de sim ou não com probabilidade ajustável.
3. NPC rápido e "salvar no caderno".
4. Pacote inicial de tabelas em português, de fantasia medieval, que serve aos dois sistemas.

### Fase 3: módulos de cada sistema

D&D 5e:
1. Rastreador de iniciativa (ordem, rodada, PV, CA, condições, concentração).
2. Condições e tabela de CD, traduzidas do SRD.
3. Importar monstros do SRD (Open5e) para o caderno.
4. Montar encontro com o orçamento de XP de 2024.

Daggerheart:
1. Contador de Medo com histórico de gastos.
2. Dados de dualidade com o resultado interpretado (1 de Medo somado sozinho).
3. Rastreador de adversários (PV, Estresse, limiares) e lista de holofote.
4. Contagens regressivas.
5. Montar encontro por Pontos de Batalha.

### Fase 4: escudo completo

1. Layout em painéis configuráveis: arrastar, mudar de tamanho, recolher, travar, mais de uma aba ("Exploração", "Combate").
2. Cartões de referência escritos pelo mestre, as próprias colas.
3. Modo escuro e tela cheia.
4. Offline (PWA): a sessão de hoje e as tabelas em cache, e as anotações sincronizam quando a internet voltar.

### Fase 5: extras

- Exportar a campanha em Markdown ou PDF.
- Ferramentas de segurança para a sessão zero (linhas e véus, carta X). O guia de sessão zero do Daggerheart fala disso.
- Talvez: um link somente leitura para os jogadores. (Acabou virando a mesa virtual.)

Fora do escopo: ficha dos jogadores, geração por IA, outros sistemas e conteúdo do Daggerheart sem resposta da Darrington Press.

## 7. Rascunho do modelo de dados

Caderno:
- Usuario: id, nome, e-mail (único), hash da senha, data de criação.
- Campanha: id, id do usuário, nome, id do sistema, descrição, arquivada.
- Sistema: id, nome (D&D 5e 2024, Daggerheart). Tabela de domínio, carregada pelo script.
- ModeloAtributo: id do sistema, tipo de registro, chave (por exemplo, "Limiar Maior"), ordem. Define os campos de cada tipo em cada sistema.
- Registro: id, id da campanha, tipo (NPC, PJ, Criatura, Local, Facção, Item, Missão), nome, resumo, descrição, origem (criado, improvisado ou importado do SRD).
- AtributoRegistro: id do registro, chave, valor, ordem.
- Tag e RegistroTag.
- Sessao: id da campanha, número, data, começo forte, resumo.
- CenaSessao, Segredo, SessaoRegistro e AnotacaoSessao.

Improviso:
- TabelaAleatoria: id, id do usuário, id da campanha (nulo para todas), nome, categoria, pacote inicial.
- ItemTabela: id da tabela, texto (pode ter `{OutraTabela}`), peso.

Escudo:
- Combate: id da sessão, rodada atual, ativo.
- Combatente: id do combate, id do registro (opcional), nome, PV atual, PV máximo, ordem (iniciativa no D&D, nula no Daggerheart), Estresse (Daggerheart), condições.
- ContadorSessao (Daggerheart): id da sessão, Medo atual.
- MovimentoMedo (Daggerheart): id da sessão, horário, variação (+1 ou −1), motivo.
- ContagemRegressiva (Daggerheart): id da sessão, nome, valor atual, valor inicial, consequência.
- LayoutEscudo: id do usuário, id da campanha, nome da aba, painéis (JSON), travado.
- CartaoReferencia: id do usuário, id do sistema (nulo para todos), título, conteúdo (Markdown).

Ficou em aberto, e decidi assim:
1. Uma tabela `Registro` com `ModeloAtributo` por sistema, em vez de tabelas separadas como `MonstroDnd` e `AdversarioDaggerheart`. É mais flexível e os dois sistemas usam o mesmo código; tabelas separadas dariam mais validação, mas muito mais código.
2. O layout do escudo pode ficar como JSON.

## 8. Roteiro em etapas

| Etapa | Entrega | Como conferir |
| --- | --- | --- |
| 0 | Repositório, `.gitignore`, solução .NET e front com Vite, `.env.example`, histórias de usuário | `dotnet build` e `npm run dev` rodam |
| 1 | MER e script SQL do MVP, com as tabelas `Sistema` e `ModeloAtributo` preenchidas e uma campanha de exemplo para cada sistema | As consultas de teste retornam os dados |
| 2 | Cadastro e login (cookie, CSRF e hash) | Nenhum token no `localStorage` |
| 3 | Campanhas com sistema | Um usuário não vê a campanha de outro |
| 4 | Registros com atributos vindos do modelo do sistema | Uma criatura de Daggerheart mostra "Limiar Maior", e uma de D&D mostra "CA" |
| 5 | Sessões: 8 passos e segredos | Um segredo pendente aparece na sessão seguinte |
| 6 | Escudo v1: painel da sessão, Ctrl+K e anotações | Conduzir uma sessão sem abrir outra aba |
| 7 | Rolador de dados (leitor de notação em JS, com testes) | Os testes cobrem `XdY`, `+/-`, `kh/kl` e expressão inválida |
| 8 | Marco do MVP: usar numa sessão real | Lista de ajustes escrita |
| 9 | Tabelas aleatórias com pesos e aninhadas | Uma referência circular não trava o app |
| 10 | Oráculo, NPC rápido, "salvar no caderno" e pacote de tabelas com créditos | O NPC salvo aparece na busca |
| 11 | D&D: iniciativa, condições, CD | Rodar um combate inteiro no escudo |
| 12 | D&D: importação do SRD (Open5e) e encontro por XP | O orçamento bate com a tabela do SRD 5.2.1 |
| 13 | Daggerheart: Medo e dados de dualidade | Os testes cobrem os 5 resultados (sucesso ou falha, com Esperança ou Medo, e crítico) |
| 14 | Daggerheart: adversários, holofote, contagens, Pontos de Batalha | Rodar uma cena de conflito inteira |
| 15 | Layout configurável e travável, e cartões de referência | O layout volta igual ao recarregar |
| 16 | E-mail para a Darrington Press, deploy e política de privacidade | Página de créditos com as frases das licenças |
| 17 | PWA e offline | Desligar a internet e continuar anotando |

## 9. Tecnologias que levantei

| Necessidade | Opção | Observação |
| --- | --- | --- |
| Painéis arrastáveis | Uma biblioteca de grid para React (por exemplo, `react-grid-layout`) | Pesquisar na etapa 15: licença, versão e se ainda tem manutenção. |
| Rolagem de dados | Leitor próprio em JS, com `@dice-roller/rpg-dice-roller` (MIT, versão 5.5.1) como referência | A dualidade do Daggerheart é uma regra por cima do leitor: dois d12 com papéis diferentes. |
| Tabelas aleatórias | Lógica própria | `rpg-table-randomizer` como referência. |
| Monstros de D&D | API Open5e v2 (`/v2/creatures/`) | Uma fonte de terceiros diz que a Open5e cobre o SRD de 2024 melhor que a `dnd5eapi.co`. Falta confirmar. |
| Offline | PWA (service worker e IndexedDB) | Fase 4. |

## 10. Hospedagem, LGPD e riscos

Hospedagem: as opções gratuitas costumam oferecer PostgreSQL, e o MySQL é o que limita. O Azure for Students (sem cartão, e para em vez de cobrar) parece a melhor opção para a API. O Railway tem MySQL por créditos. O Render dorme depois de 15 minutos sem uso, o que é ruim na hora do jogo. O front pode ir para a Vercel ou a Netlify. Vou conferir os limites quando for publicar. Por enquanto, a mesa roda no meu PC e os jogadores entram por um túnel (veja o README).

LGPD: política de privacidade própria, com versão e data. Coletar só nome, e-mail e senha, e permitir excluir a conta. Referência: o *Guia de Requisitos Mínimos de Privacidade e Segurança para Aplicações Web* (PPSI, gov.br).

Riscos:
- Escopo: dois sistemas dobram a parte de regras. Mantenho o núcleo comum e faço um sistema de cada vez.
- Licença do Daggerheart: veja a seção 4. Não publico com conteúdo do SRD sem resposta.
- Regras erradas: vários números desta pesquisa vieram de fãs. Cada regra programada precisa ser conferida no SRD oficial e ter teste.
- O escudo ficar lento, ou a internet cair no meio da sessão.

## Fontes

- Licença do Daggerheart: [DPCGL (página oficial)](https://darringtonpress.com/license), [FAQ do Daggerheart](https://www.daggerheart.com/faq/), [SRD do Daggerheart](https://www.daggerheart.com/srd), [DPCGL de 30/07/2025 (PDF)](https://darringtonpress.com/wp-content/uploads/2025/07/DPCGL-July-30th-2025.pdf), [Sistema Daggerheart no Foundry](https://foundryvtt.com/packages/daggerheart), [Daggerheart Brews](https://next-daggerheart.vercel.app/about), [EN World: onde baixar o SRD](https://www.enworld.org/goto/post?id=9683242)
- Regras do Daggerheart: [Mecânicas centrais (SRD em Markdown)](https://nemo-illusionist.github.io/ttrpg_srd_markdown/en/daggerheart/srd-1.0/07_CoreMechanics), [Glossário (SRD em Markdown)](https://nemo-illusionist.github.io/ttrpg_srd_markdown/en/daggerheart/srd-1.0/17_Glossary/00_Glossary), [Escudo do mestre "Old Gus" (PDF de fã)](https://callmepartario.github.io/og-dhsrd/pdfs/old-gus-daggerheart-gm-screen.pdf), [Fear Tracker (Foundry)](https://foundryvtt.com/packages/fear-tracker), [Medo, adversários e holofote](https://louisbranch.github.io/fracturing.space/reference/daggerheart-playbooks/playbook-gm-fear-adversaries-and-spotlight.html), [Esperança e Medo](https://dungeonmister.com/daggerheart/hope-fear-in-daggerheart/), [Mestrando Daggerheart: primeiras impressões](https://dungeonmister.com/daggerheart/running-daggerheart-a-gms-first-impressions/), [Fichas adicionais (oficial)](https://www.daggerheart.com/wp-content/uploads/2025/05/Additional-Sheets-Daggerheart-Aug252026.pdf), [Aventura introdutória (oficial)](https://www.daggerheart.com/wp-content/uploads/2025/05/Quickstart-Adventure-5-20-2025.pdf), [Sessão zero do Daggerheart](https://www.daggerheart.com/sessionzero/)
- Daggerheart no Brasil: [Onde comprar (oficial)](https://www.daggerheart.com/buy/), [Caixa brasileira (Final Boss)](https://finalboss.io/daggerheart-finally-hit-brazil-but-can-a-fancy), [Tradução não oficial do SRD (Foundry)](https://foundryvtt.com/packages/daggerheart-translation-pt-BR), [Escudo para Daggerheart (itch.io)](https://ratoruja.itch.io/escudo-para-daggerheart/purchase)
- D&D em português: [ComicBook: fim das traduções](https://comicbook.com/gaming/news/dungeons-dragons-discontinues-portuguese-translations), [EN World: D&D sem português](https://www.enworld.org/goto/post?id=9273621), [EinerD: Galápagos distribuidora](https://www.einerd.com/galapagos-asmodee-sera-a-distribuidora-oficial-de-dungeons-dragons-no-brasil/), [D&D no Brasil (Wizards)](https://dnd.wizards.com/pt-BR/news/expanding-dnd-ptbr), [Glossário UFRGS](https://www.ufrgs.br/tesauros/index.php/thesa/terms/1728)
- SRD e licença de D&D: [D&D Beyond: SRD 5.2.1](https://dndbeyond.com/srd), [PC Gamer: SRD em Creative Commons](https://www.pcgamer.com/games/board-games/the-core-of-the-new-dungeons-and-dragons-rules-have-been-released-into-the-creative-commons-so-you-can-use-them-when-you-publish-your-own-d-and-d-material), [Tribality: SRD 5.2](https://www.tribality.com/2025/04/23/dd-system-reference-document-v5-2/), [Open5e: documentação](https://open5e.com/api-docs), [Lorekeeper MCP: endpoints da Open5e](https://glama.ai/mcp/servers/@frap129/lorekeeper-mcp/blob/81520084d6a7a97c0a6173c4da67b08edc17a118/docs/apis/open5e-api.md)
- Iniciativa e encontros de D&D: [Arcane Eye: rastreadores de iniciativa](https://arcaneeye.com/dm-tools-5e/best-dnd-5e-initiative-tracker/), [D&D Battle Tracker](https://github.com/Paul-Ladyman/dnd-battle-tracker), [Encounter Builder (Obsidian)](https://community.obsidian.md/plugins/encounter-builder), [Encounter Builder 2024 (Foundry)](https://foundryvtt.com/packages/encounter-builder-2024)
- Escudos digitais: [Game Master Screen (Obsidian)](https://community.obsidian.md/plugins/game-master-screen), [Virtual GM Screen](https://nothingtoseeheregaming.itch.io/virtual-gm-screen), [Overseer Studio](https://groupfinder.eu/library/overseer-studio), [PrismScrollDM](https://apps.apple.com/bm/app/prismscrolldm/id1408372017)
- Improviso e tabelas: [Lazy GM's Resource Document](https://slyflourish.com/lazy_gm_resource_document.html), [Gerador de nomes (Sly Flourish)](https://slyflourish.com/random_name_generator.html), [Página de nomes (Sly Flourish)](https://slyflourish.com/names.html), [A ferramenta mais valiosa: uma lista de nomes](https://www.goodreads.com/author_blog_posts/20291929-the-most-valuable-dm-tool-a-good-list-of-names), [rpg-table-randomizer](https://github.com/derikb/rpg-table-randomizer), [Oracles and Tables](https://dragonsarereal.bearblog.dev/oracles-and-tables/), [emacs-rpgdm](https://git.tfcconnection.org/chris/emacs-rpgdm/raw/branch/main/README.org), [Word Mill Games (Mythic)](https://www.wordmillgames.com/)
- Caderno e wikis: [Kanka: recursos](https://kanka.io/features), [LegendKeeper: alternativas ao World Anvil](https://www.legendkeeper.com/best-world-anvil-alternatives/), [StartPlaying: apps de lore](https://startplaying.games/blog/posts/the-top-lore-tracking-apps-for-game-masters)
- O que atrapalha os mestres: [r/AskGameMasters](https://reddit.birdcat.cafe/r/AskGameMasters/comments/1luqabp/gms_what_would_actually_help_you_prep_and_run), [EN World: pesquisa com mestres](https://www.enworld.org/threads/gm-survey-your-prep-tools-pain-points-3-minutes.717863)
- Lazy DM: [Os 8 passos (2023)](https://slyflourish.com/eight_steps_2023.html), [Modelo de preparo](https://slyflourish.com/rotldm_template.html)
- Dados: [@dice-roller/rpg-dice-roller](https://npmjs.com/package/@dice-roller/rpg-dice-roller)
- Hospedagem: [Flavio Copes: free tiers (set/2026)](https://flaviocopes.com/hosting-free-tiers.md), [Render: free tiers 2026](https://render-www.onrender.com/articles/platforms-with-a-real-free-tier-for-developers-in-2026)
- LGPD: [gov.br: LGPD](https://www.gov.br/pt-br/temas/lgpd), [Guia de requisitos mínimos (PPSI)](https://www.gov.br/governodigital/pt-br/privacidade-e-seguranca/ppsi/guia_requisitos_minimos_web.pdf)
