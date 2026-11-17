using Dapper;

namespace DMAegis.Repository;

public interface ISaudeRepository
{
    /// <summary>True se o banco responde a um SELECT 1. Nunca lança exceção.</summary>
    Task<bool> BancoRespondeAsync();
}

public class SaudeRepository : ISaudeRepository
{
    private readonly IConexaoFactory _conexaoFactory;

    public SaudeRepository(IConexaoFactory conexaoFactory)
    {
        _conexaoFactory = conexaoFactory;
    }

    public async Task<bool> BancoRespondeAsync()
    {
        if (!_conexaoFactory.Configurada)
            return false;

        try
        {
            await using var conexao = _conexaoFactory.Criar();
            return await conexao.ExecuteScalarAsync<int>("SELECT 1") == 1;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
