-- DMAegis — 01: estrutura do MVP (Fase 1) e tabelas de domínio
-- Recria o banco do zero: pode ser executado de novo sem erro (APAGA os dados).
-- Ver docs/MER.md.

DROP DATABASE IF EXISTS dmaegis;
CREATE DATABASE dmaegis CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
USE dmaegis;

-- ---------------------------------------------------------------
-- Domínio
-- ---------------------------------------------------------------

CREATE TABLE tb_sistema (
    co_seq_sistema INT NOT NULL AUTO_INCREMENT,
    no_sistema     VARCHAR(60) NOT NULL,
    CONSTRAINT pk_sistema PRIMARY KEY (co_seq_sistema),
    CONSTRAINT uk_sistema_nome UNIQUE (no_sistema)
) ENGINE = InnoDB;

CREATE TABLE tb_tipo_registro (
    co_seq_tipo_registro INT NOT NULL AUTO_INCREMENT,
    no_tipo_registro     VARCHAR(30) NOT NULL,
    CONSTRAINT pk_tipo_registro PRIMARY KEY (co_seq_tipo_registro),
    CONSTRAINT uk_tipo_registro_nome UNIQUE (no_tipo_registro)
) ENGINE = InnoDB;

CREATE TABLE tb_origem_registro (
    co_seq_origem_registro INT NOT NULL AUTO_INCREMENT,
    no_origem_registro     VARCHAR(30) NOT NULL,
    CONSTRAINT pk_origem_registro PRIMARY KEY (co_seq_origem_registro),
    CONSTRAINT uk_origem_registro_nome UNIQUE (no_origem_registro)
) ENGINE = InnoDB;

CREATE TABLE tb_modelo_atributo (
    co_seq_modelo_atributo INT NOT NULL AUTO_INCREMENT,
    co_sistema             INT NOT NULL,
    co_tipo_registro       INT NOT NULL,
    no_chave               VARCHAR(60) NOT NULL,
    nu_ordem               INT NOT NULL,
    CONSTRAINT pk_modelo_atributo PRIMARY KEY (co_seq_modelo_atributo),
    CONSTRAINT uk_modelo_atributo UNIQUE (co_sistema, co_tipo_registro, no_chave),
    CONSTRAINT fk_modelo_atributo_sistema FOREIGN KEY (co_sistema) REFERENCES tb_sistema (co_seq_sistema),
    CONSTRAINT fk_modelo_atributo_tipo FOREIGN KEY (co_tipo_registro) REFERENCES tb_tipo_registro (co_seq_tipo_registro)
) ENGINE = InnoDB;

-- ---------------------------------------------------------------
-- Caderno
-- ---------------------------------------------------------------

CREATE TABLE tb_usuario (
    co_seq_usuario INT NOT NULL AUTO_INCREMENT,
    no_usuario     VARCHAR(100) NOT NULL,
    ds_email       VARCHAR(254) NOT NULL,
    ds_hash_senha  VARCHAR(255) NOT NULL,
    dt_criacao     DATETIME NOT NULL,
    CONSTRAINT pk_usuario PRIMARY KEY (co_seq_usuario),
    CONSTRAINT uk_usuario_email UNIQUE (ds_email)
) ENGINE = InnoDB;

CREATE TABLE tb_campanha (
    co_seq_campanha INT NOT NULL AUTO_INCREMENT,
    co_usuario      INT NOT NULL,
    co_sistema      INT NOT NULL,
    no_campanha     VARCHAR(100) NOT NULL,
    ds_campanha     TEXT NULL,
    st_arquivada    BOOLEAN NOT NULL DEFAULT FALSE,
    dt_criacao      DATETIME NOT NULL,
    CONSTRAINT pk_campanha PRIMARY KEY (co_seq_campanha),
    CONSTRAINT fk_campanha_usuario FOREIGN KEY (co_usuario) REFERENCES tb_usuario (co_seq_usuario) ON DELETE CASCADE,
    CONSTRAINT fk_campanha_sistema FOREIGN KEY (co_sistema) REFERENCES tb_sistema (co_seq_sistema)
) ENGINE = InnoDB;

CREATE TABLE tb_registro (
    co_seq_registro    INT NOT NULL AUTO_INCREMENT,
    co_campanha        INT NOT NULL,
    co_tipo_registro   INT NOT NULL,
    co_origem_registro INT NOT NULL,
    no_registro        VARCHAR(150) NOT NULL,
    ds_resumo          VARCHAR(200) NULL,
    ds_registro        MEDIUMTEXT NULL,
    dt_criacao         DATETIME NOT NULL,
    dt_atualizacao     DATETIME NOT NULL,
    CONSTRAINT pk_registro PRIMARY KEY (co_seq_registro),
    CONSTRAINT fk_registro_campanha FOREIGN KEY (co_campanha) REFERENCES tb_campanha (co_seq_campanha) ON DELETE CASCADE,
    CONSTRAINT fk_registro_tipo FOREIGN KEY (co_tipo_registro) REFERENCES tb_tipo_registro (co_seq_tipo_registro),
    CONSTRAINT fk_registro_origem FOREIGN KEY (co_origem_registro) REFERENCES tb_origem_registro (co_seq_origem_registro),
    INDEX ix_registro_campanha_nome (co_campanha, no_registro)
) ENGINE = InnoDB;

CREATE TABLE tb_atributo_registro (
    co_seq_atributo_registro INT NOT NULL AUTO_INCREMENT,
    co_registro              INT NOT NULL,
    no_chave                 VARCHAR(60) NOT NULL,
    ds_valor                 VARCHAR(500) NOT NULL,
    nu_ordem                 INT NOT NULL,
    CONSTRAINT pk_atributo_registro PRIMARY KEY (co_seq_atributo_registro),
    CONSTRAINT fk_atributo_registro_registro FOREIGN KEY (co_registro) REFERENCES tb_registro (co_seq_registro) ON DELETE CASCADE
) ENGINE = InnoDB;

CREATE TABLE tb_tag (
    co_seq_tag  INT NOT NULL AUTO_INCREMENT,
    co_campanha INT NOT NULL,
    no_tag      VARCHAR(50) NOT NULL,
    CONSTRAINT pk_tag PRIMARY KEY (co_seq_tag),
    CONSTRAINT uk_tag_campanha_nome UNIQUE (co_campanha, no_tag),
    CONSTRAINT fk_tag_campanha FOREIGN KEY (co_campanha) REFERENCES tb_campanha (co_seq_campanha) ON DELETE CASCADE
) ENGINE = InnoDB;

CREATE TABLE tb_registro_tag (
    co_registro INT NOT NULL,
    co_tag      INT NOT NULL,
    CONSTRAINT pk_registro_tag PRIMARY KEY (co_registro, co_tag),
    CONSTRAINT fk_registro_tag_registro FOREIGN KEY (co_registro) REFERENCES tb_registro (co_seq_registro) ON DELETE CASCADE,
    CONSTRAINT fk_registro_tag_tag FOREIGN KEY (co_tag) REFERENCES tb_tag (co_seq_tag) ON DELETE CASCADE
) ENGINE = InnoDB;

-- ---------------------------------------------------------------
-- Sessões
-- ---------------------------------------------------------------

CREATE TABLE tb_sessao (
    co_seq_sessao        INT NOT NULL AUTO_INCREMENT,
    co_campanha          INT NOT NULL,
    nu_sessao            INT NOT NULL,
    dt_sessao            DATE NULL,
    ds_notas_personagens TEXT NULL,
    ds_comeco_forte      TEXT NULL,
    ds_resumo            TEXT NULL,
    CONSTRAINT pk_sessao PRIMARY KEY (co_seq_sessao),
    CONSTRAINT uk_sessao_campanha_numero UNIQUE (co_campanha, nu_sessao),
    CONSTRAINT fk_sessao_campanha FOREIGN KEY (co_campanha) REFERENCES tb_campanha (co_seq_campanha) ON DELETE CASCADE
) ENGINE = InnoDB;

CREATE TABLE tb_cena_sessao (
    co_seq_cena_sessao INT NOT NULL AUTO_INCREMENT,
    co_sessao          INT NOT NULL,
    ds_cena            VARCHAR(500) NOT NULL,
    nu_ordem           INT NOT NULL,
    CONSTRAINT pk_cena_sessao PRIMARY KEY (co_seq_cena_sessao),
    CONSTRAINT fk_cena_sessao_sessao FOREIGN KEY (co_sessao) REFERENCES tb_sessao (co_seq_sessao) ON DELETE CASCADE
) ENGINE = InnoDB;

CREATE TABLE tb_segredo (
    co_seq_segredo      INT NOT NULL AUTO_INCREMENT,
    co_campanha         INT NOT NULL,
    co_sessao_criacao   INT NULL,
    co_sessao_revelacao INT NULL,
    ds_segredo          VARCHAR(500) NOT NULL,
    dt_revelacao        DATETIME NULL,
    CONSTRAINT pk_segredo PRIMARY KEY (co_seq_segredo),
    CONSTRAINT fk_segredo_campanha FOREIGN KEY (co_campanha) REFERENCES tb_campanha (co_seq_campanha) ON DELETE CASCADE,
    CONSTRAINT fk_segredo_sessao_criacao FOREIGN KEY (co_sessao_criacao) REFERENCES tb_sessao (co_seq_sessao) ON DELETE SET NULL,
    CONSTRAINT fk_segredo_sessao_revelacao FOREIGN KEY (co_sessao_revelacao) REFERENCES tb_sessao (co_seq_sessao) ON DELETE SET NULL
) ENGINE = InnoDB;

CREATE TABLE tb_sessao_registro (
    co_sessao   INT NOT NULL,
    co_registro INT NOT NULL,
    CONSTRAINT pk_sessao_registro PRIMARY KEY (co_sessao, co_registro),
    CONSTRAINT fk_sessao_registro_sessao FOREIGN KEY (co_sessao) REFERENCES tb_sessao (co_seq_sessao) ON DELETE CASCADE,
    CONSTRAINT fk_sessao_registro_registro FOREIGN KEY (co_registro) REFERENCES tb_registro (co_seq_registro) ON DELETE CASCADE
) ENGINE = InnoDB;

CREATE TABLE tb_anotacao_sessao (
    co_seq_anotacao_sessao INT NOT NULL AUTO_INCREMENT,
    co_sessao              INT NOT NULL,
    dt_anotacao            DATETIME NOT NULL,
    ds_anotacao            TEXT NOT NULL,
    CONSTRAINT pk_anotacao_sessao PRIMARY KEY (co_seq_anotacao_sessao),
    CONSTRAINT fk_anotacao_sessao_sessao FOREIGN KEY (co_sessao) REFERENCES tb_sessao (co_seq_sessao) ON DELETE CASCADE
) ENGINE = InnoDB;

-- ---------------------------------------------------------------
-- Dados de domínio (ids fixos: o código depende deles)
-- ---------------------------------------------------------------

INSERT INTO tb_sistema (co_seq_sistema, no_sistema) VALUES
    (1, 'D&D 5e (2024)'),
    (2, 'Daggerheart');

INSERT INTO tb_tipo_registro (co_seq_tipo_registro, no_tipo_registro) VALUES
    (1, 'NPC'), (2, 'PJ'), (3, 'Criatura'), (4, 'Local'), (5, 'Facção'), (6, 'Item'), (7, 'Missão');

INSERT INTO tb_origem_registro (co_seq_origem_registro, no_origem_registro) VALUES
    (1, 'Criado'), (2, 'Improvisado'), (3, 'Importado');

-- Criatura no D&D 5e (2024)
INSERT INTO tb_modelo_atributo (co_sistema, co_tipo_registro, no_chave, nu_ordem) VALUES
    (1, 3, 'CA', 1),
    (1, 3, 'PV', 2),
    (1, 3, 'Deslocamento', 3),
    (1, 3, 'ND', 4),
    (1, 3, 'XP', 5);

-- Criatura ("Adversário") no Daggerheart
-- conferir termos no livro da Jambô
INSERT INTO tb_modelo_atributo (co_sistema, co_tipo_registro, no_chave, nu_ordem) VALUES
    (2, 3, 'Tipo', 1),
    (2, 3, 'Patamar', 2),
    (2, 3, 'Dificuldade', 3),
    (2, 3, 'Limiar Maior', 4),
    (2, 3, 'Limiar Grave', 5),
    (2, 3, 'PV', 6),
    (2, 3, 'Estresse', 7),
    (2, 3, 'Ataque', 8),
    (2, 3, 'Dano', 9);
