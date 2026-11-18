namespace DMAegis.Model;

/// <summary>
/// tb_segredo (passo 4). Pertence à campanha: fica pendente enquanto IdSessaoRevelacao for nulo,
/// e por isso aparece em todas as sessões até ser revelado.
/// </summary>
public class Segredo
{
    /// <summary>co_seq_segredo</summary>
    public int Id { get; set; }

    /// <summary>co_campanha</summary>
    public int IdCampanha { get; set; }

    /// <summary>co_sessao_criacao</summary>
    public int? IdSessaoCriacao { get; set; }

    /// <summary>co_sessao_revelacao (nulo = pendente)</summary>
    public int? IdSessaoRevelacao { get; set; }

    /// <summary>ds_segredo</summary>
    public string Texto { get; set; } = string.Empty;

    /// <summary>dt_revelacao</summary>
    public DateTime? DataRevelacao { get; set; }
}
