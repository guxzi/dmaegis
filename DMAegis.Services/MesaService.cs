using DMAegis.Model;
using DMAegis.Repository;
using DMAegis.Services.Exceptions;

namespace DMAegis.Services;

public interface IMesaService
{
    Task<IEnumerable<MesaResumoServiceModel>> ListarAsync(int idUsuario, int idCampanha);
    Task<MesaServiceModel> ObterAsync(int idUsuario, int id);
    Task<MesaServiceModel> InserirAsync(int idUsuario, int idCampanha, SalvarMesaServiceModel dados);
    Task<MesaServiceModel> AtualizarAsync(int idUsuario, int id, SalvarMesaServiceModel dados);
    Task ExcluirAsync(int idUsuario, int id);
    Task<MesaServiceModel> AtivarAsync(int idUsuario, int id);

    Task<MesaServiceModel> InserirParticipantesAsync(int idUsuario, int idMesa, InserirParticipantesServiceModel dados);
    Task<MesaServiceModel> AtualizarParticipanteAsync(int idUsuario, int id, SalvarParticipanteServiceModel dados);
    Task MoverParticipanteAsync(int idUsuario, int id, int? coluna, int? linha);
    Task<MesaServiceModel> ExcluirParticipanteAsync(int idUsuario, int id);

    Task<MesaServiceModel> IniciarCombateAsync(int idUsuario, int idMesa, IEnumerable<IniciativaServiceModel> iniciativas);
    Task<MesaServiceModel> ProximoTurnoAsync(int idUsuario, int idMesa);
    Task<MesaServiceModel> EncerrarCombateAsync(int idUsuario, int idMesa);

    Task<VisaoJogadorServiceModel> ObterVisaoJogadorAsync(int idUsuario, int idMesa);

    /// <summary>Revela e esconde células da névoa num lote só (o que o pincel pintou de uma vez).</summary>
    Task<MesaServiceModel> AlterarNevoaAsync(int idUsuario, int idMesa,
        IEnumerable<CelulaServiceModel> revelar, IEnumerable<CelulaServiceModel> esconder);

    /// <summary>Cobre a mesa inteira de novo.</summary>
    Task<MesaServiceModel> EsconderTudoAsync(int idUsuario, int idMesa);

    /// <summary>Sinal passageiro do mestre (ping, reação ou arrasto ao vivo). Nada é gravado.</summary>
    Task SinalizarAsync(int idUsuario, int idMesa, SinalMesaServiceModel sinal);

    /// <summary>
    /// Estado completo da mesa ativa da campanha, sem conferir dono: só para o JogadorService,
    /// que já conferiu o convite e sempre filtra com FiltrarParaJogador antes de responder.
    /// </summary>
    Task<MesaServiceModel?> MontarAtivaAsync(int idCampanha);
}

/// <summary>
/// Mesa virtual: mesas, fichas, combate, névoa e sinais. A classe é dividida em arquivos por assunto
/// (MesaService.Participantes.cs, .Combate.cs, .Nevoa.cs e .Sinais.cs); este tem as mesas e o apoio.
/// </summary>
public partial class MesaService : IMesaService
{
    public const int MaximoParticipantes = 60;
    public const int IdTipoJogador = 2;

    private readonly IMesaRepository _mesaRepository;
    private readonly IParticipanteMesaRepository _participanteRepository;
    private readonly ICondicaoParticipanteRepository _condicaoRepository;
    private readonly IRegistroRepository _registroRepository;
    private readonly IAtributoRegistroRepository _atributoRepository;
    private readonly IImagemRegistroRepository _imagemRepository;
    private readonly ICampanhaService _campanhaService;
    private readonly ICanalMesa _canal;
    private readonly IEventoMesaRepository _eventoRepository;
    private readonly ICelulaReveladaRepository _celulaRepository;

    public MesaService(
        IMesaRepository mesaRepository,
        IParticipanteMesaRepository participanteRepository,
        ICondicaoParticipanteRepository condicaoRepository,
        IRegistroRepository registroRepository,
        IAtributoRegistroRepository atributoRepository,
        IImagemRegistroRepository imagemRepository,
        ICampanhaService campanhaService,
        ICanalMesa canal,
        ICelulaReveladaRepository celulaRepository,
        IEventoMesaRepository eventoRepository)
    {
        _eventoRepository = eventoRepository;
        _mesaRepository = mesaRepository;
        _participanteRepository = participanteRepository;
        _condicaoRepository = condicaoRepository;
        _registroRepository = registroRepository;
        _atributoRepository = atributoRepository;
        _imagemRepository = imagemRepository;
        _campanhaService = campanhaService;
        _canal = canal;
        _celulaRepository = celulaRepository;
    }

    // ---------- Mesas ----------

    public async Task<IEnumerable<MesaResumoServiceModel>> ListarAsync(int idUsuario, int idCampanha)
    {
        await _campanhaService.ObterDoUsuarioOuLancarAsync(idUsuario, idCampanha);
        return (await _mesaRepository.ListarPorCampanhaAsync(idCampanha))
            .Select(m => new MesaResumoServiceModel
            {
                Id = m.Id, Nome = m.Nome, IdImagemRegistro = m.IdImagemRegistro, Ativa = m.Ativa, EmCombate = m.EmCombate,
            })
            .ToList();
    }

    public async Task<MesaServiceModel> ObterAsync(int idUsuario, int id) =>
        await MontarAsync(await ObterMesaDoUsuarioOuLancarAsync(idUsuario, id));

    public async Task<MesaServiceModel> InserirAsync(int idUsuario, int idCampanha, SalvarMesaServiceModel dados)
    {
        await _campanhaService.ObterDoUsuarioOuLancarAsync(idUsuario, idCampanha);
        await ConferirImagemDaCampanhaAsync(idCampanha, dados.IdImagemRegistro);

        var agora = Agora();
        // A primeira mesa da campanha já nasce ativa.
        var primeira = !(await _mesaRepository.ListarPorCampanhaAsync(idCampanha)).Any();
        var mesa = new Mesa
        {
            IdCampanha = idCampanha,
            Nome = dados.Nome.Trim(),
            IdImagemRegistro = dados.IdImagemRegistro,
            TamanhoCelula = dados.TamanhoCelula,
            DeslocamentoX = dados.DeslocamentoX,
            DeslocamentoY = dados.DeslocamentoY,
            GradeVisivel = dados.GradeVisivel,
            MovimentoLivre = dados.MovimentoLivre,
            Nevoa = dados.Nevoa,
            Clima = dados.Clima,
            Ativa = primeira,
            DataCriacao = agora,
            DataAtualizacao = agora,
        };
        mesa.Id = await _mesaRepository.InserirAsync(mesa);
        return await AvisarEMontarAsync(mesa);
    }

    public async Task<MesaServiceModel> AtualizarAsync(int idUsuario, int id, SalvarMesaServiceModel dados)
    {
        var mesa = await ObterMesaDoUsuarioOuLancarAsync(idUsuario, id);
        await ConferirImagemDaCampanhaAsync(mesa.IdCampanha, dados.IdImagemRegistro);

        mesa.Nome = dados.Nome.Trim();
        mesa.IdImagemRegistro = dados.IdImagemRegistro;
        mesa.TamanhoCelula = dados.TamanhoCelula;
        mesa.DeslocamentoX = dados.DeslocamentoX;
        mesa.DeslocamentoY = dados.DeslocamentoY;
        mesa.GradeVisivel = dados.GradeVisivel;
        mesa.MovimentoLivre = dados.MovimentoLivre;
        mesa.Nevoa = dados.Nevoa;
        mesa.Clima = dados.Clima;
        mesa.DataAtualizacao = Agora();
        await _mesaRepository.AtualizarAsync(mesa);
        return await AvisarEMontarAsync(mesa);
    }

    public async Task ExcluirAsync(int idUsuario, int id)
    {
        var mesa = await ObterMesaDoUsuarioOuLancarAsync(idUsuario, id);
        await _mesaRepository.ExcluirAsync(id);
        _canal.Avisar(mesa.IdCampanha);
    }

    public async Task<MesaServiceModel> AtivarAsync(int idUsuario, int id)
    {
        var mesa = await ObterMesaDoUsuarioOuLancarAsync(idUsuario, id);
        await _mesaRepository.DefinirAtivaAsync(mesa.IdCampanha, mesa.Id);
        mesa.Ativa = true;
        return await AvisarEMontarAsync(mesa);
    }

    // ---------- Apoio ----------

    private async Task<List<ParticipanteMesa>> NovosJogadoresAsync(Mesa mesa, List<ParticipanteMesa> existentes)
    {
        var naMesa = existentes.Where(p => p.IdRegistro.HasValue).Select(p => p.IdRegistro!.Value).ToHashSet();
        var jogadores = await _registroRepository.ListarAsync(mesa.IdCampanha, new FiltroRegistro { IdTipoRegistro = IdTipoJogador });
        var novos = new List<ParticipanteMesa>();
        foreach (var registro in jogadores.Where(r => !naMesa.Contains(r.Id)).OrderBy(r => r.Nome))
            novos.Add(await ParticipanteDoRegistroAsync(registro, registro.Nome));
        return novos;
    }

    private async Task<List<ParticipanteMesa>> NovosDoPedidoAsync(Mesa mesa, InserirParticipantesServiceModel dados, List<ParticipanteMesa> existentes)
    {
        var quantidade = Math.Clamp(dados.Quantidade, 1, MaximoParticipantes);
        if (dados.IdRegistro is null)
        {
            var nome = (dados.Nome ?? string.Empty).Trim();
            if (nome.Length == 0)
                throw new BusinessRuleException((BusinessErrors.DadosInvalidos.Code, "Informe o nome da ficha."));
            return RegrasMesa.NomesNumerados(nome, quantidade, existentes.Select(p => p.Nome))
                .Select(n => new ParticipanteMesa { Nome = n, Tamanho = 1, Visivel = true })
                .ToList();
        }

        var registro = await _registroRepository.ObterAsync(dados.IdRegistro.Value);
        if (registro is null || registro.IdCampanha != mesa.IdCampanha)
            throw new BusinessRuleException(BusinessErrors.RegistroNaoEncontrado);

        var nomeBase = string.IsNullOrWhiteSpace(dados.Nome) ? registro.Nome : dados.Nome.Trim();
        var novos = new List<ParticipanteMesa>();
        foreach (var nome in RegrasMesa.NomesNumerados(nomeBase, quantidade, existentes.Select(p => p.Nome)))
            novos.Add(await ParticipanteDoRegistroAsync(registro, nome));
        return novos;
    }

    /// <summary>Ficha a partir do caderno: CA, PV e bônus de iniciativa vêm dos atributos, quando existem.</summary>
    private async Task<ParticipanteMesa> ParticipanteDoRegistroAsync(Registro registro, string nome)
    {
        var atributos = (await _atributoRepository.ListarPorRegistroAsync(registro.Id))
            .GroupBy(a => a.Chave.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Valor, StringComparer.OrdinalIgnoreCase);
        int? Numero(string chave) => atributos.TryGetValue(chave, out var valor) ? RegrasMesa.NumeroDoAtributo(valor) : null;

        var pv = Numero("PV");
        return new ParticipanteMesa
        {
            IdRegistro = registro.Id,
            Nome = nome.Length > 100 ? nome[..100] : nome,
            Jogador = registro.IdTipoRegistro == IdTipoJogador,
            Ca = Numero("CA"),
            PvMaximo = pv,
            PvAtual = pv,
            BonusIniciativa = Numero("Iniciativa") ?? 0,
            Tamanho = 1,
            Visivel = true,
        };
    }

    public async Task<MesaServiceModel?> MontarAtivaAsync(int idCampanha)
    {
        var mesa = await _mesaRepository.ObterAtivaAsync(idCampanha);
        return mesa is null ? null : await MontarAsync(mesa);
    }

    /// <summary>Depois de gravar: avisa as telas abertas e devolve a mesa montada.</summary>
    private async Task<MesaServiceModel> AvisarEMontarAsync(Mesa mesa)
    {
        _canal.Avisar(mesa.IdCampanha);
        return await MontarAsync(mesa);
    }

    /// <summary>Monta o estado completo da mesa (para o mestre). Também usado pela visão do jogador.</summary>
    public async Task<MesaServiceModel> MontarAsync(Mesa mesa)
    {
        var participantes = await _participanteRepository.ListarPorMesaAsync(mesa.Id);
        var condicoes = (await _condicaoRepository.ListarPorMesaAsync(mesa.Id))
            .GroupBy(c => c.IdParticipante)
            .ToDictionary(g => g.Key, g => g.Select(c => c.Nome).ToList());

        // Retrato = a primeira imagem do registro; o tipo vem junto da mesma consulta (sem o conteúdo).
        var imagens = (await _imagemRepository.ListarPorCampanhaAsync(mesa.IdCampanha)).ToList();
        var retratos = imagens.GroupBy(i => i.IdRegistro).ToDictionary(g => g.Key, g => g.OrderBy(i => i.Ordem).ThenBy(i => i.Id).First().Id);
        var tipos = await TiposDosRegistrosAsync(mesa.IdCampanha, participantes);

        return new MesaServiceModel
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
            Clima = mesa.Clima,
            CelulasReveladas = (await _celulaRepository.ListarPorMesaAsync(mesa.Id))
                .Select(c => new CelulaServiceModel { Coluna = c.Coluna, Linha = c.Linha }).ToList(),
            Ativa = mesa.Ativa,
            EmCombate = mesa.EmCombate,
            Rodada = mesa.Rodada,
            IdParticipanteVez = mesa.IdParticipanteVez,
            Participantes = RegrasMesa.OrdenarIniciativa(participantes).Select(p => new ParticipanteServiceModel
            {
                Id = p.Id,
                IdRegistro = p.IdRegistro,
                IdTipoRegistro = p.IdRegistro.HasValue && tipos.TryGetValue(p.IdRegistro.Value, out var tipo) ? tipo : null,
                IdImagemRetrato = p.IdRegistro.HasValue && retratos.TryGetValue(p.IdRegistro.Value, out var retrato) ? retrato : null,
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
                Condicoes = condicoes.TryGetValue(p.Id, out var lista) ? lista : [],
            }).ToList(),
        };
    }

    private async Task<Dictionary<int, int>> TiposDosRegistrosAsync(int idCampanha, IEnumerable<ParticipanteMesa> participantes)
    {
        if (!participantes.Any(p => p.IdRegistro.HasValue))
            return [];
        var registros = await _registroRepository.ListarAsync(idCampanha, new FiltroRegistro());
        return registros.ToDictionary(r => r.Id, r => r.IdTipoRegistro);
    }

    private async Task ConferirImagemDaCampanhaAsync(int idCampanha, int? idImagem)
    {
        if (idImagem is null)
            return;
        var imagens = await _imagemRepository.ListarPorCampanhaAsync(idCampanha);
        if (!imagens.Any(i => i.Id == idImagem))
            throw new BusinessRuleException(BusinessErrors.ImagemNaoEncontrada);
    }

    /// <summary>Mesa inexistente ou de campanha de outro usuário → MesaNaoEncontrada (404).</summary>
    private async Task<Mesa> ObterMesaDoUsuarioOuLancarAsync(int idUsuario, int id)
    {
        var mesa = await _mesaRepository.ObterAsync(id) ?? throw new BusinessRuleException(BusinessErrors.MesaNaoEncontrada);
        try
        {
            await _campanhaService.ObterDoUsuarioOuLancarAsync(idUsuario, mesa.IdCampanha);
        }
        catch (BusinessRuleException e) when (e.Code == BusinessErrors.CampanhaNaoEncontrada.Code)
        {
            throw new BusinessRuleException(BusinessErrors.MesaNaoEncontrada);
        }
        return mesa;
    }

    private async Task<(ParticipanteMesa Participante, Mesa Mesa)> ObterParticipanteDoUsuarioOuLancarAsync(int idUsuario, int id)
    {
        var participante = await _participanteRepository.ObterAsync(id)
                           ?? throw new BusinessRuleException(BusinessErrors.ParticipanteNaoEncontrado);
        try
        {
            return (participante, await ObterMesaDoUsuarioOuLancarAsync(idUsuario, participante.IdMesa));
        }
        catch (BusinessRuleException e) when (e.Code == BusinessErrors.MesaNaoEncontrada.Code)
        {
            throw new BusinessRuleException(BusinessErrors.ParticipanteNaoEncontrado);
        }
    }

    /// <summary>Uma linha no diário da mesa (para o resumo da sessão).</summary>
    private Task RegistrarAsync(int idMesa, string tipo, string texto) =>
        _eventoRepository.InserirAsync(new EventoMesa { IdMesa = idMesa, Tipo = tipo, Texto = texto.Length > 300 ? texto[..297] + "…" : texto, Data = Agora() });

    private static DateTime Agora()
    {
        var agora = DateTime.Now;
        return new DateTime(agora.Year, agora.Month, agora.Day, agora.Hour, agora.Minute, agora.Second, DateTimeKind.Unspecified);
    }
}
