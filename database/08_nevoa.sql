-- DMAegis — 08: névoa de guerra (mesa virtual, entrega 3)
-- Design: docs/superpowers/specs/2026-11-19-dmaegis-mesa-virtual-design.md (seções 6 e 9).
-- Com a névoa ligada, os jogadores só veem as células reveladas pelo mestre.

USE dmaegis;

CREATE TABLE tb_celula_revelada (
    co_seq_celula_revelada INT NOT NULL AUTO_INCREMENT,
    co_mesa                INT NOT NULL,
    nu_coluna              INT NOT NULL,
    nu_linha               INT NOT NULL,
    CONSTRAINT pk_celula_revelada PRIMARY KEY (co_seq_celula_revelada),
    CONSTRAINT uq_celula_revelada UNIQUE (co_mesa, nu_coluna, nu_linha),
    CONSTRAINT fk_celula_revelada_mesa FOREIGN KEY (co_mesa) REFERENCES tb_mesa (co_seq_mesa) ON DELETE CASCADE
) ENGINE = InnoDB;

ALTER TABLE tb_mesa ADD COLUMN st_nevoa TINYINT(1) NOT NULL DEFAULT 0 AFTER st_movimento_livre;
