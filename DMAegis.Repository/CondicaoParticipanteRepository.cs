using Dapper;
using DMAegis.Model;

namespace DMAegis.Repository;

public interface ICondicaoParticipanteRepository
{
    /// <summary>Condições de todos os participantes da mesa, em ordem alfabética.</summary>
    Task<IEnumerable<CondicaoParticipante>> ListarPorMesaAsync(int idMesa);

    /// <summary>Troca as condições do participante por estas (use dentro de uma transação).</summary>
    Task SubstituirAsync(int idParticipante, IEnumerable<string> nomes);
}

public class CondicaoParticipanteRepository : RepositoryBase, ICondicaoParticipanteRepository
{
    public CondicaoParticipanteRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<CondicaoParticipante>> ListarPorMesaAsync(int idMesa)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<CondicaoParticipante>("""
            SELECT c.co_seq_condicao_participante AS Id,
                   c.co_participante              AS IdParticipante,
                   c.no_condicao                  AS Nome
              FROM tb_condicao_participante c
              JOIN tb_participante_mesa p ON p.co_seq_participante_mesa = c.co_participante
             WHERE p.co_mesa = @idMesa
             ORDER BY c.no_condicao
            """, new { idMesa });
    }

    public async Task SubstituirAsync(int idParticipante, IEnumerable<string> nomes)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync(
            "DELETE FROM tb_condicao_participante WHERE co_participante = @idParticipante", new { idParticipante });
        foreach (var nome in nomes)
        {
            await conexao.ExecuteAsync(
                "INSERT INTO tb_condicao_participante (co_participante, no_condicao) VALUES (@idParticipante, @nome)",
                new { idParticipante, nome });
        }
    }
}
