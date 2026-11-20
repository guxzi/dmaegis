using Dapper;
using DMAegis.Model;

namespace DMAegis.Repository;

public interface IEntregaRepository
{
    /// <summary>As entregas da campanha, da mais nova para a mais velha.</summary>
    Task<IEnumerable<Entrega>> ListarPorCampanhaAsync(int idCampanha);

    /// <summary>As que um PJ pode ver: as para todos e as só dele.</summary>
    Task<IEnumerable<Entrega>> ListarDoJogadorAsync(int idCampanha, int idRegistro);

    Task<Entrega?> ObterAsync(int id);
    Task<int> InserirAsync(Entrega entrega);
    Task ExcluirAsync(int id);
}

public class EntregaRepository : RepositoryBase, IEntregaRepository
{
    private const string Colunas = """
        co_seq_entrega      AS Id,
        co_campanha         AS IdCampanha,
        no_titulo           AS Titulo,
        ds_texto            AS Texto,
        co_imagem_registro  AS IdImagemRegistro,
        co_registro_destino AS IdRegistroDestino,
        dt_entrega          AS Data
        """;

    public EntregaRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<Entrega>> ListarPorCampanhaAsync(int idCampanha)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<Entrega>(
            $"SELECT {Colunas} FROM tb_entrega WHERE co_campanha = @idCampanha ORDER BY co_seq_entrega DESC",
            new { idCampanha });
    }

    public async Task<IEnumerable<Entrega>> ListarDoJogadorAsync(int idCampanha, int idRegistro)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<Entrega>($"""
            SELECT {Colunas} FROM tb_entrega
             WHERE co_campanha = @idCampanha AND (co_registro_destino IS NULL OR co_registro_destino = @idRegistro)
             ORDER BY co_seq_entrega DESC
            """, new { idCampanha, idRegistro });
    }

    public async Task<Entrega?> ObterAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QuerySingleOrDefaultAsync<Entrega>(
            $"SELECT {Colunas} FROM tb_entrega WHERE co_seq_entrega = @id", new { id });
    }

    public async Task<int> InserirAsync(Entrega entrega)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.ExecuteScalarAsync<int>("""
            INSERT INTO tb_entrega (co_campanha, no_titulo, ds_texto, co_imagem_registro, co_registro_destino, dt_entrega)
            VALUES (@IdCampanha, @Titulo, @Texto, @IdImagemRegistro, @IdRegistroDestino, @Data);
            SELECT LAST_INSERT_ID();
            """, entrega);
    }

    public async Task ExcluirAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("DELETE FROM tb_entrega WHERE co_seq_entrega = @id", new { id });
    }
}
