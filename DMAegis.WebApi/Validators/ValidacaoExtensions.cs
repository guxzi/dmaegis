using DMAegis.Services.Exceptions;
using FluentValidation;

namespace DMAegis.WebApi.Validators;

public static class ValidacaoExtensions
{
    /// <summary>
    /// Valida o modelo e, se houver erro, lança BusinessRuleException com o código DadosInvalidos (400)
    /// e as mensagens dos campos juntas, na ordem das regras.
    /// </summary>
    public static async Task ValidarOuLancarAsync<T>(this IValidator<T> validator, T modelo)
    {
        var resultado = await validator.ValidateAsync(modelo);
        if (resultado.IsValid)
            return;

        var mensagens = string.Join(" ", resultado.Errors.Select(e => e.ErrorMessage).Distinct());
        throw new BusinessRuleException((BusinessErrors.DadosInvalidos.Code, mensagens));
    }
}
