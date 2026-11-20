namespace DMAegis.Model;

/// <summary>tb_evento_mesa: um acontecimento da mesa (combate começou ou terminou, ficha caiu), para o resumo da sessão.</summary>
public class EventoMesa
{
    /// <summary>co_seq_evento_mesa</summary>
    public int Id { get; set; }

    /// <summary>co_mesa</summary>
    public int IdMesa { get; set; }

    /// <summary>ds_tipo ("combate-inicio", "combate-fim" ou "caiu")</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>ds_texto (a frase pronta, por exemplo "Goblin 1 caiu.")</summary>
    public string Texto { get; set; } = string.Empty;

    /// <summary>dt_evento</summary>
    public DateTime Data { get; set; }
}
