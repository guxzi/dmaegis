using Dapper;
using DMAegis.Model;

namespace DMAegis.Repository;

public interface IEventoMesaRepository
{
    Task InserirAsync(EventoMesa evento);

    /// <summary>Os eventos da mesa a partir de uma data, do mais velho para o mais novo.</summary>
    Task<IEnumerable<EventoMesa>> ListarDesdeAsync(int idMesa, DateTime desde);
}

public class EventoMesaRepository : RepositoryBase, IEventoMesaRepository
{
    public EventoMesaRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task InserirAsync(EventoMesa evento)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("""
            INSERT INTO tb_evento_mesa (co_mesa, ds_tipo, ds_texto, dt_evento)
            VALUES (@IdMesa, @Tipo, @Texto, @Data)
            """, evento);
    }

    public async Task<IEnumerable<EventoMesa>> ListarDesdeAsync(int idMesa, DateTime desde)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<EventoMesa>("""
            SELECT co_seq_evento_mesa AS Id, co_mesa AS IdMesa, ds_tipo AS Tipo, ds_texto AS Texto, dt_evento AS Data
              FROM tb_evento_mesa
             WHERE co_mesa = @idMesa AND dt_evento >= @desde
             ORDER BY dt_evento, co_seq_evento_mesa
            """, new { idMesa, desde });
    }
}
