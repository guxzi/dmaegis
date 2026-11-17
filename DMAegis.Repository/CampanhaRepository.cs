using Dapper;
using DMAegis.Model;

namespace DMAegis.Repository;

public interface ICampanhaRepository
{
    /// <summary>Campanhas do usuário: ativas primeiro, depois por nome.</summary>
    Task<IEnumerable<Campanha>> ListarPorUsuarioAsync(int idUsuario);

    Task<Campanha?> ObterAsync(int id);
    Task<int> InserirAsync(Campanha campanha);

    /// <summary>Grava nome, descrição e arquivada. Nunca altera o sistema nem o dono.</summary>
    Task AtualizarAsync(Campanha campanha);

    /// <summary>Exclui a campanha; o banco apaga em cascata tudo o que pertence a ela.</summary>
    Task ExcluirAsync(int id);
}

public class CampanhaRepository : RepositoryBase, ICampanhaRepository
{
    private const string Colunas = """
        co_seq_campanha AS Id,
        co_usuario      AS IdUsuario,
        co_sistema      AS IdSistema,
        no_campanha     AS Nome,
        ds_campanha     AS Descricao,
        st_arquivada    AS Arquivada,
        dt_criacao      AS DataCriacao
        """;

    public CampanhaRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<Campanha>> ListarPorUsuarioAsync(int idUsuario)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<Campanha>($"""
            SELECT {Colunas} FROM tb_campanha
            WHERE co_usuario = @idUsuario
            ORDER BY st_arquivada, no_campanha
            """, new { idUsuario });
    }

    public async Task<Campanha?> ObterAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QuerySingleOrDefaultAsync<Campanha>(
            $"SELECT {Colunas} FROM tb_campanha WHERE co_seq_campanha = @id", new { id });
    }

    public Task<int> InserirAsync(Campanha campanha) =>
        ExecutarGravacaoAsync(conexao => conexao.ExecuteScalarAsync<int>("""
            INSERT INTO tb_campanha (co_usuario, co_sistema, no_campanha, ds_campanha, st_arquivada, dt_criacao)
            VALUES (@IdUsuario, @IdSistema, @Nome, @Descricao, @Arquivada, @DataCriacao);
            SELECT LAST_INSERT_ID();
            """, campanha));

    public Task AtualizarAsync(Campanha campanha) =>
        ExecutarGravacaoAsync(conexao => conexao.ExecuteAsync("""
            UPDATE tb_campanha
            SET no_campanha = @Nome, ds_campanha = @Descricao, st_arquivada = @Arquivada
            WHERE co_seq_campanha = @Id
            """, campanha));

    public async Task ExcluirAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("DELETE FROM tb_campanha WHERE co_seq_campanha = @id", new { id });
    }
}
