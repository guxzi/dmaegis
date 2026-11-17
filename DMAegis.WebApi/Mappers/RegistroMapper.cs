using DMAegis.Services;
using DMAegis.WebApi.ApiModels.Registros;

namespace DMAegis.WebApi.Mappers;

public static class RegistroMapper
{
    public static SalvarRegistroServiceModel ParaServiceModel(this SalvarRegistroRequestApiModel request) => new()
    {
        IdTipoRegistro = request.IdTipoRegistro,
        Nome = request.Nome,
        Resumo = request.Resumo,
        Descricao = request.Descricao,
        Atributos = (request.Atributos ?? [])
            .Select(a => new AtributoServiceModel { Chave = a.Chave ?? string.Empty, Valor = a.Valor ?? string.Empty })
            .ToList(),
        Tags = (request.Tags ?? []).Select(t => t ?? string.Empty).ToList(),
    };

    public static RegistroResponseApiModel ParaResponse(this RegistroServiceModel registro) => new()
    {
        Id = registro.Id,
        IdCampanha = registro.IdCampanha,
        IdTipoRegistro = registro.IdTipoRegistro,
        Nome = registro.Nome,
        Resumo = registro.Resumo,
        Descricao = registro.Descricao,
        Atributos = registro.Atributos.Select(a => new AtributoApiModel { Chave = a.Chave, Valor = a.Valor }).ToList(),
        Tags = registro.Tags.Select(ParaResponse).ToList(),
        DataCriacao = registro.DataCriacao,
        DataAtualizacao = registro.DataAtualizacao,
    };

    public static RegistroResumoResponseApiModel ParaResponse(this RegistroResumoServiceModel registro) => new()
    {
        Id = registro.Id,
        IdTipoRegistro = registro.IdTipoRegistro,
        Nome = registro.Nome,
        Resumo = registro.Resumo,
        Tags = registro.Tags.Select(ParaResponse).ToList(),
    };

    public static TagResponseApiModel ParaResponse(this TagServiceModel tag) => new() { Id = tag.Id, Nome = tag.Nome };

    public static TipoRegistroResponseApiModel ParaResponse(this TipoRegistroServiceModel tipo) =>
        new() { Id = tipo.Id, Nome = tipo.Nome };

    public static ModeloAtributoResponseApiModel ParaResponse(this ModeloAtributoServiceModel modelo) =>
        new() { IdTipoRegistro = modelo.IdTipoRegistro, Chave = modelo.Chave, Ordem = modelo.Ordem };
}
