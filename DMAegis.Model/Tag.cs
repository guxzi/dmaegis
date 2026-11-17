namespace DMAegis.Model;

/// <summary>tb_tag</summary>
public class Tag
{
    /// <summary>co_seq_tag</summary>
    public int Id { get; set; }

    /// <summary>co_campanha</summary>
    public int IdCampanha { get; set; }

    /// <summary>no_tag</summary>
    public string Nome { get; set; } = string.Empty;
}
