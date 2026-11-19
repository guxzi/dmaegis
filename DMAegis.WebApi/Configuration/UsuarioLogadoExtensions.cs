using System.Security.Claims;
using DMAegis.Services;

namespace DMAegis.WebApi.Configuration;

public static class UsuarioLogadoExtensions
{
    /// <summary>Id do usuário logado (claim NameIdentifier gravada no login).</summary>
    public static int ObterIdUsuario(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? throw new InvalidOperationException("Usuário sem a claim NameIdentifier."));

    public const string ClaimIdCampanha = "dmaegis:campanha";
    public const string ClaimIdRegistro = "dmaegis:registro";
    public const string ClaimMarca = "dmaegis:marca";
    public const string ClaimNomeCampanha = "dmaegis:nome-campanha";

    /// <summary>Claims do cookie de jogador, gravadas ao entrar pelo link.</summary>
    public static ClaimsPrincipal ParaPrincipalDeJogador(this JogadorLogadoServiceModel jogador, string esquema) =>
        new(new ClaimsIdentity(
        [
            new Claim(ClaimIdCampanha, jogador.IdCampanha.ToString()),
            new Claim(ClaimIdRegistro, jogador.IdRegistro.ToString()),
            new Claim(ClaimMarca, jogador.Marca),
            new Claim(ClaimTypes.Name, jogador.NomePersonagem),
            new Claim(ClaimNomeCampanha, jogador.NomeCampanha),
        ], esquema));

    /// <summary>O jogador do cookie de convite.</summary>
    public static JogadorLogadoServiceModel ObterJogador(this ClaimsPrincipal user) => new()
    {
        IdCampanha = int.Parse(user.FindFirstValue(ClaimIdCampanha) ?? throw new InvalidOperationException("Jogador sem campanha.")),
        IdRegistro = int.Parse(user.FindFirstValue(ClaimIdRegistro) ?? throw new InvalidOperationException("Jogador sem personagem.")),
        Marca = user.FindFirstValue(ClaimMarca) ?? string.Empty,
        NomePersonagem = user.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
        NomeCampanha = user.FindFirstValue(ClaimNomeCampanha) ?? string.Empty,
    };
}
