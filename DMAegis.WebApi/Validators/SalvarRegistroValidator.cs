using DMAegis.WebApi.ApiModels.Registros;
using FluentValidation;

namespace DMAegis.WebApi.Validators;

public class SalvarRegistroValidator : AbstractValidator<SalvarRegistroRequestApiModel>
{
    public const int TamanhoMaximoNome = 150;
    public const int TamanhoMaximoResumo = 200;
    public const int TamanhoMaximoDescricao = 20000;
    public const int MaximoAtributos = 50;
    public const int TamanhoMaximoChave = 60;
    public const int TamanhoMaximoValor = 500;
    public const int MaximoTags = 20;
    public const int TamanhoMaximoTag = 50;

    public SalvarRegistroValidator()
    {
        RuleFor(x => x.IdTipoRegistro).GreaterThan(0).WithMessage("Escolha o tipo do registro.");

        RuleFor(x => x.Nome)
            .Must(nome => !string.IsNullOrWhiteSpace(nome)).WithMessage("Informe o nome do registro.")
            .Must(nome => (nome ?? string.Empty).Trim().Length <= TamanhoMaximoNome)
            .WithMessage($"O nome pode ter até {TamanhoMaximoNome} caracteres.");

        RuleFor(x => x.Resumo)
            .Must(r => (r ?? string.Empty).Trim().Length <= TamanhoMaximoResumo)
            .WithMessage($"O resumo pode ter até {TamanhoMaximoResumo} caracteres.");

        RuleFor(x => x.Descricao)
            .Must(d => (d ?? string.Empty).Trim().Length <= TamanhoMaximoDescricao)
            .WithMessage($"A descrição pode ter até {TamanhoMaximoDescricao} caracteres.");

        RuleFor(x => x.Atributos)
            .NotNull()
            .Must(a => a.Count <= MaximoAtributos).WithMessage($"Um registro pode ter até {MaximoAtributos} atributos.");

        RuleForEach(x => x.Atributos).ChildRules(atributo =>
        {
            atributo.RuleFor(a => a.Chave)
                .Must((a, chave) => string.IsNullOrWhiteSpace(a.Valor) || !string.IsNullOrWhiteSpace(chave))
                .WithMessage("Todo atributo com valor precisa de um nome.")
                .Must(chave => (chave ?? string.Empty).Trim().Length <= TamanhoMaximoChave)
                .WithMessage($"O nome do atributo pode ter até {TamanhoMaximoChave} caracteres.");
            atributo.RuleFor(a => a.Valor)
                .Must(valor => (valor ?? string.Empty).Trim().Length <= TamanhoMaximoValor)
                .WithMessage($"O valor do atributo pode ter até {TamanhoMaximoValor} caracteres.");
        });

        RuleFor(x => x.Atributos).Custom((atributos, contexto) =>
        {
            var vistas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var chave in (atributos ?? [])
                         .Where(a => !string.IsNullOrWhiteSpace(a.Valor))
                         .Select(a => (a.Chave ?? string.Empty).Trim())
                         .Where(c => c.Length > 0))
            {
                if (!vistas.Add(chave))
                    contexto.AddFailure("Atributos", $"Atributo repetido: {chave}.");
            }
        });

        RuleFor(x => x.Tags)
            .NotNull()
            .Must(t => t.Count <= MaximoTags).WithMessage($"Um registro pode ter até {MaximoTags} tags.");

        RuleForEach(x => x.Tags)
            .Must(tag => (tag ?? string.Empty).Trim().Length <= TamanhoMaximoTag)
            .WithMessage($"Cada tag pode ter até {TamanhoMaximoTag} caracteres.");
    }
}
