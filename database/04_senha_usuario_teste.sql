-- DMAegis — 04: senha do usuário de exemplo (etapa 2)
-- Troca o placeholder do 02 por um hash de verdade (PasswordHasher, PBKDF2).
-- Dado de TESTE, só para o banco local:
--   e-mail: mestre@dmaegis.test
--   senha:  dmaegis-teste

USE dmaegis;

UPDATE tb_usuario
SET ds_hash_senha = 'AQAAAAIAAYagAAAAEHzh6kJ+QcGhjugz+FylzhN7oXKM6JkC6Jo7ndjGKdIbv4EPU+D3zLDyEugHhijWRw=='
WHERE ds_email = 'mestre@dmaegis.test';
