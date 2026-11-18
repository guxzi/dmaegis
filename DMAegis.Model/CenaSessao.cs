namespace DMAegis.Model;

/// <summary>tb_cena_sessao (passo 3)</summary>
public class CenaSessao
{
    /// <summary>co_seq_cena_sessao</summary>
    public int Id { get; set; }

    /// <summary>co_sessao</summary>
    public int IdSessao { get; set; }

    /// <summary>ds_cena</summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>nu_ordem</summary>
    public int Ordem { get; set; }
}
