using Dapper;
using DMAegis.Model;

namespace DMAegis.Repository;

// Repositórios pequenos do caderno (tabelas de domínio e tags). O de registros fica em RegistroRepository.cs.

public interface ITipoRegistroRepository
{
    Task<IEnumerable<TipoRegistro>> ListarAsync();
}

public class TipoRegistroRepository : RepositoryBase, ITipoRegistroRepository
{
    public TipoRegistroRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<TipoRegistro>> ListarAsync()
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<TipoRegistro>(
            "SELECT co_seq_tipo_registro AS Id, no_tipo_registro AS Nome FROM tb_tipo_registro ORDER BY co_seq_tipo_registro");
    }
}

public interface IModeloAtributoRepository
{
    /// <summary>Modelo de atributos do sistema (de um tipo ou de todos), em ordem.</summary>
    Task<IEnumerable<ModeloAtributo>> ListarAsync(int idSistema, int? idTipoRegistro);
}

public class ModeloAtributoRepository : RepositoryBase, IModeloAtributoRepository
{
    public ModeloAtributoRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<ModeloAtributo>> ListarAsync(int idSistema, int? idTipoRegistro)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<ModeloAtributo>("""
            SELECT co_seq_modelo_atributo AS Id, co_sistema AS IdSistema, co_tipo_registro AS IdTipoRegistro,
                   no_chave AS Chave, nu_ordem AS Ordem
            FROM tb_modelo_atributo
            WHERE co_sistema = @idSistema
              AND (@idTipoRegistro IS NULL OR co_tipo_registro = @idTipoRegistro)
            ORDER BY co_tipo_registro, nu_ordem
            """, new { idSistema, idTipoRegistro });
    }
}

public interface ITagRepository
{
    Task<IEnumerable<Tag>> ListarPorCampanhaAsync(int idCampanha);

    /// <summary>Tags da campanha com esses nomes (comparação do collation: sem maiúsculas nem acentos).</summary>
    Task<IEnumerable<Tag>> ListarPorNomesAsync(int idCampanha, IEnumerable<string> nomes);

    Task<int> InserirAsync(Tag tag);
}

public class TagRepository : RepositoryBase, ITagRepository
{
    private const string Colunas = "co_seq_tag AS Id, co_campanha AS IdCampanha, no_tag AS Nome";

    public TagRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<Tag>> ListarPorCampanhaAsync(int idCampanha)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<Tag>(
            $"SELECT {Colunas} FROM tb_tag WHERE co_campanha = @idCampanha ORDER BY no_tag", new { idCampanha });
    }

    public async Task<IEnumerable<Tag>> ListarPorNomesAsync(int idCampanha, IEnumerable<string> nomes)
    {
        var lista = nomes.ToList();
        if (lista.Count == 0)
            return [];

        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<Tag>(
            $"SELECT {Colunas} FROM tb_tag WHERE co_campanha = @idCampanha AND no_tag IN @nomes",
            new { idCampanha, nomes = lista });
    }

    public Task<int> InserirAsync(Tag tag) =>
        ExecutarGravacaoAsync(conexao => conexao.ExecuteScalarAsync<int>("""
            INSERT INTO tb_tag (co_campanha, no_tag) VALUES (@IdCampanha, @Nome);
            SELECT LAST_INSERT_ID();
            """, tag));
}

public interface IRegistroTagRepository
{
    /// <summary>Tags de cada registro (para montar a lista e o detalhe).</summary>
    Task<IEnumerable<(int IdRegistro, Tag Tag)>> ListarPorRegistrosAsync(IEnumerable<int> idsRegistro);

    /// <summary>Troca todas as tags do registro por estas.</summary>
    Task SubstituirAsync(int idRegistro, IEnumerable<int> idsTag);
}

public class RegistroTagRepository : RepositoryBase, IRegistroTagRepository
{
    public RegistroTagRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<(int IdRegistro, Tag Tag)>> ListarPorRegistrosAsync(IEnumerable<int> idsRegistro)
    {
        var ids = idsRegistro.ToList();
        if (ids.Count == 0)
            return [];

        await using var conexao = ConexaoFactory.Criar();
        var linhas = await conexao.QueryAsync<LinhaRegistroTag>("""
            SELECT rt.co_registro AS IdRegistro, t.co_seq_tag AS Id, t.co_campanha AS IdCampanha, t.no_tag AS Nome
            FROM tb_registro_tag rt
            JOIN tb_tag t ON t.co_seq_tag = rt.co_tag
            WHERE rt.co_registro IN @ids
            ORDER BY t.no_tag
            """, new { ids });
        return linhas.Select(l => (l.IdRegistro, new Tag { Id = l.Id, IdCampanha = l.IdCampanha, Nome = l.Nome })).ToList();
    }

    public async Task SubstituirAsync(int idRegistro, IEnumerable<int> idsTag)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("DELETE FROM tb_registro_tag WHERE co_registro = @idRegistro", new { idRegistro });
        var linhas = idsTag.Distinct().Select(idTag => new { idRegistro, idTag }).ToList();
        if (linhas.Count > 0)
            await conexao.ExecuteAsync(
                "INSERT INTO tb_registro_tag (co_registro, co_tag) VALUES (@idRegistro, @idTag)", linhas);
    }

    private sealed class LinhaRegistroTag
    {
        public int IdRegistro { get; set; }
        public int Id { get; set; }
        public int IdCampanha { get; set; }
        public string Nome { get; set; } = string.Empty;
    }
}
