namespace DMAegis.Model;

/// <summary>tb_campanha</summary>
public class Campanha
{
    /// <summary>co_seq_campanha</summary>
    public int Id { get; set; }

    /// <summary>co_usuario (dono)</summary>
    public int IdUsuario { get; set; }

    /// <summary>co_sistema (não muda depois de criada)</summary>
    public int IdSistema { get; set; }

    /// <summary>no_campanha</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>ds_campanha</summary>
    public string? Descricao { get; set; }

    /// <summary>st_arquivada</summary>
    public bool Arquivada { get; set; }

    /// <summary>dt_criacao</summary>
    public DateTime DataCriacao { get; set; }
}
