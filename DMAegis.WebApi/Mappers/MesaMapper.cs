using DMAegis.Services;
using DMAegis.WebApi.ApiModels.Mesas;

namespace DMAegis.WebApi.Mappers;

public static class MesaMapper
{
    private static CelulaApiModel ParaApi(CelulaServiceModel c) => new() { Coluna = c.Coluna, Linha = c.Linha };

    public static List<CelulaServiceModel> ParaServiceModel(this IEnumerable<CelulaApiModel>? celulas) =>
        (celulas ?? []).Select(c => new CelulaServiceModel { Coluna = c.Coluna, Linha = c.Linha }).ToList();

    public static SalvarMesaServiceModel ParaServiceModel(this SalvarMesaRequestApiModel request) => new()
    {
        Nome = request.Nome,
        IdImagemRegistro = request.IdImagemRegistro,
        TamanhoCelula = request.TamanhoCelula,
        DeslocamentoX = request.DeslocamentoX,
        DeslocamentoY = request.DeslocamentoY,
        GradeVisivel = request.GradeVisivel,
        MovimentoLivre = request.MovimentoLivre,
        Nevoa = request.Nevoa,
    };

    public static InserirParticipantesServiceModel ParaServiceModel(this InserirParticipantesRequestApiModel request) => new()
    {
        TodosJogadores = request.TodosJogadores,
        IdRegistro = request.IdRegistro,
        Nome = request.Nome,
        Quantidade = request.Quantidade,
    };

    public static SalvarParticipanteServiceModel ParaServiceModel(this SalvarParticipanteRequestApiModel request) => new()
    {
        Nome = request.Nome,
        Iniciativa = request.Iniciativa,
        BonusIniciativa = request.BonusIniciativa,
        Ca = request.Ca,
        PvAtual = request.PvAtual,
        PvMaximo = request.PvMaximo,
        Tamanho = request.Tamanho,
        Visivel = request.Visivel,
        Ordem = request.Ordem,
        Condicoes = (request.Condicoes ?? []).Select(c => c ?? string.Empty).ToList(),
    };

    public static List<IniciativaServiceModel> ParaServiceModel(this IniciarCombateRequestApiModel? request) =>
        (request?.Iniciativas ?? [])
            .Select(i => new IniciativaServiceModel { IdParticipante = i.IdParticipante, Iniciativa = i.Iniciativa })
            .ToList();

    public static MesaResumoResponseApiModel ParaResponse(this MesaResumoServiceModel mesa) => new()
    {
        Id = mesa.Id,
        Nome = mesa.Nome,
        IdImagemRegistro = mesa.IdImagemRegistro,
        Ativa = mesa.Ativa,
        EmCombate = mesa.EmCombate,
    };

    public static MesaResponseApiModel ParaResponse(this MesaServiceModel mesa) => new()
    {
        Id = mesa.Id,
        IdCampanha = mesa.IdCampanha,
        Nome = mesa.Nome,
        IdImagemRegistro = mesa.IdImagemRegistro,
        TamanhoCelula = mesa.TamanhoCelula,
        DeslocamentoX = mesa.DeslocamentoX,
        DeslocamentoY = mesa.DeslocamentoY,
        GradeVisivel = mesa.GradeVisivel,
        MovimentoLivre = mesa.MovimentoLivre,
        Nevoa = mesa.Nevoa,
        CelulasReveladas = mesa.CelulasReveladas.Select(ParaApi).ToList(),
        Ativa = mesa.Ativa,
        EmCombate = mesa.EmCombate,
        Rodada = mesa.Rodada,
        IdParticipanteVez = mesa.IdParticipanteVez,
        Participantes = mesa.Participantes.Select(p => new ParticipanteResponseApiModel
        {
            Id = p.Id,
            IdRegistro = p.IdRegistro,
            IdTipoRegistro = p.IdTipoRegistro,
            IdImagemRetrato = p.IdImagemRetrato,
            Nome = p.Nome,
            Jogador = p.Jogador,
            Iniciativa = p.Iniciativa,
            BonusIniciativa = p.BonusIniciativa,
            Ca = p.Ca,
            PvAtual = p.PvAtual,
            PvMaximo = p.PvMaximo,
            Coluna = p.Coluna,
            Linha = p.Linha,
            Tamanho = p.Tamanho,
            Visivel = p.Visivel,
            Ordem = p.Ordem,
            Condicoes = p.Condicoes,
        }).ToList(),
    };

    public static VisaoJogadorResponseApiModel ParaResponse(this VisaoJogadorServiceModel visao) => new()
    {
        IdMesa = visao.IdMesa,
        Nome = visao.Nome,
        IdImagemRegistro = visao.IdImagemRegistro,
        TamanhoCelula = visao.TamanhoCelula,
        DeslocamentoX = visao.DeslocamentoX,
        DeslocamentoY = visao.DeslocamentoY,
        GradeVisivel = visao.GradeVisivel,
        MovimentoLivre = visao.MovimentoLivre,
        Nevoa = visao.Nevoa,
        CelulasReveladas = visao.CelulasReveladas.Select(ParaApi).ToList(),
        EmCombate = visao.EmCombate,
        Rodada = visao.Rodada,
        IdParticipanteVez = visao.IdParticipanteVez,
        Participantes = visao.Participantes.Select(p => new ParticipanteVisaoResponseApiModel
        {
            Id = p.Id,
            Nome = p.Nome,
            Jogador = p.Jogador,
            IdTipoRegistro = p.IdTipoRegistro,
            IdImagemRetrato = p.IdImagemRetrato,
            Coluna = p.Coluna,
            Linha = p.Linha,
            Tamanho = p.Tamanho,
            Ca = p.Ca,
            PvAtual = p.PvAtual,
            PvMaximo = p.PvMaximo,
            EstadoSaude = p.EstadoSaude,
            Condicoes = p.Condicoes,
            EhMinha = p.EhMinha,
        }).ToList(),
    };
}
