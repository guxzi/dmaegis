<div align="center">

<img src="dmaegis.web/public/favicon.svg" width="96" alt="Emblema do DMAegis: um escudo com uma constelação">

# DMAegis

Um escudo do mestre digital, em português, para D&D 5e (2024) e Daggerheart.

</div>

## Por que fiz

Mestrando, eu vivia procurando coisas no meio da sessão: o nome daquele NPC, a regra de agarrar, a imagem do lugar que os jogadores acabaram de achar. O DMAegis junta tudo num lugar só e deixa à mão o que importa na mesa daquele dia, só do sistema daquela campanha. Fiz para a minha mesa e para os meus jogadores, que jogam comigo pelo Discord.

## O que ele faz

Cada campanha é de D&D 5e (regras de 2024) ou de Daggerheart, e mostra só os termos, os atributos e as regras do próprio sistema.

O caderno guarda NPCs, personagens, criaturas (adversários, no Daggerheart), locais, facções, itens e missões, com tags, filtros e busca. A galeria junta todas as imagens da campanha pelo nome do NPC ou do local; enviar uma imagem com um nome novo já cria o registro.

O preparo de sessão segue os 8 passos do *Lazy DM* (começo forte, cenas, segredos e pistas, locais, NPCs, monstros e tesouros). Segredo que não foi revelado passa sozinho para a sessão seguinte.

Durante o jogo, o escudo mostra a cena atual, os segredos revelados e quem está em cena. Nas anotações, o nome de um registro vira um botão que abre o cartão dele. Tem rolador de dados (`2d20kh1+5`, `4d6kl3`…), busca rápida com Ctrl+K, uma aba de regras do sistema da mesa e uma de improviso, com oráculo de sim ou não e NPC rápido que vai para o caderno com um clique.

A mesa virtual coloca um mapa da galeria com grade, fichas redondas com retrato (ou a inicial, enquanto o personagem não tem imagem) e o combate ao lado: iniciativa, turno e rodada, dano e cura, condições e fichas escondidas. Com o pincel da névoa de guerra eu revelo o mapa aos poucos, e a régua (tecla R) mede a distância em quadrados e metros. Errou um clique? Ctrl+Z desfaz.

Cada jogador recebe um link, abre a mesa no navegador (no celular também, com zoom de dois dedos) e move a própria ficha, tudo em tempo real. As rolagens de dados feitas na mesa aparecem para todos; eu ainda posso rolar em segredo. Ele só vê o que deve: PV de monstro, ficha escondida, monstro na névoa, anotações e segredos ficam comigo. Também dá para mandar qualquer imagem para uma janela de projeção, copiar ou baixar.

O visual é de magia cósmica: espaço profundo, cobalto e violeta, estrelas que se movem a cada página e uma faísca de luz no clique. As animações desligam quando o sistema pede menos movimento.

## Tecnologias

| Parte | Stack |
| --- | --- |
| API | .NET 10 (ASP.NET Core com controllers), Dapper com MySqlConnector, FluentValidation, DotNetEnv |
| Banco | MySQL 8 |
| Site | React, Vite, Bootstrap, React Router e Axios, em JavaScript |
| Testes | xUnit v3 e Moq na API, Vitest no site |

A API é dividida em Controller, Service e Repository, e sempre responde com o envelope `{ responseCode, data, errorMessage }`. O login usa cookie `HttpOnly` e `SameSite=Strict` com proteção CSRF, a senha fica só como hash e há limite de tentativas. Toda rota confere se a campanha é de quem está logado. As regras que sigo no código estão em [`docs/CONVENCOES.md`](docs/CONVENCOES.md).

## Como rodar

Precisa do [.NET 10 SDK](https://dotnet.microsoft.com/download), do [Node.js](https://nodejs.org) e do MySQL 8.

1. Rode os scripts de `database/` em ordem, do `01` ao `10`. O `04` cria um usuário de teste: `mestre@dmaegis.test`, senha `dmaegis-teste`.
2. Copie o `.env.example` para `.env` na raiz e troque `SUA_SENHA` pela senha do seu MySQL. O `.env` não vai para o git.
3. Instale as dependências do site:

   ```bash
   npm --prefix dmaegis.web install
   ```

4. No Windows, dê dois cliques em `Abrir DMAegis.cmd`, que sobe a API e o site e abre o navegador. Se preferir subir na mão, use dois terminais:

   ```bash
   dotnet run --project DMAegis.WebApi
   ```

   ```bash
   npm --prefix dmaegis.web run dev
   ```

   O site abre em http://localhost:5173 e a API em http://localhost:5080.

## Jogar com os amigos pela internet

O DMAegis roda no meu PC. Para os jogadores abrirem a mesa de casa, uso um túnel do Cloudflare, que cria um endereço https apontando para o meu computador enquanto a gente joga. É de graça e o banco continua no PC.

1. Instale o cloudflared uma vez:

   ```bash
   winget install --id Cloudflare.cloudflared
   ```

2. Feche a janela "DMAegis API", se estiver aberta, e dê dois cliques em `Abrir mesa para os jogadores.cmd`. Ele compila o site, sobe a API em modo mesa (sem Swagger e com cookies seguros) e mostra um endereço como `https://alguma-coisa.trycloudflare.com`.
3. Abra esse endereço no navegador e entre como mestre.
4. Na página Mesa, clique em Jogadores, cole o endereço e gere o link de cada PJ. Mande cada link para o jogador do personagem.

O endereço muda toda vez que a mesa abre, então os links precisam ser gerados de novo a cada sessão. Trocar o link de um personagem corta o acesso do link antigo.

## Backup

Tudo fica no MySQL do meu PC, inclusive as imagens. Para não perder o preparo, dou dois cliques em `Fazer backup do banco.cmd`: ele salva o banco inteiro em `Documentos\Backups DMAegis`, num arquivo `.sql` com a data no nome. Para restaurar:

```bash
mysql -u root -p < "caminho do arquivo .sql"
```

## Testes

```bash
dotnet test
```

```bash
npm --prefix dmaegis.web test
```

## Documentação

- [`docs/ROTEIRO.md`](docs/ROTEIRO.md): a pesquisa, as licenças, as fases e as etapas.
- [`docs/design/`](docs/design/): o design do MVP e o da mesa virtual.
- [`docs/MER.md`](docs/MER.md): o modelo do banco.
- [`docs/USER_STORIES.md`](docs/USER_STORIES.md): as histórias de usuário.
- [`docs/CONVENCOES.md`](docs/CONVENCOES.md): as regras do código (camadas, nomes, banco, segurança e identidade visual).

## O que vem depois

Tabelas aleatórias e oráculo, montar encontros no D&D, o contador de Medo e os dados de dualidade no Daggerheart, layout configurável e uso offline. A lista completa está no [roteiro](docs/ROTEIRO.md#8-roteiro-em-etapas).

## Licenças e créditos

As regras de D&D no escudo são traduzidas e resumidas do SRD 5.2.1. *This work includes material from the System Reference Document 5.2.1 ("SRD 5.2.1") by Wizards of the Coast LLC, available at https://www.dndbeyond.com/srd. The SRD 5.2.1 is licensed under the Creative Commons Attribution 4.0 International License.*

Do Daggerheart, o app traz só as mecânicas, escritas por mim. Nenhum texto do SRD do Daggerheart está incluído, e o app não tem ligação com a Darrington Press.

O visual, os ícones, o cursor e o emblema são originais.
