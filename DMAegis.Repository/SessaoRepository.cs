using Dapper;
using DMAegis.Model;

namespace DMAegis.Repository;

public interface ISessaoRepository
{
    Task<IEnumerable<Sessao>> ListarPorCampanhaAsync(int idCampanha);
    Task<Sessao?> ObterAsync(int id);

    /// <summary>Maior número de sessão da campanha (0 se não houver nenhuma).</summary>
    Task<int> ObterMaiorNumeroAsync(int idCampanha);

    /// <summary>Número repetido na campanha → RegistroDuplicadoException.</summary>
    Task<int> InserirAsync(Sessao sessao);

    /// <summary>Número repetido na campanha → RegistroDuplicadoException.</summary>
    Task AtualizarAsync(Sessao sessao);

    /// <summary>Apaga cenas, vínculos e anotações em cascata; segredos ficam com a sessão nula.</summary>
    Task ExcluirAsync(int id);
}

public class SessaoRepository : RepositoryBase, ISessaoRepository
{
    private const string Colunas = """
        co_seq_sessao        AS Id,
        co_campanha          AS IdCampanha,
        nu_sessao            AS Numero,
        dt_sessao            AS Data,
        ds_notas_personagens AS NotasPersonagens,
        ds_comeco_forte      AS ComecoForte,
        ds_resumo            AS Resumo
        """;

    public SessaoRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<Sessao>> ListarPorCampanhaAsync(int idCampanha)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<Sessao>(
            $"SELECT {Colunas} FROM tb_sessao WHERE co_campanha = @idCampanha ORDER BY nu_sessao DESC", new { idCampanha });
    }

    public async Task<Sessao?> ObterAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QuerySingleOrDefaultAsync<Sessao>(
            $"SELECT {Colunas} FROM tb_sessao WHERE co_seq_sessao = @id", new { id });
    }

    public async Task<int> ObterMaiorNumeroAsync(int idCampanha)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.ExecuteScalarAsync<int>(
            "SELECT COALESCE(MAX(nu_sessao), 0) FROM tb_sessao WHERE co_campanha = @idCampanha", new { idCampanha });
    }

    public Task<int> InserirAsync(Sessao sessao) =>
        ExecutarGravacaoAsync(conexao => conexao.ExecuteScalarAsync<int>("""
            INSERT INTO tb_sessao (co_campanha, nu_sessao, dt_sessao, ds_notas_personagens, ds_comeco_forte, ds_resumo)
            VALUES (@IdCampanha, @Numero, @Data, @NotasPersonagens, @ComecoForte, @Resumo);
            SELECT LAST_INSERT_ID();
            """, sessao));

    public Task AtualizarAsync(Sessao sessao) =>
        ExecutarGravacaoAsync(conexao => conexao.ExecuteAsync("""
            UPDATE tb_sessao
            SET nu_sessao = @Numero, dt_sessao = @Data, ds_notas_personagens = @NotasPersonagens,
                ds_comeco_forte = @ComecoForte, ds_resumo = @Resumo
            WHERE co_seq_sessao = @Id
            """, sessao));

    public async Task ExcluirAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("DELETE FROM tb_sessao WHERE co_seq_sessao = @id", new { id });
    }
}

public interface ICenaSessaoRepository
{
    Task<IEnumerable<CenaSessao>> ListarPorSessaoAsync(int idSessao);

    /// <summary>Troca todas as cenas da sessão por estas.</summary>
    Task SubstituirAsync(int idSessao, IEnumerable<CenaSessao> cenas);
}

public class CenaSessaoRepository : RepositoryBase, ICenaSessaoRepository
{
    public CenaSessaoRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<CenaSessao>> ListarPorSessaoAsync(int idSessao)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<CenaSessao>("""
            SELECT co_seq_cena_sessao AS Id, co_sessao AS IdSessao, ds_cena AS Descricao, nu_ordem AS Ordem
            FROM tb_cena_sessao WHERE co_sessao = @idSessao ORDER BY nu_ordem
            """, new { idSessao });
    }

    public async Task SubstituirAsync(int idSessao, IEnumerable<CenaSessao> cenas)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("DELETE FROM tb_cena_sessao WHERE co_sessao = @idSessao", new { idSessao });
        var linhas = cenas.Select(c => new { idSessao, c.Descricao, c.Ordem }).ToList();
        if (linhas.Count > 0)
            await conexao.ExecuteAsync(
                "INSERT INTO tb_cena_sessao (co_sessao, ds_cena, nu_ordem) VALUES (@idSessao, @Descricao, @Ordem)", linhas);
    }
}

public interface ISessaoRegistroRepository
{
    /// <summary>Ids dos registros ligados à sessão (passos 5 a 8), na ordem em que foram gravados.</summary>
    Task<IEnumerable<int>> ListarIdsRegistroAsync(int idSessao);

    /// <summary>Troca todos os vínculos da sessão por estes.</summary>
    Task SubstituirAsync(int idSessao, IEnumerable<int> idsRegistro);
}

public class SessaoRegistroRepository : RepositoryBase, ISessaoRegistroRepository
{
    public SessaoRegistroRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<int>> ListarIdsRegistroAsync(int idSessao)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<int>(
            "SELECT co_registro FROM tb_sessao_registro WHERE co_sessao = @idSessao ORDER BY co_registro", new { idSessao });
    }

    public async Task SubstituirAsync(int idSessao, IEnumerable<int> idsRegistro)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("DELETE FROM tb_sessao_registro WHERE co_sessao = @idSessao", new { idSessao });
        var linhas = idsRegistro.Distinct().Select(idRegistro => new { idSessao, idRegistro }).ToList();
        if (linhas.Count > 0)
            await conexao.ExecuteAsync(
                "INSERT INTO tb_sessao_registro (co_sessao, co_registro) VALUES (@idSessao, @idRegistro)", linhas);
    }
}
