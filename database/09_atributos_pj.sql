-- DMAegis — 09: atributos dos personagens dos jogadores (PJ)
-- Com PV e CA no caderno, a ficha do PJ já entra na mesa com a barra de vida.
-- Os termos do Daggerheart precisam ser conferidos no livro da Jambô.

USE dmaegis;

-- PJ no D&D 5e (2024)
INSERT INTO tb_modelo_atributo (co_sistema, co_tipo_registro, no_chave, nu_ordem) VALUES
    (1, 2, 'CA', 1),
    (1, 2, 'PV', 2),
    (1, 2, 'Iniciativa', 3),
    (1, 2, 'Deslocamento', 4);

-- PJ no Daggerheart
INSERT INTO tb_modelo_atributo (co_sistema, co_tipo_registro, no_chave, nu_ordem) VALUES
    (2, 2, 'Evasão', 1),
    (2, 2, 'PV', 2),
    (2, 2, 'Estresse', 3),
    (2, 2, 'Limiar Maior', 4),
    (2, 2, 'Limiar Grave', 5);
