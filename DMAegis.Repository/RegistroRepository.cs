using Dapper;
using DMAegis.Model;

namespace DMAegis.Repository;

/// <summary>Filtros opcionais da lista do caderno. Os que vierem nulos não filtram.</summary>
public class FiltroRegistro
{
    public int? IdTipoRegistro { get; set; }
    public int? IdTag { get; set; }

    /// <summary>Procura no nome e no resumo (busca literal: % e _ não são curingas).</summary>
    public string? Texto { get; set; }
}

public interface IRegistroRepository
{
    Task<IEnumerable<Registro>> ListarAsync(int idCampanha, FiltroRegistro filtro);
    Task<Registro?> ObterAsync(int id);
    Task<int> InserirAsync(Registro registro);
    Task AtualizarAsync(Registro registro);
    Task ExcluirAsync(int id);
}

public class RegistroRepository : RepositoryBase, IRegistroRepository
{
    private const string Colunas = """
        co_seq_registro    AS Id,
        co_campanha        AS IdCampanha,
        co_tipo_registro   AS IdTipoRegistro,
        co_origem_registro AS IdOrigemRegistro,
        no_registro        AS Nome,
        ds_resumo          AS Resumo,
        ds_registro        AS Descricao,
        dt_criacao         AS DataCriacao,
        dt_atualizacao     AS DataAtualizacao
        """;

    public RegistroRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    /// <summary>Escapa os curingas do LIKE para a busca ser literal (usar com ESCAPE '\\').</summary>
    public static string EscaparLike(string texto) =>
        texto.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");

    public async Task<IEnumerable<Registro>> ListarAsync(int idCampanha, FiltroRegistro filtro)
    {
        var padrao = filtro.Texto is null ? null : $"%{EscaparLike(filtro.Texto)}%";

        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<Registro>($"""
            SELECT {Colunas} FROM tb_registro r
            WHERE r.co_campanha = @idCampanha
              AND (@idTipo IS NULL OR r.co_tipo_registro = @idTipo)
              AND (@idTag IS NULL OR EXISTS (
                    SELECT 1 FROM tb_registro_tag rt
                    WHERE rt.co_registro = r.co_seq_registro AND rt.co_tag = @idTag))
              AND (@padrao IS NULL
                   OR r.no_registro LIKE @padrao ESCAPE '\\'
                   OR r.ds_resumo LIKE @padrao ESCAPE '\\')
            ORDER BY r.no_registro
            """, new { idCampanha, idTipo = filtro.IdTipoRegistro, idTag = filtro.IdTag, padrao });
    }

    public async Task<Registro?> ObterAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QuerySingleOrDefaultAsync<Registro>(
            $"SELECT {Colunas} FROM tb_registro WHERE co_seq_registro = @id", new { id });
    }

    public Task<int> InserirAsync(Registro registro) =>
        ExecutarGravacaoAsync(conexao => conexao.ExecuteScalarAsync<int>("""
            INSERT INTO tb_registro (co_campanha, co_tipo_registro, co_origem_registro, no_registro,
                                     ds_resumo, ds_registro, dt_criacao, dt_atualizacao)
            VALUES (@IdCampanha, @IdTipoRegistro, @IdOrigemRegistro, @Nome,
                    @Resumo, @Descricao, @DataCriacao, @DataAtualizacao);
            SELECT LAST_INSERT_ID();
            """, registro));

    public Task AtualizarAsync(Registro registro) =>
        ExecutarGravacaoAsync(conexao => conexao.ExecuteAsync("""
            UPDATE tb_registro
            SET co_tipo_registro = @IdTipoRegistro, no_registro = @Nome, ds_resumo = @Resumo,
                ds_registro = @Descricao, dt_atualizacao = @DataAtualizacao
            WHERE co_seq_registro = @Id
            """, registro));

    public async Task ExcluirAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("DELETE FROM tb_registro WHERE co_seq_registro = @id", new { id });
    }
}

public interface IAtributoRegistroRepository
{
    Task<IEnumerable<AtributoRegistro>> ListarPorRegistroAsync(int idRegistro);

    /// <summary>Troca todos os atributos do registro por estes.</summary>
    Task SubstituirAsync(int idRegistro, IEnumerable<AtributoRegistro> atributos);
}

public class AtributoRegistroRepository : RepositoryBase, IAtributoRegistroRepository
{
    public AtributoRegistroRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<AtributoRegistro>> ListarPorRegistroAsync(int idRegistro)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<AtributoRegistro>("""
            SELECT co_seq_atributo_registro AS Id, co_registro AS IdRegistro,
                   no_chave AS Chave, ds_valor AS Valor, nu_ordem AS Ordem
            FROM tb_atributo_registro
            WHERE co_registro = @idRegistro
            ORDER BY nu_ordem
            """, new { idRegistro });
    }

    public async Task SubstituirAsync(int idRegistro, IEnumerable<AtributoRegistro> atributos)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("DELETE FROM tb_atributo_registro WHERE co_registro = @idRegistro", new { idRegistro });
        var linhas = atributos.Select(a => new { idRegistro, a.Chave, a.Valor, a.Ordem }).ToList();
        if (linhas.Count > 0)
            await conexao.ExecuteAsync("""
                INSERT INTO tb_atributo_registro (co_registro, no_chave, ds_valor, nu_ordem)
                VALUES (@idRegistro, @Chave, @Valor, @Ordem)
                """, linhas);
    }
}
