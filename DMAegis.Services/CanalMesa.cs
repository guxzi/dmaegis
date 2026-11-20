using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;

namespace DMAegis.Services;

/// <summary>Um item do fluxo da mesa: "mesa" (só a versão: busque o estado de novo) ou "sinal" (efeito passageiro, em JSON).</summary>
public record AvisoMesa(string Evento, string Dados);

/// <summary>
/// Sinal passageiro da mesa: não é gravado no banco, só passa para as telas abertas.
/// Tipos: "ping" (coluna e linha), "reacao" (emoji, saindo da ficha de quem reagiu),
/// "arrasto" (a ficha sendo arrastada, em células com fração) e "arrasto-fim" (com a célula onde ela foi solta).
/// </summary>
public class SinalMesaServiceModel
{
    public string Tipo { get; set; } = string.Empty;

    /// <summary>A mesa do sinal (a tela ignora sinais de outra mesa).</summary>
    public int IdMesa { get; set; }
    public double? Coluna { get; set; }
    public double? Linha { get; set; }
    public int? IdParticipante { get; set; }
    public string? Emoji { get; set; }
    public string Autor { get; set; } = string.Empty;

    /// <summary>Marca da tela que mandou, para ela ignorar o próprio sinal quando ele voltar.</summary>
    public string? Origem { get; set; }

    /// <summary>Se os jogadores recebem. O mestre recebe todos.</summary>
    public bool Publico { get; set; }
}

/// <summary>
/// Avisos da mesa por campanha, para as telas abertas (mestre e jogadores).
/// O aviso "mesa" leva só um número de versão: cada tela busca o estado com as próprias permissões
/// (o jogador, só a visão filtrada). Os sinais passageiros só chegam aos jogadores quando são públicos.
/// </summary>
public interface ICanalMesa
{
    void Avisar(int idCampanha);

    /// <summary>Manda um sinal passageiro. Quem decide se é público é o Service.</summary>
    void Sinalizar(int idCampanha, SinalMesaServiceModel sinal);

    /// <summary>Começa a ouvir a campanha. Descartar a inscrição para de ouvir.</summary>
    (ChannelReader<AvisoMesa> Leitor, IDisposable Inscricao) Assinar(int idCampanha, bool mestre);
}

/// <summary>Em memória (um processo só, o PC do mestre). Registrado como singleton.</summary>
public class CanalMesa : ICanalMesa
{
    // Cabe uma rajada de sinais (arrasto, reações); se a tela atrasar demais, os mais antigos caem.
    private const int Capacidade = 64;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly ConcurrentDictionary<int, ConcurrentDictionary<Guid, Ouvinte>> _ouvintes = new();
    private long _versao;

    public void Avisar(int idCampanha)
    {
        if (!_ouvintes.TryGetValue(idCampanha, out var ouvintes))
            return;
        var aviso = new AvisoMesa("mesa", Interlocked.Increment(ref _versao).ToString());
        foreach (var ouvinte in ouvintes.Values)
            ouvinte.Canal.Writer.TryWrite(aviso);
    }

    public void Sinalizar(int idCampanha, SinalMesaServiceModel sinal)
    {
        if (!_ouvintes.TryGetValue(idCampanha, out var ouvintes))
            return;
        var aviso = new AvisoMesa("sinal", JsonSerializer.Serialize(sinal, Json));
        foreach (var ouvinte in ouvintes.Values)
            if (ouvinte.Mestre || sinal.Publico)
                ouvinte.Canal.Writer.TryWrite(aviso);
    }

    public (ChannelReader<AvisoMesa> Leitor, IDisposable Inscricao) Assinar(int idCampanha, bool mestre)
    {
        var canal = Channel.CreateBounded<AvisoMesa>(new BoundedChannelOptions(Capacidade) { FullMode = BoundedChannelFullMode.DropOldest });
        var id = Guid.NewGuid();
        var ouvintes = _ouvintes.GetOrAdd(idCampanha, _ => new ConcurrentDictionary<Guid, Ouvinte>());
        ouvintes[id] = new Ouvinte(canal, mestre);
        return (canal.Reader, new Inscricao(() =>
        {
            ouvintes.TryRemove(id, out _);
            canal.Writer.TryComplete();
        }));
    }

    public int ContarOuvintes(int idCampanha) => _ouvintes.TryGetValue(idCampanha, out var o) ? o.Count : 0;

    private sealed record Ouvinte(Channel<AvisoMesa> Canal, bool Mestre);

    private sealed class Inscricao(Action aoDescartar) : IDisposable
    {
        private int _descartada;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _descartada, 1) == 0)
                aoDescartar();
        }
    }
}
