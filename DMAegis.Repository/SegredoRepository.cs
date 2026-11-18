using Dapper;
using DMAegis.Model;

namespace DMAegis.Repository;

public interface ISegredoRepository
{
    /// <summary>
    /// Segredos que a sessão mostra: os da campanha ainda não revelados + os revelados nela mesma.
    /// É isso que faz um segredo pendente "voltar sozinho" nas sessões seguintes.
    /// </summary>
    Task<IEnumerable<Segredo>> ListarDaSessaoAsync(int idCampanha, int idSessao);

    Task<Segredo?> ObterAsync(int id);
    Task<int> InserirAsync(Segredo segredo);

    /// <summary>Grava o texto, a sessão de revelação e a data de revelação.</summary>
    Task AtualizarAsync(Segredo segredo);

    Task ExcluirAsync(int id);
}

public class SegredoRepository : RepositoryBase, ISegredoRepository
{
    private const string Colunas = """
        co_seq_segredo      AS Id,
        co_campanha         AS IdCampanha,
        co_sessao_criacao   AS IdSessaoCriacao,
        co_sessao_revelacao AS IdSessaoRevelacao,
        ds_segredo          AS Texto,
        dt_revelacao        AS DataRevelacao
        """;

    public SegredoRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<Segredo>> ListarDaSessaoAsync(int idCampanha, int idSessao)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<Segredo>($"""
            SELECT {Colunas} FROM tb_segredo
            WHERE co_campanha = @idCampanha
              AND (co_sessao_revelacao IS NULL OR co_sessao_revelacao = @idSessao)
            ORDER BY co_seq_segredo
            """, new { idCampanha, idSessao });
    }

    public async Task<Segredo?> ObterAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QuerySingleOrDefaultAsync<Segredo>(
            $"SELECT {Colunas} FROM tb_segredo WHERE co_seq_segredo = @id", new { id });
    }

    public async Task<int> InserirAsync(Segredo segredo)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.ExecuteScalarAsync<int>("""
            INSERT INTO tb_segredo (co_campanha, co_sessao_criacao, co_sessao_revelacao, ds_segredo, dt_revelacao)
            VALUES (@IdCampanha, @IdSessaoCriacao, @IdSessaoRevelacao, @Texto, @DataRevelacao);
            SELECT LAST_INSERT_ID();
            """, segredo);
    }

    public async Task AtualizarAsync(Segredo segredo)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("""
            UPDATE tb_segredo
            SET ds_segredo = @Texto, co_sessao_revelacao = @IdSessaoRevelacao, dt_revelacao = @DataRevelacao
            WHERE co_seq_segredo = @Id
            """, segredo);
    }

    public async Task ExcluirAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("DELETE FROM tb_segredo WHERE co_seq_segredo = @id", new { id });
    }
}
