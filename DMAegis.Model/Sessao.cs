namespace DMAegis.Model;

/// <summary>tb_sessao</summary>
public class Sessao
{
    /// <summary>co_seq_sessao</summary>
    public int Id { get; set; }

    /// <summary>co_campanha</summary>
    public int IdCampanha { get; set; }

    /// <summary>nu_sessao (único na campanha)</summary>
    public int Numero { get; set; }

    /// <summary>dt_sessao (só a data)</summary>
    public DateTime? Data { get; set; }

    /// <summary>ds_notas_personagens (passo 1)</summary>
    public string? NotasPersonagens { get; set; }

    /// <summary>ds_comeco_forte (passo 2)</summary>
    public string? ComecoForte { get; set; }

    /// <summary>ds_resumo (pós-sessão)</summary>
    public string? Resumo { get; set; }
}
