namespace DMAegis.Model;

/// <summary>tb_celula_revelada: uma célula da grade que o mestre tirou da névoa.</summary>
public class CelulaRevelada
{
    /// <summary>co_seq_celula_revelada</summary>
    public int Id { get; set; }

    /// <summary>co_mesa</summary>
    public int IdMesa { get; set; }

    /// <summary>nu_coluna</summary>
    public int Coluna { get; set; }

    /// <summary>nu_linha</summary>
    public int Linha { get; set; }
}
