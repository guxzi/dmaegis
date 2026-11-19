using DMAegis.WebApi.ApiModels.Mesas;
using FluentValidation;

namespace DMAegis.WebApi.Validators;

public static class RegrasMesaApi
{
    public const int TamanhoMaximoNome = 100;
    public const int CelulaMinima = 10;
    public const int CelulaMaxima = 400;
    public const int DeslocamentoMaximo = 400;
    public const int ValorMaximo = 9999;
    public const int MaximoCondicoes = 20;
    public const int TamanhoMaximoCondicao = 40;
    public const int PosicaoMaxima = 500;
}

public class SalvarMesaValidator : AbstractValidator<SalvarMesaRequestApiModel>
{
    public SalvarMesaValidator()
    {
        RuleFor(x => x.Nome).Must(n => !string.IsNullOrWhiteSpace(n)).WithMessage("Informe o nome da mesa.");
        RuleFor(x => x.Nome).TamanhoAte(RegrasMesaApi.TamanhoMaximoNome, "O nome da mesa");
        RuleFor(x => x.TamanhoCelula).InclusiveBetween(RegrasMesaApi.CelulaMinima, RegrasMesaApi.CelulaMaxima)
            .WithMessage($"A célula da grade vai de {RegrasMesaApi.CelulaMinima} a {RegrasMesaApi.CelulaMaxima} px.");
        RuleFor(x => x.DeslocamentoX).InclusiveBetween(-RegrasMesaApi.DeslocamentoMaximo, RegrasMesaApi.DeslocamentoMaximo)
            .WithMessage("O deslocamento da grade está fora da faixa.");
        RuleFor(x => x.DeslocamentoY).InclusiveBetween(-RegrasMesaApi.DeslocamentoMaximo, RegrasMesaApi.DeslocamentoMaximo)
            .WithMessage("O deslocamento da grade está fora da faixa.");
    }
}

public class InserirParticipantesValidator : AbstractValidator<InserirParticipantesRequestApiModel>
{
    public InserirParticipantesValidator()
    {
        RuleFor(x => x.Quantidade).InclusiveBetween(1, 20).WithMessage("A quantidade vai de 1 a 20.");
        RuleFor(x => x.Nome).TamanhoAte(RegrasMesaApi.TamanhoMaximoNome, "O nome da ficha");
    }
}

public class SalvarParticipanteValidator : AbstractValidator<SalvarParticipanteRequestApiModel>
{
    public SalvarParticipanteValidator()
    {
        RuleFor(x => x.Nome).Must(n => !string.IsNullOrWhiteSpace(n)).WithMessage("Informe o nome da ficha.");
        RuleFor(x => x.Nome).TamanhoAte(RegrasMesaApi.TamanhoMaximoNome, "O nome da ficha");
        RuleFor(x => x.Iniciativa).InclusiveBetween(-99, 99).When(x => x.Iniciativa.HasValue)
            .WithMessage("A iniciativa vai de -99 a 99.");
        RuleFor(x => x.BonusIniciativa).InclusiveBetween(-20, 20).WithMessage("O bônus de iniciativa vai de -20 a 20.");
        RuleFor(x => x.Ca).InclusiveBetween(0, 99).When(x => x.Ca.HasValue).WithMessage("A CA vai de 0 a 99.");
        RuleFor(x => x.PvMaximo).InclusiveBetween(1, RegrasMesaApi.ValorMaximo).When(x => x.PvMaximo.HasValue)
            .WithMessage($"O PV máximo vai de 1 a {RegrasMesaApi.ValorMaximo}.");
        RuleFor(x => x.PvAtual).InclusiveBetween(-RegrasMesaApi.ValorMaximo, RegrasMesaApi.ValorMaximo).When(x => x.PvAtual.HasValue)
            .WithMessage("O PV atual está fora da faixa.");
        RuleFor(x => x.Tamanho).InclusiveBetween(1, 4).WithMessage("O tamanho da ficha vai de 1 a 4 células.");
        RuleFor(x => x.Condicoes).Must(c => (c ?? []).Count <= RegrasMesaApi.MaximoCondicoes)
            .WithMessage($"Até {RegrasMesaApi.MaximoCondicoes} condições por ficha.");
        RuleForEach(x => x.Condicoes).TamanhoAte(RegrasMesaApi.TamanhoMaximoCondicao, "A condição");
    }
}

public class MoverParticipanteValidator : AbstractValidator<MoverParticipanteRequestApiModel>
{
    public MoverParticipanteValidator()
    {
        RuleFor(x => x.Coluna).InclusiveBetween(0, RegrasMesaApi.PosicaoMaxima).When(x => x.Coluna.HasValue)
            .WithMessage("A posição está fora do mapa.");
        RuleFor(x => x.Linha).InclusiveBetween(0, RegrasMesaApi.PosicaoMaxima).When(x => x.Linha.HasValue)
            .WithMessage("A posição está fora do mapa.");
    }
}
