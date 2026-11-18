namespace DMAegis.Model;

/// <summary>tb_anotacao_sessao: anotação feita durante a sessão, com o horário.</summary>
public class AnotacaoSessao
{
    /// <summary>co_seq_anotacao_sessao</summary>
    public int Id { get; set; }

    /// <summary>co_sessao</summary>
    public int IdSessao { get; set; }

    /// <summary>dt_anotacao (horário do servidor)</summary>
    public DateTime Data { get; set; }

    /// <summary>ds_anotacao</summary>
    public string Texto { get; set; } = string.Empty;
}
