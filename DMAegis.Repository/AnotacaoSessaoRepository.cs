using Dapper;
using DMAegis.Model;

namespace DMAegis.Repository;

public interface IAnotacaoSessaoRepository
{
    /// <summary>Anotações da sessão em ordem cronológica.</summary>
    Task<IEnumerable<AnotacaoSessao>> ListarPorSessaoAsync(int idSessao);
    Task<AnotacaoSessao?> ObterAsync(int id);
    Task<int> InserirAsync(AnotacaoSessao anotacao);
    Task ExcluirAsync(int id);
}

public class AnotacaoSessaoRepository : RepositoryBase, IAnotacaoSessaoRepository
{
    private const string Colunas = """
        co_seq_anotacao_sessao AS Id,
        co_sessao              AS IdSessao,
        dt_anotacao            AS Data,
        ds_anotacao            AS Texto
        """;

    public AnotacaoSessaoRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<AnotacaoSessao>> ListarPorSessaoAsync(int idSessao)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<AnotacaoSessao>(
            $"SELECT {Colunas} FROM tb_anotacao_sessao WHERE co_sessao = @idSessao ORDER BY dt_anotacao, co_seq_anotacao_sessao",
            new { idSessao });
    }

    public async Task<AnotacaoSessao?> ObterAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QuerySingleOrDefaultAsync<AnotacaoSessao>(
            $"SELECT {Colunas} FROM tb_anotacao_sessao WHERE co_seq_anotacao_sessao = @id", new { id });
    }

    public async Task<int> InserirAsync(AnotacaoSessao anotacao)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.ExecuteScalarAsync<int>("""
            INSERT INTO tb_anotacao_sessao (co_sessao, dt_anotacao, ds_anotacao) VALUES (@IdSessao, @Data, @Texto);
            SELECT LAST_INSERT_ID();
            """, anotacao);
    }

    public async Task ExcluirAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("DELETE FROM tb_anotacao_sessao WHERE co_seq_anotacao_sessao = @id", new { id });
    }
}
