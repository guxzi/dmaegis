namespace DMAegis.Model;

/// <summary>tb_registro: uma entrada do caderno.</summary>
public class Registro
{
    /// <summary>co_seq_registro</summary>
    public int Id { get; set; }

    /// <summary>co_campanha</summary>
    public int IdCampanha { get; set; }

    /// <summary>co_tipo_registro</summary>
    public int IdTipoRegistro { get; set; }

    /// <summary>co_origem_registro (1 Criado, 2 Improvisado, 3 Importado)</summary>
    public int IdOrigemRegistro { get; set; }

    /// <summary>no_registro</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>ds_resumo</summary>
    public string? Resumo { get; set; }

    /// <summary>ds_registro</summary>
    public string? Descricao { get; set; }

    /// <summary>dt_criacao</summary>
    public DateTime DataCriacao { get; set; }

    /// <summary>dt_atualizacao</summary>
    public DateTime DataAtualizacao { get; set; }
}
