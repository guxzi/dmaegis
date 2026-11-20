namespace DMAegis.Model;

/// <summary>tb_entrega: algo que o mestre entregou aos jogadores (handout): título, texto e/ou imagem.</summary>
public class Entrega
{
    /// <summary>co_seq_entrega</summary>
    public int Id { get; set; }

    /// <summary>co_campanha</summary>
    public int IdCampanha { get; set; }

    /// <summary>no_titulo</summary>
    public string Titulo { get; set; } = string.Empty;

    /// <summary>ds_texto (opcional)</summary>
    public string? Texto { get; set; }

    /// <summary>co_imagem_registro (opcional: uma imagem da galeria)</summary>
    public int? IdImagemRegistro { get; set; }

    /// <summary>co_registro_destino (nulo = todos os jogadores; senão, só esse PJ)</summary>
    public int? IdRegistroDestino { get; set; }

    /// <summary>dt_entrega</summary>
    public DateTime Data { get; set; }
}
