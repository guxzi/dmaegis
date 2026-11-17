using MySqlConnector;

namespace DMAegis.Repository;

public abstract class RepositoryBase
{
    protected IConexaoFactory ConexaoFactory { get; }

    protected RepositoryBase(IConexaoFactory conexaoFactory)
    {
        ConexaoFactory = conexaoFactory;
    }

    /// <summary>
    /// Executa uma gravação e converte o erro 1062 (valor duplicado) em RegistroDuplicadoException.
    /// </summary>
    protected async Task<T> ExecutarGravacaoAsync<T>(Func<MySqlConnection, Task<T>> acao)
    {
        try
        {
            await using var conexao = ConexaoFactory.Criar();
            return await acao(conexao);
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            throw new RegistroDuplicadoException(ex);
        }
    }
}
