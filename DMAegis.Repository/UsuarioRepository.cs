using Dapper;
using DMAegis.Model;

namespace DMAegis.Repository;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<Usuario?> ObterPorIdAsync(int id);

    /// <summary>Insere e devolve o id gerado. E-mail repetido → RegistroDuplicadoException.</summary>
    Task<int> InserirAsync(Usuario usuario);
}

public class UsuarioRepository : RepositoryBase, IUsuarioRepository
{
    private const string Colunas = """
        co_seq_usuario AS Id,
        no_usuario     AS Nome,
        ds_email       AS Email,
        ds_hash_senha  AS HashSenha,
        dt_criacao     AS DataCriacao
        """;

    public UsuarioRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QuerySingleOrDefaultAsync<Usuario>(
            $"SELECT {Colunas} FROM tb_usuario WHERE ds_email = @email", new { email });
    }

    public async Task<Usuario?> ObterPorIdAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QuerySingleOrDefaultAsync<Usuario>(
            $"SELECT {Colunas} FROM tb_usuario WHERE co_seq_usuario = @id", new { id });
    }

    public Task<int> InserirAsync(Usuario usuario) =>
        ExecutarGravacaoAsync(conexao => conexao.ExecuteScalarAsync<int>("""
            INSERT INTO tb_usuario (no_usuario, ds_email, ds_hash_senha, dt_criacao)
            VALUES (@Nome, @Email, @HashSenha, @DataCriacao);
            SELECT LAST_INSERT_ID();
            """, usuario));
}
