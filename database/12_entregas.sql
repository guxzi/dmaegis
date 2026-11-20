-- DMAegis — 12: entregas aos jogadores (handouts)
-- O mestre entrega uma carta, um retrato de NPC ou um mapa achado: um título, um texto e/ou uma imagem
-- da galeria. A entrega aparece na tela dos jogadores (todos, ou só um PJ), que podem rever depois.

USE dmaegis;

CREATE TABLE tb_entrega (
    co_seq_entrega      INT NOT NULL AUTO_INCREMENT,
    co_campanha         INT NOT NULL,
    no_titulo           VARCHAR(120) NOT NULL,
    ds_texto            TEXT NULL,
    co_imagem_registro  INT NULL,
    co_registro_destino INT NULL,
    dt_entrega          DATETIME NOT NULL,
    CONSTRAINT pk_entrega PRIMARY KEY (co_seq_entrega),
    CONSTRAINT fk_entrega_campanha FOREIGN KEY (co_campanha) REFERENCES tb_campanha (co_seq_campanha) ON DELETE CASCADE,
    -- Apagar a imagem da galeria deixa a entrega só com o texto.
    CONSTRAINT fk_entrega_imagem FOREIGN KEY (co_imagem_registro) REFERENCES tb_imagem_registro (co_seq_imagem_registro) ON DELETE SET NULL,
    -- Nulo = para todos os jogadores. Apagar o PJ apaga as entregas só dele.
    CONSTRAINT fk_entrega_destino FOREIGN KEY (co_registro_destino) REFERENCES tb_registro (co_seq_registro) ON DELETE CASCADE,
    INDEX ix_entrega_campanha (co_campanha, co_seq_entrega)
) ENGINE = InnoDB;
