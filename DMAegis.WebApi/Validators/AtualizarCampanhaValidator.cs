using DMAegis.WebApi.ApiModels.Campanhas;
using FluentValidation;

namespace DMAegis.WebApi.Validators;

public class AtualizarCampanhaValidator : AbstractValidator<AtualizarCampanhaRequestApiModel>
{
    public AtualizarCampanhaValidator()
    {
        RuleFor(x => x.Nome).RegrasDeNomeDeCampanha();
        RuleFor(x => x.Descricao).RegrasDeDescricaoDeCampanha();
    }
}
