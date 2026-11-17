namespace DMAegis.Model;

/// <summary>tb_modelo_atributo: campos que o formulário mostra para um tipo em um sistema.</summary>
public class ModeloAtributo
{
    /// <summary>co_seq_modelo_atributo</summary>
    public int Id { get; set; }

    /// <summary>co_sistema</summary>
    public int IdSistema { get; set; }

    /// <summary>co_tipo_registro</summary>
    public int IdTipoRegistro { get; set; }

    /// <summary>no_chave</summary>
    public string Chave { get; set; } = string.Empty;

    /// <summary>nu_ordem</summary>
    public int Ordem { get; set; }
}
