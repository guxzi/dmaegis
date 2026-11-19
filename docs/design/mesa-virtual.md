# Design da mesa virtual

Escrevi em 19/11/2026, depois de ver como o D&D Beyond (Encounters, Scene Prep) e o Heart of Daggers (Encounter Manager, mapas com marcadores) resolvem isso.

## 1. Intenção

Vou mestrar uma campanha de D&D 5e (2024) pelo Discord, com mapa, e as sessões ainda não começaram. Meu grupo não usa mesa virtual (Roll20, Owlbear), e as outras mesas que a gente joga são teatro da mente. Quero que os jogadores vejam o mapa e as fichas no próprio navegador, sem depender da tela compartilhada.

É para mim, como mestre, e para os meus jogadores. Não é um produto público.

Vai ter dado certo quando, numa sessão real, cada jogador abrir um link, vir o mapa com as fichas mexendo em tempo real e mover o próprio personagem, enquanto eu conduzo o combate (iniciativa, PV, condições) no DMAegis, sem outra ferramenta. Tudo bonito e elegante, no tema cósmico, com transições e um CSS caprichado.

O [design do MVP](mvp.md) dizia que virar uma mesa virtual (mapa, tokens, ficha de jogador) seria um fracasso. Mudei de ideia para o mapa e as fichas no mapa; as fichas completas de jogador continuam fora.

## 2. Objetivos, não objetivos e antiobjetivos

Objetivos
- Uma página Mesa por campanha, com mapa (imagem da galeria), grade ajustável e fichas que o mestre arrasta.
- Combate na mesma tela: iniciativa, turno e rodada, PV, CA e condições.
- Visão dos jogadores em tempo real pelo navegador, por um link de convite por personagem, sem conta.
- Névoa de guerra por células da grade, que o mestre revela aos poucos.
- Acesso de fora por túnel no PC do mestre durante a sessão.
- Funciona nos dois sistemas, com PV genérico (atual/máximo). As regras próprias do Daggerheart ficam para depois.

Não objetivos (ficam para depois)
- Régua, linha de visão, iluminação dinâmica, desenho livre no mapa.
- Chat e rolagem de dados compartilhada (os jogadores rolam no Discord).
- Ficha completa de personagem.
- Monstros prontos do SRD (etapa 12) e Medo, dualidade, Pontos de Batalha (etapas 13 e 14).
- Hospedagem fixa na internet (etapa 16).
- Mais de um mestre por campanha.

Antiobjetivos (seria um fracasso mesmo funcionando)
- Um jogador ver o que não deve: PV numérico de monstros, fichas ocultas, células com névoa, anotações, segredos, o caderno ou qualquer coisa de outra campanha.
- Expor o modo de desenvolvimento (Swagger, mensagens de erro detalhadas) pela internet.
- Exigir que os jogadores criem conta ou instalem algo.
- Arrastar com atraso ou tremido. A mesa tem que parecer leve.
- Perder o estado do combate ao recarregar a página.

## 3. Restrições

- A stack das [convenções](../CONVENCOES.md), sem pacotes novos. O tempo real usa Server-Sent Events, que o .NET 10 (`TypedResults.ServerSentEvents`) e o navegador (`EventSource`) já trazem. Pan, zoom e arrasto são feitos com pointer events e CSS transform.
- Camadas, nomes, prefixos do banco, envelope, 404 para campanha de outro usuário e CSRF continuam valendo. A estrutura nova entra em `database/06_mesa.sql` (e um `07` para a névoa, se for separado).
- A identidade visual das convenções: tokens `--dm-*`, Cinzel e Alegreya Sans, animações em `estilo/movimento.css` dentro de `prefers-reduced-motion: no-preference`. Tela do mestre conferida em 1366×768. Tela do jogador conferida também em 375 px.
- Não pré-carregar conteúdo do Daggerheart. O mestre cadastra.
- O nome real dos jogadores não entra no git. Só os nomes dos personagens, e só no banco local.
- O programa do túnel (cloudflared) é instalado à parte. O app e o lançador só o usam.

## 4. Abordagens consideradas

| Abordagem | O que é | Troca |
| --- | --- | --- |
| A. Só combate no escudo | Iniciativa, PV e condições; a projeção mostra a ordem dos turnos | Pequena, mas sem mapa, e a campanha vai ter mapa |
| B. Mesa com mapa na tela do mestre | Mapa, grade, fichas e combate; a projeção local mostra o mapa | Reaproveita galeria e projeção, mas os jogadores dependem da tela compartilhada |
| C. Mesa virtual (escolhida) | B + jogadores pelo navegador em tempo real + névoa | A maior. A dificuldade de verdade é deixar os amigos alcançarem o PC (resolvida com túnel) |

Escolhi a C, em três entregas, com os jogadores entrando por um túnel no meu PC, porque o compartilhamento de tela do Discord não atende o grupo.

## 5. Entregas

Cada entrega funciona sozinha e termina com testes, build e conferência no navegador.

1. Mesa do mestre: mapa, grade, fichas, combate e a visão dos jogadores na janela de projeção local.
2. Jogadores pelo link: convites, sessão de jogador, tempo real e o modo túnel.
3. Névoa de guerra.

## 6. Modelo de dados

Todas as tabelas filhas usam `ON DELETE CASCADE` a partir da campanha ou da mesa.

`tb_mesa` (entrega 1): um mapa preparado da campanha. Uma campanha pode ter várias (a ponte, a caverna) e uma delas fica ativa, a que os jogadores veem.
- `co_seq_mesa`, `co_campanha` (FK, CASCADE), `no_mesa` (VARCHAR 100), `co_imagem_registro` (FK, `ON DELETE SET NULL`: apagar a imagem não apaga a mesa)
- `nu_tamanho_celula` (px da imagem, padrão 70), `nu_deslocamento_x`, `nu_deslocamento_y` (alinhamento da grade, px), `st_grade_visivel`
- `st_ativa` (no máximo uma por campanha: o Service garante, desmarcando as outras na mesma transação)
- `st_combate`, `nu_rodada`, `co_participante_vez` (FK para o participante, `ON DELETE SET NULL`)
- `st_movimento_livre` (entrega 2: se os jogadores podem mover a própria ficha)
- `dt_criacao`, `dt_atualizacao`

`tb_participante_mesa` (entrega 1): uma ficha no mapa e uma linha na iniciativa.
- `co_seq_participante_mesa`, `co_mesa` (FK, CASCADE), `co_registro` (FK opcional para o caderno, `ON DELETE SET NULL`)
- `no_participante` (VARCHAR 100, por exemplo "Goblin 2"), `st_jogador` (é PJ)
- `nu_iniciativa` (nulo antes de rolar), `nu_bonus_iniciativa`, `nu_ca` (nulo), `nu_pv_atual`, `nu_pv_maximo` (nulos para quem não tem PV)
- `nu_coluna`, `nu_linha` (nulos = fora do mapa, só na lista), `nu_tamanho` (1 a 4 células)
- `st_visivel` (oculta para os jogadores), `nu_ordem` (desempate manual)

`tb_condicao_participante` (entrega 1): `co_participante` (FK, CASCADE), `no_condicao` (VARCHAR 40), `UNIQUE (co_participante, no_condicao)`.

`tb_convite_jogador` (entrega 2): um link por personagem.
- `co_seq_convite_jogador`, `co_campanha` (FK, CASCADE), `co_registro` (o PJ, FK, CASCADE, `UNIQUE`)
- `ds_hash_codigo` (SHA-256 do código; o código em si só aparece uma vez, ao gerar), `dt_criacao`, `dt_ultimo_acesso`
- Gerar de novo troca o código e invalida o link antigo.

`tb_celula_revelada` (entrega 3): `co_mesa` (FK, CASCADE), `nu_coluna`, `nu_linha`, `UNIQUE (co_mesa, nu_coluna, nu_linha)`. Mais `st_nevoa` em `tb_mesa` para ligar ou desligar a névoa.

## 7. Entrega 1: mesa do mestre

### Página e navegação

Nova rota `/campanhas/:id/mesa`, com o item Mesa no menu da campanha, entre Escudo e Galeria. O escudo ganha o botão "Abrir mesa".

```
┌ Cabecalho ─────────────────────────────────────────────────────────────────┐
│ Mesa: [A ponte ▾] [+ nova]   Mapa: [escolher da galeria]   Grade ⚙   ◉ Ativa │
├──────────────────────────────────────────────────────┬─────────────────────┤
│                                                      │ COMBATE · Rodada 2  │
│        mapa com grade, fichas redondas,              │ ▶ 18 Lilica  ♥24/24 │
│        pan com arrasto no fundo, zoom na roda        │   15 Goblin 1 ♥ 7/7 │
│                                                      │   12 Goblin 2 ◌ oc. │
│   (L) (G1)                                           │ [Próximo turno →]   │
│          (G2)                                        │ + Adicionar         │
│                                       [−][100%][+]   │ ficha selecionada…  │
└──────────────────────────────────────────────────────┴─────────────────────┘
```

### Mapa e grade

- O palco mostra a imagem no tamanho natural dentro de um contêiner com `transform: translate() scale()`. Roda do mouse dá zoom em direção ao cursor (25% a 400%), arrastar o fundo move o mapa, e um botão "centralizar" encaixa o mapa na tela.
- A grade é um SVG com `pattern` por cima da imagem, alinhada por `nu_tamanho_celula` e pelos deslocamentos. O painel "Grade" tem controles deslizantes com prévia ao vivo e salva ao soltar.
- Sem imagem, a mesa mostra um campo estrelado com a grade, bom para teatro da mente com posições.

### Fichas

- Ficha redonda com o retrato do registro (a primeira imagem, como já faz `retratosPorRegistro`) ou, sem retrato, o escudo com a inicial no tom do tipo (`COR_TIPO`). É o placeholder dos PJs que ainda não têm imagem.
- Anel colorido: PJ em cobalto, monstro em vermelho suave, NPC no tom do tipo. A ficha da vez ganha um halo que gira devagar. Ficha oculta fica semitransparente e tracejada, só para o mestre.
- Arrastar encaixa na célula mais próxima. Durante o arrasto a ficha segue o cursor sem atraso, com uma sombra no destino. Ao soltar, grava a posição.
- Clicar seleciona a ficha e abre o painel dela embaixo da lista: PV com campo de dano/cura (digitar `7` e escolher Dano ou Cura), CA, condições, ocultar ou mostrar, tirar do mapa, remover.

### Combate

- Adicionar participantes: "Todos os PJs" (os registros do tipo PJ da campanha); do caderno, com quantidade ("Goblin × 3" vira Goblin 1, 2 e 3, com CA e PV tirados dos atributos `CA` e `PV` do registro, quando existirem); ou avulso (só nome).
- Iniciar combate: os monstros e NPCs rolam d20 + bônus no navegador (`crypto.getRandomValues`, reaproveitando o rolador). Os PJs ficam com o campo de iniciativa aberto, para o mestre digitar o que os jogadores rolaram no Discord. A ordem é decrescente, com desempate por bônus e depois por `nu_ordem`.
- Próximo turno: passa a vez, pulando quem está oculto e a 0 PV, e soma a rodada ao dar a volta. Teclado: `N` próximo turno, `Esc` tira a seleção.
- Condições: as 15 do D&D 2024, com os nomes de `REGRAS_DND`. No Daggerheart, Vulnerável, Contido e Oculto, com texto próprio. Aparecem como pequenos selos na ficha e na lista, com a explicação da regra no `title`.
- Encerrar combate: zera rodada e vez e mantém as fichas no mapa.

### Visão dos jogadores (componente único)

- `VisaoJogador` mostra o mapa, as fichas visíveis, a ordem dos turnos (nomes, retratos e quem é a vez) e o estado de saúde dos monstros em palavras: Ileso, Ferido (menos que o máximo), Sangrando (metade ou menos) e Caído (0). Os PJs mostram o PV em números.
- Na entrega 1 esse componente aparece na janela de projeção (`/projecao`) quando o mestre clica em "Mostrar aos jogadores" na mesa. A projeção passa a ter dois modos: imagem (já existe) e mesa.
- O componente recebe um estado já filtrado, nunca o estado do mestre. A filtragem é a mesma da entrega 2 (seção 8).

### API (entrega 1)

Todas exigem login e conferem o dono da campanha. nas rotas.
- `GET/POST /api/campanhas/{idCampanha}/mesas`, `GET/PUT/DELETE /api/mesas/{id}`
- `PUT /api/mesas/{id}/ativa`
- `POST /api/mesas/{id}/participantes` (aceita lista: um registro com quantidade, ou avulso), `PUT/DELETE /api/participantes/{id}`
- `PUT /api/participantes/{id}/posicao` (só coluna e linha, para o arrasto ser leve)
- `PUT /api/participantes/{id}/condicoes` (lista completa)
- `POST /api/mesas/{id}/combate` (iniciar, com as iniciativas), `POST /api/mesas/{id}/combate/proximo`, `DELETE /api/mesas/{id}/combate`
- `GET /api/mesas/{id}/visao-jogador` (o estado filtrado, para a projeção local)

Novos erros de negócio a partir do código 17 (por exemplo, mesa não encontrada, participante não encontrado, imagem de outra campanha).

## 8. Entrega 2: jogadores pelo link

### Convites

- No painel da mesa, a aba Jogadores lista os PJs da campanha, cada um com "Gerar link" ou "Copiar link" e "Trocar link".
- O código tem 32 bytes aleatórios (`RandomNumberGenerator`) em Base64Url. O banco guarda só o hash.
- O link é `{endereço da mesa}/convite/{codigo}`. O "endereço da mesa" é um campo no painel, preenchido com `window.location.origin` e editável, porque o endereço do túnel rápido muda a cada sessão.

### Sessão do jogador

- `/convite/{codigo}` é uma página do site que faz `POST /api/jogador/entrar` com o código. Se ele for válido, a API grava um cookie próprio, `dmaegis.jogador` (HttpOnly, SameSite=Strict, Secure atrás do túnel), com a campanha e o PJ, e manda para `/jogar`.
- O cookie de jogador é um segundo esquema de autenticação. Ele só abre as rotas `/api/jogador/*`. Todas as outras rotas continuam exigindo o cookie do mestre.
- O jogador não vê menu, caderno nem nada além de `/jogar`.

### Rotas do jogador

- `GET /api/jogador/mesa`: o estado filtrado da mesa ativa da campanha do convite.
- `PUT /api/jogador/ficha/posicao`: move a própria ficha, se `st_movimento_livre` estiver ligado e a ficha estiver no mapa.
- `GET /api/jogador/imagens/{id}`: só serve o mapa da mesa ativa e os retratos das fichas visíveis.
- `GET /api/jogador/eventos`: SSE (seção abaixo).

### Filtragem (o coração da segurança)

Uma função do Service monta o estado do jogador a partir do estado do mestre e é a única que as rotas de jogador e a projeção usam:
- Remove fichas com `st_visivel = falso` (e, na entrega 3, fichas em célula sob névoa).
- Troca PV e CA de quem não é PJ pelo estado em palavras. PJs mantêm PV e CA.
- Não inclui anotações, segredos, atributos do caderno, `co_registro` de quem não é PJ, nem outras mesas.
- Marca qual ficha é do jogador logado (`ehMinha`).

Os testes cobrem cada regra com estados montados à mão e testes de integração das rotas `/api/jogador/*`.

### Tempo real

- Um serviço singleton (`CanalMesa`) guarda, por campanha, os ouvintes conectados. Todo comando que grava algo da mesa avisa o canal depois de gravar.
- O evento carrega só `{ "tipo": "mesa", "versao": n }`. Cada tela busca de novo o próprio estado com as próprias permissões. Assim nada sensível passa pelo canal, e um único caminho de filtragem continua valendo.
- O mestre também ouve (`GET /api/mesas/{id}/eventos`), para ver o jogador mover a ficha.
- O `EventSource` se reconecta sozinho. Ao reconectar, a tela busca o estado de novo. Um aviso discreto "reconectando…" aparece se a conexão cair.
- As fichas deslizam até a posição nova com `transition: transform 0.35s var(--dm-ease)`, para o movimento dos outros parecer suave.

### Modo túnel

- A API passa a servir o site já compilado (`dmaegis.web/dist`) com `UseStaticFiles` e fallback para `index.html`, para o túnel apontar para uma única origem (`http://localhost:5080`).
- O modo mesa roda com `ASPNETCORE_ENVIRONMENT=Production`: sem Swagger e sem erros detalhados, e com cookies `Secure`. `UseForwardedHeaders` faz a API enxergar o https do túnel.
- Um novo `Abrir mesa para os jogadores.cmd` compila o site, sobe a API em modo mesa e roda `cloudflared tunnel --url http://localhost:5080`, mostrando o endereço gerado. Se o `cloudflared` não estiver instalado, o lançador explica como instalar e para.
- O README ganha a seção "Jogar com os amigos".

## 9. Entrega 3: névoa de guerra

- `st_nevoa` liga a névoa na mesa. Com ela ligada, os jogadores só veem as células reveladas. O resto fica coberto por uma névoa escura com estrelas tênues e borda suave (máscara SVG com desfoque).
- O mestre vê a névoa como um véu violeta translúcido e tem a ferramenta Revelar/Esconder: pincel por célula com arrasto, e retângulo com Shift. Salva ao soltar o mouse, em lote (`PUT /api/mesas/{id}/nevoa` com as células adicionadas e removidas).
- Fichas de monstros em células cobertas não vão para os jogadores, nem visíveis. Os PJs sempre aparecem.
- A célula revelada aparece para os jogadores com um esmaecer suave.

## 10. Interface e acabamento

- O palco do mapa tem vinheta escura nas bordas e o céu estrelado aparece onde não há mapa.
- O painel de combate usa `.cartao-astral`. As linhas da iniciativa usam `.selo-escudo` para o número, e a linha da vez tem um brilho que desliza.
- A barra de PV tem gradiente que vai do cobalto ao vermelho conforme cai, com transição na largura. Dano faz a ficha piscar em vermelho, e cura, em verde-água.
- Os selos de condição são pequenos, com ícones próprios em SVG, ao lado do nome.
- A tela do jogador é escura e limpa: o mapa ocupa tudo, a ordem dos turnos fica numa faixa no topo, e o cartão do próprio personagem fica num canto. Quando chega a vez dele, aparece "É a sua vez" com um brilho que dura dois segundos.
- As animações novas entram em `movimento.css`. Quem pede menos movimento vê tudo parado, mas funcionando.

## 11. Testes e verificação

- xUnit + Moq: dono da campanha em todos os comandos; uma mesa ativa por campanha; próximo turno (pula oculto e caído, soma rodada); a filtragem do jogador; convite (hash, troca invalida o anterior); o jogador só move a própria ficha e só com movimento livre; a imagem do jogador só sai se for o mapa ou um retrato visível.
- Integração (WebApplicationFactory): as rotas `/api/jogador/*` com cookie de jogador recusam as rotas do mestre, e as do mestre recusam o cookie de jogador.
- Vitest: encaixe na grade, ordem da iniciativa com desempates, estado de saúde em palavras, cálculo de zoom em direção ao cursor e, na entrega 3, o pincel da névoa.
- Navegador: a mesa em 1366×768, a visão do jogador em 375 px e em tela cheia, e dois navegadores ao mesmo tempo (mestre e jogador) para o tempo real.

## 12. O que deixei em aberto

- A estrutura exata dos componentes React e dos arquivos CSS (seguindo os vizinhos).
- O formato dos JSON de cada rota, dentro do padrão `{Ação}{Entidade}{Request|Response}ApiModel`.
- Os ícones das condições, desde que sejam desenho próprio.
- A divisão das entregas em tarefas no plano.
