using Dapper;
using DMAegis.Model;

namespace DMAegis.Repository;

public interface IParticipanteMesaRepository
{
    /// <summary>Participantes da mesa na ordem de criação.</summary>
    Task<IEnumerable<ParticipanteMesa>> ListarPorMesaAsync(int idMesa);
    Task<ParticipanteMesa?> ObterAsync(int id);
    Task<int> ContarPorMesaAsync(int idMesa);
    Task<int> InserirAsync(ParticipanteMesa participante);

    /// <summary>Tudo menos a posição e a mesa.</summary>
    Task AtualizarAsync(ParticipanteMesa participante);

    /// <summary>Só a posição: é o que o arrasto grava.</summary>
    Task AtualizarPosicaoAsync(int id, int? coluna, int? linha);

    Task AtualizarIniciativaAsync(int id, int? iniciativa);
    Task ExcluirAsync(int id);
}

public class ParticipanteMesaRepository : RepositoryBase, IParticipanteMesaRepository
{
    private const string Colunas = """
        co_seq_participante_mesa AS Id,
        co_mesa                  AS IdMesa,
        co_registro              AS IdRegistro,
        no_participante          AS Nome,
        st_jogador               AS Jogador,
        nu_iniciativa            AS Iniciativa,
        nu_bonus_iniciativa      AS BonusIniciativa,
        nu_ca                    AS Ca,
        nu_pv_atual              AS PvAtual,
        nu_pv_maximo             AS PvMaximo,
        nu_coluna                AS Coluna,
        nu_linha                 AS Linha,
        nu_tamanho               AS Tamanho,
        st_visivel               AS Visivel,
        nu_ordem                 AS Ordem
        """;

    public ParticipanteMesaRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<ParticipanteMesa>> ListarPorMesaAsync(int idMesa)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<ParticipanteMesa>(
            $"SELECT {Colunas} FROM tb_participante_mesa WHERE co_mesa = @idMesa ORDER BY nu_ordem, co_seq_participante_mesa",
            new { idMesa });
    }

    public async Task<ParticipanteMesa?> ObterAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QuerySingleOrDefaultAsync<ParticipanteMesa>(
            $"SELECT {Colunas} FROM tb_participante_mesa WHERE co_seq_participante_mesa = @id", new { id });
    }

    public async Task<int> ContarPorMesaAsync(int idMesa)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM tb_participante_mesa WHERE co_mesa = @idMesa", new { idMesa });
    }

    public async Task<int> InserirAsync(ParticipanteMesa participante)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.ExecuteScalarAsync<int>("""
            INSERT INTO tb_participante_mesa (co_mesa, co_registro, no_participante, st_jogador, nu_iniciativa,
                                              nu_bonus_iniciativa, nu_ca, nu_pv_atual, nu_pv_maximo, nu_coluna,
                                              nu_linha, nu_tamanho, st_visivel, nu_ordem)
            VALUES (@IdMesa, @IdRegistro, @Nome, @Jogador, @Iniciativa,
                    @BonusIniciativa, @Ca, @PvAtual, @PvMaximo, @Coluna,
                    @Linha, @Tamanho, @Visivel, @Ordem);
            SELECT LAST_INSERT_ID();
            """, participante);
    }

    public async Task AtualizarAsync(ParticipanteMesa participante)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("""
            UPDATE tb_participante_mesa
               SET no_participante = @Nome, nu_iniciativa = @Iniciativa, nu_bonus_iniciativa = @BonusIniciativa,
                   nu_ca = @Ca, nu_pv_atual = @PvAtual, nu_pv_maximo = @PvMaximo, nu_tamanho = @Tamanho,
                   st_visivel = @Visivel, nu_ordem = @Ordem
             WHERE co_seq_participante_mesa = @Id
            """, participante);
    }

    public async Task AtualizarPosicaoAsync(int id, int? coluna, int? linha)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync(
            "UPDATE tb_participante_mesa SET nu_coluna = @coluna, nu_linha = @linha WHERE co_seq_participante_mesa = @id",
            new { id, coluna, linha });
    }

    public async Task AtualizarIniciativaAsync(int id, int? iniciativa)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync(
            "UPDATE tb_participante_mesa SET nu_iniciativa = @iniciativa WHERE co_seq_participante_mesa = @id",
            new { id, iniciativa });
    }

    public async Task ExcluirAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("DELETE FROM tb_participante_mesa WHERE co_seq_participante_mesa = @id", new { id });
    }
}
