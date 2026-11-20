# Convenções do DMAegis

Estas são as regras que sigo no código. Servem para mim e para quem quiser mexer no projeto.

Se este arquivo e o design de uma fase (`docs/design/`) discordarem, vale o design, e eu atualizo este arquivo.

## Stack

.NET 10 com ASP.NET Core e controllers, Dapper com MySqlConnector, MySQL 8, FluentValidation, DotNetEnv, e xUnit v3 com Moq nos testes. No front, React com Vite, Bootstrap, Axios e React Router, em JavaScript, testado com Vitest. Os testes de ponta a ponta usam Playwright (`dmaegis.web/e2e`, arquivos `*.e2e.js`).

Não uso Entity Framework, AutoMapper, JWT, TypeScript nem Tailwind. Pacote novo só entra com motivo.

## Camadas

| Camada | Faz | Não faz |
| --- | --- | --- |
| Controller | Recebe a requisição, chama o Service e devolve o envelope com o status HTTP | Regra de negócio, SQL |
| Validator | Valida o formato (obrigatório, tamanho, faixa) | Consultar o banco |
| Service | Regras de negócio, conferência do dono da campanha, transações, `BusinessRuleException` | Saber de HTTP ou de SQL |
| Repository | SQL parametrizado com Dapper e o mapeamento de coluna para propriedade | Decidir regra, abrir transação |

## Nomes

Tudo em português: classes, métodos e variáveis.

Cada tabela tem uma classe em cada camada, com o nome da tabela sem o `tb_`. Assim, `tb_atributo_registro` vira `AtributoRegistro`, `IAtributoRegistroRepository` e `AtributoRegistroService`.

Os ApiModels seguem `{Ação}{Entidade}{Request|Response}ApiModel`, como `InserirCampanhaRequestApiModel`.

As propriedades não repetem o nome da classe (`Campanha.Id`, `Campanha.Nome`). Chave estrangeira é `Id` mais a entidade (`Campanha.IdUsuario`). O nome de outra entidade só aparece quando o dado vem dela (`NomeSistema`). Texto obrigatório começa com `string.Empty`.

## Banco de dados

Os prefixos são `tb_` (tabela), `co_seq_` (chave primária), `co_` (chave estrangeira), `no_` (nome), `ds_` (descrição), `nu_` (número), `dt_` (data), `st_` (situação) e `bi_` (conteúdo binário, como imagens).

O banco também protege a integridade: `UNIQUE` (inclusive composto), `ON DELETE CASCADE` nas tabelas filhas que só existem por causa da mãe e `ON DELETE SET NULL` onde o design pede. Não uso trigger nem function.

Mudança de estrutura vai num script novo e numerado em `database/`. Script que já rodou não muda.

## Repository

- Um Repository por tabela.
- Todo valor entra como parâmetro (`@nome`). Nada de concatenar valor no SQL.
- `JOIN` pode quando simplifica. Filtro, ordenação e paginação ficam no SQL.
- As colunas ganham alias no `SELECT` (`co_seq_campanha AS Id`).
- O erro 1062 do MySQL (valor duplicado) vira `RegistroDuplicadoException`, que o Service transforma no erro de negócio certo.

## Service

Toda regra de negócio fica aqui, mesmo a que o banco também garante.

Todo recurso de campanha confere o dono. Se a campanha não é do usuário logado, a resposta é 404, como se o recurso não existisse.

Erro de negócio é `throw new BusinessRuleException(BusinessErrors.X)`. Operação que grava mais de um comando usa `TransactionScope` com `ReadCommitted` e `TransactionScopeAsyncFlowOption.Enabled`.

## API

Toda resposta usa o envelope `{ "responseCode", "data", "errorMessage" }`. Os códigos ficam em `ResponseCodes.cs`: `SUCESSO = 0`, `ERRO = 1` e, a partir de 2, os erros de negócio em sequência.

Os status usados são 200, 201, 400, 401, 403, 404, 409 e 500. Exclusão responde 200 com o envelope.

## Segurança

- A connection string fica no `.env` da raiz, fora do git, lida pelo DotNetEnv. O modelo versionado é o `.env.example`, com `SUA_SENHA`. Credencial nunca vai para arquivo versionado.
- Senha só como hash (`PasswordHasher<Usuario>`).
- Sessão por cookie `HttpOnly` e `SameSite=Strict`, com proteção CSRF pelo cabeçalho `X-XSRF-TOKEN`. Nenhum token no `localStorage`.
- Limite de tentativas no login.
- Os jogadores que entram pelo link de convite têm um cookie próprio, que só abre `/api/jogador/*`, e um token CSRF próprio (`XSRF-TOKEN-JOGADOR`). Eles só recebem a visão filtrada da mesa (`MesaService.FiltrarParaJogador`).
- Do Daggerheart entram só as mecânicas, com texto meu, sem conteúdo do SRD. A seção 4 do [roteiro](ROTEIRO.md) explica o porquê.

## Identidade visual

O tema é magia cósmica: espaço profundo, azul cobalto, violeta, estrelas, constelações e escudos (o "aegis" do nome). Vale para todas as páginas.

- A paleta fica em `dmaegis.web/src/estilo/tema.css`, nos tokens `--dm-*` e nas variáveis do Bootstrap que já troquei ali. Componente não usa cor solta. As cores principais são fundo `#070b1f`, superfície `#0f1636`, cobalto `#2f5bff`, violeta `#8b5cf6`, estrela `#c4b5fd`, texto `#e6e9ff` e texto suave `#9aa3d4`.
- O Bootstrap roda no modo escuro (`data-bs-theme="dark"` no `<html>`), e o céu estrelado (`CeuEstrelado`) fica atrás de tudo.
- As fontes são locais (`@fontsource`): Cinzel nos títulos e na marca, Alegreya Sans no texto. Nada de fonte ou imagem de CDN.
- Os componentes do tema são o `Emblema` (o escudo com constelação, que também é o favicon), `.selo-escudo`, `.cartao-astral`, `.divisor-estelar`, `.etiqueta-registro`, `.selo-dnd` e `.selo-daggerheart`. O botão principal tem gradiente de cobalto para violeta.
- O redesign de 18/11/2026 pegou técnicas do Dungeons and Newbies, outro projeto meu. Os estilos estão em `estilo/redesign.css`: botões em pílula (`.btn-fantasma` para os secundários), `CabecalhoSecao`, `.numeros-destaque`, o `.hero` com `ArteCampanha` e `PoeiraEstelar`, `.cartao-arte`, `.cartao-registro`, `.segmentado`, `.caixas-atributo` e `DadoSvg`.
- As transições ficam em `estilo/movimento.css`, sempre dentro de `prefers-reduced-motion: no-preference`. Animação de entrada usa `backwards`, para não deixar um `transform` sobrando que prenda elementos fixos. Tela com o palco da mesa em `position: fixed` só esmaece na entrada, senão o palco nasce sem altura.
- A mesa virtual tem os estilos em `estilo/mesa.css`. O `PalcoMapa` serve ao mestre e aos jogadores.
- O cursor é uma seta e uma mão no formato clássico, azul-noite com contorno azul claro (`public/cursores/`), ligadas em `estilo/cursores.css`. Ele aparece sempre, em todo o site. Em CSS novo, use `cursor: var(--cursor-padrao)` ou `var(--cursor-clique)`, nunca uma palavra-chave solta.
- Página nova usa o `Cabecalho` e os cartões do Bootstrap. Confiro o contraste do texto (pelo menos 4.5:1) e a tela em 1366×768.
- O visual é original, só inspirado nessa atmosfera. Não uso nomes, personagens, lugares, termos nem arte da Critical Role ou da Darrington Press.

## Front

- Toda chamada à API passa por `src/api/cliente.js`. As rotas de jogador usam a instância `apiJogador`.
- A aleatoriedade (dados e tabelas) roda no navegador, com `crypto.getRandomValues`.
- Função pura (rolador, tabelas, contas da grade, névoa) tem teste no Vitest.

## Regras de jogo

Regra que veio de fã só vira código depois de conferida no SRD oficial ou no livro da Jambô.
