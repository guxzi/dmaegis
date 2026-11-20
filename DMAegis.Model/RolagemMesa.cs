namespace DMAegis.Model;

/// <summary>tb_rolagem_mesa: uma rolagem de dados feita na mesa, pelo mestre ou por um jogador.</summary>
public class RolagemMesa
{
    /// <summary>co_seq_rolagem_mesa</summary>
    public int Id { get; set; }

    /// <summary>co_mesa</summary>
    public int IdMesa { get; set; }

    /// <summary>no_autor ("Mestre" ou o nome do personagem)</summary>
    public string Autor { get; set; } = string.Empty;

    /// <summary>st_mestre</summary>
    public bool Mestre { get; set; }

    /// <summary>ds_expressao (por exemplo, "2d20kh1+5")</summary>
    public string Expressao { get; set; } = string.Empty;

    /// <summary>nu_total</summary>
    public int Total { get; set; }

    /// <summary>ds_detalhe (por exemplo, "2d20kh1+5 → ([4], 17) + 5 = 22")</summary>
    public string Detalhe { get; set; } = string.Empty;

    /// <summary>st_secreta (só o mestre vê)</summary>
    public bool Secreta { get; set; }

    /// <summary>dt_rolagem</summary>
    public DateTime Data { get; set; }
}
