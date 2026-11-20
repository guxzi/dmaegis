-- DMAegis — 11: clima por cima do mapa (chuva, neve, neblina ou brasas)
-- É só um efeito visual da mesa, que o mestre liga e os jogadores veem. Nulo = sem clima.

USE dmaegis;

ALTER TABLE tb_mesa ADD COLUMN ds_clima VARCHAR(20) NULL AFTER st_nevoa;
