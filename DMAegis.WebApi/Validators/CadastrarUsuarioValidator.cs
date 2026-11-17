using DMAegis.WebApi.ApiModels.Auth;
using FluentValidation;

namespace DMAegis.WebApi.Validators;

public class CadastrarUsuarioValidator : AbstractValidator<CadastrarUsuarioRequestApiModel>
{
    public CadastrarUsuarioValidator()
    {
        RuleFor(x => x.Nome)
            .Must(nome => !string.IsNullOrWhiteSpace(nome)).WithMessage("Informe o nome.")
            .MaximumLength(100).WithMessage("O nome pode ter até 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Informe o e-mail.")
            .MaximumLength(254).WithMessage("O e-mail pode ter até 254 caracteres.")
            .EmailAddress().WithMessage("Informe um e-mail válido.");

        RuleFor(x => x.Senha)
            .MinimumLength(8).WithMessage("A senha precisa ter pelo menos 8 caracteres.")
            .MaximumLength(100).WithMessage("A senha pode ter até 100 caracteres.");
    }
}
