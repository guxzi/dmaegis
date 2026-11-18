namespace DMAegis.Model;

/// <summary>tb_imagem_registro: imagem (mapa) anexada a um registro do caderno.</summary>
public class ImagemRegistro
{
    /// <summary>co_seq_imagem_registro</summary>
    public int Id { get; set; }

    /// <summary>co_registro</summary>
    public int IdRegistro { get; set; }

    /// <summary>no_imagem (título)</summary>
    public string Titulo { get; set; } = string.Empty;

    /// <summary>ds_tipo_conteudo (detectado pelos bytes: image/jpeg, image/png, image/webp, image/gif)</summary>
    public string TipoConteudo { get; set; } = string.Empty;

    /// <summary>nu_tamanho (bytes)</summary>
    public int Tamanho { get; set; }

    /// <summary>nu_ordem</summary>
    public int Ordem { get; set; }

    /// <summary>dt_criacao</summary>
    public DateTime DataCriacao { get; set; }

    /// <summary>bi_conteudo (vazio nas consultas de metadados)</summary>
    public byte[] Conteudo { get; set; } = [];
}
