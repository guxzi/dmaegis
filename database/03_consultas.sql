-- DMAegis — 03: consultas de teste (executar depois do 01 e do 02)
-- Cada consulta traz o resultado esperado no comentário.

USE dmaegis;

-- (a) Atributos do modelo para Criatura em cada sistema
-- Esperado: sistema 1 → 5 linhas (CA, PV, Deslocamento, ND, XP)
--           sistema 2 → 9 linhas (Tipo … Dano)
SELECT s.no_sistema, m.no_chave, m.nu_ordem
FROM tb_modelo_atributo m
JOIN tb_sistema s ON s.co_seq_sistema = m.co_sistema
WHERE m.co_tipo_registro = 3
ORDER BY m.co_sistema, m.nu_ordem;

SELECT co_sistema, COUNT(*) AS qt_atributos
FROM tb_modelo_atributo
WHERE co_tipo_registro = 3
GROUP BY co_sistema;
-- Esperado: (1, 5) e (2, 9)

-- (b) Registros da campanha D&D com as tags
-- Esperado: 5 registros; Prefeito Ulm → "culto, vila"; Irmã Vela → "vila";
--           Mina do Corvo → "culto"; Lia e Goblin sem tags (NULL)
SELECT r.no_registro, t.no_tipo_registro,
       GROUP_CONCAT(tg.no_tag ORDER BY tg.no_tag SEPARATOR ', ') AS tags
FROM tb_registro r
JOIN tb_tipo_registro t ON t.co_seq_tipo_registro = r.co_tipo_registro
LEFT JOIN tb_registro_tag rt ON rt.co_registro = r.co_seq_registro
LEFT JOIN tb_tag tg ON tg.co_seq_tag = rt.co_tag
WHERE r.co_campanha = 1
GROUP BY r.co_seq_registro, r.no_registro, t.no_tipo_registro
ORDER BY r.no_registro;

-- (c) Segredos que a sessão 2 da campanha D&D mostra:
--     os pendentes da campanha + os revelados nela mesma
-- Esperado: 1 linha → "O prefeito é um cultista."
SELECT co_seq_segredo, ds_segredo
FROM tb_segredo
WHERE co_campanha = 1
  AND (co_sessao_revelacao IS NULL OR co_sessao_revelacao = 2);

-- (d) Exclusões em cascata (dentro de uma transação, desfeita no fim)
START TRANSACTION;

-- d1: excluir a sessão 1 da D&D
DELETE FROM tb_sessao WHERE co_seq_sessao = 1;
-- Esperado: 2 segredos da campanha 1 continuam, ambos com sessão de criação NULL;
--           o segredo 1 (revelado na sessão 1) volta a ficar pendente (revelação NULL)
SELECT co_seq_segredo, co_sessao_criacao, co_sessao_revelacao FROM tb_segredo WHERE co_campanha = 1;
-- Esperado: 0 cenas, 0 vínculos e 0 anotações da sessão 1
SELECT (SELECT COUNT(*) FROM tb_cena_sessao WHERE co_sessao = 1)     AS qt_cenas,
       (SELECT COUNT(*) FROM tb_sessao_registro WHERE co_sessao = 1) AS qt_vinculos,
       (SELECT COUNT(*) FROM tb_anotacao_sessao WHERE co_sessao = 1) AS qt_anotacoes;

-- d2: excluir a campanha D&D inteira
DELETE FROM tb_campanha WHERE co_seq_campanha = 1;
-- Esperado: 0, 0, 0, 0, 0 (e os atributos do Goblin e do Prefeito também somem)
SELECT (SELECT COUNT(*) FROM tb_registro WHERE co_campanha = 1)                 AS qt_registros,
       (SELECT COUNT(*) FROM tb_sessao WHERE co_campanha = 1)                   AS qt_sessoes,
       (SELECT COUNT(*) FROM tb_segredo WHERE co_campanha = 1)                  AS qt_segredos,
       (SELECT COUNT(*) FROM tb_tag WHERE co_campanha = 1)                      AS qt_tags,
       (SELECT COUNT(*) FROM tb_atributo_registro WHERE co_registro IN (2, 4))  AS qt_atributos;
-- Esperado: a campanha Daggerheart continua intacta → 5 registros
SELECT COUNT(*) AS qt_registros_daggerheart FROM tb_registro WHERE co_campanha = 2;

ROLLBACK;

-- (e) Tag repetida na mesma campanha
-- Esperado: ERROR 1062 (23000): Duplicate entry '1-vila' for key 'tb_tag.uk_tag_campanha_nome'
-- (fica por último porque o erro interrompe o script quando rodado pelo cliente mysql)
INSERT INTO tb_tag (co_campanha, no_tag) VALUES (1, 'vila');
