using Dapper;
using DMAegis.Model;

namespace DMAegis.Repository;

public interface ISistemaRepository
{
    Task<IEnumerable<Sistema>> ListarAsync();
}

public class SistemaRepository : RepositoryBase, ISistemaRepository
{
    public SistemaRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<Sistema>> ListarAsync()
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<Sistema>(
            "SELECT co_seq_sistema AS Id, no_sistema AS Nome FROM tb_sistema ORDER BY co_seq_sistema");
    }
}
