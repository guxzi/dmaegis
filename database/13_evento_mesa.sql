-- DMAegis — 13: diário da mesa
-- Eventos que não ficam em outro lugar (o combate começou, terminou, uma ficha caiu), para o resumo da sessão.
-- As rolagens e as entregas já têm data nas tabelas delas.

USE dmaegis;

CREATE TABLE tb_evento_mesa (
    co_seq_evento_mesa INT NOT NULL AUTO_INCREMENT,
    co_mesa            INT NOT NULL,
    ds_tipo            VARCHAR(20) NOT NULL,
    ds_texto           VARCHAR(300) NOT NULL,
    dt_evento          DATETIME NOT NULL,
    CONSTRAINT pk_evento_mesa PRIMARY KEY (co_seq_evento_mesa),
    CONSTRAINT fk_evento_mesa_mesa FOREIGN KEY (co_mesa) REFERENCES tb_mesa (co_seq_mesa) ON DELETE CASCADE,
    INDEX ix_evento_mesa_mesa (co_mesa, dt_evento)
) ENGINE = InnoDB;
