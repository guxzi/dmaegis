-- DMAegis — 02: dados de exemplo (uma campanha de cada sistema)
-- Executar logo depois do 01 (os ids abaixo supõem o banco recém-criado).
-- Textos e criaturas de Daggerheart são próprios: nada vem do SRD do Daggerheart (ver ROTEIRO, seção 4).

USE dmaegis;

-- Usuário de teste. O hash é um placeholder: o login só funciona depois da etapa 2,
-- quando a API gerar um hash de verdade (PasswordHasher) e este valor for atualizado.
INSERT INTO tb_usuario (co_seq_usuario, no_usuario, ds_email, ds_hash_senha, dt_criacao) VALUES
    (1, 'Mestre de Teste', 'mestre@dmaegis.test', 'HASH_GERADO_NA_ETAPA_2', NOW());

INSERT INTO tb_campanha (co_seq_campanha, co_usuario, co_sistema, no_campanha, ds_campanha, st_arquivada, dt_criacao) VALUES
    (1, 1, 1, 'A Mina Perdida', 'Uma vila mineradora some do mapa e os aventureiros vão descobrir por quê.', FALSE, NOW()),
    (2, 1, 2, 'Sementes da Ruína', 'Uma floresta apodrece de dentro para fora, e alguém está plantando a podridão.', FALSE, NOW());

-- ---------------------------------------------------------------
-- Registros (tipos: 1 NPC, 2 PJ, 3 Criatura, 4 Local; origem 1 = Criado)
-- ---------------------------------------------------------------
INSERT INTO tb_registro (co_seq_registro, co_campanha, co_tipo_registro, co_origem_registro, no_registro, ds_resumo, ds_registro, dt_criacao, dt_atualizacao) VALUES
    (1,  1, 2, 1, 'Lia Ventoleve', 'Halfling ladina, curiosa demais para o próprio bem.', NULL, NOW(), NOW()),
    (2,  1, 1, 1, 'Prefeito Ulm', 'Prefeito simpático que evita falar da mina.', 'Sorri demais. Usa um amuleto que nunca tira.', NOW(), NOW()),
    (3,  1, 1, 1, 'Irmã Vela', 'Sacerdotisa que cuida dos feridos da vila.', NULL, NOW(), NOW()),
    (4,  1, 3, 1, 'Goblin', 'Batedor goblin que ronda a estrada da mina.', NULL, NOW(), NOW()),
    (5,  1, 4, 1, 'Mina do Corvo', 'Mina abandonada ao norte da vila.', 'Túneis estreitos, cheiro de enxofre.', NOW(), NOW()),
    (6,  2, 2, 1, 'Tarin', 'Guardião silencioso que fala com as árvores.', NULL, NOW(), NOW()),
    (7,  2, 1, 1, 'Mestra Orla', 'Herbalista que mapeia a podridão.', NULL, NOW(), NOW()),
    (8,  2, 1, 1, 'Bram, o barqueiro', 'Leva qualquer um rio acima, pelo preço certo.', NULL, NOW(), NOW()),
    (9,  2, 3, 1, 'Lobo-de-cinzas', 'Lobo de pelo cinzento que deixa rastro de fuligem.', NULL, NOW(), NOW()),
    (10, 2, 4, 1, 'Bosque Murcho', 'Clareira onde as árvores secaram em uma noite.', NULL, NOW(), NOW());

-- Atributos das criaturas, seguindo o modelo do sistema
INSERT INTO tb_atributo_registro (co_registro, no_chave, ds_valor, nu_ordem) VALUES
    (4, 'CA', '15', 1),
    (4, 'PV', '10', 2),
    (4, 'Deslocamento', '9 m', 3),
    (4, 'ND', '1/4', 4),
    (4, 'XP', '50', 5),
    (9, 'Tipo', 'Bruto', 1),
    (9, 'Patamar', '1', 2),
    (9, 'Dificuldade', '12', 3),
    (9, 'Limiar Maior', '7', 4),
    (9, 'Limiar Grave', '13', 5),
    (9, 'PV', '5', 6),
    (9, 'Estresse', '3', 7),
    (9, 'Ataque', '+2', 8),
    (9, 'Dano', '1d8+2 físico', 9);

-- Um atributo livre (fora do modelo)
INSERT INTO tb_atributo_registro (co_registro, no_chave, ds_valor, nu_ordem) VALUES
    (2, 'Segredo pessoal', 'Deve dinheiro ao culto', 1);

INSERT INTO tb_tag (co_seq_tag, co_campanha, no_tag) VALUES
    (1, 1, 'vila'),
    (2, 1, 'culto'),
    (3, 2, 'floresta'),
    (4, 2, 'aliado');

INSERT INTO tb_registro_tag (co_registro, co_tag) VALUES
    (2, 1), (2, 2), (3, 1), (5, 2),
    (7, 4), (8, 4), (10, 3);

-- ---------------------------------------------------------------
-- Sessões: a 1 preparada, a 2 vazia (em cada campanha)
-- ---------------------------------------------------------------
INSERT INTO tb_sessao (co_seq_sessao, co_campanha, nu_sessao, dt_sessao, ds_notas_personagens, ds_comeco_forte, ds_resumo) VALUES
    (1, 1, 1, '2026-11-20', 'Lia quer saber quem roubou o mapa do pai.', 'Emboscada goblin na ponte da estrada da mina.', NULL),
    (2, 1, 2, NULL, NULL, NULL, NULL),
    (3, 2, 1, '2026-11-22', 'Tarin ouviu as árvores gritarem em sonho.', 'O barco de Bram encalha num rio coberto de cinzas.', NULL),
    (4, 2, 2, NULL, NULL, NULL, NULL);

INSERT INTO tb_cena_sessao (co_sessao, ds_cena, nu_ordem) VALUES
    (1, 'Ponte Velha: a emboscada', 1),
    (1, 'Vila: o prefeito desconversa', 2),
    (3, 'Rio de cinzas', 1),
    (3, 'Bosque Murcho ao entardecer', 2);

-- Segredos: 1 revelado na sessão 1 e 1 pendente em cada campanha
INSERT INTO tb_segredo (co_seq_segredo, co_campanha, co_sessao_criacao, co_sessao_revelacao, ds_segredo, dt_revelacao) VALUES
    (1, 1, 1, 1,    'A mina tem duas entradas.', NOW()),
    (2, 1, 1, NULL, 'O prefeito é um cultista.', NULL),
    (3, 2, 3, 3,    'A fuligem vem de uma fogueira ritual.', NOW()),
    (4, 2, 3, NULL, 'Mestra Orla espalhou a podridão sem querer.', NULL);

-- Passos 5 a 8: registros ligados à sessão 1 de cada campanha
INSERT INTO tb_sessao_registro (co_sessao, co_registro) VALUES
    (1, 2), (1, 3), (1, 4), (1, 5),
    (3, 7), (3, 8), (3, 9), (3, 10);

INSERT INTO tb_anotacao_sessao (co_sessao, dt_anotacao, ds_anotacao) VALUES
    (1, '2026-11-20 20:14:00', 'Lia pagou 30 po pela informação do ferreiro.');
