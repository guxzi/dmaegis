using DMAegis.WebApi.ApiModels.Campanhas;
using FluentValidation;

namespace DMAegis.WebApi.Validators;

public class InserirCampanhaValidator : AbstractValidator<InserirCampanhaRequestApiModel>
{
    public InserirCampanhaValidator()
    {
        RuleFor(x => x.Nome).RegrasDeNomeDeCampanha();
        RuleFor(x => x.Descricao).RegrasDeDescricaoDeCampanha();
        RuleFor(x => x.IdSistema).GreaterThan(0).WithMessage("Escolha o sistema da campanha.");
    }
}

public static class RegrasCampanha
{
    public const int TamanhoMaximoNome = 100;
    public const int TamanhoMaximoDescricao = 5000;

    public static IRuleBuilderOptions<T, string> RegrasDeNomeDeCampanha<T>(this IRuleBuilder<T, string> regra) =>
        regra
            .Must(nome => !string.IsNullOrWhiteSpace(nome)).WithMessage("Informe o nome da campanha.")
            .Must(nome => (nome ?? string.Empty).Trim().Length <= TamanhoMaximoNome)
            .WithMessage($"O nome pode ter até {TamanhoMaximoNome} caracteres.");

    public static IRuleBuilderOptions<T, string?> RegrasDeDescricaoDeCampanha<T>(this IRuleBuilder<T, string?> regra) =>
        regra
            .Must(descricao => (descricao ?? string.Empty).Trim().Length <= TamanhoMaximoDescricao)
            .WithMessage($"A descrição pode ter até {TamanhoMaximoDescricao} caracteres.");
}
