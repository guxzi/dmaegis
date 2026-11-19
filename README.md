<div align="center">

<img src="dmaegis.web/public/favicon.svg" width="96" alt="Emblema do DMAegis: um escudo com uma constelação">

# DMAegis

**Escudo do mestre digital, em português, para D&D 5e (2024) e Daggerheart.**

Caderno da campanha, preparo de sessão e um escudo para conduzir a mesa ao vivo, com visual de magia cósmica.

</div>

## Por que existe

Durante a sessão, o mestre vive procurando coisas: o nome daquele NPC, a regra de agarrar, a imagem do lugar que os jogadores acabaram de descobrir. O DMAegis junta tudo num lugar só e deixa à mão só o que importa na mesa daquele dia, do sistema daquela campanha.

## O que ele faz

- **Campanhas por sistema:** D&D 5e (regras de 2024) ou Daggerheart. Cada campanha mostra só os termos, os atributos e as regras do próprio sistema.
- **Caderno:** NPCs, personagens, criaturas (adversários no Daggerheart), locais, facções, itens e missões, com atributos que seguem o modelo do sistema, tags, filtros e busca.
- **Preparo de sessão:** os 8 passos do método *Lazy DM* (começo forte, cenas, segredos e pistas, locais, NPCs, monstros e tesouros). Segredo não revelado passa para a sessão seguinte.
- **Escudo ao vivo:**
  - resumo da sessão com a cena atual, os segredos revelados e quem está em cena;
  - anotações em que o nome de um registro vira um botão que abre o cartão dele;
  - rolador de dados (`2d20kh1+5`, `4d6kl3`…);
  - busca rápida com Ctrl+K;
  - aba de regras e dicas de condução, só do sistema da mesa.
- **Galeria:** todas as imagens da campanha ligadas ao nome do NPC ou do local. Enviar imagem com um nome novo já cria o registro.
- **Mesa virtual:** mapa da galeria com grade ajustável, fichas redondas com retrato (ou a inicial, enquanto o personagem não tem imagem), pan e zoom. O combate fica ao lado: iniciativa (monstros rolam d20 + bônus), turno e rodada (atalho N), dano e cura, condições do sistema da mesa e fichas ocultas.
- **Jogadores pelo navegador:** cada PJ recebe um link. O jogador abre, vê a mesa em tempo real e move a própria ficha. Ele vê só o que deve: nada de PV de monstros, fichas ocultas, anotações ou segredos.
- **Projeção para os jogadores:** uma janela limpa para compartilhar a tela ou levar ao segundo monitor. Qualquer imagem vai para lá com um clique, com ou sem o título. Também dá para copiar ou baixar a imagem.
- **Visual:** espaço profundo, cobalto e violeta, estrelas que viajam a cada página e uma faísca de luz no clique. As animações desligam quando o sistema pede menos movimento.

## Tecnologias

| Parte | Stack |
| --- | --- |
| API | .NET 10 (ASP.NET Core com controllers), Dapper + MySqlConnector, FluentValidation, DotNetEnv |
| Banco | MySQL 8 |
| Site | React + Vite + Bootstrap + React Router + Axios, em JavaScript |
| Testes | xUnit v3 + Moq na API, Vitest no site |

A API segue camadas (Controller → Service → Repository) e responde sempre com o envelope `{ responseCode, data, errorMessage }`. A sessão usa cookie `HttpOnly` + `SameSite=Strict` com proteção CSRF, senha só como hash e limite de tentativas no login. Cada recurso confere se a campanha é do usuário logado.

## Como rodar

Você precisa do [.NET 10 SDK](https://dotnet.microsoft.com/download), do [Node.js](https://nodejs.org) e do MySQL 8.

1. **Banco:** rode os scripts de `database/` em ordem (`01` a `05`). O `04` cria um usuário de teste: `mestre@dmaegis.test`, senha `dmaegis-teste`.
2. **Conexão:** copie `.env.example` para `.env` na raiz e troque `SUA_SENHA` pela senha do seu MySQL. O `.env` fica fora do git.
3. **Dependências do site:**

   ```bash
   npm --prefix dmaegis.web install
   ```

4. **Subir tudo:** no Windows, dê dois cliques em `Abrir DMAegis.cmd`. Ele sobe a API e o site e abre o navegador. Para subir na mão, use dois terminais:

   ```bash
   dotnet run --project DMAegis.WebApi
   ```

   ```bash
   npm --prefix dmaegis.web run dev
   ```

   O site fica em http://localhost:5173 e a API em http://localhost:5080.

## Jogar com os amigos (mesa pela internet)

O DMAegis roda no seu PC. Para os jogadores abrirem a mesa de casa, um túnel do Cloudflare cria um endereço https que aponta para o seu computador enquanto vocês jogam. Não tem custo e o banco continua no seu PC.

1. Instale o cloudflared uma vez:

   ```bash
   winget install --id Cloudflare.cloudflared
   ```

2. Feche a janela "DMAegis API", se estiver aberta, e dê dois cliques em `Abrir mesa para os jogadores.cmd`. Ele compila o site, sobe a API em modo mesa (sem Swagger e com cookies seguros) e mostra um endereço como `https://alguma-coisa.trycloudflare.com`.
3. Abra esse endereço no seu navegador e entre como mestre.
4. Na página **Mesa**, clique em **Jogadores**, cole o endereço e gere o link de cada PJ. Mande cada link para o jogador do personagem.

O endereço muda a cada vez que você abre a mesa, então gere os links de novo a cada sessão. Trocar o link de um personagem derruba o acesso antigo.

## Testes

```bash
dotnet test
```

```bash
npm --prefix dmaegis.web test
```

## Documentação

- [`docs/ROTEIRO.md`](docs/ROTEIRO.md): a pesquisa, as licenças, as fases e as etapas.
- [`docs/MER.md`](docs/MER.md): o modelo do banco.
- [`docs/USER_STORIES.md`](docs/USER_STORIES.md): as histórias de usuário.
- [`AGENTS.md`](AGENTS.md): as regras do projeto (camadas, nomes, banco, segurança e identidade visual).

## Próximos passos

O MVP está pronto. Vêm aí tabelas aleatórias e oráculo, iniciativa e encontros no D&D, o contador de Medo e os dados de dualidade no Daggerheart, layout configurável e uso offline. A lista completa está no [roteiro](docs/ROTEIRO.md#8-roteiro-em-etapas).

## Licenças e créditos

- As regras de D&D no escudo são traduzidas e resumidas do SRD 5.2.1. *This work includes material from the System Reference Document 5.2.1 ("SRD 5.2.1") by Wizards of the Coast LLC, available at https://www.dndbeyond.com/srd. The SRD 5.2.1 is licensed under the Creative Commons Attribution 4.0 International License.*
- Do Daggerheart, o app traz só as mecânicas, escritas com texto próprio. Nenhum texto do SRD do Daggerheart vem incluído, e o app não é afiliado à Darrington Press.
- O visual, os ícones e o emblema são originais.
