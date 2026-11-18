-- DMAegis — 05: imagens (mapas) dos registros do caderno
-- Pedido do usuário em 18/11/2026: anexar mapas a locais e a outros registros.
-- As imagens ficam no próprio banco (MEDIUMBLOB, até 16 MB; a aplicação limita a 15 MB)
-- e somem junto com o registro (CASCADE). Prefixo novo: bi_ (binário).

USE dmaegis;

CREATE TABLE tb_imagem_registro (
    co_seq_imagem_registro INT NOT NULL AUTO_INCREMENT,
    co_registro            INT NOT NULL,
    no_imagem              VARCHAR(150) NOT NULL,
    ds_tipo_conteudo       VARCHAR(30) NOT NULL,
    nu_tamanho             INT NOT NULL,
    nu_ordem               INT NOT NULL,
    dt_criacao             DATETIME NOT NULL,
    bi_conteudo            MEDIUMBLOB NOT NULL,
    CONSTRAINT pk_imagem_registro PRIMARY KEY (co_seq_imagem_registro),
    CONSTRAINT fk_imagem_registro_registro FOREIGN KEY (co_registro) REFERENCES tb_registro (co_seq_registro) ON DELETE CASCADE,
    INDEX ix_imagem_registro_registro (co_registro, nu_ordem)
) ENGINE = InnoDB;
