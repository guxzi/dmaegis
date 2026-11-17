# DMAegis — Histórias de usuário do MVP (Fase 1)

> Fonte: `docs/superpowers/specs/2026-11-17-dmaegis-mvp-design.md`. Cada história tem critérios de aceite que podem ser conferidos.
> Formato: **Como mestre, quero … para …**

## Conta

**US01 — Cadastro.** Como mestre, quero criar uma conta com nome, e-mail e senha para guardar minhas campanhas.
- E-mail é obrigatório e único, sem diferença entre maiúsculas e minúsculas (`A@x.com` = `a@x.com`).
- A senha tem pelo menos 8 caracteres e é guardada só como hash.
- E-mail repetido mostra "Este e-mail já está cadastrado." (409).
- Depois do cadastro, já fico logado.

**US02 — Login.** Como mestre, quero entrar com e-mail e senha para acessar minhas campanhas.
- Credenciais erradas mostram uma mensagem genérica ("E-mail ou senha inválidos."), sem dizer qual dos dois errou.
- A sessão é um cookie `HttpOnly`. Nenhum token fica no `localStorage`.
- Depois de 5 tentativas erradas em 1 minuto, o login responde 429 por um tempo.

**US03 — Logout e sessão expirada.** Como mestre, quero sair da conta e ser levado ao login quando a sessão expirar.
- "Sair" apaga o cookie e volta para o login.
- Qualquer chamada que receba 401 leva para o login.

## Campanhas

**US04 — Criar campanha.** Como mestre, quero criar uma campanha escolhendo D&D 5e (2024) ou Daggerheart, para que o app se adapte ao sistema.
- Nome (até 100 caracteres) e sistema são obrigatórios. A descrição é opcional.

**US05 — Listar campanhas.** Como mestre, quero ver só as minhas campanhas, separando as ativas das arquivadas.
- Nunca aparece uma campanha de outro usuário. Acessar o id de uma campanha alheia responde 404.

**US06 — Editar campanha.** Como mestre, quero mudar o nome e a descrição de uma campanha.
- O sistema não pode ser trocado depois de criada a campanha. A tela mostra o sistema sem permitir edição.

**US07 — Arquivar e excluir.** Como mestre, quero arquivar uma campanha encerrada e excluir uma que não quero mais.
- Arquivar esconde a campanha da lista principal, e é possível desarquivar.
- Excluir pede confirmação e apaga tudo da campanha (registros, sessões, segredos, tags).

## Caderno

**US08 — Registros.** Como mestre, quero cadastrar NPCs, PJs, Criaturas, Locais, Facções, Itens e Missões para ter a campanha organizada.
- Cada registro tem tipo, nome (obrigatório, até 150), resumo de uma linha (até 200), descrição longa, tags e atributos.
- Posso editar e excluir. Excluir pede confirmação.

**US09 — Atributos do sistema.** Como mestre, quero que uma Criatura já venha com os campos do sistema da campanha.
- D&D: CA, PV, Deslocamento, ND, XP.
- Daggerheart: Tipo, Patamar, Dificuldade, Limiar Maior, Limiar Grave, PV, Estresse, Ataque, Dano. Nessas campanhas, o tipo aparece como **"Adversário"**.
- Posso adicionar atributos livres (chave + valor) em qualquer registro.

**US10 — Tags.** Como mestre, quero marcar registros com tags para encontrá-los depois.
- Digitar uma tag nova cria a tag na hora. Não há tag repetida dentro da mesma campanha.

**US11 — Filtrar o caderno.** Como mestre, quero filtrar registros por tipo, por tag e por texto.
- Os filtros se combinam (por exemplo, NPCs com a tag "vila").

## Sessões

**US12 — Criar sessão.** Como mestre, quero criar a próxima sessão da campanha.
- O número é sugerido automaticamente (o maior + 1) e não se repete na campanha. A data é opcional.

**US13 — Preparar nos 8 passos.** Como mestre, quero preparar a sessão seguindo os 8 passos do Lazy DM.
- Passo 1: lista os PJs da campanha + campo de notas.
- Passo 2: começo forte (texto).
- Passo 3: cenas em uma lista que posso reordenar.
- Passo 4: segredos e pistas (ver US14).
- Passos 5 a 8: vincular Locais, NPCs, Criaturas e Itens/tesouros já cadastrados no caderno.

**US14 — Segredos que voltam sozinhos.** Como mestre, quero que os segredos não revelados apareçam na sessão seguinte sem eu copiar nada.
- Um segredo pertence à campanha. A tela da sessão mostra todos os segredos ainda não revelados, mais os revelados nela.
- Marcar "revelado" grava a sessão e a hora. "Ocultar" desfaz.
- Excluir a sessão em que um segredo foi revelado faz ele voltar a ficar pendente.

**US15 — Resumo pós-sessão.** Como mestre, quero escrever o resumo do que aconteceu depois da sessão.

## Escudo

**US16 — Sessão de hoje.** Como mestre, quero uma tela com o preparo da sessão à vista, para conduzir o jogo sem trocar de aba.
- Mostra o começo forte, as cenas, os segredos (com a caixa de "revelado"), os NPCs, os locais, as criaturas e os tesouros.
- A sessão padrão é a de maior número, e um seletor permite trocar.
- Clicar num registro abre o cartão dele num painel lateral, sem sair do escudo.
- Cabe numa tela de 1366×768 sem rolagem horizontal.

**US17 — Busca rápida (Ctrl+K).** Como mestre, quero achar qualquer registro em segundos.
- Ctrl+K abre a busca em qualquer tela da campanha. Ela procura no nome, no resumo e nas tags, com até 10 resultados.
- Setas e Enter funcionam sem mouse. Esc fecha.

**US18 — Anotações com horário.** Como mestre, quero anotar o que acontece durante a sessão.
- Enter grava a anotação com o horário atual e limpa o campo.
- As anotações ficam salvas na sessão e aparecem da mais antiga para a mais nova.

**US19 — Rolador de dados.** Como mestre, quero rolar dados no próprio escudo.
- Aceita `XdY`, `+`/`-`, constantes, vários termos e `kh`/`kl` (`2d20kh1`).
- Botões rápidos (d20, d12, d10, d8, d6, d4) e vantagem/desvantagem.
- Mostra cada dado e o total, por exemplo `1d20+5 → (14) + 5 = 19`.
- Uma expressão inválida mostra um erro, sem travar. Limites: até 100 dados e até 1000 faces.
- Roda no navegador e funciona mesmo sem internet.
