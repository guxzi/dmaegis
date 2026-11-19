namespace DMAegis.Model;

/// <summary>tb_convite_jogador: o link de um personagem de jogador para entrar na mesa.</summary>
public class ConviteJogador
{
    /// <summary>co_seq_convite_jogador</summary>
    public int Id { get; set; }

    /// <summary>co_campanha</summary>
    public int IdCampanha { get; set; }

    /// <summary>co_registro (o PJ)</summary>
    public int IdRegistro { get; set; }

    /// <summary>ds_hash_codigo (SHA-256 do código, em hexadecimal minúsculo)</summary>
    public string HashCodigo { get; set; } = string.Empty;

    /// <summary>dt_criacao</summary>
    public DateTime DataCriacao { get; set; }

    /// <summary>dt_ultimo_acesso</summary>
    public DateTime? DataUltimoAcesso { get; set; }
}
