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
        [BusinessErrors.CampanhaNaoEncontrada.Code] = StatusCodes.Status404NotFound,
        [BusinessErrors.RegistroNaoEncontrado.Code] = StatusCodes.Status404NotFound,
        [BusinessErrors.SessaoNumeroDuplicado.Code] = StatusCodes.Status409Conflict,
        [BusinessErrors.SessaoNaoEncontrada.Code] = StatusCodes.Status404NotFound,
        [BusinessErrors.SegredoNaoEncontrado.Code] = StatusCodes.Status404NotFound,
        [BusinessErrors.AnotacaoNaoEncontrada.Code] = StatusCodes.Status404NotFound,
        [BusinessErrors.ImagemNaoEncontrada.Code] = StatusCodes.Status404NotFound,
        [BusinessErrors.MesaNaoEncontrada.Code] = StatusCodes.Status404NotFound,
        [BusinessErrors.ParticipanteNaoEncontrado.Code] = StatusCodes.Status404NotFound,
        [BusinessErrors.ConviteInvalido.Code] = StatusCodes.Status404NotFound,
        [BusinessErrors.AcessoJogadorExpirado.Code] = StatusCodes.Status401Unauthorized,
        [BusinessErrors.MovimentoBloqueado.Code] = StatusCodes.Status403Forbidden,
    };

    public static int Obter(int code) =>
        Mapa.TryGetValue(code, out var status) ? status : StatusCodes.Status400BadRequest;
}
