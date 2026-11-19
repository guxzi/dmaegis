using System.Collections.Concurrent;
using System.Threading.Channels;

namespace DMAegis.Services;

/// <summary>
/// Avisos de "a mesa mudou" por campanha, para as telas abertas (mestre e jogadores) buscarem o estado de novo.
/// O aviso leva só um número de versão: nada sensível passa por aqui, e cada tela busca o estado com as
/// próprias permissões (o jogador, só a visão filtrada).
/// </summary>
public interface ICanalMesa
{
    void Avisar(int idCampanha);

    /// <summary>Começa a ouvir a campanha. Descartar a inscrição para de ouvir.</summary>
    (ChannelReader<long> Leitor, IDisposable Inscricao) Assinar(int idCampanha);
}

/// <summary>Em memória (um processo só, o PC do mestre). Registrado como singleton.</summary>
public class CanalMesa : ICanalMesa
{
    private readonly ConcurrentDictionary<int, ConcurrentDictionary<Guid, Channel<long>>> _ouvintes = new();
    private long _versao;

    public void Avisar(int idCampanha)
    {
        if (!_ouvintes.TryGetValue(idCampanha, out var ouvintes))
            return;
        var versao = Interlocked.Increment(ref _versao);
        foreach (var canal in ouvintes.Values)
            canal.Writer.TryWrite(versao);
    }

    public (ChannelReader<long> Leitor, IDisposable Inscricao) Assinar(int idCampanha)
    {
        // Só o aviso mais recente importa: se a tela atrasar, os antigos são descartados.
        var canal = Channel.CreateBounded<long>(new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest });
        var id = Guid.NewGuid();
        var ouvintes = _ouvintes.GetOrAdd(idCampanha, _ => new ConcurrentDictionary<Guid, Channel<long>>());
        ouvintes[id] = canal;
        return (canal.Reader, new Inscricao(() =>
        {
            ouvintes.TryRemove(id, out _);
            canal.Writer.TryComplete();
        }));
    }

    public int ContarOuvintes(int idCampanha) => _ouvintes.TryGetValue(idCampanha, out var o) ? o.Count : 0;

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
