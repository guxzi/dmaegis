using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace DMAegis.Repository;

public interface IConexaoFactory
{
    /// <summary>False quando a connection string não foi configurada no .env.</summary>
    bool Configurada { get; }

    MySqlConnection Criar();
}

public class ConexaoFactory : IConexaoFactory
{
    private readonly string? _connectionString;

    public ConexaoFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    public bool Configurada => !string.IsNullOrWhiteSpace(_connectionString);

    public MySqlConnection Criar()
    {
        if (!Configurada)
            throw new InvalidOperationException("ConnectionStrings__Default não configurada no .env.");

        return new MySqlConnection(_connectionString);
    }
}
