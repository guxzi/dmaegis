namespace DMAegis.Model;

/// <summary>tb_atributo_registro: par chave/valor de um registro.</summary>
public class AtributoRegistro
{
    /// <summary>co_seq_atributo_registro</summary>
    public int Id { get; set; }

    /// <summary>co_registro</summary>
    public int IdRegistro { get; set; }

    /// <summary>no_chave</summary>
    public string Chave { get; set; } = string.Empty;

    /// <summary>ds_valor</summary>
    public string Valor { get; set; } = string.Empty;

    /// <summary>nu_ordem</summary>
    public int Ordem { get; set; }
}
