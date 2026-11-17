namespace DMAegis.WebApi.ApiModels.Auth;

public class UsuarioResponseApiModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
