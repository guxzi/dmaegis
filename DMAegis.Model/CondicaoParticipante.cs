namespace DMAegis.Model;

/// <summary>tb_condicao_participante: uma condição (Caído, Envenenado…) de um participante.</summary>
public class CondicaoParticipante
{
    /// <summary>co_seq_condicao_participante</summary>
    public int Id { get; set; }

    /// <summary>co_participante</summary>
    public int IdParticipante { get; set; }

    /// <summary>no_condicao</summary>
    public string Nome { get; set; } = string.Empty;
}
