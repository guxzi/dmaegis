using Dapper;
using DMAegis.Model;

namespace DMAegis.Repository;

public interface IConviteJogadorRepository
{
    Task<IEnumerable<ConviteJogador>> ListarPorCampanhaAsync(int idCampanha);
    Task<ConviteJogador?> ObterPorHashAsync(string hashCodigo);
    Task<ConviteJogador?> ObterPorRegistroAsync(int idRegistro);

    /// <summary>Cria o convite do PJ ou troca o código do que já existe (o link antigo para de valer).</summary>
    Task SalvarAsync(ConviteJogador convite);

    Task RegistrarAcessoAsync(int id, DateTime data);
    Task ExcluirPorRegistroAsync(int idRegistro);
}

public class ConviteJogadorRepository : RepositoryBase, IConviteJogadorRepository
{
    private const string Colunas = """
        co_seq_convite_jogador AS Id,
        co_campanha            AS IdCampanha,
        co_registro            AS IdRegistro,
        ds_hash_codigo         AS HashCodigo,
        dt_criacao             AS DataCriacao,
        dt_ultimo_acesso       AS DataUltimoAcesso
        """;

    public ConviteJogadorRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<ConviteJogador>> ListarPorCampanhaAsync(int idCampanha)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<ConviteJogador>(
            $"SELECT {Colunas} FROM tb_convite_jogador WHERE co_campanha = @idCampanha", new { idCampanha });
    }

    public async Task<ConviteJogador?> ObterPorHashAsync(string hashCodigo)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QuerySingleOrDefaultAsync<ConviteJogador>(
            $"SELECT {Colunas} FROM tb_convite_jogador WHERE ds_hash_codigo = @hashCodigo", new { hashCodigo });
    }

    public async Task<ConviteJogador?> ObterPorRegistroAsync(int idRegistro)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QuerySingleOrDefaultAsync<ConviteJogador>(
            $"SELECT {Colunas} FROM tb_convite_jogador WHERE co_registro = @idRegistro", new { idRegistro });
    }

    public async Task SalvarAsync(ConviteJogador convite)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("""
            INSERT INTO tb_convite_jogador (co_campanha, co_registro, ds_hash_codigo, dt_criacao, dt_ultimo_acesso)
            VALUES (@IdCampanha, @IdRegistro, @HashCodigo, @DataCriacao, NULL)
            ON DUPLICATE KEY UPDATE ds_hash_codigo = VALUES(ds_hash_codigo), dt_criacao = VALUES(dt_criacao),
                                    dt_ultimo_acesso = NULL
            """, convite);
    }

    public async Task RegistrarAcessoAsync(int id, DateTime data)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync(
            "UPDATE tb_convite_jogador SET dt_ultimo_acesso = @data WHERE co_seq_convite_jogador = @id", new { id, data });
    }

    public async Task ExcluirPorRegistroAsync(int idRegistro)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("DELETE FROM tb_convite_jogador WHERE co_registro = @idRegistro", new { idRegistro });
    }
}
