-- DMAegis — 07: jogadores pelo link (mesa virtual, entrega 2)
-- Design: docs/design/mesa-virtual.md (seções 6 e 8).
-- Um convite por personagem de jogador. O banco guarda só o hash SHA-256 do código do link;
-- gerar de novo troca o hash e o link antigo para de funcionar.

USE dmaegis;

CREATE TABLE tb_convite_jogador (
    co_seq_convite_jogador INT NOT NULL AUTO_INCREMENT,
    co_campanha            INT NOT NULL,
    co_registro            INT NOT NULL,
    ds_hash_codigo         CHAR(64) NOT NULL,
    dt_criacao             DATETIME NOT NULL,
    dt_ultimo_acesso       DATETIME NULL,
    CONSTRAINT pk_convite_jogador PRIMARY KEY (co_seq_convite_jogador),
    CONSTRAINT uq_convite_jogador_registro UNIQUE (co_registro),
    CONSTRAINT uq_convite_jogador_hash UNIQUE (ds_hash_codigo),
    CONSTRAINT fk_convite_jogador_campanha FOREIGN KEY (co_campanha) REFERENCES tb_campanha (co_seq_campanha) ON DELETE CASCADE,
    CONSTRAINT fk_convite_jogador_registro FOREIGN KEY (co_registro) REFERENCES tb_registro (co_seq_registro) ON DELETE CASCADE
) ENGINE = InnoDB;

-- Se os jogadores podem mover a própria ficha na mesa.
ALTER TABLE tb_mesa ADD COLUMN st_movimento_livre TINYINT(1) NOT NULL DEFAULT 1 AFTER st_grade_visivel;
