# DMAegis — Pesquisa e roteiro de desenvolvimento

> Documento de partida para o projeto. Escrito em 17/11/2026, a partir de pesquisa na web.
> Revisão 3: o app é um **escudo do mestre digital** para **D&D 5e (regras de 2024)** e **Daggerheart**, ligado a um caderno de campanha.
> Nome definido em 17/11/2026: **DMAegis** (no código: `DMAegis`). A pasta ainda se chama `AuxiliarDoMestre`.

---

## 1. A ideia

Um site em português que faz o papel do **escudo do mestre**: a tela que fica na frente do mestre durante a sessão, com tudo organizado para **lembrar** e **improvisar** sem parar o jogo. Por trás do escudo há um **caderno virtual** onde fica guardada toda a informação da campanha.

Cada campanha é de **D&D 5e** ou de **Daggerheart**, e o escudo se adapta ao sistema escolhido.

| Parte | Para quê | Igual nos dois sistemas? |
| --- | --- | --- |
| **Caderno** | Guardar NPCs, locais, facções, itens, missões, segredos e o histórico das sessões | Sim. Só os atributos das criaturas mudam. |
| **Improviso** | Tabelas aleatórias, oráculo de sim/não, nomes, NPC rápido | Sim |
| **Escudo** | A tela da sessão: preparo, busca rápida, anotações e os painéis de regra | **Não**: cada sistema tem os próprios painéis (seção 3). |

**O que liga as partes:** tudo o que é improvisado no escudo pode ser **salvo no caderno com um clique**. O NPC inventado na hora vira um registro e fica ligado à sessão.

**Não é uma mesa virtual.** Mapa com grid, tokens e ficha dos jogadores ficam de fora (Foundry, Roll20, Demiplane e Owlbear já fazem isso). É uma ferramenta **só do mestre**.

## 2. O que a pesquisa mostrou

### 2.1 As dores dos mestres durante a sessão

- **Abas demais:** o mestre fica alternando entre muitas abas e ferramentas.
- **Esquecer informação:** esquece o nome de um NPC no meio da sessão ou perde tempo procurando nas anotações.
- **Parar o jogo para consultar:** procurar uma regra no livro quebra o ritmo.
- **Cansaço de ferramenta:** configurar mesa virtual e preparar sessão vira um "segundo hobby".

Atenção: boa parte dessas fontes vem de quem está criando ou vendendo ferramenta.

### 2.2 Escudos digitais que já existem

| Ferramenta | O que aproveitar |
| --- | --- |
| **Game Master Screen** (plugin do Obsidian) | Telas em abas com **blocos** que se movem, se redimensionam, se recolhem e podem ser **travados**. Blocos de dados, notas, tabelas aleatórias, relógios e trilha sonora. É o modelo mais próximo. |
| **Virtual GM Screen** (Android, Pathfinder 2e) | Painéis **ligados entre si**: abrir uma cena carrega as informações dela. |
| **Overseer Studio** (desktop) | Funciona **offline**. |
| **PrismScrollDM** (celular, D&D 5e) | Mostrar e esconder blocos para limpar a tela. |
| **Fear Tracker** (módulo do Foundry) | Contador de Medo do Daggerheart: mostra que essa mecânica precisa de um painel próprio. |
| **Escudos em PDF de Daggerheart** (fãs, incluindo um brasileiro no itch.io) | O que os mestres querem ter à vista: regras de Medo, resultados das rolagens, contagens regressivas, custos de adversários. |
| **Kanka, LegendKeeper, World Anvil** | Wikis de campanha (o "caderno"). O Kanka tem atributos livres por registro. |

**Conclusão:** não encontrei nenhum app em português que junte escudo, improviso e caderno, nem um que atenda D&D e Daggerheart no mesmo lugar.

### 2.3 Ferramentas de improviso

- **Oráculo de sim/não** com a escala **"Não, e…" / "Não" / "Não, mas…" / "Sim, mas…" / "Sim" / "Sim, e…"**: a resposta traz uma complicação ou um bônus em vez de um sim seco. A ideia vem das regras do FU e é genérica, então pode ser escrita com texto próprio.
- **Tabelas aleatórias** (NPCs, tavernas, eventos, clima, complicações), que ajudam o mestre a fugir da primeira ideia.
- **Lista de nomes:** Sly Flourish chama uma boa lista de nomes de "a ferramenta mais valiosa do mestre".
- **Tabelas que chamam outras**, como "O taverneiro {nome} parece {humor}". O projeto open source `rpg-table-randomizer` (derikb) serve de referência.

### 2.4 Um método de preparo: os 8 passos do "Lazy DM"

O livro *Return of the Lazy Dungeon Master* (Sly Flourish) divide o preparo em 8 passos opcionais, feitos para ajudar a improvisar. Serve para os dois sistemas e é a base do painel "Sessão de hoje":

1. **Personagens**. 2. **Começo forte**. 3. **Cenas**. 4. **Segredos e pistas** (o passo mais importante, segundo o autor). 5. **Locais fantásticos**. 6. **NPCs importantes**. 7. **Monstros/adversários possíveis**. 8. **Tesouros possíveis**.

No escudo, cada segredo tem um botão **"revelado"**. Os que não forem revelados voltam sozinhos para a próxima sessão.

## 3. O que cada sistema precisa no escudo

> Os números abaixo vieram em parte de SRDs espelhados por fãs e de escudos não oficiais. **Confira cada regra no SRD oficial** antes de programar.

### 3.1 D&D 5e (regras de 2024 / SRD 5.2.1)

| Painel | O que mostra |
| --- | --- |
| **Iniciativa** | Ordem fixa por rolagem de d20, rodada atual, PV, PV temporário, CA, condições e concentração de cada criatura. |
| **Condições** | Cartões com o efeito de cada condição do SRD (agarrado, amedrontado, caído, envenenado, exaustão etc.). |
| **Classes de Dificuldade** | A tabela de CD do SRD (muito fácil a quase impossível). |
| **Montar encontro** | Orçamento de XP por personagem nas regras de 2024 (**Baixo / Moderado / Alto**, sem multiplicador de grupo) e XP por Nível de Desafio. Plugins do Obsidian e do Foundry já fazem esse cálculo a partir do SRD 5.2.1. |
| **Monstros** | Bloco de estatísticas do SRD, importado da API Open5e, ou criado pelo mestre. |
| **Dados** | `1d20+5`, vantagem e desvantagem (`2d20kh1` / `2d20kl1`), dano. |

### 3.2 Daggerheart

O Daggerheart funciona de outro jeito: **não há iniciativa fixa**. O "holofote" passa entre os jogadores, e o mestre age quando um jogador falha ou rola **com Medo**.

| Painel | O que mostra |
| --- | --- |
| **Medo do mestre** | Contador de fichas de Medo, que sobe quando um jogador rola "com Medo". Um guia de fãs cita o máximo de **10**: confirmar no SRD. Um botão "gastar" registra no que o Medo foi usado (movimento extra, adversário extra, habilidade de Medo, interrupção). |
| **Dados de dualidade** | Dois d12 (Esperança e Medo) somados mais o modificador, contra uma **Dificuldade**. Os resultados possíveis: sucesso ou falha, com Esperança ou com Medo. **Dados iguais = crítico**. Vantagem e desvantagem usam um **d6** a mais. O app mostra o resultado já interpretado e soma +1 Medo automaticamente. |
| **Adversários** | Ficha com **Dificuldade, limiares de dano (Maior / Grave), PV e Estresse** para marcar, além do tipo (lacaio, horda, líder, bruto, solo…). |
| **Montar encontro** | **Pontos de Batalha**. Um escudo de fãs cita orçamento de 3 × número de PJs + 2, e custos de 1 (lacaios, social/suporte) a 5 (solo): **confirmar no livro oficial**. |
| **Contagens regressivas** | Relógios que descem a cada gatilho até uma consequência. |
| **Ambientes** | Cartões de ambiente com as habilidades que gastam Medo. |
| **Holofote** | Uma lista simples de quem já teve a vez na cena, para o mestre distribuir o holofote. |

### 3.3 Português e nomes dos termos

- **D&D:** a Wizards anunciou o fim das traduções para o português em fevereiro de 2024. Há notícias de que a Galápagos/Asmodee trouxe a edição de 2024, mas as fontes se contradizem. O SRD 5.2.1 é em inglês, então **a tradução dos termos será nossa**. O app pode mostrar o termo em inglês ao lado (por exemplo, "Agarrado (*Grappled*)"), o que ajuda quem joga com material em inglês.
- **Daggerheart:** a **Jambô Editora** é a editora oficial no Brasil. **Use os termos da edição da Jambô** (confirme no livro). Isso evita confundir quem joga com o livro em português.

## 4. Licenças (importante)

| Fonte | Licença | Uso no app |
| --- | --- | --- |
| **SRD 5.2.1 de D&D** | CC-BY-4.0, irrevogável | **Pode usar e traduzir**, com crédito: *"Dungeons & Dragons SRD 5.2 by Wizards of the Coast LLC is licensed under CC BY 4.0."* A CC-BY permite adaptações (traduções) desde que se dê o crédito e se indique que houve mudança. Cobre só o SRD, não a marca D&D. Confira a frase exata no PDF oficial. |
| **API Open5e** | Gratuita, sem chave | Monstros e magias do SRD. Melhor importar para o próprio banco. |
| **SRD do Daggerheart** | **Darrington Press Community Gaming License (DPCGL)**, versão 2.0 de 25/08/2026 | **Atenção, ver abaixo.** |
| **Lazy GM's Resource Document** | CC-BY 4.0 | Pode usar nas tabelas iniciais, com a frase de crédito que o documento pede. |
| **Gerador de nomes do Sly Flourish** (o script) | CC BY | Pode usar, com crédito. A *página* de nomes é CC BY-NC: evitar. |
| **Mythic GM Emulator** | Produto comercial | Não copiar as tabelas. |

### O caso do Daggerheart

O que a DPCGL diz, segundo a página oficial e o FAQ:

- **Permite** criar e vender conteúdo baseado nas regras do Daggerheart, sem royalties, em **formatos permitidos**. Exige frases de crédito e o logo de conteúdo da comunidade.
- **Mesas virtuais** entraram como formato permitido em junho de 2025, mas só as de uma **lista aprovada** (Roll20, Demiplane, Foundry, Alchemy e Fantasy Grounds, em 30/07/2025). Outras precisam pedir por e-mail.
- **Não diz nada específico sobre apps ou sites**, e o FAQ manda perguntar em `info@darringtonpress.com`.
- **Proíbe** usar logos, arte, personagens e lugares da Darrington Press ou da Critical Role, e reivindicar como seus o sistema e o modelo de bloco de estatísticas. "Daggerheart™ Compatible" só pode aparecer de forma discreta e nunca no título.

**Recomendação:**

1. **Programar as mecânicas como ferramentas** (contador de Medo, dados de dualidade, contagens regressivas, ficha de adversário em branco), com texto próprio. São ferramentas para o mestre usar, não uma cópia do livro.
2. **Não pré-carregar adversários, ambientes nem textos do SRD do Daggerheart** até ter uma resposta da Darrington Press. O mestre cadastra os adversários que usa.
3. **Antes de publicar o site**, mandar um e-mail para a Darrington Press descrevendo o app. Para uso pessoal e no curso, os itens 1 e 2 bastam.
4. Não usar o nome "Daggerheart" no nome do app.

Isto é um resumo da pesquisa, não uma orientação jurídica.

## 5. Decisões que você precisa tomar

| # | Decisão | Opções | Recomendação |
| --- | --- | --- | --- |
| 1 | **Stack** | (a) a mesma do RetroGames: .NET 10 + MySQL + React/Vite/Bootstrap; (b) outra | **(a)**: você já conhece. |
| 2 | **É trabalho do curso?** | Sim / Não | Se for, as regras do professor (AGENTS.md) valem aqui. |
| 3 | **Qual sistema primeiro** | D&D 5e / Daggerheart | **D&D primeiro**: a licença é clara, e o conteúdo do SRD pode vir pronto. O Daggerheart entra logo depois, com o contador de Medo e os dados de dualidade, que são simples e marcantes. |
| 4 | **D&D 2014 ou 2024** | 2024 / os dois | **2024 (SRD 5.2.1).** Muitas mesas ainda jogam 2014. No escudo, as diferenças principais são a exaustão e o cálculo de encontro. Dá para deixar os cartões de condição editáveis. |
| 5 | **Funcionar offline** | Desde o início / depois | **Depois** (fase 4). |
| 6 | **Onde roda a aleatoriedade** | Navegador / servidor | **Navegador**: só o mestre vê, e funciona mesmo se a internet cair. |

## 6. Funcionalidades por fase

### Fase 1 — MVP: caderno + escudo básico (comum aos dois sistemas)

1. **Conta e login** (cookie com proteção CSRF, senha guardada só como hash).
2. **Campanhas** com o **sistema escolhido** (D&D 5e 2024 ou Daggerheart).
3. **Registros**: NPCs, Personagens dos jogadores, Criaturas/Adversários, Locais, Facções, Itens e Missões. Cada um tem nome, resumo de uma linha, descrição, tags e atributos. **Os atributos de Criatura vêm de um modelo do sistema** (D&D: CA, PV, ND, deslocamento; Daggerheart: Dificuldade, limiares Maior/Grave, PV, Estresse, tipo).
4. **Sessões** com o preparo nos 8 passos e **segredos** com status.
5. **Escudo v1**: painel "Sessão de hoje", **busca rápida (Ctrl+K)**, **anotações com horário** e **rolador de dados**.

### Fase 2 — Improviso (comum aos dois sistemas)

1. **Tabelas aleatórias próprias** com pesos e tabelas aninhadas `{…}`.
2. **Oráculo de sim/não** com probabilidade ajustável.
3. **NPC rápido** e **"salvar no caderno"**.
4. **Pacote inicial de tabelas em português**, de fantasia medieval, que serve aos dois sistemas.

### Fase 3 — Módulos de sistema

**3a. Módulo D&D 5e**
1. Rastreador de iniciativa (ordem, rodada, PV, CA, condições, concentração).
2. Cartões de condições e tabela de CD, traduzidos do SRD.
3. Importar monstros do SRD (Open5e) para o caderno.
4. Montar encontro com orçamento de XP de 2024.

**3b. Módulo Daggerheart**
1. Contador de **Medo** com histórico de gastos.
2. **Dados de dualidade** com o resultado interpretado (+1 Medo automático).
3. Rastreador de **adversários** (PV, Estresse, limiares) e lista de **holofote**.
4. **Contagens regressivas**.
5. Montar encontro por **Pontos de Batalha**.

### Fase 4 — Escudo completo

1. **Layout em painéis configuráveis**: arrastar, redimensionar, recolher, travar, mais de uma aba ("Exploração", "Combate").
2. **Cartões de referência** escritos pelo mestre (as próprias "colas").
3. **Modo escuro e tela cheia**.
4. **Offline (PWA)**: a sessão de hoje e as tabelas em cache, e as anotações sincronizam quando a internet voltar.

### Fase 5 — Extras

- Exportar a campanha em Markdown ou PDF.
- Ferramentas de segurança (linhas e véus, carta X) na sessão zero. O guia de sessão zero do próprio Daggerheart traz esse tema.
- **Talvez:** um link somente leitura para os jogadores.

**Fora do escopo:** mesa virtual, ficha dos jogadores, geração por IA, outros sistemas e conteúdo do Daggerheart sem resposta da Darrington Press.

## 7. Rascunho do modelo de dados

**Caderno**
- **Usuario** — id, nome, email (único), hash da senha, data de criação.
- **Campanha** — id, id do usuário, nome, **id do sistema**, descrição, arquivada.
- **Sistema** — id, nome (D&D 5e 2024, Daggerheart). Tabela de domínio, carregada pelo script.
- **ModeloAtributo** — id do sistema, tipo de registro, chave (ex.: "Limiar Maior"), ordem. Define os campos que aparecem para cada tipo em cada sistema.
- **Registro** — id, id da campanha, tipo (NPC, PJ, Criatura, Local, Facção, Item, Missão), nome, resumo, descrição, origem (criado / improvisado / importado do SRD).
- **AtributoRegistro** — id do registro, chave, valor, ordem.
- **Tag** e **RegistroTag**.
- **Sessao** — id da campanha, número, data, começo forte, resumo.
- **CenaSessao**, **Segredo**, **SessaoRegistro**, **AnotacaoSessao** (como na revisão anterior).

**Improviso**
- **TabelaAleatoria** — id, id do usuário, id da campanha (nulo = todas), nome, categoria, pacote inicial.
- **ItemTabela** — id da tabela, texto (pode ter `{OutraTabela}`), peso.

**Escudo**
- **Combate** — id da sessão, rodada atual, ativo.
- **Combatente** — id do combate, id do registro (opcional), nome, PV atual, PV máximo, ordem (D&D: iniciativa; Daggerheart: nula), Estresse (Daggerheart), condições.
- **ContadorSessao** (Daggerheart) — id da sessão, Medo atual.
- **MovimentoMedo** (Daggerheart) — id da sessão, horário, variação (+1 / −1), motivo.
- **ContagemRegressiva** (Daggerheart) — id da sessão, nome, valor atual, valor inicial, consequência.
- **LayoutEscudo** — id do usuário, id da campanha, nome da aba, painéis (JSON), travado.
- **CartaoReferencia** — id do usuário, id do sistema (nulo = todos), título, conteúdo (Markdown).

**Escolhas de modelagem a discutir:**
1. **Uma tabela `Registro` + `ModeloAtributo` por sistema** (flexível, os dois sistemas no mesmo código) ou **tabelas próprias** como `MonstroDnd` e `AdversarioDaggerheart` (mais validação, muito mais código). Recomendo a primeira.
2. **Layout do escudo como JSON**: se o projeto for do curso, confirme com o professor.

## 8. Roteiro em etapas

| Etapa | Entrega | Confira |
| --- | --- | --- |
| 0 | Repositório, `.gitignore`, solução .NET + front Vite, `.env.example`, histórias de usuário | `dotnet build` e `npm run dev` rodam |
| 1 | MER e script SQL do MVP, com as tabelas `Sistema` e `ModeloAtributo` preenchidas e uma campanha de exemplo para cada sistema | Consultas de teste retornam os dados |
| 2 | Cadastro e login (cookie + CSRF + hash) | Sem token no `localStorage` |
| 3 | CRUD de Campanha com sistema | Um usuário não vê a campanha de outro |
| 4 | CRUD de Registros com atributos vindos do modelo do sistema | Uma Criatura de Daggerheart mostra "Limiar Maior", e uma de D&D mostra "CA" |
| 5 | Sessões: 8 passos e segredos | Segredo pendente aparece na sessão seguinte |
| 6 | **Escudo v1**: painel da sessão + Ctrl+K + anotações | Conduzir uma sessão sem abrir outra aba |
| 7 | Rolador de dados (leitor de notação em JS + testes) | Testes cobrem `XdY`, `+/-`, `kh/kl` e expressão inválida |
| 8 | **Marco: MVP.** Usar numa sessão real | Lista de ajustes escrita |
| 9 | Tabelas aleatórias com pesos e aninhadas | Uma referência circular não trava o app |
| 10 | Oráculo + NPC rápido + "salvar no caderno" + pacote de tabelas com créditos | O NPC salvo aparece na busca |
| 11 | **D&D:** iniciativa, condições, CD | Rodar um combate completo no escudo |
| 12 | **D&D:** importação do SRD (Open5e) + montar encontro por XP | O orçamento confere com a tabela do SRD 5.2.1 |
| 13 | **Daggerheart:** Medo + dados de dualidade | Testes cobrem os 5 resultados (sucesso/falha × Esperança/Medo, e crítico) |
| 14 | **Daggerheart:** adversários, holofote, contagens, Pontos de Batalha | Rodar uma cena de conflito completa |
| 15 | Layout configurável e travável + cartões de referência | O layout volta igual ao recarregar |
| 16 | E-mail para a Darrington Press, deploy e política de privacidade | Página de créditos com as frases das licenças |
| 17 | PWA / offline | Desligar a internet e continuar anotando |

## 9. Tecnologias levantadas

| Necessidade | Opção | Observação |
| --- | --- | --- |
| Painéis arrastáveis | Uma biblioteca de grid para React (por exemplo, `react-grid-layout`) | **A pesquisar** na etapa 15: licença, versão e se ainda é mantida. |
| Rolagem de dados | Leitor próprio em JS, com `@dice-roller/rpg-dice-roller` (MIT, versão 5.5.1) como referência | A dualidade do Daggerheart é uma regra própria por cima do leitor: dois d12 com papéis diferentes. |
| Tabelas aleatórias | Lógica própria | `rpg-table-randomizer` como referência. |
| Monstros de D&D | API Open5e v2 (`/v2/creatures/`) | Uma fonte de terceiros diz que a Open5e cobre o SRD de 2024 melhor que a `dnd5eapi.co`. Confirme. |
| Offline | PWA (service worker + IndexedDB) | Fase 4. |

## 10. Hospedagem, LGPD e riscos

**Hospedagem:** as opções gratuitas costumam oferecer PostgreSQL, e o MySQL é o fator que limita. **Azure for Students** (sem cartão, para em vez de cobrar) é a melhor opção para a API. **Railway** tem MySQL por créditos. O **Render** "dorme" após 15 minutos sem uso, o que é ruim na hora do jogo. Front em Vercel ou Netlify. Confira os limites na hora de publicar.

**LGPD:** política de privacidade própria, com versão e data. Coletar só nome, e-mail e senha, e permitir excluir a conta. Referência: o *Guia de Requisitos Mínimos de Privacidade e Segurança para Aplicações Web* (PPSI, gov.br).

**Riscos:**
- **Escopo:** dois sistemas dobram a parte de regras. Mantenha o núcleo comum e faça um sistema de cada vez.
- **Licença do Daggerheart:** ver a seção 4. Não publique com conteúdo do SRD sem resposta.
- **Regras erradas:** vários números desta pesquisa vieram de fãs. Cada regra programada precisa ser conferida no SRD oficial e ter teste.
- **Velocidade do escudo** e **internet caindo** no meio da sessão.

## 11. Para começar o próximo chat

Abra o Claude Code **nesta pasta** (`C:\Users\gusta\Documents\AuxiliarDoMestre`) e use algo como:

> Leia `docs/ROTEIRO.md`. Minhas decisões da seção 5 são: [stack], [é/não é do curso], [D&D primeiro], [2024], [offline depois], [aleatoriedade no navegador]. Vamos começar pela etapa 0. Antes de escrever código, crie o `AGENTS.md` do projeto e as histórias de usuário do MVP.

Se o projeto for do curso, copie para cá o `AGENTS.md` do RetroGames e adapte. Tenha à mão os PDFs oficiais do **SRD 5.2.1** e do **SRD do Daggerheart** para conferir as regras.

---

## Fontes

- Daggerheart — licença: [DPCGL (página oficial)](https://darringtonpress.com/license), [FAQ do Daggerheart](https://www.daggerheart.com/faq/), [SRD do Daggerheart](https://www.daggerheart.com/srd), [DPCGL de 30/07/2025 (PDF)](https://darringtonpress.com/wp-content/uploads/2025/07/DPCGL-July-30th-2025.pdf), [Sistema Daggerheart no Foundry](https://foundryvtt.com/packages/daggerheart), [Daggerheart Brews](https://next-daggerheart.vercel.app/about), [EN World — onde baixar o SRD](https://www.enworld.org/goto/post?id=9683242)
- Daggerheart — regras: [Mecânicas centrais (SRD em Markdown)](https://nemo-illusionist.github.io/ttrpg_srd_markdown/en/daggerheart/srd-1.0/07_CoreMechanics), [Glossário (SRD em Markdown)](https://nemo-illusionist.github.io/ttrpg_srd_markdown/en/daggerheart/srd-1.0/17_Glossary/00_Glossary), [Escudo do mestre "Old Gus" (PDF de fã)](https://callmepartario.github.io/og-dhsrd/pdfs/old-gus-daggerheart-gm-screen.pdf), [Fear Tracker (Foundry)](https://foundryvtt.com/packages/fear-tracker), [Medo, adversários e holofote](https://louisbranch.github.io/fracturing.space/reference/daggerheart-playbooks/playbook-gm-fear-adversaries-and-spotlight.html), [Esperança e Medo](https://dungeonmister.com/daggerheart/hope-fear-in-daggerheart/), [Mestrando Daggerheart: primeiras impressões](https://dungeonmister.com/daggerheart/running-daggerheart-a-gms-first-impressions/), [Fichas adicionais (oficial)](https://www.daggerheart.com/wp-content/uploads/2025/05/Additional-Sheets-Daggerheart-Aug252026.pdf), [Aventura introdutória (oficial)](https://www.daggerheart.com/wp-content/uploads/2025/05/Quickstart-Adventure-5-20-2025.pdf), [Daggerheart — sessão zero](https://www.daggerheart.com/sessionzero/)
- Daggerheart no Brasil: [Onde comprar (oficial)](https://www.daggerheart.com/buy/), [Caixa brasileira (Final Boss)](https://finalboss.io/daggerheart-finally-hit-brazil-but-can-a-fancy), [Tradução não oficial do SRD (Foundry)](https://foundryvtt.com/packages/daggerheart-translation-pt-BR), [Escudo para Daggerheart (itch.io)](https://ratoruja.itch.io/escudo-para-daggerheart/purchase)
- D&D em português: [ComicBook — fim das traduções](https://comicbook.com/gaming/news/dungeons-dragons-discontinues-portuguese-translations), [EN World — D&D sem português](https://www.enworld.org/goto/post?id=9273621), [EinerD — Galápagos distribuidora](https://www.einerd.com/galapagos-asmodee-sera-a-distribuidora-oficial-de-dungeons-dragons-no-brasil/), [D&D no Brasil (Wizards)](https://dnd.wizards.com/pt-BR/news/expanding-dnd-ptbr), [Glossário UFRGS](https://www.ufrgs.br/tesauros/index.php/thesa/terms/1728)
- D&D — SRD e licença: [D&D Beyond — SRD 5.2.1](https://dndbeyond.com/srd), [PC Gamer — SRD em Creative Commons](https://www.pcgamer.com/games/board-games/the-core-of-the-new-dungeons-and-dragons-rules-have-been-released-into-the-creative-commons-so-you-can-use-them-when-you-publish-your-own-d-and-d-material), [Tribality — SRD 5.2](https://www.tribality.com/2025/04/23/dd-system-reference-document-v5-2/), [Open5e — documentação](https://open5e.com/api-docs), [Lorekeeper MCP — endpoints da Open5e](https://glama.ai/mcp/servers/@frap129/lorekeeper-mcp/blob/81520084d6a7a97c0a6173c4da67b08edc17a118/docs/apis/open5e-api.md)
- D&D — iniciativa e encontros: [Arcane Eye — rastreadores de iniciativa](https://arcaneeye.com/dm-tools-5e/best-dnd-5e-initiative-tracker/), [D&D Battle Tracker](https://github.com/Paul-Ladyman/dnd-battle-tracker), [Encounter Builder (Obsidian)](https://community.obsidian.md/plugins/encounter-builder), [Encounter Builder 2024 (Foundry)](https://foundryvtt.com/packages/encounter-builder-2024)
- Escudos digitais: [Game Master Screen (Obsidian)](https://community.obsidian.md/plugins/game-master-screen), [Virtual GM Screen](https://nothingtoseeheregaming.itch.io/virtual-gm-screen), [Overseer Studio](https://groupfinder.eu/library/overseer-studio), [PrismScrollDM](https://apps.apple.com/bm/app/prismscrolldm/id1408372017)
- Improviso e tabelas: [Lazy GM's Resource Document](https://slyflourish.com/lazy_gm_resource_document.html), [Gerador de nomes (Sly Flourish)](https://slyflourish.com/random_name_generator.html), [Página de nomes (Sly Flourish)](https://slyflourish.com/names.html), [A ferramenta mais valiosa: uma lista de nomes](https://www.goodreads.com/author_blog_posts/20291929-the-most-valuable-dm-tool-a-good-list-of-names), [rpg-table-randomizer](https://github.com/derikb/rpg-table-randomizer), [Oracles and Tables](https://dragonsarereal.bearblog.dev/oracles-and-tables/), [emacs-rpgdm](https://git.tfcconnection.org/chris/emacs-rpgdm/raw/branch/main/README.org), [Word Mill Games (Mythic)](https://www.wordmillgames.com/)
- Caderno / wikis: [Kanka — recursos](https://kanka.io/features), [LegendKeeper — alternativas ao World Anvil](https://www.legendkeeper.com/best-world-anvil-alternatives/), [StartPlaying — apps de lore](https://startplaying.games/blog/posts/the-top-lore-tracking-apps-for-game-masters)
- Dores dos mestres: [r/AskGameMasters](https://reddit.birdcat.cafe/r/AskGameMasters/comments/1luqabp/gms_what_would_actually_help_you_prep_and_run), [EN World — pesquisa com mestres](https://www.enworld.org/threads/gm-survey-your-prep-tools-pain-points-3-minutes.717863)
- Lazy DM: [Os 8 passos (2023)](https://slyflourish.com/eight_steps_2023.html), [Modelo de preparo](https://slyflourish.com/rotldm_template.html)
- Dados: [@dice-roller/rpg-dice-roller](https://npmjs.com/package/@dice-roller/rpg-dice-roller)
- Hospedagem: [Flavio Copes — free tiers (set/2026)](https://flaviocopes.com/hosting-free-tiers.md), [Render — free tiers 2026](https://render-www.onrender.com/articles/platforms-with-a-real-free-tier-for-developers-in-2026)
- LGPD: [gov.br — LGPD](https://www.gov.br/pt-br/temas/lgpd), [Guia de requisitos mínimos (PPSI)](https://www.gov.br/governodigital/pt-br/privacidade-e-seguranca/ppsi/guia_requisitos_minimos_web.pdf)
