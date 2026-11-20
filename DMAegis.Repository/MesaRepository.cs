using Dapper;
using DMAegis.Model;

namespace DMAegis.Repository;

public interface IMesaRepository
{
    /// <summary>Mesas da campanha, a ativa primeiro e depois pelo nome.</summary>
    Task<IEnumerable<Mesa>> ListarPorCampanhaAsync(int idCampanha);
    Task<Mesa?> ObterAsync(int id);
    Task<Mesa?> ObterAtivaAsync(int idCampanha);
    Task<int> InserirAsync(Mesa mesa);

    /// <summary>Nome, mapa e grade.</summary>
    Task AtualizarAsync(Mesa mesa);

    /// <summary>Marca esta mesa como a ativa da campanha e desmarca as outras (um só comando).</summary>
    Task DefinirAtivaAsync(int idCampanha, int idMesa);

    Task AtualizarCombateAsync(int id, bool emCombate, int rodada, int? idParticipanteVez);
    Task ExcluirAsync(int id);
}

public class MesaRepository : RepositoryBase, IMesaRepository
{
    private const string Colunas = """
        co_seq_mesa         AS Id,
        co_campanha         AS IdCampanha,
        no_mesa             AS Nome,
        co_imagem_registro  AS IdImagemRegistro,
        nu_tamanho_celula   AS TamanhoCelula,
        nu_deslocamento_x   AS DeslocamentoX,
        nu_deslocamento_y   AS DeslocamentoY,
        st_grade_visivel    AS GradeVisivel,
        st_movimento_livre  AS MovimentoLivre,
        st_nevoa            AS Nevoa,
        ds_clima            AS Clima,
        st_ativa            AS Ativa,
        st_combate          AS EmCombate,
        nu_rodada           AS Rodada,
        co_participante_vez AS IdParticipanteVez,
        dt_criacao          AS DataCriacao,
        dt_atualizacao      AS DataAtualizacao
        """;

    public MesaRepository(IConexaoFactory conexaoFactory) : base(conexaoFactory) { }

    public async Task<IEnumerable<Mesa>> ListarPorCampanhaAsync(int idCampanha)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryAsync<Mesa>(
            $"SELECT {Colunas} FROM tb_mesa WHERE co_campanha = @idCampanha ORDER BY st_ativa DESC, no_mesa, co_seq_mesa",
            new { idCampanha });
    }

    public async Task<Mesa?> ObterAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QuerySingleOrDefaultAsync<Mesa>(
            $"SELECT {Colunas} FROM tb_mesa WHERE co_seq_mesa = @id", new { id });
    }

    public async Task<Mesa?> ObterAtivaAsync(int idCampanha)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.QueryFirstOrDefaultAsync<Mesa>(
            $"SELECT {Colunas} FROM tb_mesa WHERE co_campanha = @idCampanha AND st_ativa = 1 ORDER BY co_seq_mesa LIMIT 1",
            new { idCampanha });
    }

    public async Task<int> InserirAsync(Mesa mesa)
    {
        await using var conexao = ConexaoFactory.Criar();
        return await conexao.ExecuteScalarAsync<int>("""
            INSERT INTO tb_mesa (co_campanha, no_mesa, co_imagem_registro, nu_tamanho_celula, nu_deslocamento_x,
                                 nu_deslocamento_y, st_grade_visivel, st_movimento_livre, st_nevoa, ds_clima, st_ativa, st_combate, nu_rodada,
                                 dt_criacao, dt_atualizacao)
            VALUES (@IdCampanha, @Nome, @IdImagemRegistro, @TamanhoCelula, @DeslocamentoX,
                    @DeslocamentoY, @GradeVisivel, @MovimentoLivre, @Nevoa, @Clima, @Ativa, 0, 0, @DataCriacao, @DataAtualizacao);
            SELECT LAST_INSERT_ID();
            """, mesa);
    }

    public async Task AtualizarAsync(Mesa mesa)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("""
            UPDATE tb_mesa
               SET no_mesa = @Nome, co_imagem_registro = @IdImagemRegistro, nu_tamanho_celula = @TamanhoCelula,
                   nu_deslocamento_x = @DeslocamentoX, nu_deslocamento_y = @DeslocamentoY,
                   st_grade_visivel = @GradeVisivel, st_movimento_livre = @MovimentoLivre, st_nevoa = @Nevoa, ds_clima = @Clima, dt_atualizacao = @DataAtualizacao
             WHERE co_seq_mesa = @Id
            """, mesa);
    }

    public async Task DefinirAtivaAsync(int idCampanha, int idMesa)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync(
            "UPDATE tb_mesa SET st_ativa = (co_seq_mesa = @idMesa) WHERE co_campanha = @idCampanha",
            new { idCampanha, idMesa });
    }

    public async Task AtualizarCombateAsync(int id, bool emCombate, int rodada, int? idParticipanteVez)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("""
            UPDATE tb_mesa SET st_combate = @emCombate, nu_rodada = @rodada, co_participante_vez = @idParticipanteVez
             WHERE co_seq_mesa = @id
            """, new { id, emCombate, rodada, idParticipanteVez });
    }

    public async Task ExcluirAsync(int id)
    {
        await using var conexao = ConexaoFactory.Criar();
        await conexao.ExecuteAsync("DELETE FROM tb_mesa WHERE co_seq_mesa = @id", new { id });
    }
}
