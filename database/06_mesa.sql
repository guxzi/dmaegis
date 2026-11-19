-- DMAegis — 06: mesa virtual (entrega 1)
-- Design: docs/superpowers/specs/2026-11-19-dmaegis-mesa-virtual-design.md (seção 6).
-- Uma campanha tem várias mesas (mapas preparados) e no máximo uma ativa, a que os jogadores veem.
-- Cada mesa tem participantes (fichas no mapa e linhas da iniciativa) com condições.

USE dmaegis;

CREATE TABLE tb_mesa (
    co_seq_mesa          INT NOT NULL AUTO_INCREMENT,
    co_campanha          INT NOT NULL,
    no_mesa              VARCHAR(100) NOT NULL,
    co_imagem_registro   INT NULL,
    nu_tamanho_celula    INT NOT NULL DEFAULT 70,
    nu_deslocamento_x    INT NOT NULL DEFAULT 0,
    nu_deslocamento_y    INT NOT NULL DEFAULT 0,
    st_grade_visivel     TINYINT(1) NOT NULL DEFAULT 1,
    st_ativa             TINYINT(1) NOT NULL DEFAULT 0,
    st_combate           TINYINT(1) NOT NULL DEFAULT 0,
    nu_rodada            INT NOT NULL DEFAULT 0,
    co_participante_vez  INT NULL,
    dt_criacao           DATETIME NOT NULL,
    dt_atualizacao       DATETIME NOT NULL,
    CONSTRAINT pk_mesa PRIMARY KEY (co_seq_mesa),
    CONSTRAINT fk_mesa_campanha FOREIGN KEY (co_campanha) REFERENCES tb_campanha (co_seq_campanha) ON DELETE CASCADE,
    -- Apagar a imagem não apaga a mesa: ela fica sem mapa (campo estrelado).
    CONSTRAINT fk_mesa_imagem FOREIGN KEY (co_imagem_registro) REFERENCES tb_imagem_registro (co_seq_imagem_registro) ON DELETE SET NULL,
    INDEX ix_mesa_campanha (co_campanha)
) ENGINE = InnoDB;

CREATE TABLE tb_participante_mesa (
    co_seq_participante_mesa INT NOT NULL AUTO_INCREMENT,
    co_mesa                  INT NOT NULL,
    co_registro              INT NULL,
    no_participante          VARCHAR(100) NOT NULL,
    st_jogador               TINYINT(1) NOT NULL DEFAULT 0,
    nu_iniciativa            INT NULL,
    nu_bonus_iniciativa      INT NOT NULL DEFAULT 0,
    nu_ca                    INT NULL,
    nu_pv_atual              INT NULL,
    nu_pv_maximo             INT NULL,
    nu_coluna                INT NULL,
    nu_linha                 INT NULL,
    nu_tamanho               INT NOT NULL DEFAULT 1,
    st_visivel               TINYINT(1) NOT NULL DEFAULT 1,
    nu_ordem                 INT NOT NULL DEFAULT 0,
    CONSTRAINT pk_participante_mesa PRIMARY KEY (co_seq_participante_mesa),
    CONSTRAINT fk_participante_mesa_mesa FOREIGN KEY (co_mesa) REFERENCES tb_mesa (co_seq_mesa) ON DELETE CASCADE,
    -- Apagar o registro do caderno mantém a ficha, só sem o vínculo.
    CONSTRAINT fk_participante_mesa_registro FOREIGN KEY (co_registro) REFERENCES tb_registro (co_seq_registro) ON DELETE SET NULL,
    INDEX ix_participante_mesa_mesa (co_mesa, nu_ordem)
) ENGINE = InnoDB;

-- A vez aponta para um participante; apagar o participante limpa a vez.
ALTER TABLE tb_mesa
    ADD CONSTRAINT fk_mesa_participante_vez FOREIGN KEY (co_participante_vez)
        REFERENCES tb_participante_mesa (co_seq_participante_mesa) ON DELETE SET NULL;

CREATE TABLE tb_condicao_participante (
    co_seq_condicao_participante INT NOT NULL AUTO_INCREMENT,
    co_participante              INT NOT NULL,
    no_condicao                  VARCHAR(40) NOT NULL,
    CONSTRAINT pk_condicao_participante PRIMARY KEY (co_seq_condicao_participante),
    CONSTRAINT uq_condicao_participante UNIQUE (co_participante, no_condicao),
    CONSTRAINT fk_condicao_participante_participante FOREIGN KEY (co_participante)
        REFERENCES tb_participante_mesa (co_seq_participante_mesa) ON DELETE CASCADE
) ENGINE = InnoDB;
