using Dapper;
using DMAegis.Model;

namespace DMAegis.Repository;

public interface IRolagemMesaRepository
{
    /// <summary>As últimas rolagens da mesa, da mais nova para a mais velha.</summary>
    Task<IEnumerable<RolagemMesa>> ListarRecentesAsync(int idMesa, bool incluirSecretas, int limite);
    Task<int> InserirAsync(RolagemMesa rolagem);
}

public class RolagemMesaRepository : RepositoryBase, IRolagemMesaRepository
{
    public RolagemMesaRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<RolagemMesa>> ListarRecentesAsync(int idMesa, bool incluirSecretas, int limite)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<RolagemMesa>("""
            SELECT co_seq_rolagem_mesa AS Id,
                   co_mesa             AS IdMesa,
                   no_autor            AS Autor,
                   st_mestre           AS Mestre,
                   ds_expressao        AS Expressao,
                   nu_total            AS Total,
                   ds_detalhe          AS Detalhe,
                   st_secreta          AS Secreta,
                   dt_rolagem          AS Data
              FROM tb_rolagem_mesa
             WHERE co_mesa = @idMesa AND (@incluirSecretas OR st_secreta = 0)
             ORDER BY co_seq_rolagem_mesa DESC
             LIMIT @limite
            """, new { idMesa, incluirSecretas, limite });
    }

    public async Task<int> InserirAsync(RolagemMesa rolagem)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.ExecuteScalarAsync<int>("""
            INSERT INTO tb_rolagem_mesa (co_mesa, no_autor, st_mestre, ds_expressao, nu_total, ds_detalhe, st_secreta, dt_rolagem)
            VALUES (@IdMesa, @Autor, @Mestre, @Expressao, @Total, @Detalhe, @Secreta, @Data);
            SELECT LAST_INSERT_ID();
            """, rolagem);
    }
}
