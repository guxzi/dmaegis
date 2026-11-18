using DMAegis.Services;
using DMAegis.WebApi.ApiModels.Sessoes;

namespace DMAegis.WebApi.Mappers;

/// <summary>A data da sessão é só dia: DateOnly na API ("yyyy-MM-dd"), DateTime no Service.</summary>
public static class SessaoMapper
{
    private static DateOnly? ParaDateOnly(DateTime? data) => data.HasValue ? DateOnly.FromDateTime(data.Value) : null;
    private static DateTime? ParaDateTime(DateOnly? data) => data?.ToDateTime(TimeOnly.MinValue);

    public static InserirSessaoServiceModel ParaServiceModel(this InserirSessaoRequestApiModel? request) => new()
    {
        Numero = request?.Numero,
        Data = ParaDateTime(request?.Data),
    };

    public static SalvarSessaoServiceModel ParaServiceModel(this SalvarSessaoRequestApiModel request) => new()
    {
        Numero = request.Numero,
        Data = ParaDateTime(request.Data),
        NotasPersonagens = request.NotasPersonagens,
        ComecoForte = request.ComecoForte,
        Resumo = request.Resumo,
        Cenas = (request.Cenas ?? []).Select(c => c ?? string.Empty).ToList(),
        IdsRegistro = request.IdsRegistro ?? [],
    };

    public static SessaoResumoResponseApiModel ParaResponse(this SessaoResumoServiceModel sessao) => new()
    {
        Id = sessao.Id,
        Numero = sessao.Numero,
        Data = ParaDateOnly(sessao.Data),
        ComecoForte = sessao.ComecoForte,
    };

    public static SessaoResponseApiModel ParaResponse(this SessaoServiceModel sessao) => new()
    {
        Id = sessao.Id,
        IdCampanha = sessao.IdCampanha,
        Numero = sessao.Numero,
        Data = ParaDateOnly(sessao.Data),
        NotasPersonagens = sessao.NotasPersonagens,
        ComecoForte = sessao.ComecoForte,
        Resumo = sessao.Resumo,
        Cenas = sessao.Cenas,
        IdsRegistro = sessao.IdsRegistro,
        Segredos = sessao.Segredos.Select(ParaResponse).ToList(),
    };

    public static SegredoResponseApiModel ParaResponse(this SegredoServiceModel segredo) => new()
    {
        Id = segredo.Id,
        Texto = segredo.Texto,
        IdSessaoCriacao = segredo.IdSessaoCriacao,
        IdSessaoRevelacao = segredo.IdSessaoRevelacao,
        DataRevelacao = segredo.DataRevelacao,
    };
}

public static class AnotacaoMapper
{
    public static AnotacaoResponseApiModel ParaResponse(this AnotacaoServiceModel anotacao) => new()
    {
        Id = anotacao.Id,
        Data = anotacao.Data,
        Texto = anotacao.Texto,
    };
}
