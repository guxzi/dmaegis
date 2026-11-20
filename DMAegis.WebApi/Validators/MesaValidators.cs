using DMAegis.Services;
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
        RuleFor(x => x.Clima).Must(c => string.IsNullOrWhiteSpace(c) || RegrasMesa.Climas.Contains(c))
            .WithMessage("Clima desconhecido.");
    }
}

public class SinalMesaValidator : AbstractValidator<SinalMesaRequestApiModel>
{
    public SinalMesaValidator()
    {
        RuleFor(x => x.Tipo).Must(t => t is not null && RegrasMesa.TiposSinal.Contains(t)).WithMessage("Sinal desconhecido.");
        When(x => x.Tipo is RegrasMesa.SinalPing or RegrasMesa.SinalArrasto, () =>
        {
            RuleFor(x => x.Coluna).NotNull().WithMessage("Informe a posição.");
            RuleFor(x => x.Linha).NotNull().WithMessage("Informe a posição.");
        });
        RuleFor(x => x.Coluna).InclusiveBetween(-10, RegrasMesaApi.PosicaoMaxima).When(x => x.Coluna.HasValue).WithMessage("Posição fora do mapa.");
        RuleFor(x => x.Linha).InclusiveBetween(-10, RegrasMesaApi.PosicaoMaxima).When(x => x.Linha.HasValue).WithMessage("Posição fora do mapa.");
        When(x => x.Tipo == RegrasMesa.SinalReacao, () =>
            RuleFor(x => x.Emoji).Must(e => e is not null && RegrasMesa.Reacoes.Contains(e)).WithMessage("Reação desconhecida."));
        RuleFor(x => x.Origem).TamanhoAte(40, "A origem do sinal");
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

public class MoverMinhaFichaValidator : AbstractValidator<MoverMinhaFichaRequestApiModel>
{
    public MoverMinhaFichaValidator()
    {
        RuleFor(x => x.Coluna).InclusiveBetween(0, RegrasMesaApi.PosicaoMaxima).WithMessage("A posição está fora do mapa.");
        RuleFor(x => x.Linha).InclusiveBetween(0, RegrasMesaApi.PosicaoMaxima).WithMessage("A posição está fora do mapa.");
    }
}

public class AlterarNevoaValidator : AbstractValidator<AlterarNevoaRequestApiModel>
{
    public AlterarNevoaValidator()
    {
        RuleFor(x => x).Must(x => (x.Revelar?.Count ?? 0) + (x.Esconder?.Count ?? 0) <= 5000)
            .WithMessage("No máximo 5000 células por vez.");
        RuleForEach(x => x.Revelar).Must(CelulaValida).WithMessage("Há células fora do mapa.");
        RuleForEach(x => x.Esconder).Must(CelulaValida).WithMessage("Há células fora do mapa.");
    }

    private static bool CelulaValida(CelulaApiModel? c) =>
        c is not null && c.Coluna is >= 0 and <= RegrasMesaApi.PosicaoMaxima && c.Linha is >= 0 and <= RegrasMesaApi.PosicaoMaxima;
}

public class InserirEntregaValidator : AbstractValidator<InserirEntregaRequestApiModel>
{
    public InserirEntregaValidator()
    {
        RuleFor(x => x.Titulo).Must(t => !string.IsNullOrWhiteSpace(t)).WithMessage("Dê um título à entrega.");
        RuleFor(x => x.Titulo).TamanhoAte(120, "O título");
        RuleFor(x => x.Texto).TamanhoAte(4000, "O texto");
    }
}

public class RolarValidator : AbstractValidator<RolarRequestApiModel>
{
    public RolarValidator()
    {
        RuleFor(x => x.Expressao).Must(e => !string.IsNullOrWhiteSpace(e)).WithMessage("Informe a expressão da rolagem.");
        RuleFor(x => x.Expressao).TamanhoAte(200, "A expressão");
        RuleFor(x => x.Detalhe).Must(d => !string.IsNullOrWhiteSpace(d)).WithMessage("Informe o detalhe da rolagem.");
        RuleFor(x => x.Detalhe).TamanhoAte(500, "O detalhe da rolagem");
        RuleFor(x => x.Total).InclusiveBetween(-10_000_000, 10_000_000).WithMessage("O total está fora da faixa.");
    }
}
