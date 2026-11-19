# Design do MVP (fase 1)

Escrevi em 17/11/2026, a partir do [roteiro](../ROTEIRO.md). Aqui estão a arquitetura, as convenções e a fase 1 (etapas 0 a 8 do roteiro). As fases seguintes ganham um design próprio quando chegarem.

## 1. Intenção

O DMAegis é um escudo do mestre digital em português, de uso pessoal, para conduzir sessões de D&D 5e (2024) e Daggerheart sem parar o jogo para procurar coisas. O MVP entrega a base comum aos dois sistemas: o caderno da campanha, o preparo da sessão pelos 8 passos do Lazy DM e uma primeira versão do escudo, a tela usada durante a sessão.

Sucesso do MVP: conduzir uma sessão real usando só o escudo, sem abrir outra aba (etapa 8 do roteiro), e sair dela com uma lista de ajustes.

## 2. Objetivos, não objetivos e antiobjetivos

### Objetivos

- Conta com login seguro (cookie + CSRF + hash da senha).
- Campanhas de D&D 5e ou de Daggerheart.
- Registros (NPC, PJ, Criatura, Local, Facção, Item, Missão) com tags e atributos. Os atributos de Criatura vêm do modelo do sistema da campanha.
- Sessões com os 8 passos e segredos que passam sozinhos para a sessão seguinte.
- Escudo v1: sessão de hoje, busca rápida (Ctrl+K), anotações com horário e rolador de dados.

### Fica para as próximas fases

- Tabelas aleatórias, oráculo e NPC rápido (fase 2).
- Iniciativa, condições, Medo, dualidade, contagens regressivas e encontros (fase 3).
- Painéis arrastáveis, cartões de referência, modo escuro e offline (fase 4).
- Exclusão de conta, política de privacidade e deploy (etapa 16).

### Seria um fracasso mesmo funcionando

- O escudo ficar lento ou exigir muitos cliques durante a sessão.
- Virar uma mesa virtual (mapa, tokens, ficha de jogador). Mudei de ideia sobre o mapa em 19/11/2026; veja [mesa-virtual.md](mesa-virtual.md).
- Incluir conteúdo do SRD do Daggerheart (ver seção 4 do roteiro).
- Um usuário conseguir ver dados da campanha de outro.

## 3. Restrições

- É um projeto pessoal, então as regras do curso que segui no RetroGames não valem aqui. Posso usar JOIN, subconsulta e restrições no banco.
- Máquina: Windows 11, .NET SDK 10.0.403, Node 22, MySQL 8.0 (serviço `MySQL80`, fora do PATH), Git.
- Todo texto da interface em português. Termos de D&D com o inglês ao lado quando ajudar. Termos de Daggerheart seguem a edição da Jambô e precisam ser conferidos no livro.

## 4. Abordagens consideradas (acesso ao banco)

| | Abordagem | Resultado |
| --- | --- | --- |
| A | ADO.NET puro, como no RetroGames | Descartada: muito código de mapeamento repetido para cerca de 15 tabelas. |
| B | Dapper + MySqlConnector | Escolhida. O SQL continua escrito à mão e visível, e o Dapper preenche os objetos. Mantém o modo de pensar do RetroGames com cerca de 1/3 do código no Repository. |
| C | Entity Framework Core | Descartada: esconde o SQL e exige muito aprendizado novo. |

## 5. Stack e estrutura

| Parte | Escolha |
| --- | --- |
| API | .NET 10, ASP.NET Core com controllers, FluentValidation, DotNetEnv, Swagger UI |
| Dados | MySQL 8, Dapper, MySqlConnector, scripts SQL numerados em `database/` |
| Testes da API | xUnit v3 + Moq (Services e validators) |
| Front | React + Vite + Bootstrap + Axios + React Router, em JavaScript |
| Testes do front | Vitest, para o rolador de dados e outras funções puras |

A estrutura é a mesma do RetroGames, com o nome `DMAegis`:

```
AuxiliarDoMestre/
├── DMAegis.slnx
├── DMAegis.Model/            # uma classe por tabela
├── DMAegis.Repository/       # SQL com Dapper, um por tabela
├── DMAegis.Services/         # regras de negócio
├── DMAegis.WebApi/           # controllers, mappers, middlewares
├── DMAegis.WebApi.ApiModels/ # requests e responses
├── DMAegis.UnitTests/
├── dmaegis.web/                      # React
├── database/                        # 01_criar_tabelas.sql, 02_dados_iniciais.sql…
├── docs/                            # roteiro, MER, histórias de usuário, designs, convenções
├── .env.example
└── .gitignore
```

## 6. Convenções (herdadas do RetroGames, com folga)

As convenções completas estão em [`CONVENCOES.md`](../CONVENCOES.md).

Mantidas:
- Camadas Controller → Service → Repository, com as mesmas responsabilidades.
- Nomes em português; classes com o nome da tabela sem `tb_`; `Id` e `Id` + entidade nas chaves; ApiModels `{Ação}{Entidade}{Request|Response}ApiModel`.
- Banco com o prefixo `tb_` e colunas `co_seq_`, `co_`, `no_`, `ds_`, `nu_`, `dt_`, `st_`.
- Envelope `{ responseCode, data, errorMessage }`, `ResponseCodes.cs`, `BusinessRuleException` e `GlobalExceptionHandler`.
- `.env` com a connection string, fora do git; `.env.example` versionado; sem senha em `appsettings`.
- `TransactionScope` quando a operação grava mais de um comando (por exemplo, um registro com os atributos e as tags).

Liberadas por ser um projeto pessoal:
- `JOIN` no SQL quando simplifica a consulta (por exemplo, a busca por tag).
- `ON DELETE CASCADE` nas tabelas filhas que só existem por causa da tabela mãe (atributos, tags do registro, cenas, anotações, vínculos da sessão).
- `UNIQUE` composto onde fizer sentido (tag por campanha, número da sessão por campanha).

## 7. Modelo de dados do MVP

Tabelas de domínio, preenchidas pelo script:
- `tb_sistema` (1 = D&D 5e 2024, 2 = Daggerheart).
- `tb_tipo_registro` (NPC, PJ, Criatura, Local, Facção, Item, Missão).
- `tb_origem_registro` (Criado, Improvisado, Importado). No MVP só se usa "Criado", mas a coluna já existe para as fases 2 e 3.
- `tb_modelo_atributo`: sistema, tipo de registro, chave, ordem. `UNIQUE (sistema, tipo, chave)`.

Caderno:
- `tb_usuario`: nome, e-mail (`UNIQUE`), hash da senha, data de criação.
- `tb_campanha`: usuário, sistema, nome, descrição, arquivada, data de criação. O sistema não pode ser trocado depois de criada a campanha, porque os atributos dependem dele.
- `tb_registro`: campanha, tipo, origem, nome, resumo (uma linha, até 200 caracteres), descrição (texto longo), datas de criação e de atualização.
- `tb_atributo_registro`: registro, chave, valor (texto), ordem.
- `tb_tag`: campanha, nome. `UNIQUE (campanha, nome)`.
- `tb_registro_tag`: registro, tag.

Sessões:
- `tb_sessao`: campanha, número (`UNIQUE` por campanha), data (opcional), notas sobre os personagens (passo 1), começo forte (passo 2), resumo pós-sessão.
- `tb_cena_sessao`: sessão, descrição, ordem (passo 3).
- `tb_segredo`: campanha, sessão em que foi criado, sessão em que foi revelado (opcional), texto, data da revelação.
- `tb_sessao_registro`: sessão, registro (passos 5 a 8: locais, NPCs, criaturas e itens/tesouros ligados à sessão).
- `tb_anotacao_sessao`: sessão, data e hora, texto.

Como os segredos "voltam sozinhos": o segredo pertence à campanha, não à sessão. A tela de uma sessão mostra os segredos da campanha ainda não revelados, mais os que foram revelados nela mesma. Marcar "revelado" grava a sessão e a hora. Assim não se copia nada de uma sessão para a outra: o segredo continua aparecendo até ser revelado.

Passo 1 (Personagens): mostra automaticamente os registros PJ da campanha, mais o campo de notas da sessão.

Modelo inicial de atributos (Criatura): os termos de Daggerheart precisam ser conferidos no livro da Jambô:
- D&D 5e: CA, PV, Deslocamento, ND, XP.
- Daggerheart: Tipo, Patamar, Dificuldade, Limiar Maior, Limiar Grave, PV, Estresse, Ataque, Dano.

Os outros tipos começam sem modelo. Em qualquer registro o mestre pode adicionar atributos livres além dos do modelo, como no Kanka. O modelo só monta o formulário: os valores ficam guardados por chave em `tb_atributo_registro`, então mudar o modelo depois não apaga nada. Nas campanhas de Daggerheart, o tipo Criatura aparece na tela como "Adversário".

Tags: o mestre digita no formulário do registro, e as tags que ainda não existem são criadas na hora.

Exclusões: arquivar é o caminho normal. Excluir uma campanha apaga tudo o que pertence a ela, em cascata. Excluir uma sessão apaga as cenas, as anotações e os vínculos. Nos segredos, a sessão de criação vira nula, e a sessão de revelação também, o que faz o segredo voltar a ficar pendente (`ON DELETE SET NULL`). O e-mail é guardado em minúsculas.

Os dados de exemplo (`02_dados_iniciais.sql`) trazem um usuário de teste e uma campanha de cada sistema, com alguns registros e uma sessão preparada.

## 8. Segurança

- Autenticação por cookie do ASP.NET Core (`AddCookie`), sem ASP.NET Identity completo: `HttpOnly`, `SameSite=Strict`, `Secure` fora do ambiente de desenvolvimento, expiração deslizante.
- Hash da senha com `PasswordHasher<Usuario>` (pacote `Microsoft.Extensions.Identity.Core`), o mesmo do RetroGames.
- CSRF: antiforgery do ASP.NET Core com o cabeçalho `X-XSRF-TOKEN`. A rota `GET /api/auth/csrf` grava o cookie `XSRF-TOKEN`, que o Axios lê e devolve sozinho no cabeçalho.
- Mesma origem em desenvolvimento: o Vite faz proxy de `/api` para a API. Assim não é preciso configurar CORS e o cookie funciona.
- Limite de tentativas no login com o rate limiter nativo do ASP.NET Core.
- Dono da campanha: todo Service que recebe um id de campanha, registro ou sessão confere se ela pertence ao usuário logado. Se não pertencer, responde 404, para não revelar que o id existe. Há testes para isso.
- Cadastro aberto: qualquer pessoa pode criar uma conta.

## 9. API (visão geral)

Rotas no estilo REST, aninhadas na campanha quando o recurso pertence a ela:

- `POST /api/auth/cadastro`, `POST /api/auth/login`, `POST /api/auth/logout`, `GET /api/auth/eu`, `GET /api/auth/csrf`
- `GET /api/sistemas` e `GET /api/sistemas/{id}/modelos-atributo?tipo=`
- `GET|POST /api/campanhas`, `GET|PUT|DELETE /api/campanhas/{id}`
- `GET|POST /api/campanhas/{id}/registros` (filtros: tipo, tag, texto), `GET|PUT|DELETE /api/registros/{id}`
- `GET /api/campanhas/{id}/tags`
- `GET|POST /api/campanhas/{id}/sessoes`, `GET|PUT|DELETE /api/sessoes/{id}`, com cenas, vínculos e anotações como sub-rotas
- `GET|POST /api/campanhas/{id}/segredos`, `PUT /api/segredos/{id}`, `POST /api/segredos/{id}/revelar` e `/ocultar`
- `GET /api/campanhas/{id}/busca?q=`: busca rápida em nome, resumo e tags dos registros, com até 10 resultados, usando `LIKE` (FULLTEXT só se ficar lento)

## 10. Front

Telas:
1. Login e cadastro.
2. Lista de campanhas (criar, arquivar).
3. Caderno da campanha: lista de registros com filtro por tipo e por tag, e formulário com os atributos do modelo + atributos livres.
4. Sessões: lista e tela de preparo nos 8 passos.
5. Escudo (`/campanhas/:id/escudo`).

Rascunho do escudo v1:

```
┌───────────────────────────────────────────────────────────────┐
│ Mina Perdida · D&D 5e · Sessão 4 ▾          [Ctrl+K] Buscar…   │
├──────────────────────────────────┬────────────────────────────┤
│ SESSÃO DE HOJE                   │ ANOTAÇÕES                  │
│ Começo forte: emboscada na ponte │ 20:14 Grom pagou 30 po     │
│ Cenas: 1. Ponte  2. Vila  3. Mina│ 20:31 Lia desconfia do…    │
│ Segredos:                        │ [ escrever… ⏎ ]            │
│  ☐ O prefeito é um cultista      ├────────────────────────────┤
│  ☑ A mina tem duas entradas      │ DADOS                      │
│ NPCs: Prefeito Ulm · Irmã Vela   │ [1d20+5] [Rolar]           │
│ Locais: Ponte Velha · Mina       │ 1d20+5 → (14)+5 = 19       │
│ Criaturas: 4 goblins · Bugbear   │ d20 d12 d8 d6 d4 vant desv │
│ Tesouros: Amuleto de Ulm         │ Histórico: …               │
└──────────────────────────────────┴────────────────────────────┘
```

- Clicar num NPC, local, criatura ou resultado da busca abre o cartão do registro num painel lateral, sem sair do escudo.
- Sessão de hoje = a sessão escolhida no seletor. O padrão é a de maior número.
- Ctrl+K abre a busca em qualquer tela da campanha. Funciona com setas e Enter, sem mouse.
- Anotações: Enter grava com o horário atual. Ficam salvas na sessão.
- Rolador de dados no navegador (`src/dados/`): aceita `XdY`, `+`/`-`, constantes, vários termos e `kh`/`kl` (`2d20kh1`), com botões rápidos e vantagem/desvantagem. Usa `crypto.getRandomValues`. Expressão inválida mostra um erro, sem travar. O histórico fica só na memória da tela no MVP.

## 11. Testes e como conferir

- Services: regras de dono da campanha, segredo pendente que aparece na sessão seguinte, nome de tag duplicado, sistema que não pode ser trocado.
- Validators: campos obrigatórios e tamanhos.
- Rolador (Vitest): `XdY`, `+/-`, `kh/kl`, vários termos, limites (por exemplo, no máximo 100 dados e 1000 faces) e expressões inválidas.
- Banco: `03_consultas.sql` com consultas de teste, como no RetroGames.
- Cada etapa do roteiro mantém o item "Confira".

## 12. Ordem de entrega

Segue as etapas 0 a 8 do roteiro. A etapa 0 também traz as convenções do projeto e o `docs/USER_STORIES.md` do MVP.

## 13. O que deixei em aberto

- Mensagens e códigos de erro exatos, desde que sigam o envelope.
- Visual do Bootstrap (cores, espaçamentos), desde que o escudo caiba numa tela de notebook (1366×768) sem rolagem horizontal.
- Paginação da lista de registros (só se passar de cerca de 200 registros).
