namespace DMAegis.Model;

/// <summary>tb_tipo_registro (domínio: 1 NPC, 2 PJ, 3 Criatura, 4 Local, 5 Facção, 6 Item, 7 Missão)</summary>
public class TipoRegistro
{
    /// <summary>co_seq_tipo_registro</summary>
    public int Id { get; set; }

    /// <summary>no_tipo_registro</summary>
    public string Nome { get; set; } = string.Empty;
}
