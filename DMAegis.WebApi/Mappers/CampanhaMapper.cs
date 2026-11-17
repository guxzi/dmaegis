using DMAegis.Services;
using DMAegis.WebApi.ApiModels.Campanhas;

namespace DMAegis.WebApi.Mappers;

public static class CampanhaMapper
{
    public static CampanhaResponseApiModel ParaResponse(this CampanhaServiceModel campanha) => new()
    {
        Id = campanha.Id,
        IdSistema = campanha.IdSistema,
        NomeSistema = campanha.NomeSistema,
        Nome = campanha.Nome,
        Descricao = campanha.Descricao,
        Arquivada = campanha.Arquivada,
        DataCriacao = campanha.DataCriacao,
    };

    public static SistemaResponseApiModel ParaResponse(this SistemaServiceModel sistema) => new()
    {
        Id = sistema.Id,
        Nome = sistema.Nome,
    };

    public static InserirCampanhaServiceModel ParaServiceModel(this InserirCampanhaRequestApiModel request) => new()
    {
        IdSistema = request.IdSistema,
        Nome = request.Nome,
        Descricao = request.Descricao,
    };

    public static AtualizarCampanhaServiceModel ParaServiceModel(this AtualizarCampanhaRequestApiModel request) => new()
    {
        Nome = request.Nome,
        Descricao = request.Descricao,
    };
}
