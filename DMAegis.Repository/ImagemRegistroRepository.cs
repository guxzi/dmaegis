using Dapper;
using DMAegis.Model;

namespace DMAegis.Repository;

/// <summary>Metadados de uma imagem com o nome e o tipo do registro dono (galeria da campanha).</summary>
public class ImagemDaCampanha
{
    public int Id { get; set; }
    public int IdRegistro { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string TipoConteudo { get; set; } = string.Empty;
    public int Tamanho { get; set; }
    public int Ordem { get; set; }
    public string NomeRegistro { get; set; } = string.Empty;
    public int IdTipoRegistro { get; set; }
}

public interface IImagemRegistroRepository
{
    /// <summary>Todas as imagens da campanha (sem o conteúdo), por nome do registro e ordem.</summary>
    Task<IEnumerable<ImagemDaCampanha>> ListarPorCampanhaAsync(int idCampanha);

    /// <summary>Metadados das imagens do registro, em ordem (sem o conteúdo).</summary>
    Task<IEnumerable<ImagemRegistro>> ListarPorRegistroAsync(int idRegistro);
    Task<int> ContarPorRegistroAsync(int idRegistro);

    /// <summary>Metadados de uma imagem (sem o conteúdo).</summary>
    Task<ImagemRegistro?> ObterMetadadosAsync(int id);

    Task<(string TipoConteudo, byte[] Conteudo)?> ObterConteudoAsync(int id);
    Task<int> InserirAsync(ImagemRegistro imagem);
    Task ExcluirAsync(int id);
}

public class ImagemRegistroRepository : RepositoryBase, IImagemRegistroRepository
{
    private const string Metadados = """
        co_seq_imagem_registro AS Id,
        co_registro            AS IdRegistro,
        no_imagem              AS Titulo,
        ds_tipo_conteudo       AS TipoConteudo,
        nu_tamanho             AS Tamanho,
        nu_ordem               AS Ordem,
        dt_criacao             AS DataCriacao
        """;

    public ImagemRegistroRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<ImagemDaCampanha>> ListarPorCampanhaAsync(int idCampanha)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<ImagemDaCampanha>("""
            SELECT i.co_seq_imagem_registro AS Id, i.co_registro AS IdRegistro, i.no_imagem AS Titulo,
                   i.ds_tipo_conteudo AS TipoConteudo, i.nu_tamanho AS Tamanho, i.nu_ordem AS Ordem,
                   r.no_registro AS NomeRegistro, r.co_tipo_registro AS IdTipoRegistro
            FROM tb_imagem_registro i
            JOIN tb_registro r ON r.co_seq_registro = i.co_registro
            WHERE r.co_campanha = @idCampanha
            ORDER BY r.no_registro, i.nu_ordem, i.co_seq_imagem_registro
            """, new { idCampanha });
    }

    public async Task<IEnumerable<ImagemRegistro>> ListarPorRegistroAsync(int idRegistro)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<ImagemRegistro>(
            $"SELECT {Metadados} FROM tb_imagem_registro WHERE co_registro = @idRegistro ORDER BY nu_ordem, co_seq_imagem_registro",
            new { idRegistro });
    }

    public async Task<int> ContarPorRegistroAsync(int idRegistro)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM tb_imagem_registro WHERE co_registro = @idRegistro", new { idRegistro });
    }

    public async Task<ImagemRegistro?> ObterMetadadosAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QuerySingleOrDefaultAsync<ImagemRegistro>(
            $"SELECT {Metadados} FROM tb_imagem_registro WHERE co_seq_imagem_registro = @id", new { id });
    }

    public async Task<(string TipoConteudo, byte[] Conteudo)?> ObterConteudoAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        var linha = await conexao.QuerySingleOrDefaultAsync<ImagemRegistro>("""
            SELECT ds_tipo_conteudo AS TipoConteudo, bi_conteudo AS Conteudo
            FROM tb_imagem_registro WHERE co_seq_imagem_registro = @id
            """, new { id });
        return linha is null ? null : (linha.TipoConteudo, linha.Conteudo);
    }

    public async Task<int> InserirAsync(ImagemRegistro imagem)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.ExecuteScalarAsync<int>("""
            INSERT INTO tb_imagem_registro (co_registro, no_imagem, ds_tipo_conteudo, nu_tamanho, nu_ordem, dt_criacao, bi_conteudo)
            VALUES (@IdRegistro, @Titulo, @TipoConteudo, @Tamanho, @Ordem, @DataCriacao, @Conteudo);
            SELECT LAST_INSERT_ID();
            """, imagem);
    }

    public async Task ExcluirAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("DELETE FROM tb_imagem_registro WHERE co_seq_imagem_registro = @id", new { id });
    }
}
