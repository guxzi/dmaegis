namespace DMAegis.Model;

/// <summary>tb_usuario</summary>
public class Usuario
{
    /// <summary>co_seq_usuario</summary>
    public int Id { get; set; }

    /// <summary>no_usuario</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>ds_email (sempre em minúsculas)</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>ds_hash_senha (PasswordHasher)</summary>
    public string HashSenha { get; set; } = string.Empty;

    /// <summary>dt_criacao</summary>
    public DateTime DataCriacao { get; set; }
}
