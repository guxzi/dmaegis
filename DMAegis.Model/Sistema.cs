namespace DMAegis.Model;

/// <summary>tb_sistema (domínio: 1 = D&amp;D 5e (2024), 2 = Daggerheart)</summary>
public class Sistema
{
    /// <summary>co_seq_sistema</summary>
    public int Id { get; set; }

    /// <summary>no_sistema</summary>
    public string Nome { get; set; } = string.Empty;
}
