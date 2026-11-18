using DMAegis.WebApi.ApiModels.Sessoes;
using FluentValidation;

namespace DMAegis.WebApi.Validators;

public static class RegrasSessao
{
    public const int NumeroMaximo = 9999;
    public const int TamanhoMaximoTextoCurto = 5000;
    public const int TamanhoMaximoResumo = 20000;
    public const int MaximoCenas = 30;
    public const int TamanhoMaximoCena = 500;
    public const int MaximoVinculos = 200;
    public const int TamanhoMaximoSegredo = 500;

    public static IRuleBuilderOptions<T, string?> TamanhoAte<T>(this IRuleBuilder<T, string?> regra, int maximo, string campo) =>
        regra.Must(texto => (texto ?? string.Empty).Trim().Length <= maximo)
            .WithMessage($"{campo} pode ter até {maximo} caracteres.");
}

public class InserirSessaoValidator : AbstractValidator<InserirSessaoRequestApiModel>
{
    public InserirSessaoValidator()
    {
        RuleFor(x => x.Numero)
            .InclusiveBetween(1, RegrasSessao.NumeroMaximo)
            .When(x => x.Numero.HasValue)
            .WithMessage($"O número da sessão vai de 1 a {RegrasSessao.NumeroMaximo}.");
    }
}

public class SalvarSessaoValidator : AbstractValidator<SalvarSessaoRequestApiModel>
{
    public SalvarSessaoValidator()
    {
        RuleFor(x => x.Numero)
            .InclusiveBetween(1, RegrasSessao.NumeroMaximo)
            .WithMessage($"O número da sessão vai de 1 a {RegrasSessao.NumeroMaximo}.");
        RuleFor(x => x.NotasPersonagens).TamanhoAte(RegrasSessao.TamanhoMaximoTextoCurto, "As notas dos personagens");
        RuleFor(x => x.ComecoForte).TamanhoAte(RegrasSessao.TamanhoMaximoTextoCurto, "O começo forte");
        RuleFor(x => x.Resumo).TamanhoAte(RegrasSessao.TamanhoMaximoResumo, "O resumo");

        RuleFor(x => x.Cenas)
            .NotNull()
            .Must(c => c.Count <= RegrasSessao.MaximoCenas)
            .WithMessage($"Uma sessão pode ter até {RegrasSessao.MaximoCenas} cenas.");
        RuleForEach(x => x.Cenas)
            .Must(cena => (cena ?? string.Empty).Trim().Length <= RegrasSessao.TamanhoMaximoCena)
            .WithMessage($"Cada cena pode ter até {RegrasSessao.TamanhoMaximoCena} caracteres.");

        RuleFor(x => x.IdsRegistro)
            .NotNull()
            .Must(ids => ids.Count <= RegrasSessao.MaximoVinculos)
            .WithMessage($"Uma sessão pode ter até {RegrasSessao.MaximoVinculos} registros vinculados.");
    }
}

public class SalvarSegredoValidator : AbstractValidator<SalvarSegredoRequestApiModel>
{
    public SalvarSegredoValidator()
    {
        RuleFor(x => x.Texto)
            .Must(t => !string.IsNullOrWhiteSpace(t)).WithMessage("Escreva o segredo.")
            .Must(t => (t ?? string.Empty).Trim().Length <= RegrasSessao.TamanhoMaximoSegredo)
            .WithMessage($"O segredo pode ter até {RegrasSessao.TamanhoMaximoSegredo} caracteres.");
    }
}

public class SalvarAnotacaoValidator : AbstractValidator<SalvarAnotacaoRequestApiModel>
{
    public const int TamanhoMaximo = 2000;

    public SalvarAnotacaoValidator()
    {
        RuleFor(x => x.Texto)
            .Must(t => !string.IsNullOrWhiteSpace(t)).WithMessage("Escreva a anotação.")
            .Must(t => (t ?? string.Empty).Trim().Length <= TamanhoMaximo)
            .WithMessage($"A anotação pode ter até {TamanhoMaximo} caracteres.");
    }
}
