using System.Security.Claims;

namespace DMAegis.WebApi.Configuration;

public static class UsuarioLogadoExtensions
{
    /// <summary>Id do usuário logado (claim NameIdentifier gravada no login).</summary>
    public static int ObterIdUsuario(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? throw new InvalidOperationException("Usuário sem a claim NameIdentifier."));
}
