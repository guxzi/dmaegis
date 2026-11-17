using DMAegis.WebApi.ApiModels.Auth;
using FluentValidation;

namespace DMAegis.WebApi.Validators;

public class LoginValidator : AbstractValidator<LoginRequestApiModel>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Informe o e-mail.");
        RuleFor(x => x.Senha).NotEmpty().WithMessage("Informe a senha.");
    }
}
