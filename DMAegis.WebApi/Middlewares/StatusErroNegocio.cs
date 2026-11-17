using DMAegis.Services.Exceptions;

namespace DMAegis.WebApi.Middlewares;

/// <summary>
/// Status HTTP de cada erro de negócio. O Service não conhece HTTP; o mapeamento fica aqui.
/// Código que não está na tabela → 400.
/// </summary>
public static class StatusErroNegocio
{
    private static readonly Dictionary<int, int> Mapa = new()
    {
        [BusinessErrors.EmailJaCadastrado.Code] = StatusCodes.Status409Conflict,
        [BusinessErrors.CredenciaisInvalidas.Code] = StatusCodes.Status401Unauthorized,
    };

    public static int Obter(int code) =>
        Mapa.TryGetValue(code, out var status) ? status : StatusCodes.Status400BadRequest;
}
