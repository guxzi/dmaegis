namespace DMAegis.WebApi.ApiModels;

/// <summary>
/// Códigos do campo responseCode. 0 e 1 são fixos; os erros de negócio
/// (BusinessErrors) usam números a partir de 2, sempre o próximo livre.
/// </summary>
public static class ResponseCodes
{
    public const int SUCESSO = 0;
    public const int ERRO = 1;
}
