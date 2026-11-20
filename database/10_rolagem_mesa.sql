-- DMAegis — 10: rolagens da mesa
-- As rolagens feitas na mesa (pelo mestre ou pelos jogadores) ficam registradas e aparecem para todos.
-- Os dados são rolados no navegador; aqui fica só o resultado. Rolagem secreta só o mestre vê.

USE dmaegis;

CREATE TABLE tb_rolagem_mesa (
    co_seq_rolagem_mesa INT NOT NULL AUTO_INCREMENT,
    co_mesa             INT NOT NULL,
    no_autor            VARCHAR(100) NOT NULL,
    st_mestre           TINYINT(1) NOT NULL DEFAULT 0,
    ds_expressao        VARCHAR(200) NOT NULL,
    nu_total            INT NOT NULL,
    ds_detalhe          VARCHAR(500) NOT NULL,
    st_secreta          TINYINT(1) NOT NULL DEFAULT 0,
    dt_rolagem          DATETIME NOT NULL,
    CONSTRAINT pk_rolagem_mesa PRIMARY KEY (co_seq_rolagem_mesa),
    CONSTRAINT fk_rolagem_mesa_mesa FOREIGN KEY (co_mesa) REFERENCES tb_mesa (co_seq_mesa) ON DELETE CASCADE,
    INDEX ix_rolagem_mesa_mesa (co_mesa, co_seq_rolagem_mesa)
) ENGINE = InnoDB;
