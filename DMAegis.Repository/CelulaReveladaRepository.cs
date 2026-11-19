using Dapper;
using DMAegis.Model;

namespace DMAegis.Repository;

public interface ICelulaReveladaRepository
{
    Task<IEnumerable<CelulaRevelada>> ListarPorMesaAsync(int idMesa);

    /// <summary>Revela as células (as que já estavam reveladas são ignoradas). Use dentro de uma transação.</summary>
    Task RevelarAsync(int idMesa, IEnumerable<(int Coluna, int Linha)> celulas);

    /// <summary>Devolve as células à névoa. Use dentro de uma transação.</summary>
    Task EsconderAsync(int idMesa, IEnumerable<(int Coluna, int Linha)> celulas);

    /// <summary>Cobre a mesa inteira de novo.</summary>
    Task EsconderTudoAsync(int idMesa);
}

public class CelulaReveladaRepository : RepositoryBase, ICelulaReveladaRepository
{
    public CelulaReveladaRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<CelulaRevelada>> ListarPorMesaAsync(int idMesa)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<CelulaRevelada>("""
            SELECT co_seq_celula_revelada AS Id, co_mesa AS IdMesa, nu_coluna AS Coluna, nu_linha AS Linha
              FROM tb_celula_revelada
             WHERE co_mesa = @idMesa
             ORDER BY nu_linha, nu_coluna
            """, new { idMesa });
    }

    public async Task RevelarAsync(int idMesa, IEnumerable<(int Coluna, int Linha)> celulas)
    {
        var parametros = celulas.Select(c => new { idMesa, coluna = c.Coluna, linha = c.Linha }).ToList();
        if (parametros.Count == 0)
            return;
        await using var conexao = ConexaoFactory.Criar();
        // O Dapper executa o comando uma vez por item da lista, sempre parametrizado.
        await conexao.ExecuteAsync(
            "INSERT IGNORE INTO tb_celula_revelada (co_mesa, nu_coluna, nu_linha) VALUES (@idMesa, @coluna, @linha)",
            parametros);
    }

    public async Task EsconderAsync(int idMesa, IEnumerable<(int Coluna, int Linha)> celulas)
    {
        var parametros = celulas.Select(c => new { idMesa, coluna = c.Coluna, linha = c.Linha }).ToList();
        if (parametros.Count == 0)
            return;
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync(
            "DELETE FROM tb_celula_revelada WHERE co_mesa = @idMesa AND nu_coluna = @coluna AND nu_linha = @linha",
            parametros);
    }

    public async Task EsconderTudoAsync(int idMesa)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("DELETE FROM tb_celula_revelada WHERE co_mesa = @idMesa", new { idMesa });
    }
}
